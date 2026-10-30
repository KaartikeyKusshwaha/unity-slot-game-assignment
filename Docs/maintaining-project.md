# Maintaining the project

Open the project with Unity 6000.6.7f1. The main scene is `Assets/Scenes/Main.unity`. Gameplay settings are stored in editable assets under `Assets/Resources`, so most presentation and payout adjustments do not require changing code. Inspector fields are grouped by purpose. Reel timing, transitions and reward timing live in the Presentation configuration; symbols, bets, credits and payouts live in Game configuration.

## Common adjustments

- Change starting credits, bet options, symbols or payout multipliers in `Assets/Resources/GameConfig.asset`.
- Change menu fade, zoom, lever pull, reel stop spacing, reward timing and button transitions in `Assets/Resources/Presentation.asset`.
- Adjust colours, UI wording and element placement in `Assets/Scenes/Main.unity` using the Unity Inspector.
- Change reel motion in `Assets/Scripts/Presentation/ReelView.cs`, sound behavior in `Assets/Scripts/Audio/GameAudio.cs`, cabinet framing and lever motion in `Assets/Scripts/Presentation/CabinetView.cs`, or decorative effects in `Assets/Scripts/Effects/CasinoEffects.cs`.
- Change accounting in `Assets/Scripts/Gameplay/RoundSession.cs` and `Assets/Scripts/Gameplay/Wallet.cs`; change outcome selection and payout rules in the other small files under `Assets/Scripts/Gameplay`. Keep these classes independent of animation and UI.
- Change save data and migration in `Assets/Scripts/Persistence/SaveStore.cs`.

`Assets/Editor/ProjectBuilder.cs` creates the initial scene and reusable prefabs. Running `ProjectBuilder.Create` replaces the generated scene and default configuration. Use it for a clean regeneration, then make visual adjustments in the scene and asset files. The game itself runs from the saved scene and does not need the scene builder at runtime.

## Project layout

- `Assets/Scripts/Audio`: sound playback, mixing and audio settings.
- `Assets/Scripts/Configuration`: editable game and presentation settings.
- `Assets/Scripts/Effects`: reward, light and ambient effects.
- `Assets/Scripts/Gameplay`: game flow, random outcomes, payout evaluation, wallet and round accounting. Each model responsibility has its own source file.
- `Assets/Scripts/Persistence`: save data, migration and local storage.
- `Assets/Scripts/Presentation`: cabinet, reel and button animation.
- `Assets/Editor`: scene creation, build setup and verification tools.
- `Assets/Scenes`: editable Unity scenes.
- `Assets/Resources`: symbol, payout and presentation configuration.
- `Assets/Prefabs`: reusable reel and symbol objects.
- `Assets/Animations`: reserved for Unity animation assets; current transitions use coroutines.
- `Assets/UI`: supplied artwork and supplementary presentation art.
- `Assets/Sounds`: processed sound cues and their CC0 source clips.
- `Build/WebGL`: browser build and responsive page files.

Keep the `.meta` files alongside Unity assets. Do not edit files under `Library`; Unity regenerates them. Use four spaces and the repository `.editorconfig` when editing C#.
