# Cycling — Sprint Tasks

## Sprint 1: Capsule on a Spline
> Get a single capsule moving around a closed spline track using real cycling physics. Third-person camera follows it.

- [x] Add `com.unity.splines` package to manifest.json
- [x] Create `Assets/Scripts/Core/Constants.cs` — physical constants (gravity, air density, Crr, CdA defaults)
- [x] Create `Assets/Scripts/Cycling/CyclingPhysics.cs` — static class: power-to-acceleration equation
- [x] Create `Assets/Scripts/Track/TrackSpline.cs` — MonoBehaviour wrapping SplineContainer (position/rotation/gradient at distance)
- [x] Create `Assets/Scripts/Cycling/RiderMotor.cs` — MonoBehaviour: advances along spline each FixedUpdate based on power input
- [x] Create `Assets/Scripts/Camera/RiderCamera.cs` — third-person follow cam with smooth damping
- [x] Create `Assets/Scenes/RaceScene.unity` — closed spline loop, capsule with RiderMotor, camera
- [x] **Test:** Play mode — capsule loops at ~30 km/h with hardcoded 200W. Camera follows smoothly.

---

## Sprint 2: Debug Input + Gears + HUD
> Control power with a debug slider, shift virtual gears, see live stats on a HUD.

- [x] Create `CyclingActions.inputactions` — ShiftUp, ShiftDown, CameraToggle, Pause, ToggleDebug
- [x] Create `Assets/Scripts/Input/InputManager.cs` — reads input actions, fires events
- [x] Create `Assets/Scripts/Data/GearTableData.cs` — ScriptableObject: chainring + cassette teeth
- [x] Create `Assets/Data/GearTable_Default.asset` — default gear ratios
- [x] Create `Assets/Scripts/Cycling/GearSystem.cs` — gear index, shift up/down, ratio calculation
- [x] Create `Assets/Scripts/UI/DebugPanel.cs` — toggleable panel (F1) with power/cadence/HR sliders
- [x] Create `Assets/Scripts/UI/RaceHUD.cs` — UGUI canvas: watts, speed, cadence, HR, gear, gradient, position
- [x] Wire debug slider power → RiderMotor, gear shifts → GearSystem
- [x] **Test:** Slider changes speed realistically. E/Q shifts gears. HUD updates every frame.

---

## Sprint 3: Elevation & Gradient
> Track has hills. Speed drops on climbs, increases on descents. Gradient displays on HUD.

- [x] Create `Assets/Scripts/Track/ElevationSampler.cs` — computes gradient from spline Y samples
- [x] Modify RaceScene spline: raise control points to create a hill on one side
- [x] Wire gradient from TrackSpline → CyclingPhysics → RiderMotor
- [x] Display gradient % on RaceHUD
- [x] **Test:** Noticeable slowdown climbing, acceleration descending. Gradient reads correctly on HUD.

---

## Sprint 4: AI Riders + Drafting
> 20 AI capsules on the track. Drafting forms a natural peloton. Positions and laps tracked.

- [x] Create `Assets/Scripts/Core/EventBus.cs` — static C# events for decoupling
- [x] Create `Assets/Scripts/Data/TeamData.cs` — ScriptableObject: name, primary/secondary colour
- [x] Create `Assets/Scripts/Data/RiderData.cs` — ScriptableObject: name, team, FTP, weight
- [x] Create `Assets/Scripts/Data/BikeData.cs` — ScriptableObject: mass, CdA, Crr, gear table ref
- [x] Create `Assets/Data/Teams/` — ~18 colour-coded team assets
- [x] Create `Assets/Data/Riders/` — ~20 rider assets assigned to teams
- [x] Create `Assets/Scripts/Race/RiderIdentity.cs` — MonoBehaviour: name, team, isPlayer flag
- [x] Create `Assets/Scripts/Race/RaceManager.cs` — spawns riders, state machine (Setup→Countdown→Racing→Finished)
- [x] Create `Assets/Scripts/Race/PositionTracker.cs` — sorts riders by effective distance each frame
- [x] Create `Assets/Scripts/Race/LapTracker.cs` — detects spline wrap-around, counts laps per rider
- [x] Create `Assets/Scripts/Cycling/DraftingSystem.cs` — calculates draft factor per rider (~30% behind one, ~45% in group)
- [x] Create `Assets/Scripts/AI/AIStrategyData.cs` — ScriptableObject: aggressiveness, sprint, climbing
- [x] Create `Assets/Scripts/AI/AIRiderBrain.cs` — simple constant power with slight randomisation (full AI in Sprint 5)
- [x] Create `Assets/Prefabs/AIRider.prefab` — capsule + RiderMotor + RiderIdentity + AIRiderBrain (spawned dynamically by RaceManager)
- [ ] **Test:** 20 capsules bunch up naturally from drafting. Positions update on HUD. Laps count correctly.

