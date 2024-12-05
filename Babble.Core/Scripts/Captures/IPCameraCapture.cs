using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System.Net.Http.Headers;
using System.Text;

namespace Babble.Core.Scripts.Captures;

/// <summary>
/// Captures and decodes an MJPEG stream, commonly used by IP Cameras
/// Mobile-platform specific implementation
/// https://github.com/Larry57/SimpleMJPEGStreamViewer
/// https://stackoverflow.com/questions/3801275/how-to-convert-image-to-byte-array
/// </summary>
public class IPCameraCapture : Capture
{ 
    public override (int width, int height) Dimensions 
    {
        get
        {
            var x = BabbleCore.Instance.Settings.GeneralSettings.GuiCamResolutionX;
            var y = BabbleCore.Instance.Settings.GeneralSettings.GuiCamResolutionY;
            if (x > 0 && y > 0)
            {
                // Support custom IP camera resolutions.
                // This must be set by the user, we need res to create the Mat below
                return (x, y);
            }
            else
            {
                // Babble Cam res (240x240)
                return DefaultFrameDimensions;
            }
        }       
    }

    public override uint FrameCount { get; protected set; }
    public override Mat RawMat { get; } = new Mat();
    public override bool IsReady { get; protected set; }
    public override string Url { get; set; } = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    // JPEG delimiters
    private const byte picMarker = 0xFF;
    private const byte picStart = 0xD8;
    private const byte picEnd = 0xD9;

    // Determine if we've got stuck on the same frame
    private Mat _prevMat = new Mat();

    public IPCameraCapture(string Url) : base(Url)
    {
    }

    public override Task<bool> StartCapture()
    {
        Task.Run(() => StartStreaming(Url, null, null, _cancellationTokenSource.Token, 1024, Dimensions.width * Dimensions.height));
        IsReady = true;
        return Task.FromResult(true);
    }

    /// <summary>
    /// Start a MJPEG on a http stream
    /// </summary>
    /// <param name="url">url of the http stream (only basic auth is implemented)</param>
    /// <param name="login">optional login</param>
    /// <param name="password">optional password (only basic auth is implemented)</param>
    /// <param name="token">cancellation token used to cancel the stream parsing</param>
    /// <param name="chunkMaxSize">Max chunk byte size when reading stream</param>
    /// <param name="frameBufferSize">Maximum frame byte size</param>
    /// <returns></returns>
    /// 
    public async Task StartStreaming(string url, string? login = null, string? password = null, CancellationToken? token = null, int chunkMaxSize = 1024, int frameBufferSize = 1024 * 1024)
    {
        var tok = token ?? CancellationToken.None;

        using var cli = new HttpClient();

        if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{login}:{password}")));

        using var stream = await cli.GetStreamAsync(url).ConfigureAwait(false);

        var streamBuffer = new byte[chunkMaxSize];      // Stream chunk read
        var frameBuffer = new byte[frameBufferSize];    // Frame buffer

        var frameIdx = 0;       // Last written byte location in the frame buffer
        var inPicture = false;  // Are we currently parsing a picture ?
        byte current = 0x00;    // The last byte read
        byte previous = 0x00;   // The byte before

        // Continuously pump the stream. The cancellation token is used to get out of there
        while (true)
        {
            var streamLength = await stream.ReadAsync(streamBuffer, 0, chunkMaxSize, tok).ConfigureAwait(false);
            ParseStreamBuffer(frameBuffer, ref frameIdx, streamLength, streamBuffer, ref inPicture, ref previous, ref current);
        };
    }

    // Parse the stream buffer

    private void ParseStreamBuffer(byte[] frameBuffer, ref int frameIdx, int streamLength, byte[] streamBuffer, ref bool inPicture, ref byte previous, ref byte current)
    {
        var idx = 0;
        while (idx < streamLength)
        {
            if (inPicture)
            {
                ParsePicture(frameBuffer, ref frameIdx, ref streamLength, streamBuffer, ref idx, ref inPicture, ref previous, ref current);
            }
            else
            {
                SearchPicture(frameBuffer, ref frameIdx, ref streamLength, streamBuffer, ref idx, ref inPicture, ref previous, ref current);
            }
        }
    }

    // While we are looking for a picture, look for a FFD8 (end of JPEG) sequence.

    private void SearchPicture(byte[] frameBuffer, ref int frameIdx, ref int streamLength, byte[] streamBuffer, ref int idx, ref bool inPicture, ref byte previous, ref byte current)
    {
        do
        {
            previous = current;
            current = streamBuffer[idx++];

            // JPEG picture start ?
            if (previous == picMarker && current == picStart)
            {
                frameIdx = 2;
                frameBuffer[0] = picMarker;
                frameBuffer[1] = picStart;
                inPicture = true;
                return;
            }
        } while (idx < streamLength);
    }

    // While we are parsing a picture, fill the frame buffer until a FFD9 is reach.
    private void ParsePicture(byte[] frameBuffer, ref int frameIdx, ref int streamLength, byte[] streamBuffer, ref int idx, ref bool inPicture, ref byte previous, ref byte current)
    {
        do
        {
            previous = current;
            current = streamBuffer[idx++];
            frameBuffer[frameIdx++] = current;

            // JPEG picture end ?
            if (previous == picMarker && current == picEnd)
            {
                // Using a memory stream this way prevent arrays copy and allocations
                using (var s = new MemoryStream(frameBuffer, 0, frameIdx))
                {
                    try
                    {
                        FrameCount++;
                        Mat.FromImageData(TrimEnd(frameBuffer), ImreadModes.Color).CopyTo(RawMat);
                    }
                    catch (Exception e)
                    {
                        // We don't care about badly decoded pictures
                        BabbleCore.Instance.Logger.LogError(e.Message);
                    }
                }

                inPicture = false;
                return;
            }
        } while (idx < streamLength);
    }

    public override bool StopCapture()
    {
        _cancellationTokenSource.Cancel();
        IsReady = false;
        return true;
    }

    public static byte[] TrimEnd(byte[] array)
    {
        int lastIndex = Array.FindLastIndex(array, b => b != 0);

        Array.Resize(ref array, lastIndex + 1);

        return array;
    }
}
