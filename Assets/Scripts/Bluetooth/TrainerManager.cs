using UnityEngine;
using Cycling.Cycling;
using Cycling.UI;

namespace Cycling.Bluetooth
{
    public class TrainerManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] RiderMotor playerMotor;

        [Header("Settings")]
        [SerializeField] bool useDebugTransport = true;

        [Header("State (Read Only)")]
        [SerializeField] bool isConnected;
        [SerializeField] float lastPower;
        [SerializeField] float lastCadence;
        [SerializeField] float lastSpeed;
        [SerializeField] float lastHR;

        IBleTransport _transport;
        string _connectedDeviceId;
        TrainerData _latestData;
        float _lastGradientSent;
        float _gradientSendInterval = 0.5f;
        float _gradientSendTimer;

        public TrainerData LatestData => _latestData;
        public bool IsConnected => isConnected;

        public static TrainerManager Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            CreateTransport();
        }

        void CreateTransport()
        {
            if (useDebugTransport)
            {
                _transport = new BleTransportDebug();
                return;
            }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            _transport = new BleTransportMac();
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            _transport = new BleTransportWindows();
#else
            Debug.LogWarning("[TrainerManager] No BLE transport for this platform, using debug.");
            _transport = new BleTransportDebug();
#endif
        }

        void OnEnable()
        {
            if (_transport == null) return;
            _transport.OnDeviceDiscovered += OnDeviceDiscovered;
            _transport.OnConnected += OnConnected;
            _transport.OnDisconnected += OnDisconnected;
            _transport.OnNotification += OnNotification;
        }

        void OnDisable()
        {
            if (_transport == null) return;
            _transport.OnDeviceDiscovered -= OnDeviceDiscovered;
            _transport.OnConnected -= OnConnected;
            _transport.OnDisconnected -= OnDisconnected;
            _transport.OnNotification -= OnNotification;
        }

        void OnDestroy()
        {
            _transport?.Dispose();
        }

        /// <summary>
        /// Start scanning for FTMS trainers and HR monitors.
        /// </summary>
        public void StartScan()
        {
            _transport?.StartScan(new[]
            {
                FtmsParser.FtmsServiceUuid,
                HrParser.HrServiceUuid
            });
        }

        public void StopScan()
        {
            _transport?.StopScan();
        }

        public void ConnectToDevice(string deviceId)
        {
            _transport?.Connect(deviceId);
        }

        public void DisconnectDevice()
        {
            if (_connectedDeviceId != null)
                _transport?.Disconnect(_connectedDeviceId);
        }

        void Update()
        {
            // When using debug transport, read from DebugPanel sliders
            if (useDebugTransport)
            {
                var debug = DebugPanel.Instance;
                if (debug != null && playerMotor != null)
                {
                    _latestData.PowerWatts = playerMotor.PowerWatts;
                    _latestData.CadenceRpm = debug.SimulatedCadence;
                    _latestData.HeartRateBpm = debug.SimulatedHR;
                    _latestData.SpeedKmh = playerMotor.SpeedKmh;
                    _latestData.IsConnected = true;
                }
                return;
            }

            // Send gradient to trainer periodically
            if (isConnected && playerMotor != null)
            {
                _gradientSendTimer += Time.deltaTime;
                if (_gradientSendTimer >= _gradientSendInterval)
                {
                    _gradientSendTimer = 0f;
                    float gradient = playerMotor.Gradient * 100f; // convert to percentage
                    if (Mathf.Abs(gradient - _lastGradientSent) > 0.1f)
                    {
                        SendSimulationParams(gradient);
                        _lastGradientSent = gradient;
                    }
                }

                // Apply trainer power to motor
                playerMotor.PowerWatts = _latestData.PowerWatts;
            }

            // Update inspector display
            lastPower = _latestData.PowerWatts;
            lastCadence = _latestData.CadenceRpm;
            lastSpeed = _latestData.SpeedKmh;
            lastHR = _latestData.HeartRateBpm;
        }

        void SendSimulationParams(float gradientPercent)
        {
            if (_transport == null || _connectedDeviceId == null) return;
            byte[] data = FtmsParser.EncodeSimulationParams(gradientPercent);
            _transport.Write(_connectedDeviceId, FtmsParser.FtmsServiceUuid,
                FtmsParser.ControlPointUuid, data);
        }

        void OnDeviceDiscovered(BleDevice device)
        {
            Debug.Log($"[TrainerManager] Discovered: {device.Name} ({device.Id})");
            // Auto-connect to first FTMS device found (can be made smarter later)
            ConnectToDevice(device.Id);
            StopScan();
        }

        void OnConnected(string deviceId)
        {
            Debug.Log($"[TrainerManager] Connected to {deviceId}");
            _connectedDeviceId = deviceId;
            isConnected = true;

            // Subscribe to Indoor Bike Data
            _transport.Subscribe(deviceId, FtmsParser.FtmsServiceUuid,
                FtmsParser.IndoorBikeDataUuid);

            // Subscribe to HR if available
            _transport.Subscribe(deviceId, HrParser.HrServiceUuid,
                HrParser.HrMeasurementUuid);
        }

        void OnDisconnected(string deviceId)
        {
            Debug.Log($"[TrainerManager] Disconnected from {deviceId}");
            isConnected = false;
            _connectedDeviceId = null;
            _latestData = default;
        }

        void OnNotification(string deviceId, string characteristicUuid, byte[] data)
        {
            if (characteristicUuid == FtmsParser.IndoorBikeDataUuid)
            {
                var parsed = FtmsParser.DecodeIndoorBikeData(data);
                _latestData.PowerWatts = parsed.PowerWatts;
                _latestData.CadenceRpm = parsed.CadenceRpm;
                _latestData.SpeedKmh = parsed.SpeedKmh;
                _latestData.IsConnected = true;
            }
            else if (characteristicUuid == HrParser.HrMeasurementUuid)
            {
                _latestData.HeartRateBpm = HrParser.Decode(data);
            }
        }
    }
}
