using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame
{
    public class CabinetView : MonoBehaviour
    {
        public RectTransform cabinet, lever;
        public Image transitionFade;
        public PresentationConfig config;
        public Vector2 standingPosition = new Vector2(0, -20);
        public float standingScale = .88f;
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
                float eased = Ease.InOut(Mathf.Clamp01(time / config.zoomSeconds));
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
        }
        private IEnumerator PullRoutine(bool reduced)
        {
            if (reduced)
                yield break;
            yield return RotateLever(55, config.pullSeconds, true);
            yield return RotateLever(0, config.leverReturnSeconds, false);
        }
        private IEnumerator RotateLever(float angle, float seconds, bool easeIn)
        {
            float from = lever.localEulerAngles.z;
            for (float time = 0; time < seconds; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.Clamp01(time / seconds);
                lever.localRotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(from, angle, easeIn ? Ease.In(progress) : Ease.Out(progress)));
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
