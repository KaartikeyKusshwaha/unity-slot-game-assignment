# Supplied asset usage

All 16 supplied PNGs must be actual dependencies of the generated Main scene. `ProjectBuilder.VerifyAssetUse` enforces this; importing an unused file does not pass.

| Source | In-game role |
|---|---|
| bg_gradient.png | Atmosphere-gradient overlay over the supplementary casino room |
| slot-machine1.png | Main-menu machine hero |
| slot-machine2.png | Attached rotating lever texture |
| slot-machine3.png | Lowered-lever cap during the pull state |
| slot-machine4.png | Main playable cabinet |
| slot-machine5.png | Reel glass/backing assembly |
| slot-symbol1.png through slot-symbol4.png | Actual outcome symbols, menu accents and payout table |
| slot_machine_Middle_box.png | Glossy action buttons and cabinet nameplate |
| popup.png | Payout, settings, reset, exit and reward frame |
| slot_machine_buttons-02.png | Circular close controls with normal, hover, pressed and disabled states |
| slot_machine_buttons-03.png | Increase-bet arrow with all states |
| slot_machine_buttons-04.png | Decrease-bet arrow with all states |
| Yes_No_Btn.png | Reset confirmation YES/NO with supplied interaction states |
| 4tXlXs.gif | Original 85-frame motion reference; not embedded as a replacement for live reels |

Source files remain unchanged. Atlas regions are referenced by Unity Sprite assets, so appearance and provenance remain tied to the provided artwork. Additional lighting, transitions and coin effects supplement these assets.
