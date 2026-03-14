using System;

namespace Cycling.Core
{
    public static class EventBus
    {
        // Race lifecycle
        public static event Action OnRaceSetup;
        public static event Action OnRaceCountdown;
        public static event Action OnRaceStart;
        public static event Action OnRaceFinished;

        // Rider events
        public static event Action<int> OnLapCompleted;       // riderInstanceID
        public static event Action<int[]> OnPositionsUpdated; // ordered riderInstanceIDs

        public static void RaceSetup() => OnRaceSetup?.Invoke();
        public static void RaceCountdown() => OnRaceCountdown?.Invoke();
        public static void RaceStart() => OnRaceStart?.Invoke();
        public static void RaceFinished() => OnRaceFinished?.Invoke();
        public static void LapCompleted(int riderId) => OnLapCompleted?.Invoke(riderId);
        public static void PositionsUpdated(int[] ids) => OnPositionsUpdated?.Invoke(ids);
    }
}
