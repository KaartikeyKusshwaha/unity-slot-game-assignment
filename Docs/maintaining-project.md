# Maintaining the project

Open the project with Unity 6000.6.7f1. The main scene is `Assets/Scenes/Main.unity`. Gameplay settings are stored in editable assets under `Assets/Resources`, so most presentation and payout adjustments do not require changing code.

## Common adjustments

- Change starting credits, bet options, symbols or payout multipliers in `Assets/Resources/GameConfig.asset`.
- Change menu fade, zoom, lever pull, reel stop spacing, reward timing and button easing in `Assets/Resources/Presentation.asset`.
- Adjust colours, UI wording and element placement in `Assets/Scenes/Main.unity` using the Unity Inspector.
- Change reel motion in `Assets/Scripts/ReelView.cs`, sound behavior in `Assets/Scripts/GameAudio.cs`, cabinet framing and lever motion in `Assets/Scripts/CabinetView.cs`, or decorative effects in `Assets/Scripts/CasinoEffects.cs`.
- Change accounting or persistence in `Assets/Scripts/RoundSession.cs`, `Assets/Scripts/SlotModel.cs` and `Assets/Scripts/SaveStore.cs`. Keep these classes independent of animation and UI.

`Assets/Editor/ProjectBuilder.cs` creates the initial scene and reusable prefabs. Running `ProjectBuilder.Create` replaces the generated scene and default configuration. Use it for a clean regeneration, then make visual adjustments in the scene and asset files. The game itself runs from the saved scene and does not need the scene builder at runtime.

## Project layout

- `Assets/Scripts`: game flow, accounting, persistence, audio and presentation behavior.
- `Assets/Editor`: scene creation, build setup and verification tools.
- `Assets/Scenes`: editable Unity scenes.
- `Assets/Resources`: symbol, payout and presentation configuration.
- `Assets/Prefabs`: reusable reel and symbol objects.
- `Assets/Animations`: reserved for Unity animation assets; current transitions use coroutines.
- `Assets/UI`: supplied artwork and supplementary presentation art.
- `Assets/Sounds`: processed sound cues and their CC0 source clips.
- `Build/WebGL`: browser build and responsive page files.

Keep the `.meta` files alongside Unity assets. Do not edit files under `Library`; Unity regenerates them. Use four spaces and the repository `.editorconfig` when editing C#.
