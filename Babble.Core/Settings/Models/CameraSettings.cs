using System.Text.Json.Serialization;

namespace Babble.Core.Settings.Models;

public sealed class CameraSettings
{
    [JsonPropertyName("rotation_angle")]
    public int RotationAngle { get; set; }

    [JsonPropertyName("roi_window_x")]
    public int RoiWindowX { get; set; }

    [JsonPropertyName("roi_window_y")]
    public int RoiWindowY { get; set; }

    [JsonPropertyName("roi_window_w")]
    public int RoiWindowW { get; set; }

    [JsonPropertyName("roi_window_h")]
    public int RoiWindowH { get; set; }

    [JsonPropertyName("capture_source")]
    public string CaptureSource { get; set; }

    [JsonPropertyName("gui_vertical_flip")]
    public bool GuiVerticalFlip { get; set; }

    [JsonPropertyName("gui_horizontal_flip")]
    public bool GuiHorizontalFlip { get; set; }

    [JsonPropertyName("use_ffmpeg")]
    public bool UseFfmpeg { get; set; }

    public CameraSettings()
    {
        // Default to no rotation
        RotationAngle = 0;

        // Default ROI window to 256x256 (No crop)
        RoiWindowX = 0;
        RoiWindowY = 0;
        RoiWindowW = 256;
        RoiWindowH = 256;

        // Default to first available camera
        CaptureSource = "0";

        // Default to normal orientation
        GuiVerticalFlip = false;
        GuiHorizontalFlip = false;

        // Default to not using FFmpeg
        UseFfmpeg = false;
    }
}