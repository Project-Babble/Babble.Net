using System.Text.Json.Serialization;

namespace Babble.Core.Settings.Models;

public sealed class GeneralSettings
{
    [JsonPropertyName("gui_min_cutoff")]
    public string GuiMinCutoff { get; set; }

    [JsonPropertyName("gui_speed_coefficient")]
    public string GuiSpeedCoefficient { get; set; }

    [JsonPropertyName("gui_osc_address")]
    public string GuiOscAddress { get; set; }

    [JsonPropertyName("gui_osc_port")]
    public int GuiOscPort { get; set; }

    [JsonPropertyName("gui_osc_receiver_port")]
    public int GuiOscReceiverPort { get; set; }

    [JsonPropertyName("gui_osc_recalibrate_address")]
    public string GuiOscRecalibrateAddress { get; set; }

    [JsonPropertyName("gui_update_check")]
    public bool GuiUpdateCheck { get; set; }

    [JsonPropertyName("gui_ROSC")]
    public bool GuiROSC { get; set; }

    [JsonPropertyName("gui_osc_location")]
    public string GuiOscLocation { get; set; }

    [JsonPropertyName("gui_multiply")]
    public double GuiMultiply { get; set; }

    [JsonPropertyName("gui_model_file")]
    public string GuiModelFile { get; set; }

    [JsonPropertyName("gui_runtime")]
    public string GuiRuntime { get; set; }

    [JsonPropertyName("gui_use_gpu")]
    public bool GuiUseGpu { get; set; }

    [JsonPropertyName("gui_gpu_index")]
    public int GuiGpuIndex { get; set; }

    [JsonPropertyName("gui_inference_threads")]
    public int GuiInferenceThreads { get; set; }

    [JsonPropertyName("gui_use_red_channel")]
    public bool GuiUseRedChannel { get; set; }

    [JsonPropertyName("calib_deadzone")]
    public double CalibDeadzone { get; set; }

    [JsonPropertyName("calib_array")]
    public string CalibArray { get; set; }

    [JsonPropertyName("gui_cam_resolution_x")]
    public int GuiCamResolutionX { get; set; }

    [JsonPropertyName("gui_cam_resolution_y")]
    public int GuiCamResolutionY { get; set; }

    [JsonPropertyName("gui_cam_framerate")]
    public int GuiCamFramerate { get; set; }

    [JsonPropertyName("use_calibration")]
    public bool UseCalibration { get; set; }

    [JsonPropertyName("calibration_mode")]
    public string CalibrationMode { get; set; }

    [JsonPropertyName("gui_language")]
    public string GuiLanguage { get; set; }

    public GeneralSettings()
    {
        // Filter settings
        GuiMinCutoff = "1.0";
        GuiSpeedCoefficient = "1.0";

        // OSC communication settings
        GuiOscAddress = "127.0.0.1";
        GuiOscPort = 9000;
        GuiOscReceiverPort = 9001;
        GuiOscRecalibrateAddress = "/avatar/parameters/babble_recalibrate";
        GuiOscLocation = "";
        GuiROSC = false;

        // Update and GUI settings
        GuiUpdateCheck = true;
        GuiMultiply = 1.0;

        // Model and runtime settings
        GuiModelFile = "model.onnx";
        GuiRuntime = "ONNX";
        GuiUseGpu = false;
        GuiGpuIndex = 0;
        GuiInferenceThreads = 2;
        GuiUseRedChannel = false;

        // Calibration settings
        CalibDeadzone = -0.1;
        CalibArray = "[[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1]]";
        UseCalibration = false;
        CalibrationMode = "Neutral";

        // Camera settings
        GuiCamResolutionX = 0;
        GuiCamResolutionY = 0;
        GuiCamFramerate = 0;

        // Language
        GuiLanguage = "English";
    }
}