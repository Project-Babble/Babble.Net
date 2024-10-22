using Emgu.CV;

namespace Babble.Maui.Scripts;

// https://github.com/thohemp/OpenCVSharpCameraDeviceEnumerator/blob/master/OpenCVDeviceEnumerator/OpenCVDeviceEnumerator.cs
public static class EmguCVDeviceEnumerator
{
    private static List<int> camIdList = new List<int>();
    private const int CONNECTION_ATTEPMTS = 5;

    /// <summary>
    /// Lists all connected cameras to this PC and their respective IDs (as ints)
    /// </summary>
    /// <param name="camIds"></param>
    /// <param name="cameraTimeoutMS"></param>
    /// <returns></returns>
    public static bool EnumerateCameras(out Dictionary<int, VideoCapture.API> camIds, int cameraTimeoutMS = 0)
    {
        camIds = new Dictionary<int, VideoCapture.API>();

        // list of all CAP drivers in EmguCV
        var drivers = Utils.EnumerateEnum<VideoCapture.API>();

        foreach (var driverEnum in drivers)
        {
            var maxID = 100;                                         // 100 IDs between drivers
            if (driverEnum == VideoCapture.API.Vfw)
                maxID = 10;                                          // VWF opens same camera after 10 ?!?

            for (int idx = 0; idx < maxID; idx++)
            {
                var sum = (int)driverEnum + idx;
                using VideoCapture cap = new VideoCapture(sum);      // open the camera

                for (int i = 0; i < CONNECTION_ATTEPMTS; i++)
                {
                    if (cap.IsOpened)                                // check if we succeeded after a timeout period
                    {
                        cap.Release();                               // important!
                        camIds.Add(sum, driverEnum);
                        break;
                    }
                    Thread.Sleep(cameraTimeoutMS);
                }
            }
        }

        return camIds.Count > 0;
    }
}
