using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame
{
    public class SlotController : MonoBehaviour
    {
        public SlotConfig config;
        public ReelView[] reels;
        public Text credits, bet, lastWin, status, muteLabel;
        public Button spinButton, betButton, resetButton, helpButton;
        public GameObject helpPanel, resetPanel;
        public GameAudio audioPlayer;

        private readonly SaveStore store = new SaveStore();
        private SaveData data;
        private Wallet wallet;
        private OutcomeGenerator outcomes;
        private bool spinning;
        private int stoppedReels;

        private void Start()
        {
            data = store.Load();
            wallet = new Wallet(data.credits);
            outcomes = new OutcomeGenerator(new RandomSource());
            for (int index = 0; index < reels.Length; index++)
                reels[index].Show(index, config.symbols);
            audioPlayer.Mute(data.muted);
            Refresh();
        }

        private void Save()
        {
            data.credits = wallet.Credits;
            store.Save(data);
        }

        private void Refresh()
        {
            credits.text = wallet.Credits.ToString("N0");
            bet.text = "BET  " + config.bets[data.betIndex];
            muteLabel.text = data.muted ? "SOUND OFF" : "SOUND ON";
            spinButton.interactable = !spinning && wallet.Credits >= config.bets[data.betIndex];
            betButton.interactable = !spinning;
            resetButton.interactable = !spinning;
            helpButton.interactable = !spinning;
        }

        public void Spin()
        {
            if (spinning || helpPanel.activeSelf || resetPanel.activeSelf)
                return;
            int stake = config.bets[data.betIndex];
            if (!wallet.Spend(stake))
                return;

            // Lock before starting the coroutine, so repeated input cannot charge twice.
            spinning = true;
            Save();
            Refresh();
            StartCoroutine(Resolve(stake));
        }

        private IEnumerator Resolve(int stake)
        {
            stoppedReels = 0;
            int[] result = outcomes.Spin();
            status.text = "Good luck. Reels in motion...";
            lastWin.text = "0";
            audioPlayer.Play(audioPlayer.spin);
            for (int index = 0; index < reels.Length; index++)
                StartCoroutine(AnimateReel(index, result[index]));

            // Settle only after every animation has landed, including on slow frames.
            yield return new WaitUntil(() => stoppedReels == reels.Length);
            int payout = PayoutEvaluator.Evaluate(result, stake, config.payouts);
            wallet.Award(payout);
            Save();
            lastWin.text = payout.ToString("N0");
            status.text = payout > 0 ? "MATCH!  +" + payout + " CREDITS" : "A new chance with every spin.";
            if (payout > 0)
            {
                audioPlayer.Play(audioPlayer.win);
                for (float elapsed = 0; elapsed < .8f; elapsed += Time.deltaTime)
                {
                    foreach (var reel in reels)
                        reel.glow.color = new Color(1, .73f, .25f, .2f + .15f * Mathf.Sin(elapsed * 18));
                    yield return null;
                }
            }
            spinning = false;
            Refresh();
            if (!spinButton.interactable)
                status.text = "Choose a smaller bet or reset your credits.";
        }

        private IEnumerator AnimateReel(int index, int target)
        {
            yield return reels[index].Spin(target, config.symbols, config.spinSeconds + index * config.stopDelay);
            audioPlayer.Play(audioPlayer.stop);
            stoppedReels++;
        }

        public void ChangeBet()
        {
            if (spinning) return;
            data.betIndex = (data.betIndex + 1) % config.bets.Length;
            Save();
            Refresh();
        }

        public void ToggleMute()
        {
            data.muted = !data.muted;
            audioPlayer.Mute(data.muted);
            Save();
            Refresh();
        }

        public void ShowHelp() { if (!spinning) helpPanel.SetActive(true); }
        public void CloseHelp() { helpPanel.SetActive(false); }
        public void AskReset() { if (!spinning) resetPanel.SetActive(true); }
        public void CancelReset() { resetPanel.SetActive(false); }

        public void ConfirmReset()
        {
            wallet = new Wallet(config.startingCredits);
            Save();
            resetPanel.SetActive(false);
            lastWin.text = "0";
            status.text = "Fresh credits. Ready when you are.";
            Refresh();
        }
    }
}
