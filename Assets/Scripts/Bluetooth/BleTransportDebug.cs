using System;
using UnityEngine;

namespace Cycling.Bluetooth
{
    /// <summary>
    /// Debug BLE transport — no actual Bluetooth. 
    /// Uses DebugPanel sliders for trainer input simulation.
    /// </summary>
    public class BleTransportDebug : IBleTransport
    {
        public event Action<BleDevice> OnDeviceDiscovered;
        public event Action<string> OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string, string, byte[]> OnNotification;

        public void StartScan(string[] serviceUuids)
        {
            Debug.Log("[BLE Debug] Scan started (no-op)");
        }

        public void StopScan()
        {
            Debug.Log("[BLE Debug] Scan stopped (no-op)");
        }

        public void Connect(string deviceId)
        {
            Debug.Log("[BLE Debug] Connect (no-op)");
            OnConnected?.Invoke(deviceId);
        }

        public void Disconnect(string deviceId)
        {
            Debug.Log("[BLE Debug] Disconnect (no-op)");
            OnDisconnected?.Invoke(deviceId);
        }

        public void Subscribe(string deviceId, string serviceUuid, string characteristicUuid)
        {
            Debug.Log($"[BLE Debug] Subscribe {characteristicUuid} (no-op)");
        }

        public void Write(string deviceId, string serviceUuid, string characteristicUuid, byte[] data)
        {
            // Log simulation params for debugging
            if (characteristicUuid == FtmsParser.ControlPointUuid && data != null && data.Length >= 5)
            {
                short grade = (short)(data[3] | (data[4] << 8));
                Debug.Log($"[BLE Debug] Sim params: grade={grade * 0.01f:F1}%");
            }
        }

        public void Dispose() { }
    }
}
