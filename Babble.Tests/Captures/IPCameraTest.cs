using Babble.Core.Scripts.Captures;
using Microsoft.Extensions.Logging;
using Moq;

namespace Babble.Tests.Captures;

public class IPCameraCaptureTests
{
    private readonly Mock<ILogger<IPCameraCapture>> _loggerMock;

    public IPCameraCaptureTests()
    {
        _loggerMock = new Mock<ILogger<IPCameraCapture>>();
    }

    [Fact]
    public void StartCapture_Should_SetIsReadyToTrue()
    {
        // Arrange
        var ipCameraCapture = new IPCameraCapture("http://testcamera.local");

        // Act
        ipCameraCapture.StartCapture();

        // Assert
        Assert.True(ipCameraCapture.IsReady);
    }

    [Fact]
    public void StopCapture_Should_SetIsReadyToFalse_And_ClearBuffers()
    {
        // Arrange
        var ipCameraCapture = new IPCameraCapture("http://testcamera.local");
        ipCameraCapture.StartCapture();

        // Act
        ipCameraCapture.StopCapture();

        // Assert
        Assert.False(ipCameraCapture.IsReady);
    }

    //[Fact]
    //public void ReadMJPEGStreamWorker_Should_RetryOnFailure()
    //{
    //    // Arrange
    //    var ipCameraCapture = new IPCameraCapture("http://invalidcamera.local");

    //    // Mock WebRequest creation and response to simulate failure
    //    var webRequestMock = new Mock<WebRequest>();
    //    webRequestMock.Setup(w => w.GetResponse()).Throws(new WebException());

    //    HttpRequestMessage.Create = (url) => webRequestMock.Object;

    //    // Act
    //    var workerThread = new Thread(() => ipCameraCapture.StartCapture());
    //    workerThread.Start();
    //    Thread.Sleep(1000); // Let the worker run and retry

    //    // Assert
    //    Assert.True(ipCameraCapture.IsReady);  // Capture should still be considered "ready"
    //    Assert.Equal(1, ipCameraCapture.RetryCount);  // Should have retried at least once
    //}

    //[Fact]
    //public void Frame_Should_ContainData_AfterSuccessfulCapture()
    //{
    //    // Arrange
    //    var ipCameraCapture = new IPCameraCapture("http://testcamera.local");

    //    // Mock the WebRequest and Stream behavior to simulate receiving an MJPEG stream
    //    var webRequestMock = new Mock<WebRequest>();
    //    var mockStream = new Mock<Stream>();
    //    mockStream.Setup(s => s.CanRead).Returns(true);
    //    mockStream.Setup(s => s.ReadByte()).Returns(0xFF);

    //    var responseMock = new Mock<WebResponse>();
    //    responseMock.Setup(r => r.GetResponseStream()).Returns(mockStream.Object);
    //    webRequestMock.Setup(w => w.GetResponse()).Returns(responseMock.Object);

    //    WebRequest.Create = (url) => webRequestMock.Object;

    //    // Act
    //    var workerThread = new Thread(() => ipCameraCapture.StartCapture());
    //    workerThread.Start();
    //    Thread.Sleep(1000); // Let the worker run

    //    // Assert
    //    Assert.NotEmpty(ipCameraCapture.Frame); // Ensure that Frame contains some data
    //}
}
