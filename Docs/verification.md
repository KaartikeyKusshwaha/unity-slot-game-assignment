# Submission verification

Verified using Unity 6000.6.7f1 and Microsoft Edge on Windows.

- Current-source WebGL build: Unity reported Build Finished, Result: Success.
- All 64 outcomes: exactly four winning combinations, each matching its configured multiplier.
- Unity Play Mode: repeated-click protection, bet lock, saved stake, exact BAR win settlement, reel landing, mute persistence, reset/cancel, saved bet, corrupt-save fallback and insufficient funds passed.
- Edge: game loaded over HTTP; losing spin deducted 10 from 1,000; balance/bet/mute survived reload; payout help and reset/cancel worked; reload during a 25-credit spin preserved the deducted balance of 965; confirmed reset restored 1,000.
- Edge responsive presentation: 960x600 and 1280x800 fitted the canvas and controls without page overflow.
- Fresh local Git clone: required Unity assets and WebGL files present, source assets have metadata, Unity import and all 64 payout checks passed without using the original project cache.
- Largest build file is approximately 26 MB, below GitHub's normal file limit.

The initial browser build reported a Unity storage-sync deprecation warning; the current generated page opts into automatic synchronization. No console errors were observed during the initial browser smoke test. Audio mute behavior was tested; actual loudspeaker audibility was not verified. Visual feedback remains open for iteration.

Reel motion uses coroutines in ReelView rather than AnimationClip assets. The Animations directory is retained for project organization.
