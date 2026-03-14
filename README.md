# Cycling Game

Single-player cycling racing game where you ride against AI opponents on custom-built circuit tracks. Inspired by Zwift and Rouvy but with authored 3D maps. Connects to smart trainers via Bluetooth FTMS — your legs are the controller.

## Requirements

- **Unity 6** (6000.3.11f1 or later)
- **Universal Render Pipeline** (included in project)
- macOS or Windows

## Setup

1. **Clone the repo**
   ```
   git clone git@github.com:ayceallchorn/CyclingGame.git
   ```

2. **Open in Unity Hub** — select Unity 6000.3.11f1 or later

3. **Import Easy Bike System** (free, required for rider/bike models)
   - Open Package Manager (Window > Package Manager)
   - Switch to "My Assets"
   - Search for "Easy Bike System" by RayznGames
   - Download and Import
   - Asset Store link: https://assetstore.unity.com/packages/templates/systems/easy-bike-system-316532

4. **Download HDR sky textures** (excluded from git due to size)
   - Download the following from [Poly Haven](https://polyhaven.com/) or your preferred source:
     - `HdrSkyMorning004_HDR_8K.exr` — morning sky HDRI
     - `HdrOutdoorFieldDayOvercast004_HDR_8K.exr` — overcast sky HDRI (optional)
   - Place them in `Assets/Textures/Sky/`

5. **Open `Assets/Scenes/MainMenu.unity`** and hit Play

## Controls

| Key | Action |
|-----|--------|
| E | Shift gear up |
| Q | Shift gear down |
| C | Toggle camera (Close / Wide / Overhead) |
| ` (backtick) | Toggle debug panel |
| Escape | Pause |

## Debug Panel (F1)

- **Power slider** — simulates trainer wattage (0-500W)
- **Cadence slider** — simulates pedalling RPM (0-150)
- **HR slider** — simulates heart rate (60-200 bpm)
- **Difficulty slider** — scales AI strength (0-100%)

## Architecture

```
Assets/Scripts/
├── AI/             # AI rider brain, state machine, strategy data
├── Audio/          # Procedural wind + gear click sounds
├── Bluetooth/      # BLE transport layer + FTMS/HR parsers
├── Camera/         # Third-person follow camera with presets
├── Core/           # Constants, EventBus, GameManager
├── Cycling/        # Physics, RiderMotor, GearSystem, DraftingSystem
├── Data/           # ScriptableObject definitions (teams, riders, bikes, tracks)
├── Editor/         # Editor setup scripts (track, UI, assets)
├── Input/          # New Input System manager
├── Race/           # RaceManager, PositionTracker, LapTracker, RiderIdentity
├── Track/          # Spline-based track, elevation sampling
└── UI/             # HUD, DebugPanel, MainMenu, RaceResults, LapProgressBar
```

## Trainer Connectivity (Sprint 7)

The Bluetooth FTMS layer is built but requires native plugins to connect to real hardware:

- **macOS**: Import [UnityCoreBluetooth](https://github.com/fuziki/UnityCoreBluetooth) to `Assets/Plugins/macOS/`
- **Windows**: Import [BleWinrtDll](https://github.com/adabru/BleWinrtDll) to `Assets/Plugins/Windows/`

Then set `useDebugTransport = false` on the TrainerManager component in RaceScene.

For testing without hardware, use [zwack](https://github.com/paixaop/zwack) to simulate a BLE FTMS trainer.

## Game Flow

Main Menu → Configure race (track, laps, AI count, difficulty) → Start Race → 3-2-1-Go countdown → Race with 20 AI riders → Results screen → Back to Menu

## License

Third-party assets (Easy Bike System) are subject to their own license terms from the Unity Asset Store. Project code is private.
