# Submission verification

Verified on 30 October 2026 using Unity 6000.6.7f1 and Microsoft Edge on Windows.

- `ProjectBuilder.VerifyLogic` checks all 64 exact outcomes: exactly four win and each uses its configured payout. It also checks stakes, duplicate input, interrupted rounds, save migration, corrupt-save recovery and insufficient credits.
- `RuntimeVerification.Run` passed in Play Mode. It covers menu/play, guarded lever input, locked controls, zoom, exact reel landing, pending payout, single collection, automatic loss return, reduced motion, saved volume/mute, reset/menu, three reveal accents, travel-synchronised reel ticks, payout count-up ticks and motor cleanup.
- Edge loaded the WebGL build over HTTP. The current smoke test entered the cabinet, accepted a 25-credit lever pull, showed the zoomed reel presentation, resolved a non-match and returned to the standing view with the balance at 825.
- The project verifies that all 16 supplied PNGs are direct dependencies of the main scene and that all 17 original provided files remain byte-for-byte unchanged.
- A fresh local Git clone has previously imported the Unity project and run the 64-outcome verification without using the original project cache. The largest WebGL file is below GitHub's normal 100 MB limit.

The bundled effects are validated as runtime events; speaker loudness and subjective mixing still depend on the browser and local device. Reel motion uses coroutines in `ReelView` rather than AnimationClip assets. The `Animations` directory is retained for the assignment's required project organization.
