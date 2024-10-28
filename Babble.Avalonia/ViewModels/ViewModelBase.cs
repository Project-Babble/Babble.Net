using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace Babble.Avalonia.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        private ObservableCollection<CalibrationItem> _calibrationItems;
        private string _selectedCalibrationMode;

        public ObservableCollection<CalibrationItem> CalibrationItems
        {
            get => _calibrationItems;
            set => this.RaiseAndSetIfChanged(ref _calibrationItems, value);
        }

        public string SelectedCalibrationMode
        {
            get => _selectedCalibrationMode;
            set => this.RaiseAndSetIfChanged(ref _selectedCalibrationMode, value);
        }

        public ReactiveCommand<Unit, Unit> ResetMinCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetMaxCommand { get; }

        public ViewModelBase()
        {
            _selectedCalibrationMode = "Neutral";
            InitializeCalibrationItems();

            ResetMinCommand = ReactiveCommand.Create(ExecuteResetMin);
            ResetMaxCommand = ReactiveCommand.Create(ExecuteResetMax);
        }

        private void InitializeCalibrationItems()
        {
            CalibrationItems = new ObservableCollection<CalibrationItem>
            {
                new CalibrationItem { ShapeName = "cheekPuffLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "cheekSuckLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "jawOpen", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "jawForward", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "jawLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "noseSneerLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "mouthFunnel", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "mouthPucker", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "mouthLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "mouthRollUpper", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "mouthRollLower", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "mouthShrugUpper", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "mouthShrugLower", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "mouthClose", LeftValue = "0.0", RightValue = "1.0" },
                new CalibrationItem { ShapeName = "mouthSmileLeft/Right", LeftValue = "0.0", RightValue = "1.0" }
            };
        }

        private void ExecuteResetMin()
        {
            foreach (var item in CalibrationItems)
            {
                item.LeftValue = "0.0";
            }
        }

        private void ExecuteResetMax()
        {
            foreach (var item in CalibrationItems)
            {
                item.RightValue = "1.0";
            }
        }
    }

    public class CalibrationItem : ReactiveObject
    {
        private string _shapeName;
        private string _leftValue;
        private string _rightValue;

        public string ShapeName
        {
            get => _shapeName;
            set => this.RaiseAndSetIfChanged(ref _shapeName, value);
        }

        public string LeftValue
        {
            get => _leftValue;
            set => this.RaiseAndSetIfChanged(ref _leftValue, value);
        }

        public string RightValue
        {
            get => _rightValue;
            set => this.RaiseAndSetIfChanged(ref _rightValue, value);
        }
    }

    public class AlgoSettingsViewModel : ReactiveObject
    {
        private string _modelFile;
        private int _inferenceThreads;
        private string _runtime;
        private int _gpuIndex;
        private bool _useGpu;
        private double _modelOutputMultiplier;
        private double _calibrationDeadzone;
        private double _minFrequencyCutoff;
        private double _speedCoefficient;

        public string ModelFile
        {
            get => _modelFile;
            set => this.RaiseAndSetIfChanged(ref _modelFile, value);
        }

        public int InferenceThreads
        {
            get => _inferenceThreads;
            set => this.RaiseAndSetIfChanged(ref _inferenceThreads, value);
        }

        public string Runtime
        {
            get => _runtime;
            set => this.RaiseAndSetIfChanged(ref _runtime, value);
        }

        public int GpuIndex
        {
            get => _gpuIndex;
            set => this.RaiseAndSetIfChanged(ref _gpuIndex, value);
        }

        public bool UseGpu
        {
            get => _useGpu;
            set => this.RaiseAndSetIfChanged(ref _useGpu, value);
        }

        public double ModelOutputMultiplier
        {
            get => _modelOutputMultiplier;
            set => this.RaiseAndSetIfChanged(ref _modelOutputMultiplier, value);
        }

        public double CalibrationDeadzone
        {
            get => _calibrationDeadzone;
            set => this.RaiseAndSetIfChanged(ref _calibrationDeadzone, value);
        }

        public double MinFrequencyCutoff
        {
            get => _minFrequencyCutoff;
            set => this.RaiseAndSetIfChanged(ref _minFrequencyCutoff, value);
        }

        public double SpeedCoefficient
        {
            get => _speedCoefficient;
            set => this.RaiseAndSetIfChanged(ref _speedCoefficient, value);
        }

        public AlgoSettingsViewModel()
        {
            ModelFile = "Models/3MEFFB0E7MSE/";
            InferenceThreads = 2;
            Runtime = "ONNX";
            GpuIndex = 0;
            UseGpu = true;
            ModelOutputMultiplier = 1.0;
            CalibrationDeadzone = -0.1;
            MinFrequencyCutoff = 0.9;
            SpeedCoefficient = 0.9;
        }
    }

    public class SettingsViewModel : ReactiveObject
    {
        private bool _checkForUpdates;
        private string _locationPrefix;
        private string _address;
        private int _port;
        private bool _receiveFunctions;
        private int _receiverPort;
        private string _recalibrateAddress;
        private bool _useRedChannel;
        private int _xResolution;
        private int _yResolution;
        private int _framerate;

        public bool CheckForUpdates
        {
            get => _checkForUpdates;
            set => this.RaiseAndSetIfChanged(ref _checkForUpdates, value);
        }

        public string LocationPrefix
        {
            get => _locationPrefix;
            set => this.RaiseAndSetIfChanged(ref _locationPrefix, value);
        }

        public string Address
        {
            get => _address;
            set => this.RaiseAndSetIfChanged(ref _address, value);
        }

        public int Port
        {
            get => _port;
            set => this.RaiseAndSetIfChanged(ref _port, value);
        }

        public bool ReceiveFunctions
        {
            get => _receiveFunctions;
            set => this.RaiseAndSetIfChanged(ref _receiveFunctions, value);
        }

        public int ReceiverPort
        {
            get => _receiverPort;
            set => this.RaiseAndSetIfChanged(ref _receiverPort, value);
        }

        public string RecalibrateAddress
        {
            get => _recalibrateAddress;
            set => this.RaiseAndSetIfChanged(ref _recalibrateAddress, value);
        }

        public bool UseRedChannel
        {
            get => _useRedChannel;
            set => this.RaiseAndSetIfChanged(ref _useRedChannel, value);
        }

        public int XResolution
        {
            get => _xResolution;
            set => this.RaiseAndSetIfChanged(ref _xResolution, value);
        }

        public int YResolution
        {
            get => _yResolution;
            set => this.RaiseAndSetIfChanged(ref _yResolution, value);
        }

        public int Framerate
        {
            get => _framerate;
            set => this.RaiseAndSetIfChanged(ref _framerate, value);
        }

        public SettingsViewModel()
        {
            CheckForUpdates = true;
            Address = "127.0.0.1";
            Port = 8888;
            ReceiverPort = 9001;
            RecalibrateAddress = "/avatar/parameters/babble_recalibrate";
            XResolution = 0;
            YResolution = 0;
            Framerate = 0;
        }
    }
}