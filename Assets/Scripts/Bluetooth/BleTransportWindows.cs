using System;
using UnityEngine;

namespace Cycling.Bluetooth
{
    /// <summary>
    /// Windows BLE transport wrapping BleWinrtDll.
    /// Requires the BleWinrtDll plugin in Assets/Plugins/Windows/.
    /// See: https://github.com/adabru/BleWinrtDll
    /// </summary>
    public class BleTransportWindows : IBleTransport
    {
        public event Action<BleDevice> OnDeviceDiscovered;
        public event Action<string> OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string, string, byte[]> OnNotification;

        public void StartScan(string[] serviceUuids)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Debug.Log("[BLE Win] Starting scan...");
            // BleApi.StartDeviceScan();
            // Poll BleApi.PollDevice() in Update for discovered devices
            Debug.LogWarning("[BLE Win] BleWinrtDll plugin not yet imported. Using debug transport.");
#endif
        }

        public void StopScan()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // BleApi.StopDeviceScan();
#endif
        }

        public void Connect(string deviceId)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Debug.Log($"[BLE Win] Connecting to {deviceId}...");
            // Connection is implicit in BleWinrtDll — subscribing connects automatically
            OnConnected?.Invoke(deviceId);
#endif
        }

        public void Disconnect(string deviceId)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // BleApi.Quit();
            OnDisconnected?.Invoke(deviceId);
#endif
        }

        public void Subscribe(string deviceId, string serviceUuid, string characteristicUuid)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Debug.Log($"[BLE Win] Subscribing to {characteristicUuid}");
            // BleApi.SubscribeCharacteristic(deviceId, serviceUuid, characteristicUuid, false);
            // Poll BleApi.PollData() in Update for notifications
#endif
        }

        public void Write(string deviceId, string serviceUuid, string characteristicUuid, byte[] data)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // BleApi.SendData(deviceId, serviceUuid, characteristicUuid, data, false);
#endif
        }

        public void Dispose()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // BleApi.Quit();
#endif
        }
    }
}
