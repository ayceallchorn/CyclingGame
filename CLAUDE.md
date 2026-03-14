# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6 (6000.3.11f1) cycling project using Universal Render Pipeline (URP 17.3.0). Early-stage — currently a skeleton with no game code.

## Unity Configuration

- **Render Pipeline:** URP with two quality tiers — PC (`PC_RPAsset.asset`) and Mobile (`Mobile_RPAsset.asset`), each with its own renderer
- **Input:** New Input System 1.19.0, configured via `Assets/InputSystem_Actions.inputactions`
- **C# Version:** 9.0, targeting `netstandard2.1`
- **Color Space:** Linear
- **Notable Packages:** AI Navigation, Timeline, Multiplayer Center, Test Framework, unity-mcp (CoplayDev)

## Build & Development

Unity projects are built through the Unity Editor, not CLI. Scripts go in `Assets/` and are compiled automatically.

- **Solution:** `Cycling.slnx` with `Assembly-CSharp.csproj` (runtime) and `Assembly-CSharp-Editor.csproj` (editor tools)
- **IDE:** Rider and Visual Studio integrations are installed
- **Tests:** `com.unity.test-framework` is available; tests use NUnit attributes (`[Test]`, `[UnityTest]`) and run via Unity's Test Runner window

## Architecture Notes

- Two-tier quality system: scripts and assets should respect Mobile vs PC quality levels
- URP shaders only — do not use Built-in RP shader APIs
- New Input System only — do not use legacy `Input.GetKey` etc.; bind actions through `InputSystem_Actions.inputactions`
- Post-processing is handled via URP Volume Profiles in `Assets/Settings/`

## File Conventions

- Scene files in `Assets/Scenes/`
- URP and rendering settings in `Assets/Settings/`
- Design documentation in `docs/`
