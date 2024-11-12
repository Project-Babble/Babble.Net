using Babble.Core;
using Babble.Core.Enums;
using Babble.Core.Scripts;
using Babble.OSC.Collections;
using Babble.OSC.Configuration;
using Babble.OSC.Desktop.Mappings;
using Babble.OSC.Enums;
using Babble.OSC.Expressions;
using Babble.OSC.Mappings;
using Microsoft.Extensions.Logging;
using Rug.Osc;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace Babble.OSC;

public class BabbleOSC
{
    private OscSender _sender;
    private ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _sendTask;

    private List<ExpressionMapping> _mappings = new();
    private const int SlidingWindowSize = 60; // 60 frames (1 second at 60Hz)
    private uint _frameCount = 0;
    private Dictionary<string, Queue<float>> _expressionUsageHistory = new();
    private UserExpressionConfig _userConfig;
    private readonly string _userConfigFilePath;

    private readonly int _resolvedLocalPort;

    private readonly int _resolvedRemotePort;

    private int _connectionAttempts;

    private readonly string _resolvedHost;

    public const string DEFAULT_HOST = "127.0.0.1";

    public const int DEFAULT_LOCAL_PORT = 44444;

    public const int DEFAULT_REMOTE_PORT = 8888;

    private const int TIMEOUT_MS = 10000;

    private const int SEND_INTERVAL_MS = 200;

    private const int MAX_RETRIES = 5;

    public event EventHandler? OnConnectionLost;

    public BabbleOSC(string? host = null, int? remotePort = null, string? userConfigFilePath = null)
    {
        var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        foreach (var resource in resources)
        {
            var extractedResource = Path.Combine(AppContext.BaseDirectory, resource);
            Utils.ExtractEmbeddedResource(Assembly.GetExecutingAssembly(), resource, extractedResource, overwrite: true);
        }

        LoadMappingsFromFile(Path.Combine(AppContext.BaseDirectory, "Babble.OSC.LipExpressionsManifest.txt"));

        _resolvedHost = host ?? DEFAULT_HOST;
        _resolvedRemotePort = remotePort ?? DEFAULT_REMOTE_PORT;
        _resolvedLocalPort = DEFAULT_LOCAL_PORT;
        _userConfigFilePath = userConfigFilePath ?? "user_config.json";
        _userConfig = UserExpressionConfig.Load(_userConfigFilePath);

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = factory.CreateLogger("BabbleOSC");

        ConfigureReceiver();

        _cancellationTokenSource  = new CancellationTokenSource();
        _sendTask = Task.Run(() => SendLoopAsync(_cancellationTokenSource.Token));
    }

