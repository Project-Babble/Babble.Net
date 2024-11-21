using Babble.Core;
using Babble.Core.Settings;
using Babble.OSC;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Hypernex.ExtendedTracking;
using Microsoft.Extensions.Logging;
using VRCFaceTracking;
using VRCFaceTracking.BabbleNative;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Services;

namespace Babble.Benchmarks.Babble.Avalonia;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[Config(typeof(AvaloniaBenchmarkConfig))]
public class AvaloniaBenchmarks
{
    private MainIntegrated _mainIntegrated;
    private UnifiedLibManager _libManager;
    private BabbleModule _babbleModule;

    public static BabbleSettings Settings { get; set; } = null;

    private class AvaloniaBenchmarkConfig : ManualConfig
    {
        public AvaloniaBenchmarkConfig()
        {
            AddJob(Job.Default
                .WithWarmupCount(3)     // Number of warmup iterations
                .WithIterationCount(10)  // Number of target iterations
                .WithInvocationCount(64) // How many times to invoke per iteration, must be a multiple of 16
            );
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        // Setup VRCFT
        var settings = new FaceTrackingServices.FTSettings();
        var loggerFactory = new FaceTrackingServices.FTLoggerFactory();
        var dispatcher = new FaceTrackingServices.FTDispatcher();
        var moduleDataServiceLogger = loggerFactory.CreateLogger<ModuleDataService>();
        var mutatorLogger = loggerFactory.CreateLogger<UnifiedTrackingMutator>();
        var moduleDataService = new ModuleDataService(new FaceTrackingServices.BabbleIdentity(), moduleDataServiceLogger);
        var libManager = new UnifiedLibManager(loggerFactory, dispatcher, moduleDataService);
        var mutator = new UnifiedTrackingMutator(mutatorLogger, settings);
        _mainIntegrated = new MainIntegrated(loggerFactory, libManager, mutator);
        Task.Run(async () => await _mainIntegrated.InitializeAsync());
        ParameterSenderService.AllParametersRelevant = true;

        // Setup module in isolation
        _babbleModule = new BabbleModule();

        // Give camera time to initialize
        Thread.Sleep(1000);
    }

    [Benchmark(Description = "BabbleCore Expression Data Retrieval")]
    public bool GetExpressionData()
    {
        return BabbleCore.Instance.GetExpressionData(out _);
    }

    [Benchmark(Description = "Module Update Method")]
    public void ModuleUpdate()
    {
        _babbleModule.Update();
    }

    [Benchmark(Description = "UnifiedLibManager Module Initialization")]
    public void InitializeModule()
    {
        _libManager.Initialize();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _mainIntegrated.Teardown();
    }
}