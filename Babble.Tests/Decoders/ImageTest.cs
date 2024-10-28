using Babble.Core;
using Babble.Core.Scripts.Decoders;
using Babble.Core.Settings;
using Emgu.CV;

namespace Babble.Tests.Decoders;

public class TestPlatformConnector : PlatformConnector
{
    public TestPlatformConnector(string url) : base(url)
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        Capture = new ImageCapture(Url);
    }
}

public class ImageCaptureTests : IDisposable
{
    private readonly string _testImagePath = Path.Combine(Directory.GetCurrentDirectory(), "0085.png") ;
    private const string TestWindowName = "After";
    private TestPlatformConnector _connector;
    private bool _disposed;


    public ImageCaptureTests()
    {
        BabbleSettings settings = new BabbleSettings();
        settings.Cam.CaptureSource = _testImagePath;
        BabbleCore.Instance.Start(settings);
        _connector = new TestPlatformConnector(_testImagePath); // We don't technically need to pass in the image here
        _connector.Initialize();
    }

    [Fact]
    public void GetFrameData_ShouldProcessAndDisplayImage()
    {
        // Arrange
        Assert.True(_connector.Capture.StartCapture());
        _connector.WaitForCamera();

        // Act
        float[] frameData = _connector.GetFrameData();

        // Assert
        Assert.NotNull(frameData);
        Assert.NotEmpty(frameData);
    }

    [Fact]
    public void GetFrameData_ShouldReturnEmptyArray_WhenFrameIsEmpty()
    {
        // Arrange
        _connector = new TestPlatformConnector("nonexistent.jpg");
        _connector.Initialize();
        _connector.Capture.StartCapture();

        // Act/Assert
        Assert.Throws<FileNotFoundException>(() =>
            _connector.GetFrameData());
    }

    [Fact]
    public void Initialize_ShouldCreateImageCapture()
    {
        // Arrange & Act
        _connector.Initialize();

        // Assert
        Assert.NotNull(_connector.Capture);
        Assert.IsType<ImageCapture>(_connector.Capture);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        BabbleCore.Instance.Stop();

        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    _connector?.Terminate();
                    CvInvoke.DestroyAllWindows();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }

            _disposed = true;
        }
    }
}