using System;

namespace Cycling.Bluetooth
{
    public struct BleDevice
    {
        public string Id;
        public string Name;
    }

    public interface IBleTransport
    {
        event Action<BleDevice> OnDeviceDiscovered;
        event Action<string> OnConnected;
        event Action<string> OnDisconnected;
        event Action<string, string, byte[]> OnNotification; // deviceId, characteristicUuid, data

        void StartScan(string[] serviceUuids);
        void StopScan();
        void Connect(string deviceId);
        void Disconnect(string deviceId);
        void Subscribe(string deviceId, string serviceUuid, string characteristicUuid);
        void Write(string deviceId, string serviceUuid, string characteristicUuid, byte[] data);
        void Dispose();
    }
}
