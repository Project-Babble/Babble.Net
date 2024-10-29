using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Captures and decodes an MJPEG stream, commonly used by IP Cameras
/// Mobile-platform specific implementation, assumes a fixed camera size of 256x256
/// https://gist.github.com/lightfromshadows/79029ca480393270009173abc7cad858
/// </summary>
public class IPCameraCapture : Capture
{
    public override byte[] Frame { get => finalFrameBuffer; }
    public override (int width, int height) Dimensions => (640, 480);
    public override bool IsReady { get; set; }
    public override string Url { get; set; }

    private byte[] finalFrameBuffer;

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public IPCameraCapture(string Url) : base(Url)
    {
    }

    public override bool StartCapture()
    {
        Task.Run(StartStreaming);
        IsReady = true;
        return true;
    }

    public async Task StartStreaming()
    {
        try
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to connect to MJPEG stream: {response.StatusCode}");
                return;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            await ReadFramesFromStream(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MJPEG streaming: {ex.Message}");
        }
    }

    private async Task ReadFramesFromStream(Stream stream)
    {
        var buffer = new byte[1024 * 1024];
        int bytesRead;
        var frameBuffer = new MemoryStream();

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
        {
            frameBuffer.Write(buffer, 0, bytesRead);

            // Convert buffer to string to locate headers
            var frameData = frameBuffer.ToArray();
            var content = System.Text.Encoding.UTF8.GetString(frameData);

            // Find the start of the JPEG frame by locating the end of headers
            int headerEndIndex = content.IndexOf("\r\n\r\n");
            if (headerEndIndex != -1)
            {
                // Parse headers to find Content-Length
                var headers = content.Substring(0, headerEndIndex);
                var contentLengthMatch = System.Text.RegularExpressions.Regex.Match(headers, @"Content-Length: (\d+)");
                if (contentLengthMatch.Success && int.TryParse(contentLengthMatch.Groups[1].Value, out int contentLength))
                {
                    // Calculate the start index of the JPEG data
                    int jpegStartIndex = headerEndIndex + 4; // 4 = length of "\r\n\r\n"

                    // Ensure we have the full JPEG frame based on Content-Length
                    if (frameData.Length >= jpegStartIndex + contentLength)
                    {
                        // Extract the JPEG frame
                        var jpegFrame = new byte[contentLength];
                        Array.Copy(frameData, jpegStartIndex, jpegFrame, 0, contentLength);

                        // Display the JPEG frame
                        var jpegMat = new Mat();
                        CvInvoke.Imdecode(jpegFrame, ImreadModes.Color, jpegMat);
                        CvInvoke.CvtColor(jpegMat, jpegMat, ColorConversion.Bgr2Gray);
                        finalFrameBuffer = jpegMat.GetRawData();

                        // Clear frame buffer after displaying the image
                        frameBuffer.SetLength(0);
                        frameBuffer.Position = 0;

                        // Copy any remaining data beyond the current frame into the buffer
                        if (frameData.Length > jpegStartIndex + contentLength)
                        {
                            frameBuffer.Write(frameData, jpegStartIndex + contentLength, frameData.Length - jpegStartIndex - contentLength);
                        }
                    }
                }
            }
        }
    }


    public override bool StopCapture()
    {
        _cancellationTokenSource.Cancel();
        IsReady = false;
        return true;
    }
}
