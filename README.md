# The Lucky Line

Unity slot-machine internship assignment, using Unity 6000.6.7f1.

## Gameplay
Three reels, four equally likely symbols, one central winning line. Three identical centre symbols win. Start with 1,000 credits; cycle between bets of 10, 25 and 50. Seven pays 5 times the bet, cherries 10, bell 20, and BAR 25. Payout means the total amount credited after the stake has been deducted.

## Controls
SPIN starts a round. BET cycles stakes. PAYOUTS explains the rules. SOUND toggles audio. RESET CREDITS asks for confirmation before replacing the balance with 1,000. Controls prevent overlapping spins or bet changes during a round.

## Development
Open Assets/Scenes/Main.unity in Unity 6000.6.7f1. Web Build Support is required for browser export. ProjectBuilder.Create generates the editable scene and reusable prefabs; ProjectBuilder.VerifyLogic checks all 64 outcomes and basic wallet accounting; ProjectBuilder.BuildWeb exports to Build/WebGL. Invoke these static methods with Unity Editor's batch-mode executeMethod argument.

## Design
Outcome generation, payout evaluation and wallet arithmetic are independent of presentation. Reels reveal results chosen before animation. Configuration holds symbols, stakes, multipliers and timing. Credits are saved after stake deduction and settlement. Reloading during an unfinished spin forfeits that stake; it cannot refund or replay the round. Invalid saves fall back to defaults. Browser storage is local to its origin.

## Assets and extras
Artwork comes from the supplied Slot Machine assignment archive. Audio tones are generated for this project. Polish includes staggered stops, win highlights, sound/mute, payout help, saved credits and confirmed reset. No paid plugins are used.

## Delivery status
In progress: scene and core logic compile; initial exhaustive outcome tests pass. Visual review, expanded tests, browser verification and final WebGL delivery remain outstanding.
