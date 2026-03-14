# Cycling — Project Brief

## Overview

Single-player cycling racing game where you ride against AI opponents on custom-built circuit tracks. Inspired by Zwift and Rouvy but with authored 3D maps instead of real-world footage. Connects to smart trainers and heart rate monitors via Bluetooth. Your legs are the controller — match cadence and power to keep up, break away, or fall behind.

## Core Pillars

1. **Accessible competition** — AI difficulty scales from casual non-cyclist to pro level
2. **Trainer-driven gameplay** — your legs are the controller; power and cadence drive everything
3. **Moddable identity** — no licensed names/teams, but community can mod in real ones

## Gameplay

- **Perspective:** Third-person behind rider (Zwift-style)
- **Controls:** No steering. Cyclists are locked to the track rail. Only input is pedalling harder/easier and shifting virtual gears
- **Core loop:** Pick a race → ride the circuit → manage effort and gears vs AI field → finish and see results
- **Overtaking:** When faster than the rider ahead, your cyclist automatically pulls out of the line and passes, then tucks back in. No lateral player input
- **Drafting:** Real mechanic — riding behind others reduces the power needed to maintain the same speed. Scales with group size: ~30% power saving directly behind one rider, up to ~40-45% deep in a peloton. Drops off quickly as gap increases (effective within ~2-3 bike lengths)
- **Virtual gears:** Zwift Click-style virtual shifting (up/down mapped to trainer buttons or keyboard)
- **Heart rate:** Connected and displayed; potential for HR-zone based features later
- **Race format:** Mass-start road races on circuits. No time trials or ERG/training modes at launch (ERG could come later for structured training)

## HUD

Zwift-inspired but own visual style. Displays:
- Power (watts)
- Cadence (rpm)
- Heart rate (bpm)
- Speed (km/h)
- Current gear
- Gradient (%)
- Race position / standings
- Minimap or track progress indicator

## AI System

- ~20 AI riders at launch (configurable in settings for performance scaling)
- Difficulty slider: 0–100% where 100% ≈ professional peloton, low end is accessible to non-cyclists
- AI riders belong to colour-coded teams (e.g. white team, yellow team) — never named after real-world squads
- AI should behave like a peloton: drafting, breakaways, gruppetto logic
- Goal: increase rider count over time as performance allows

## Teams & Modding

- Ships with generic colour-coded teams (white, yellow, sky blue, red, etc.) matching roughly the number of real World Tour teams (~18)
- No real names, no real likenesses, no licensed content
- Mod system allows users to replace team names, rider names, and potentially kit textures/colours
- Mod format TBD — likely JSON config + texture overrides in a known folder

## Player Customisation

- Player chooses their bike model and rider outfit/kit
- Different bikes may have slightly different stats (lower priority)
- No progression system at launch — pick and ride

## Track Design

- **Format:** Circuits (loops). Player chooses number of laps when setting up a race
- **First track:** Small city-block or velodrome-style loop. Slight hill up one side, descent on the other. Keeps scope small for testing
- **Elevation:** Mandatory — hills and descents drive trainer resistance and race tactics. Flat tracks are not interesting
- **Future tracks:** Mountainous stages, longer circuits, varied terrain
- **All tracks are dev-authored** — no track editor or community-created maps

## Hardware & Connectivity

- **Protocol:** Bluetooth FTMS at launch (ANT+ FE-C deferred — requires USB dongle on PC)
- **Trainer interaction:** Simulation mode — game sends gradient/resistance based on track elevation; reads power, cadence, speed from trainer. No ERG mode at launch
- **Heart rate:** Bluetooth HR profile (UUID `0x180D`, characteristic `0x2A37`)
- **Primary test device:** Wahoo Kickr Core (macOS, Bluetooth)
- **Debug/dev mode:** Sliders and toggles for simulating power, cadence, gradient, and trainer responses without hardware attached

### Bluetooth Architecture

Two-layer approach: platform-specific BLE transport + cross-platform FTMS protocol in C#.

**BLE Transport (per-platform native plugins):**
- **macOS:** [UnityCoreBluetooth](https://github.com/fuziki/UnityCoreBluetooth) — Swift wrapper around CoreBluetooth with Unity C# bindings (MIT)
- **Windows:** [BleWinrtDll](https://github.com/adabru/BleWinrtDll) — C++ DLL wrapping WinRT BLE API (WTFPL, permissive)
- Both proven to work inside Unity Editor, both open source

**FTMS Protocol (C#, cross-platform):**
- Hand-rolled FTMS parser (preferred over FTMS.NET which is GPL-3.0)
- FTMS Service UUID: `0x1826`
- Indoor Bike Data characteristic: `0x2AD2` (notifications — speed, cadence, power)
- Fitness Machine Control Point: `0x2AD9` (write resistance/simulation params)
- The protocol parsing is manageable — flags byte + conditional fields

**C# abstraction layer** sits between game code and native plugins:
```
Game Systems → IBleDevice (scan, connect, subscribe, write) → native plugin per platform
            → FtmsParser (decode bike data, encode control commands)
```

**Reference project:** [BLE_FTMS_IndoorBike](https://github.com/frakw/BLE_FTMS_IndoorBike) (MIT) — working Unity proof-of-concept using BleWinrtDll + FTMS on Windows

**Dev testing without hardware:** [zwack](https://github.com/paixaop/zwack) — Node.js BLE FTMS trainer simulator, can broadcast fake trainer data over Bluetooth

## Technical Scope

- **Engine:** Unity 6 (6000.3.11f1)
- **Render pipeline:** URP (already configured with PC + Mobile quality tiers)
- **Target platforms:** macOS first (primary dev machine), Windows second. Both must work
- **Multiplayer:** Not at launch. Solo vs AI only
- **Input system:** New Input System (already installed)

## Art Direction

- **Style:** Stylised/clean — not photorealistic (no real footage to match)
- **Tracks:** Authored 3D environments — roads, terrain, scenery
- **Reference:** Zwift's visual level is a reasonable target

## Key Features (prioritised)

| Priority | Feature | Notes |
|----------|---------|-------|
| P0 | Trainer connectivity (Bluetooth FTMS) | Power, cadence, resistance control |
| P0 | Rail-locked cycling movement | Speed from power, auto-overtake |
| P0 | Drafting mechanic | Reduced power behind other riders |
| P0 | Virtual gears | Shift up/down, affects resistance sent to trainer |
| P0 | AI opponents (~20) | Difficulty slider, team colours |
| P0 | First circuit track | Small loop with a hill, for testing |
| P0 | HUD | Watts, cadence, HR, speed, gear, gradient, position |
| P0 | Debug/dev mode | Simulate trainer input without hardware |
| P1 | HR monitor support | Bluetooth HR profile, display on HUD |
| P1 | Race results / standings | Live position + finish screen |
| P1 | Peloton AI behaviour | Drafting, breakaways, group dynamics |
| P1 | AI rider count in settings | User can lower count for weaker hardware |
| P2 | Player bike/outfit selection | Choose bike model and kit |
| P2 | Mod system (names/teams) | JSON + texture folder convention |
| P2 | Multiple tracks | Track selection menu |
| P2 | Bike stats variation | Different bikes = slightly different handling |
| P2 | Mobile support | Leverage existing URP mobile preset |
| P3 | ANT+ FE-C support | USB dongle connectivity |
| P3 | ERG / training mode | Structured workouts |

## Open Questions

- UnityCoreBluetooth last updated early 2023 — may need minor updates for Unity 6. Verify before committing
- Exact drafting curve shape — linear falloff or stepped? Tune during playtesting
- Art style specifics — low-poly, stylised realism, or cel-shaded?
