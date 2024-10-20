using Emgu.CV;
using System.Drawing;
using System.Drawing.Imaging;

class Program
{
    private static VideoCapture capture;
    private static string ipCameraUrl = @"http://192.168.0.173:4747/video"; // Replace with the actual URL of your IP camera
    private static bool capturing;

    static void Main()
    {
        // Start capturing video from the IP camera
        capturing = true;
        StartCapture();
        Console.ReadKey();
    }

    private static void StartCapture()
    {
        try
        {
            // Initialize the capture from the IP camera
            capture = new VideoCapture(ipCameraUrl);

            if (!capture.IsOpened)
            {
                throw new Exception("Could not open the camera stream.");
            }

            // Grab the frame from the video stream
            using Mat frame = capture.QueryFrame();
            if (frame != null)
            {
                // Convert the frame to a Bitmap and display it
                Bitmap bitmap = frame.ToBitmap();

                // Save the frame as a PNG
                bitmap.Save("test.png", ImageFormat.Png);

                // Stop capturing after saving the frame
                capturing = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error connecting to the camera: " + ex.Message);
        }
    }
}
