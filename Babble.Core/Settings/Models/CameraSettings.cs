using Newtonsoft.Json;

namespace Babble.Core.Settings.Models;

public sealed class CameraSettings
{
    [JsonProperty("rotation_angle")]
    public double RotationAngle { get; set; }

    [JsonProperty("roi_window_x")]
    public int RoiWindowX { get; set; }

    [JsonProperty("roi_window_y")]
    public int RoiWindowY { get; set; }

    [JsonProperty("roi_window_w")]
    public int RoiWindowW { get; set; }

    [JsonProperty("roi_window_h")]
    public int RoiWindowH { get; set; }

    [JsonProperty("capture_source")]
    public string CaptureSource { get; set; }

    [JsonProperty("gui_vertical_flip")]
    public bool GuiVerticalFlip { get; set; }

    [JsonProperty("gui_horizontal_flip")]
    public bool GuiHorizontalFlip { get; set; }

    [JsonProperty("use_ffmpeg")]
    public bool UseFfmpeg { get; set; }

    public CameraSettings()
    {
        // Default to no rotation
        RotationAngle = 0;

        // Default ROI window to 256x256 (with no crop)
        RoiWindowX = 0;
        RoiWindowY = 0;
        RoiWindowW = 0;
        RoiWindowH = 0;

        // Default to first available camera
        CaptureSource = "0";

        // Default to normal orientation
        GuiVerticalFlip = false;
        GuiHorizontalFlip = false;

        // Default to not using FFmpeg
        UseFfmpeg = false;
    }
}