---

## Sprint 5: Overtaking + AI Behaviour
> Riders visually overtake. AI has breakaways, sprints, fatigue. Difficulty slider works.

- [x] Add lateral offset logic to RiderMotor (pull out +1.5m to pass, return after clearing)
- [x] Implement AI state machine: Peloton → Chase → Breakaway → Sprint → Gruppetto
- [x] Implement simple fatigue model (above-threshold effort degrades power)
- [x] Wire difficulty slider → AI FTP scaling (0%=100W, 100%=380W)
- [x] Add difficulty slider to DebugPanel or race setup
- [ ] **Test:** Riders pull out to pass. AI occasionally attacks. Sprint in final stretch. Difficulty slider clearly changes the challenge.

---

## Sprint 6: Race Flow + Menus
> Full loop: main menu → race setup → countdown → race → results → back to menu.

- [x] Create `Assets/Scenes/MainMenu.unity`
- [x] Create `Assets/Scripts/Core/GameManager.cs` — scene transitions, persists race config
- [x] Create `Assets/Scripts/UI/MainMenuUI.cs` — race setup (track, laps 1-20, AI count 5-25, difficulty)
- [x] Create `Assets/Scripts/UI/RaceResultsUI.cs` — finishing order, time gaps
- [x] Create `Assets/Scripts/Data/TrackDefinition.cs` — ScriptableObject: name, prefab, length, thumbnail
- [x] Create `Assets/Data/Tracks/CircuitOne.asset`
- [x] Add countdown sequence to RaceManager (3-2-1-Go)
- [x] **Test:** Complete flow from menu through race to results and back. All settings apply correctly.

---

## Sprint 7: Bluetooth Trainer Connectivity
> Real Wahoo Kickr Core drives the player via Bluetooth FTMS.

- [x] Create `Assets/Scripts/Bluetooth/IBleTransport.cs` — interface: scan, connect, subscribe, write
- [x] Create `Assets/Scripts/Bluetooth/TrainerData.cs` — struct: power, cadence, speed, hr
- [x] Create `Assets/Scripts/Bluetooth/FtmsParser.cs` — decode Indoor Bike Data (0x2AD2), encode Simulation Params (0x2AD9)
- [x] Create `Assets/Scripts/Bluetooth/HrParser.cs` — decode HR characteristic (0x2A37)
- [x] Create `Assets/Scripts/Bluetooth/BleTransportDebug.cs` — implements IBleTransport with no-ops (debug sliders still work)
- [x] Create `Assets/Scripts/Bluetooth/BleTransportMac.cs` — wraps UnityCoreBluetooth native plugin
- [x] Create `Assets/Scripts/Bluetooth/BleTransportWindows.cs` — wraps BleWinrtDll native plugin
- [x] Create `Assets/Scripts/Bluetooth/TrainerManager.cs` — orchestrates BLE connection, exposes TrainerData
- [ ] Import UnityCoreBluetooth to `Assets/Plugins/macOS/` *(manual — download from GitHub)*
- [ ] Import BleWinrtDll to `Assets/Plugins/Windows/` *(manual — download from GitHub)*
- [x] Wire TrainerManager → RiderMotor (power from trainer replaces debug slider)
- [x] Wire gradient changes → trainer resistance via FTMS control point
- [ ] **Test:** Connect to Kickr Core. Pedal = capsule moves. Hill = trainer resistance increases. Power/cadence on HUD. *(requires native plugins)*

---

## Sprint 8: Art & Polish
> Replace placeholders with real visuals. Road surface, rider models, team kits, environment.

- [ ] Replace capsules with rider + bike mesh *(needs 3D assets — placeholder capsules for now)*
- [x] Team colour-coded materials on rider kits *(via RiderIdentity.Init)*
- [x] Road surface mesh extruded along spline (Spline Extrude component)
- [x] Basic environment: green ground, trees inside and outside track
- [x] Minimap or lap progress bar on HUD *(green fill bar at top of screen)*
- [x] Pedalling animation driven by cadence *(needs rider model first)*
- [x] Sound: wind (speed-based), gear clicks *(procedural audio — no assets needed)*
- [x] Camera toggle between 3 presets — Close/Wide/Overhead (C key)
- [x] **Test:** Visually recognisable race. Teams distinguishable.

## Ideas

- [ ] Add a start/finish line
- [ ] Flat road
- [ ] Real rider and team names
- [ ] Background (mountains or something)
- [ ] Leaderboard