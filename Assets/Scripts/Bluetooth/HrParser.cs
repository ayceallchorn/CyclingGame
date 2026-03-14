namespace Cycling.Bluetooth
{
    /// <summary>
    /// Parses Bluetooth Heart Rate Measurement characteristic (0x2A37).
    /// </summary>
    public static class HrParser
    {
        public const string HrServiceUuid = "0000180d-0000-1000-8000-00805f9b34fb";
        public const string HrMeasurementUuid = "00002a37-0000-1000-8000-00805f9b34fb";

        /// <summary>
        /// Decode HR Measurement. Returns heart rate in BPM, or 0 if invalid.
        /// </summary>
        public static float Decode(byte[] data)
        {
            if (data == null || data.Length < 2) return 0f;

            byte flags = data[0];
            bool is16Bit = (flags & 0x01) != 0;

            if (is16Bit && data.Length >= 3)
                return (ushort)(data[1] | (data[2] << 8));

            return data[1];
        }
    }
}
