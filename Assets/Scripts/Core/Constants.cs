namespace Cycling.Core
{
    public static class Constants
    {
        public const float Gravity = 9.80665f;
        public const float AirDensity = 1.225f; // kg/m³ at sea level

        // Default rider + bike
        public const float DefaultRiderMass = 75f; // kg (rider + bike)
        public const float DefaultCdA = 0.32f;     // m² (drops position)
        public const float DefaultCrr = 0.004f;    // rolling resistance coefficient

        // Speed limits
        public const float MinSpeed = 0.5f;  // m/s — clamp to avoid division by zero
        public const float MaxSpeed = 30f;   // m/s (~108 km/h)
    }
}