    private void ConfigureReceiver()
    {
        IPAddress address = IPAddress.Parse(_resolvedHost);
        _sender = new OscSender(address, _resolvedLocalPort, _resolvedRemotePort)
        {
            DisconnectTimeout = TIMEOUT_MS
        };
        _sender.Connect();
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        var settings = BabbleCore.Instance.Settings;

        while (!cancellationToken.IsCancellationRequested)
        {
            var mul = (float) settings.GetSetting<double>("gui_multiply");
            var forceRelevancy = settings.GetSetting<bool>("gui_force_relevancy");
            var prefix = settings.GetSetting<string>("gui_osc_location");
            prefix = string.IsNullOrEmpty(prefix) ? "/" : prefix;

            try
            {
                if (forceRelevancy && _sender.State == OscSocketState.Connected)
                {
                    _connectionAttempts = 0;
                    foreach (var mapping in _mappings)
                    {
                        if (mapping.ShouldUpdate())
                        {
                            var candidate = mapping.ComputeFloatValue(UnifiedExpressionToFloatMapping.Expressions);

                            if (char.IsDigit(mapping.Address.Last()))
                            {
                                if (candidate == 0f)
                                {
                                    if (mapping.HasSignificantChange(candidate))
                                    {
                                        var message = new OscMessage(
                                            $"{prefix}{mapping.Address}",
                                            false
                                        );
                                        _sender.Send(message);
                                    }
                                }
                                else if (candidate == 1f)
                                {
                                    if (mapping.HasSignificantChange(candidate))
                                    {
                                        var message = new OscMessage(
                                            $"{prefix}{mapping.Address}",
                                            true
                                        );
                                        _sender.Send(message);
                                    }
                                }
                            }
                            else
                            {
                                candidate *= mul;

                                // Track expression usage for priority adjustments
                                TrackExpressionUsage(mapping.Address, candidate);

                                if (mapping.HasSignificantChange(candidate))
                                {
                                    var message = new OscMessage(
                                        $"{prefix}{mapping.Address}",
                                        candidate
                                    );
                                    _sender.Send(message);
                                }
                            }
                        }
                    }

                    //// Check and update expression priorities every second
                    //if (_frameCount % 60 == 0)
                    //{
                    //    UpdateExpressionPriorities();
                    //}

                    //_frameCount++;
                }
                else if (_sender.State == OscSocketState.Connected)
                {
                    _connectionAttempts = 0;
                    foreach (var exp in UnifiedExpressionToStringMapping.Mappings)
                    {
                        string value = exp.Value;
                        float key = UnifiedExpressionToFloatMapping.Expressions[exp.Key];
                        _sender.Send(new OscMessage($"{prefix}{value}", key * mul));
                    }
                }
                else if (_sender.State == OscSocketState.Closed)
                {
                    if (_connectionAttempts < MAX_RETRIES)
                    {
                        _connectionAttempts++;

                        // Close and dispose the current sender
                        _sender.Close();
                        _sender.Dispose();

                        // Delay before attempting to reconnect
                        await Task.Delay(1000, cancellationToken);

                        // Attempt to reconfigure the receiver and reconnect
                        ConfigureReceiver();
                    }
                    else
                    {
                        // Trigger the connection lost event after max retries reached
                        OnConnectionLost?.Invoke(this, EventArgs.Empty);
                        return; // Exit the loop
                    }
                }
            }
            catch (Exception e)
            {
                //IGNORE THIS
            }
            finally
            {
                // Delay between each loop iteration to avoid high CPU usage
                await Task.Delay(SEND_INTERVAL_MS, cancellationToken);
            }
        }
    }
    
    public void Teardown()
    {
        _cancellationTokenSource.Cancel();
        _sender.Close();
        _sender.Dispose();
        _cancellationTokenSource.Dispose();
    }

    private ExpressionPriority GetPriorityForExpression(string expressionName)
    {
        // Check if the user has a custom priority set
        //if (_userConfig.ExpressionPriorities.TryGetValue(expressionName, out var customPriority))
        //{
        //    return customPriority;
        //}

        // High priority for core expressions
        if (expressionName.StartsWith("Jaw") ||
            expressionName.StartsWith("Mouth") ||
            expressionName.StartsWith("Lip"))
        {
            return ExpressionPriority.Low;
        }

        // Low priority for less noticeable expressions
        if (expressionName.StartsWith("Cheek") ||
            expressionName.StartsWith("Nose"))
        {
            return ExpressionPriority.Low;
        }

        // Medium priority for everything else
        return ExpressionPriority.Low;
    }

