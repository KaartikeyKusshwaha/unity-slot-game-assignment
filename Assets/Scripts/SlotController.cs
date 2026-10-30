using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame
{
    public enum GameState
    {
        Menu, Entering, Ready, Pulling, ZoomingIn, Spinning, Result, AwaitingCollect, Collecting, Returning
    }

    public class SlotController : MonoBehaviour
    {
        public SlotConfig config;
        public PresentationConfig presentation;
        public ReelView[] reels;
        public Text credits, bet, lastWin, status, muteLabel, rewardLabel;
        public Button previousBetButton, nextBetButton;
        public Button leverButton, betButton, resetButton, helpButton, menuButton, collectButton, motionButton;
        public GameObject helpPanel, resetPanel, settingsPanel, exitPanel, hud;
        public CanvasGroup menuGroup;
        public Slider volumeSlider;
        public Text motionLabel;
        public GameAudio audioPlayer;
        public CasinoEffects effects;
        public CabinetView view;
        public GameState State { get; private set; } = GameState.Menu;
        public RoundSession Session => session;
        private RoundSession session;
        private int stoppedReels;
        private readonly Dictionary<CanvasGroup, int> panelTransitions = new Dictionary<CanvasGroup, int>();
        private bool settingsUpdating;
        private bool ModalOpen => helpPanel.activeSelf || resetPanel.activeSelf || settingsPanel.activeSelf || exitPanel.activeSelf;

        private void Start()
        {
            session = new RoundSession(new SaveStore(), new RandomSource(), config.bets, config.payouts);
            for (int index = 0; index < reels.Length; index++)
                reels[index].Show(index, config.symbols);
            hud.SetActive(false);
            view.cabinet.gameObject.SetActive(false);
            menuGroup.gameObject.SetActive(true);
            menuGroup.alpha = 1;
            effects.HideReward();
            ApplySettings();
            Refresh();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Spin();
        }

        private void SetState(GameState state)
        {
            State = state;
            Refresh();
        }
        private void Refresh()
        {
            if (session == null)
                return;
            var data = session.Data;
            credits.text = data.credits.ToString("N0");
            bet.text = "BET  " + config.bets[data.betIndex];
            lastWin.text = data.lastWin.ToString("N0");
            muteLabel.text = data.muted ? "SOUND OFF" : "SOUND ON";
            bool ready = State == GameState.Ready && !data.pendingRound;
            leverButton.interactable = ready && data.credits >= config.bets[data.betIndex];
            betButton.interactable = resetButton.interactable = helpButton.interactable = menuButton.interactable = ready;
            previousBetButton.interactable = nextBetButton.interactable = ready;
            collectButton.interactable = State == GameState.AwaitingCollect;
            motionButton.interactable = State == GameState.Menu || ready;
            foreach (var motion in FindObjectsByType<ButtonMotion>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                motion.RefreshAppearance();
            if (ready)
                status.text = leverButton.interactable ? "PULL THE LEVER  /  PRESS SPACE" : "Choose a smaller bet or reset your credits.";
        }

        public void Play()
        {
            if (State != GameState.Menu || ModalOpen)
                return;
            SetState(GameState.Entering);
            audioPlayer.Click();
            audioPlayer.Play(audioPlayer.entry, .7f);
            StartCoroutine(Enter());
        }
        private IEnumerator Enter()
        {
            view.cabinet.gameObject.SetActive(true);
            hud.SetActive(true);
            yield return FadePanel(menuGroup, false, presentation.entrySeconds);
            if (session.Data.pendingRound)
            {
                yield return view.SetClose(true, session.Data.reducedMotion);
                for (int i = 0; i < reels.Length; i++)
                    reels[i].Show(session.Data.result[i], config.symbols);
                yield return ShowResult();
            }
            else
                SetState(GameState.Ready);
        }

        public void Spin()
        {
            if (State != GameState.Ready || ModalOpen || !session.TryBegin())
                return;
            SetState(GameState.Pulling);
            audioPlayer.Play(audioPlayer.lever, .8f);
            view.Pull(session.Data.reducedMotion);
            StartCoroutine(Round());
        }
        private IEnumerator Round()
        {
            yield return new WaitForSeconds(.12f);
            SetState(GameState.ZoomingIn);
            yield return view.SetClose(true, session.Data.reducedMotion);
            SetState(GameState.Spinning);
            status.text = "WATCH THE CENTRE LINE";
            effects.Rolling = true;
            audioPlayer.StartReels();
            stoppedReels = 0;
            for (int i = 0; i < reels.Length; i++)
                StartCoroutine(AnimateReel(i, session.Data.result[i]));
            yield return new WaitUntil(() => stoppedReels == reels.Length);
            audioPlayer.StopReels();
            effects.Rolling = false;
            yield return ShowResult();
        }
        private IEnumerator AnimateReel(int index, int symbol)
        {
            float duration = presentation.firstStopSeconds + index * presentation.stopGapSeconds;
            yield return reels[index].Spin(symbol, config.symbols, duration - .18f, () => audioPlayer.ReelTick(index));
            audioPlayer.Reveal(index);
            yield return reels[index].Settle(config.symbols, session.Data.reducedMotion);
            stoppedReels++;
        }
        private IEnumerator ShowResult()
        {
            SetState(GameState.Result);
            int payout = session.Data.pendingPayout;
            if (payout > 0)
            {
                status.text = "THREE MATCHING SYMBOLS";
                foreach (var reel in reels)
                    reel.glow.color = new Color(1, .75f, .2f, .25f);
                audioPlayer.Play(audioPlayer.win, .8f);
                rewardLabel.text = "Congratulations - you won";
                yield return effects.ShowReward(payout);
                SetState(GameState.AwaitingCollect);
            }
            else
            {
                status.text = "NO MATCH";
                yield return new WaitForSeconds(presentation.lossPauseSeconds);
                session.FinishLoss();
                yield return Return();
            }
        }
        public void CollectReward()
        {
            if (State != GameState.AwaitingCollect)
                return;
            SetState(GameState.Collecting);
            int reward = session.Collect();
            if (reward <= 0)
                return;
            Refresh();
            audioPlayer.Play(audioPlayer.collect, .85f);
            StartCoroutine(Collection());
        }
        private IEnumerator Collection()
        {
            yield return effects.Collect();
            yield return Return();
        }
        private IEnumerator Return()
        {
            SetState(GameState.Returning);
            yield return view.SetClose(false, session.Data.reducedMotion);
            SetState(GameState.Ready);
        }

        public void ChangeBet()
        {
            if (State != GameState.Ready || ModalOpen)
                return;
            session.ChangeBet();
            Refresh();
        }
        public void PreviousBet()
        {
            if (State != GameState.Ready || ModalOpen)
                return;
            session.ChangeBet(-1);
            Refresh();
        }
        public void ShowHelp()
        {
            if (State == GameState.Ready)
                OpenPanel(helpPanel);
        }
        public void CloseHelp()
        {
            ClosePanel(helpPanel);
        }
        public void AskReset()
        {
            if (State == GameState.Ready)
                OpenPanel(resetPanel);
        }
        public void CancelReset()
        {
            ClosePanel(resetPanel);
        }
        public void ConfirmReset()
        {
            if (State != GameState.Ready || session.Data.pendingRound)
                return;
            session.Reset(config.startingCredits);
            ClosePanel(resetPanel);
            Refresh();
        }
        public void ShowSettings()
        {
            OpenPanel(settingsPanel);
            ApplySettings();
        }
        public void CloseSettings()
        {
            ClosePanel(settingsPanel);
        }
        public void ToggleMute()
        {
            session.Data.muted = !session.Data.muted;
            session.Save();
            ApplySettings();
            Refresh();
        }
        public void SetVolume(float value)
        {
            if (settingsUpdating || session == null)
                return;
            session.Data.volume = value;
            session.Save();
            ApplySettings();
        }
        public void ToggleMotion()
        {
            if (State != GameState.Ready && State != GameState.Menu)
                return;
            session.Data.reducedMotion = !session.Data.reducedMotion;
            session.Save();
            ApplySettings();
        }
        private void ApplySettings()
        {
            settingsUpdating = true;
            audioPlayer.ApplySettings(session.Data);
            volumeSlider.SetValueWithoutNotify(session.Data.volume);
            motionLabel.text = session.Data.reducedMotion ? "REDUCED MOTION: ON" : "REDUCED MOTION: OFF";
            effects.ReducedMotion = session.Data.reducedMotion;
            foreach (var motion in FindObjectsByType<ButtonMotion>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                motion.ReducedMotion = session.Data.reducedMotion;
            settingsUpdating = false;
        }
        public void MainMenu()
        {
            if (State != GameState.Ready || session.Data.pendingRound)
                return;
            SetState(GameState.Menu);
            hud.SetActive(false);
            view.cabinet.gameObject.SetActive(false);
            StartCoroutine(FadePanel(menuGroup, true, .4f));
        }
        public void Exit()
        {
            if (State == GameState.Menu)
                OpenPanel(exitPanel);
        }
        public void ReturnFromExit()
        {
            ClosePanel(exitPanel);
        }

        private void OpenPanel(GameObject panel)
        {
            StartCoroutine(FadePanel(panel.GetComponent<CanvasGroup>(), true, .22f));
        }
        private void ClosePanel(GameObject panel)
        {
            StartCoroutine(FadePanel(panel.GetComponent<CanvasGroup>(), false, .18f));
        }
        private IEnumerator FadePanel(CanvasGroup panel, bool show, float seconds)
        {
            int token = panelTransitions.TryGetValue(panel, out int previous) ? previous + 1 : 1;
            panelTransitions[panel] = token;
            bool wasActive = panel.gameObject.activeSelf;
            if (show && !wasActive)
                panel.alpha = 0;
            Vector2 fromPosition = ((RectTransform)panel.transform).anchoredPosition;
            if (show && !wasActive && !session.Data.reducedMotion)
                fromPosition = new Vector2(0, 16);
            panel.gameObject.SetActive(true);
            panel.interactable = panel.blocksRaycasts = show;
            float from = panel.alpha;
            for (float time = 0; time < seconds; time += Time.unscaledDeltaTime)
            {
                if (panelTransitions[panel] != token)
                    yield break;
                float progress = Mathf.Clamp01(time / seconds);
                float eased = show ? Ease.Out(progress) : Ease.In(progress);
                panel.alpha = Mathf.Lerp(from, show ? 1 : 0, eased);
                if (!session.Data.reducedMotion && panel != menuGroup)
                    ((RectTransform)panel.transform).anchoredPosition = Vector2.Lerp(fromPosition, show ? Vector2.zero : new Vector2(0, 12), eased);
                yield return null;
            }
            if (panelTransitions[panel] != token)
                yield break;
            panel.alpha = show ? 1 : 0;
            ((RectTransform)panel.transform).anchoredPosition = Vector2.zero;
            if (!show)
                panel.gameObject.SetActive(false);
        }
    }
}
