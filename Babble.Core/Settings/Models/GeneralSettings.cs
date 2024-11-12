﻿using Newtonsoft.Json;

namespace Babble.Core.Settings.Models;

public sealed class GeneralSettings
{
    [JsonProperty("gui_min_cutoff")]
    public string GuiMinCutoff { get; set; }

    [JsonProperty("gui_speed_coefficient")]
    public string GuiSpeedCoefficient { get; set; }

    [JsonProperty("gui_osc_address")]
    public string GuiOscAddress { get; set; }

    [JsonProperty("gui_osc_port")]
    public int GuiOscPort { get; set; }

    [JsonProperty("gui_osc_receiver_port")]
    public int GuiOscReceiverPort { get; set; }

    [JsonProperty("gui_osc_recalibrate_address")]
    public string GuiOscRecalibrateAddress { get; set; }

    [JsonProperty("gui_update_check")]
    public bool GuiUpdateCheck { get; set; }

    [JsonProperty("gui_ROSC")]
    public bool GuiROSC { get; set; }

    [JsonProperty("gui_osc_location")]
    public string GuiOscLocation { get; set; }

    [JsonProperty("gui_multiply")]
    public double GuiMultiply { get; set; }

    [JsonProperty("gui_model_file")]
    public string GuiModelFile { get; set; }

    [JsonProperty("gui_runtime")]
    public string GuiRuntime { get; set; }

    [JsonProperty("gui_use_gpu")]
    public bool GuiUseGpu { get; set; }

    [JsonProperty("gui_gpu_index")]
    public int GuiGpuIndex { get; set; }

    [JsonProperty("gui_inference_threads")]
    public int GuiInferenceThreads { get; set; }

    [JsonProperty("gui_use_red_channel")]
    public bool GuiUseRedChannel { get; set; }

    [JsonProperty("gui_force_relevancy")]
    public bool GuiForceRelevancy { get; set; }

    [JsonProperty("calib_deadzone")]
    public double CalibDeadzone { get; set; }

    [JsonProperty("calib_array")]
    public string CalibArray { get; set; }

    [JsonProperty("gui_cam_resolution_x")]
    public int GuiCamResolutionX { get; set; }

    [JsonProperty("gui_cam_resolution_y")]
    public int GuiCamResolutionY { get; set; }

    [JsonProperty("gui_cam_framerate")]
    public int GuiCamFramerate { get; set; }

    [JsonProperty("use_calibration")]
    public bool UseCalibration { get; set; }

    [JsonProperty("calibration_mode")]
    public string CalibrationMode { get; set; }

    [JsonProperty("gui_language")]
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