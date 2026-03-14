using UnityEngine;
using Cycling.Core;

namespace Cycling.Cycling
{
    public static class CyclingPhysics
    {
        /// <summary>
        /// Calculates net acceleration given current state.
        /// </summary>
        /// <param name="powerWatts">Rider power output in watts</param>
        /// <param name="speedMs">Current speed in m/s</param>
        /// <param name="gradient">Track gradient as a fraction (e.g. 0.05 = 5%)</param>
        /// <param name="massKg">Total mass of rider + bike in kg</param>
        /// <param name="cdA">Drag coefficient * frontal area in m²</param>
        /// <param name="crr">Rolling resistance coefficient</param>
        /// <returns>Acceleration in m/s²</returns>
        public static float CalculateAcceleration(
            float powerWatts,
            float speedMs,
            float gradient,
            float massKg = Constants.DefaultRiderMass,
            float cdA = Constants.DefaultCdA,
            float crr = Constants.DefaultCrr)
        {
            // Clamp speed to avoid division by zero
            float v = Mathf.Max(speedMs, Constants.MinSpeed);

            // Driving force from power
            float drivingForce = powerWatts / v;

            // Resistance forces
            float gravityForce = massKg * Constants.Gravity * gradient;
            float rollingForce = crr * massKg * Constants.Gravity;
            float aeroForce = 0.5f * Constants.AirDensity * cdA * v * v;

            float netForce = drivingForce - gravityForce - rollingForce - aeroForce;

            return netForce / massKg;
        }
    }
}
