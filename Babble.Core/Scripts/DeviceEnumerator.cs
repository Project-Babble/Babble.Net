using OpenCvSharp;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace Babble.Core.Scripts;

public static class DeviceEnumerator
{
    public static List<string> ListCameraNames()
    {
        List<string> camNames = new List<string>();

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            camNames.AddRange(ListCamerasOpenCV());
        }
        else if (OperatingSystem.IsLinux())
        {
            camNames.AddRange(ListLinuxUvcDevices());
        }
        camNames.AddRange(ListSerialPorts());

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

    private static List<string> ListLinuxUvcDevices()
    {
        [DllImport("libudev.so")] static extern IntPtr udev_new();
        [DllImport("libudev.so")] static extern IntPtr udev_unref(IntPtr udev);
        [DllImport("libudev.so")] static extern IntPtr udev_enumerate_new(IntPtr udev);
        [DllImport("libudev.so")] static extern int udev_enumerate_add_match_subsystem(IntPtr udev_enumerate, [MarshalAs(UnmanagedType.LPUTF8Str)] string subsystem);
        [DllImport("libudev.so")] static extern int udev_enumerate_scan_devices(IntPtr udev_enumerate);
        [DllImport("libudev.so")] static extern IntPtr udev_enumerate_get_list_entry(IntPtr udev_enumerate);
        [DllImport("libudev.so")] static extern IntPtr udev_list_entry_get_next(IntPtr list_entry);
        [DllImport("libudev.so")] static extern IntPtr udev_list_entry_get_name(IntPtr list_entry);
        [DllImport("libudev.so")] static extern IntPtr udev_device_new_from_syspath(IntPtr udev, IntPtr syspath);
        [DllImport("libudev.so")] static extern IntPtr udev_device_get_devnode(IntPtr udev_device);
        [DllImport("libudev.so")] static extern IntPtr udev_enumerate_unref(IntPtr udev_enumerate);

        [DllImport("libc.so.6")] static extern int open(IntPtr file, int oflag, int _unused);
        [DllImport("libc.so.6")] static extern int close(int fd);
        [DllImport("libc.so.6", SetLastError=true)] static extern int ioctl(int fd, nuint request, ref uint arg);

        const int O_RDWR = 0x2, O_NONBLOCK = 0x800, EINTR = 4, EAGAIN = 11, ETIMEDOUT = 110;
        const uint VIDIOC_QUERYCAP = 0x80685600, V4L2_CAP_VIDEO_CAPTURE = 0x1, V4L2_CAP_DEVICE_CAPS = 0x80000000;

        var devices = new List<string>();
        try
        {
            IntPtr udev = udev_new();
            IntPtr enumerate = udev_enumerate_new(udev);
            try
            {
                udev_enumerate_add_match_subsystem(enumerate, "video4linux");
                udev_enumerate_scan_devices(enumerate);
                Span<uint> capsStruct = stackalloc uint[26];
                for (IntPtr iter = udev_enumerate_get_list_entry(enumerate); iter != IntPtr.Zero; iter = udev_list_entry_get_next(iter))
                {
                    IntPtr v4l2_device = udev_device_get_devnode(udev_device_new_from_syspath(udev, udev_list_entry_get_name(iter)));
                    int fd = open(v4l2_device, O_RDWR | O_NONBLOCK, 0);
                    if (fd < 0)
                        continue;
                    try
                    {
                        int result, tries = 0;
                        do
                        {
                            result = ioctl(fd, VIDIOC_QUERYCAP, ref MemoryMarshal.GetReference(capsStruct));
                        } while (result != 0 && (Marshal.GetLastPInvokeError() is EINTR or EAGAIN or ETIMEDOUT) && ++tries < 4);
                        if (result < 0)
                            continue;
                    }
                    finally
                    {
                        close(fd);
                    }
                    uint caps = (capsStruct[21] & V4L2_CAP_DEVICE_CAPS) != 0 ? capsStruct[22] : capsStruct[21];
                    if ((caps & V4L2_CAP_VIDEO_CAPTURE) != 0)
                        devices.Add(Marshal.PtrToStringUTF8(v4l2_device));
                }
            }
            finally
            {
                if (enumerate != IntPtr.Zero)
                    udev_enumerate_unref(enumerate);
                udev_unref(udev);
            }
        }
        catch (Exception e)
        {
            devices.Add($"Error listing UVC devices: {e.Message}");
        }

        return devices;
    }

    // List serial ports available on the system
    private static string[] ListSerialPorts()
    {
        if (OperatingSystem.IsWindows())
            return SerialPort.GetPortNames();
        if (OperatingSystem.IsLinux())
            return Directory.GetFiles("/dev/serial/by-id");
        if (OperatingSystem.IsMacOS())
            return Directory.GetFiles("/dev", "tty*");
        return new string[0];
    }
}