    private List<ExpressionMapping> CreateMappingsFromAddress(string address)
    {
        var mappings = new List<ExpressionMapping>();
        string baseName = address.Replace("v2/", string.Empty);
        bool isNegative = false;

        if (baseName.EndsWith("Negative"))
        {
            isNegative = true;
            baseName = baseName.Replace("Negative", string.Empty);
        }

        var priority = GetPriorityForExpression(baseName);

        // Now comes the enum parsing. First, is this a non-combined expression?
        if (Enum.TryParse<UnifiedExpression>(baseName, out var expression))
        {
            mappings.Add(new ExpressionMapping(
                address,
                expressions => isNegative
                    ? -expressions[expression]
                    : expressions[expression],
                priority
            ));
            return mappings;
        }
        // Next, is this a combined "X" param?
        else if (baseName.EndsWith("X"))
        {
            var baseNameNoX = baseName.Remove(baseName.Length - 1);

            Enum.TryParse<UnifiedExpression>($"{baseNameNoX}Left", out var expressionLeft);
            Enum.TryParse<UnifiedExpression>($"{baseNameNoX}Right", out var expressionRight);

           Func<Dictionary<UnifiedExpression, float>, float> computeValue =
                expressions => isNegative
                    ? -(expressions[expressionRight] - expressions[expressionLeft])
                    : expressions[expressionRight] - expressions[expressionLeft];

            var mainMapping = new ExpressionMapping(
                address,
                computeValue,
                priority
            );

            mappings.Add(mainMapping);
            mappings.AddRange(CreateBinaryMappingsForAddress(address, computeValue));

            return mappings;
        }
        // Next, is this a combined "Y" param?
        else if (baseName.EndsWith("Y"))
        {
            var baseNameNoX = baseName.Remove(baseName.Length - 1);

            Enum.TryParse<UnifiedExpression>($"{baseNameNoX}Up", out var expressionUp);
            Enum.TryParse<UnifiedExpression>($"{baseNameNoX}Down", out var expressionDown);

           Func<Dictionary<UnifiedExpression, float>, float> computeValue =
                expressions => isNegative
                    ? -(expressions[expressionDown] - expressions[expressionUp])
                    : expressions[expressionDown] - expressions[expressionUp];

            var mainMapping = new ExpressionMapping(
                address,
                computeValue,
                priority
            );

            mappings.Add(mainMapping);
            mappings.AddRange(CreateBinaryMappingsForAddress(address, computeValue));

            return mappings;
        }
        // Next, is this a combined "Left" param?
        // IE LipPuckerLeft is LipPuckerLowerLeft and LipPuckerUpperLeft
        else if (baseName.EndsWith("Left"))
        {
            var newBaseName = baseName.Replace("Left", string.Empty);
            var upperLeft = Enum.TryParse<UnifiedExpression>($"{newBaseName}LowerLeft", out var expressionLowerLeft);
            var upperRight = Enum.TryParse<UnifiedExpression>($"{newBaseName}UpperLeft", out var expressionUpperLeft);

            if (upperLeft && upperRight)
            {
               Func<Dictionary<UnifiedExpression, float>, float> computeValue =
                            expressions => isNegative
                    ? -(expressions[expressionUpperLeft] + expressions[expressionLowerLeft]) / 2
                    : expressions[expressionUpperLeft] + expressions[expressionLowerLeft] / 2;

                var mainMapping = new ExpressionMapping(
                    address,
                    computeValue,
                    priority
                );

                mappings.Add(mainMapping);
                mappings.AddRange(CreateBinaryMappingsForAddress(address, computeValue));

                return mappings;
            }
        }
        // Next, is this a combined "Right" param?
        else if (baseName.EndsWith("Right"))
        {
            var newBaseName = baseName.Replace("Right", string.Empty);
            var upperLeft = Enum.TryParse<UnifiedExpression>($"{newBaseName}UpperRight", out var expressionUpperRight);
            var upperRight = Enum.TryParse<UnifiedExpression>($"{newBaseName}LowerRight", out var expressionLowerRight);

            if (upperLeft && upperRight)
            {
               Func<Dictionary<UnifiedExpression, float>, float> computeValue =
                            expressions => isNegative
                    ? -(expressions[expressionUpperRight] + expressions[expressionLowerRight]) / 2
                    : expressions[expressionUpperRight] + expressions[expressionLowerRight] / 2;

                var mainMapping = new ExpressionMapping(
                    address,
                    computeValue,
                    priority
                );

                mappings.Add(mainMapping);
                mappings.AddRange(CreateBinaryMappingsForAddress(address, computeValue));

                return mappings;
            }
        }
        // Next, is this an *implicitly* combined parameter?
        // IE MouthDimple is MouthDimpleLeft and MouthDimpleRight
        else
        {
            var left = Enum.TryParse<UnifiedExpression>($"{baseName}Left", out var expressionLeft);
            var right = Enum.TryParse<UnifiedExpression>($"{baseName}Right", out var expressionRight);

            if (left && right)
            {
               Func<Dictionary<UnifiedExpression, float>, float> computeValue =
                            expressions => isNegative
                    ? -(expressions[expressionRight] - expressions[expressionLeft])
                    : expressions[expressionRight] - expressions[expressionLeft];

                var mainMapping = new ExpressionMapping(
                    address,
                    computeValue,
                    priority
                );

                mappings.Add(mainMapping);
                mappings.AddRange(CreateBinaryMappingsForAddress(address, computeValue));

                return mappings;
            }
            
            var up = Enum.TryParse<UnifiedExpression>($"{baseName}Up", out var expressionUp);
            var down = Enum.TryParse<UnifiedExpression>($"{baseName}Down", out var expressionDown);

            if (up && down)
            {
               Func<Dictionary<UnifiedExpression, float>, float> computeValue =
                            expressions => isNegative
                    ? -(expressions[expressionDown] - expressions[expressionUp])
                    : expressions[expressionDown] - expressions[expressionUp];

                var mainMapping = new ExpressionMapping(
                    address,
                    computeValue,
                    priority
                );

                mappings.Add(mainMapping);
                mappings.AddRange(CreateBinaryMappingsForAddress(address, computeValue));

                return mappings;
            }
        }

        return mappings;
    }

