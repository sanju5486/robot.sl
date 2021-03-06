﻿using Microsoft.IoT.Lightning.Providers;
using robot.sl.Audio;
using robot.sl.Audio.AudioPlaying;
using robot.sl.CarControl;
using robot.sl.Devices;
using robot.sl.Helper;
using robot.sl.Sensors;
using robot.sl.Web;
using System;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace robot.sl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AccelerometerGyroscopeSensor _accelerometerSensor;
        private AutomaticSpeakController _automaticSpeakController;
        private MotorController _motorController;
        private ServoController _servoController;
        private DistanceMeasurementSensor _distanceMeasurementSensor;
        private AutomaticDrive _automaticDrive;
        private Camera _camera;
        private HttpServerController _httpServerController;
        private GamepadController _gamepadController;
        private SpeechRecognition _speechRecognation;

        private const int AUDIO_RENDER_VOLUME = 100;
        private const double AUDIO_CAPTURE_VOLUME = 96.14;
        private const int I2C_ADDRESS_SERVO = 56;

        public MainPage()
        {
            InitializeComponent();
            
            Loaded += PageLoaded;
        }
        
        private async void PageLoaded(object sender, RoutedEventArgs eventArgs)
        {
            await Initialze();
        }

        private async Task Initialze()
        {
            if (LightningProvider.IsLightningEnabled)
            {
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
            }
            else
            {
                throw new Exception("Lightning drivers not enabled. Please enable Lightning drivers.");
            }

            _camera = new Camera();
            await _camera.Initialize();
            
            SpeedSensor.Initialize();
            SpeedSensor.Start();

            SpeechSynthesis.Initialze();

            await AudioPlayerController.Initialize();

            _accelerometerSensor = new AccelerometerGyroscopeSensor();
            await _accelerometerSensor.Initialize();
            _accelerometerSensor.Start();

            _automaticSpeakController = new AutomaticSpeakController(_accelerometerSensor);

            _motorController = new MotorController();
            await _motorController.Initialize(_automaticSpeakController);

            _servoController = new ServoController();
            await _servoController.Initialize();

            _distanceMeasurementSensor = new DistanceMeasurementSensor();
            await _distanceMeasurementSensor.Initialize(I2C_ADDRESS_SERVO);

            _automaticDrive = new AutomaticDrive(_motorController, _servoController, _distanceMeasurementSensor);

            _speechRecognation = new SpeechRecognition();
            await _speechRecognation.Initialze(_motorController, _servoController, _automaticDrive);
            _speechRecognation.Start();

            _gamepadController = new GamepadController(_motorController, _servoController, _automaticDrive, _accelerometerSensor);

            _camera.Start();

            _httpServerController = new HttpServerController(_motorController, _servoController, _automaticDrive, _camera);

            SystemController.Initialize(_accelerometerSensor, _automaticSpeakController, _motorController, _servoController, _automaticDrive, _camera, _httpServerController, _speechRecognation, _gamepadController);

            await SystemController.SetAudioRenderVolume(AUDIO_RENDER_VOLUME, true);
            await SystemController.SetAudioCaptureVolume(AUDIO_CAPTURE_VOLUME, true);
            
            await AudioPlayerController.PlayAndWaitAsync(AudioName.Welcome);

            _automaticSpeakController.Start();
        }
    }
}
