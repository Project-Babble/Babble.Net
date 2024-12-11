using OpenCvSharp;
using System.Diagnostics;
using System.IO.Ports;

namespace Babble.Core.Scripts;

public static class DeviceEnumerator
{
    public static List<string> ListCameraNames()
    {
        List<string> camNames = [];

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            camNames.AddRange(ListCamerasOpenCV());
            camNames.AddRange(ListSerialPorts());
        }
        else if (OperatingSystem.IsLinux())
        {
            camNames.AddRange(ListLinuxUvcDevices());
            camNames.AddRange(ListSerialPorts());
        }

        return camNames;
    }

    // Use OpenCVSharp to detect available cameras
    private static List<string> ListCamerasOpenCV()
    {
        var cameraIndexes = new List<string>();
        int index = 0;

        while (true)
        {
            var capture = new VideoCapture(index);
            if (!capture.IsOpened())
            {
                break;
            }
            else
            {
                cameraIndexes.Add($"/dev/video{index}"); // For Linux/Mac (you can adjust it for Windows if needed)
            }
            capture.Release();
            index++;
        }

        return cameraIndexes;
    }

    // Check for UVC devices on Linux using `v4l2-ctl` command
    private static List<string> ListLinuxUvcDevices()
    {
        var devices = new List<string>();
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "v4l2-ctl",
                    Arguments = "--list-devices",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Process the output to get UVC device paths
            var lines = output.Split('\n');
            string currentDevice = null!;

            foreach (var line in lines)
            {
                if (!line.StartsWith("\t"))
                {
                    currentDevice = line.Trim();
                }
                else
                {
                    if (line.Contains("/dev/video") && IsUvcDevice(line.Trim()))
                    {
                        devices.Add(line.Trim());
                    }
                }
            }
        }
        catch (Exception e)
        {
            devices.Add($"Error listing UVC devices: {e.Message}");
        }

        return devices;
    }

    // Helper function to check if the device is a valid UVC device
    private static bool IsUvcDevice(string device)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "v4l2-ctl",
                    Arguments = $"--device={device} --all",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Check for "UVC Payload Header Metadata" to exclude non-video devices
            if (output.Contains("UVC Payload Header Metadata"))
            {
                return false;  // Not a valid video device
            }

            return true;  // Valid video device
        }
        catch
        {
            return false; // Error checking the device
        }
    }

    // List serial ports available on the system
    private static List<string> ListSerialPorts()
    {
        List<string> result = [];

        if (OperatingSystem.IsWindows())
        {
            result.AddRange(SerialPort.GetPortNames());
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            var ports = Directory.GetFiles("/dev", "tty*");
            result.AddRange(ports);
        }

        return result;
    }
}