    private List<ExpressionMapping> CreateBinaryMappingsForAddress(string baseAddress,Func<Dictionary<UnifiedExpression, float>, float> computeValue)
    {
        var binaryMappings = new List<ExpressionMapping>();

        foreach (int power in Float8Converter.BinaryPowers)
        {
            var binaryAddress = $"{baseAddress}{power}";

            // Create a mapping that extracts the appropriate bit
            var binaryMapping = new ExpressionMapping(
                binaryAddress,
                expressions =>
                {
                    float value = computeValue(expressions);
                    bool[] bits = Float8Converter.GetBits(value);
                    // Convert bool to float (0f or 1f)
                    return bits[Array.IndexOf(Float8Converter.BinaryPowers, power)] ? 1f : 0f;
                },
                ExpressionPriority.Medium  // Binary parameters are low priority
            );

            binaryMappings.Add(binaryMapping);
        }

        return binaryMappings;
    }

    //private void UpdateExpressionPriorities()
    //{
    //    // Find the top 5 most used expressions
    //    var topExpressions = _expressionUsageHistory
    //        .OrderByDescending(x => x.Value.Average())
    //        .Take(5)
    //        .ToDictionary(x => x.Key, x => x.Value.Average());

    //    // Update the user config with the new priorities
    //    _userConfig.ExpressionPriorities.Clear();
    //    foreach (var (expr, avgValue) in topExpressions)
    //    {
    //        if (avgValue > 0.1f) // Only promote expressions used more than 10% of the time
    //        {
    //            _userConfig.ExpressionPriorities[expr] = ExpressionPriority.High;
    //        }
    //        else if (avgValue > 0.05f) // Medium priority for expressions used more than 5% of the time
    //        {
    //            _userConfig.ExpressionPriorities[expr] = ExpressionPriority.Medium;
    //        }
    //    }

    //    // Save the updated user config
    //    _userConfig.Save(_userConfigFilePath);
    //}

    private void TrackExpressionUsage(string expressionName, float value)
    {
        if (!_expressionUsageHistory.ContainsKey(expressionName))
        {
            _expressionUsageHistory[expressionName] = new Queue<float>(SlidingWindowSize);
        }

        _expressionUsageHistory[expressionName].Enqueue(value);

        if (_expressionUsageHistory[expressionName].Count > SlidingWindowSize)
        {
            _expressionUsageHistory[expressionName].Dequeue();
        }
    }

    private void LoadMappingsFromFile(string filePath)
    {
        try
        {
            var addresses = File.ReadAllLines(filePath)
                              .Select(l => l.Trim())
                              .Where(l => !string.IsNullOrEmpty(l));

            _mappings.Clear(); // Clear existing mappings
            LoadExpressionMappings(addresses.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading mappings: {ex.Message}");
        }
    }

    private void LoadExpressionMappings(string[] addresses)
    {
        foreach (var addr in addresses)
        {
            var mapping = CreateMappingsFromAddress(addr);
            if (mapping != null)
            {
                _mappings.AddRange(mapping);
            }
        }
    }
}