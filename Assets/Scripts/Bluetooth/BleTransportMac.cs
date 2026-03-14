using System;
using UnityEngine;

namespace Cycling.Bluetooth
{
    /// <summary>
    /// macOS BLE transport wrapping UnityCoreBluetooth.
    /// Requires the UnityCoreBluetooth plugin in Assets/Plugins/macOS/.
    /// See: https://github.com/fuziki/UnityCoreBluetooth
    /// </summary>
    public class BleTransportMac : IBleTransport
    {
        public event Action<BleDevice> OnDeviceDiscovered;
        public event Action<string> OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string, string, byte[]> OnNotification;

        // TODO: Initialize UnityCoreBluetooth when plugin is imported
        // private CoreBluetoothManager _manager;

        public void StartScan(string[] serviceUuids)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Debug.Log("[BLE Mac] Starting scan...");
            // _manager = new CoreBluetoothManager();
            // _manager.OnDiscoverPeripheral += (name, id) =>
            //     OnDeviceDiscovered?.Invoke(new BleDevice { Id = id, Name = name });
            // _manager.StartScan(serviceUuids);
            Debug.LogWarning("[BLE Mac] UnityCoreBluetooth plugin not yet imported. Using debug transport.");
#endif
        }

        public void StopScan()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // _manager?.StopScan();
#endif
        }

        public void Connect(string deviceId)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Debug.Log($"[BLE Mac] Connecting to {deviceId}...");
            // _manager?.Connect(deviceId);
            // On connect callback: OnConnected?.Invoke(deviceId);
#endif
        }

        public void Disconnect(string deviceId)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // _manager?.Disconnect(deviceId);
            OnDisconnected?.Invoke(deviceId);
#endif
        }

        public void Subscribe(string deviceId, string serviceUuid, string characteristicUuid)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Debug.Log($"[BLE Mac] Subscribing to {characteristicUuid}");
            // _manager?.Subscribe(deviceId, serviceUuid, characteristicUuid,
            //     data => OnNotification?.Invoke(deviceId, characteristicUuid, data));
#endif
        }

        public void Write(string deviceId, string serviceUuid, string characteristicUuid, byte[] data)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // _manager?.Write(deviceId, serviceUuid, characteristicUuid, data);
#endif
        }

        public void Dispose()
        {
            // _manager?.Dispose();
        }
    }
}
