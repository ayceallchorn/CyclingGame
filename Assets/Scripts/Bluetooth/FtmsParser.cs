using System;
using UnityEngine;

namespace Cycling.Bluetooth
{
    /// <summary>
    /// Parses FTMS Indoor Bike Data (0x2AD2) and encodes Simulation Parameters (0x2AD9).
    /// </summary>
    public static class FtmsParser
    {
        public const string FtmsServiceUuid = "00001826-0000-1000-8000-00805f9b34fb";
        public const string IndoorBikeDataUuid = "00002ad2-0000-1000-8000-00805f9b34fb";
        public const string ControlPointUuid = "00002ad9-0000-1000-8000-00805f9b34fb";

        /// <summary>
        /// Decode Indoor Bike Data characteristic notification.
        /// FTMS spec: flags (uint16) + conditional fields.
        /// </summary>
        public static TrainerData DecodeIndoorBikeData(byte[] data)
        {
            var result = new TrainerData();
            if (data == null || data.Length < 2) return result;

            int offset = 0;
            ushort flags = ReadUInt16(data, ref offset);

            // Bit 0: More Data — if 0, Instantaneous Speed is present (uint16, 0.01 km/h)
            if ((flags & 0x01) == 0)
            {
                if (offset + 2 <= data.Length)
                    result.SpeedKmh = ReadUInt16(data, ref offset) * 0.01f;
            }

            // Bit 1: Average Speed present (uint16, 0.01 km/h) — skip
            if ((flags & 0x02) != 0)
                offset += 2;

            // Bit 2: Instantaneous Cadence present (uint16, 0.5 rpm)
            if ((flags & 0x04) != 0)
            {
                if (offset + 2 <= data.Length)
                    result.CadenceRpm = ReadUInt16(data, ref offset) * 0.5f;
            }

            // Bit 3: Average Cadence present (uint16) — skip
            if ((flags & 0x08) != 0)
                offset += 2;

            // Bit 4: Total Distance present (uint24) — skip
            if ((flags & 0x10) != 0)
                offset += 3;

            // Bit 5: Resistance Level present (int16) — skip
            if ((flags & 0x20) != 0)
                offset += 2;

            // Bit 6: Instantaneous Power present (int16, watts)
            if ((flags & 0x40) != 0)
            {
                if (offset + 2 <= data.Length)
                    result.PowerWatts = ReadInt16(data, ref offset);
            }

            // Bit 7: Average Power present (int16) — skip
            if ((flags & 0x80) != 0)
                offset += 2;

            // Remaining fields (expended energy, HR, metabolic equivalent, elapsed time) — skip

            return result;
        }

        /// <summary>
        /// Encode Set Indoor Bike Simulation Parameters command for the Control Point.
        /// Op code 0x11: Wind Speed (int16, 0.001 m/s), Grade (int16, 0.01 %),
        /// Crr (uint8, 0.0001), Cw (uint8, 0.01 kg/m).
        /// </summary>
        public static byte[] EncodeSimulationParams(float gradientPercent, float crr = 0.004f, float cdA = 0.32f)
        {
            var data = new byte[7];
            data[0] = 0x11; // Op code: Set Indoor Bike Simulation Parameters

            // Wind speed: 0 m/s (int16, resolution 0.001)
            short windSpeed = 0;
            data[1] = (byte)(windSpeed & 0xFF);
            data[2] = (byte)((windSpeed >> 8) & 0xFF);

            // Grade: gradient in 0.01% units (int16)
            short grade = (short)Mathf.Clamp(Mathf.RoundToInt(gradientPercent * 100f), -32768, 32767);
            data[3] = (byte)(grade & 0xFF);
            data[4] = (byte)((grade >> 8) & 0xFF);

            // Crr: rolling resistance coefficient in 0.0001 units (uint8)
            data[5] = (byte)Mathf.Clamp(Mathf.RoundToInt(crr * 10000f), 0, 255);

            // Cw: wind resistance coefficient (CdA) in 0.01 kg/m units (uint8)
            // CdA is in m², air density ~1.225 kg/m³, so Cw = 0.5 * ρ * CdA
            float cw = 0.5f * 1.225f * cdA;
            data[6] = (byte)Mathf.Clamp(Mathf.RoundToInt(cw * 100f), 0, 255);

            return data;
        }

        static ushort ReadUInt16(byte[] data, ref int offset)
        {
            ushort val = (ushort)(data[offset] | (data[offset + 1] << 8));
            offset += 2;
            return val;
        }

        static short ReadInt16(byte[] data, ref int offset)
        {
            short val = (short)(data[offset] | (data[offset + 1] << 8));
            offset += 2;
            return val;
        }
    }
}
