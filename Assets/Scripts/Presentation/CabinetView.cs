using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame
{
    public class CabinetView : MonoBehaviour
    {
        private const float LeverPullAngle = -135f;
        private const float LeverHoldSeconds = .07f;
        private const float PunchDuration = .1f;
        private const float PunchStrength = .018f;

        [Header("Cabinet framing")]
        public RectTransform cabinet;
        public Image transitionFade;

        [Header("Lever")]
        public RectTransform lever;
        public Image pressedLeverTip;
        public PresentationConfig config;
        public GameAudio audioPlayer;

        [Header("Camera framing")]
        public Vector2 standingPosition = new Vector2(0, -20);
        public float standingScale = .95f;
        public Vector2 reelFocus = new Vector2(0, -38);
        public float closeScale = 2.35f;

        public IEnumerator SetClose(bool close, bool reduced)
        {
            Vector2 destination = close ? -reelFocus * closeScale : standingPosition;
            float scale = close ? closeScale : standingScale;
            if (reduced)
            {
                yield return Fade(.8f, .1f);
                cabinet.anchoredPosition = destination;
                cabinet.localScale = Vector3.one * scale;
                yield return Fade(0, .1f);
                yield break;
            }
            Vector2 from = cabinet.anchoredPosition;
            Vector3 initialScale = cabinet.localScale;
            for (float time = 0; time < config.zoomSeconds; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.Clamp01(time / config.zoomSeconds);
                float eased = Ease.InOut(progress);
                cabinet.anchoredPosition = Vector2.LerpUnclamped(from, destination, eased);
                cabinet.localScale = Vector3.LerpUnclamped(initialScale, Vector3.one * scale, eased);
                yield return null;
            }
            cabinet.anchoredPosition = destination;
            cabinet.localScale = Vector3.one * scale;
        }

        public void Pull(bool reduced)
        {
            StartCoroutine(PullRoutine(reduced));
            if (!reduced)
                StartCoroutine(Punch());
        }
        private IEnumerator PullRoutine(bool reduced)
        {
            if (reduced)
            {
                audioPlayer.Play(audioPlayer.latch, .5f);
                yield break;
            }
            yield return RotateLever(LeverPullAngle, config.pullSeconds, true);
            audioPlayer.Play(audioPlayer.latch, .6f);
            pressedLeverTip.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(LeverHoldSeconds);
            yield return RotateLever(0, config.leverReturnSeconds, false);
            pressedLeverTip.gameObject.SetActive(false);
        }
        private IEnumerator Punch()
        {
            Vector3 from = cabinet.localScale;
            for (float time = 0; time < PunchDuration; time += Time.unscaledDeltaTime)
            {
                float progress = time / PunchDuration;
                cabinet.localScale = from * (1 - PunchStrength * Mathf.Sin(progress * Mathf.PI));
                yield return null;
            }
            cabinet.localScale = from;
        }
        private IEnumerator RotateLever(float angle, float seconds, bool easeIn)
        {
            float from = lever.localEulerAngles.z;
            for (float time = 0; time < seconds; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.Clamp01(time / seconds);
                float eased = easeIn ? Ease.In(progress) : Ease.Out(progress);
                float rotation = Mathf.LerpAngle(from, angle, eased);
                lever.localRotation = Quaternion.Euler(0, 0, rotation);
                yield return null;
            }
            lever.localRotation = Quaternion.Euler(0, 0, angle);
        }
        private IEnumerator Fade(float target, float seconds)
        {
            Color from = transitionFade.color;
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            {
                transitionFade.color = new Color(0, 0, 0, Mathf.Lerp(from.a, target, t / seconds));
                yield return null;
            }
            transitionFade.color = new Color(0, 0, 0, target);
        }
    }
}
