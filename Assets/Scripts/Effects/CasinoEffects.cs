using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame
{
    /// <summary>Reusable visual effects pool; no accounting or outcome generation.</summary>
    public class CasinoEffects : MonoBehaviour
    {
        [Header("Animation and audio")]
        public PresentationConfig config;
        public GameAudio audioPlayer;

        [Header("Reward panel")]
        public GameObject rewardOverlay;
        public RectTransform rewardCard;
        public CanvasGroup rewardGroup;
        public Text rewardAmount;

        [Header("Cabinet lights")]
        public Image[] lights;
        public Image[] sideLights;

        [Header("Coins and ambient motion")]
        public RectTransform[] particles;
        public RectTransform balanceTarget;
        public RectTransform ambientLayer;
        public bool ReducedMotion
        {
            get; set;
        }
        public bool Rolling
        {
            get; set;
        }
        private bool celebrating;
        private bool collecting;
        private Color gold = new Color(1, .8f, .3f);

        private void Update()
        {
            float clock = Time.unscaledTime;
            if (ambientLayer != null)
            {
                Vector2 target = ReducedMotion
                    ? Vector2.zero
                    : new Vector2(
                        (Input.mousePosition.x / Mathf.Max(1, Screen.width) - .5f) * 10,
                        (Input.mousePosition.y / Mathf.Max(1, Screen.height) - .5f) * 6);
                ambientLayer.anchoredPosition = Vector2.Lerp(ambientLayer.anchoredPosition, target, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 3));
            }
            for (int i = 0; i < lights.Length; i++)
            {
                float value = ReducedMotion ? .8f : .6f + .4f * Mathf.Sin(clock * 2.5f + i * .55f);
                lights[i].color = Color.Lerp(new Color(.35f, .16f, .04f), gold, value);
            }
            for (int i = 0; i < sideLights.Length; i++)
            {
                float alpha = Rolling && !ReducedMotion ? .08f + .06f * Mathf.Sin(clock * 4 + i * 2) : 0;
                sideLights[i].color = new Color(1f, .68f, .28f, alpha);
            }
            if (celebrating && !collecting && !ReducedMotion)
                for (int i = 0; i < particles.Length; i++)
                {
                    float horizontal = -590 + i * 38 + Mathf.Sin(clock * 2 + i) * 24;
                    float fallSpeed = 130 + i % 5 * 20;
                    float vertical = 450 - Mathf.Repeat(clock * fallSpeed + i * 67, 950);
                    particles[i].anchoredPosition = new Vector2(horizontal, vertical);
                    particles[i].localRotation = Quaternion.Euler(0, 0, clock * 100 + i * 25);
                }
        }

        public IEnumerator ShowReward(int amount)
        {
            rewardOverlay.SetActive(true);
            celebrating = true;
            foreach (var particle in particles)
                particle.gameObject.SetActive(!ReducedMotion);
            float duration = Mathf.Max(config.cardSeconds, config.countSeconds);
            for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
            {
                float arrival = Mathf.Clamp01(time / config.cardSeconds);
                rewardGroup.alpha = Ease.Out(arrival);
                rewardCard.localScale = Vector3.one * (ReducedMotion ? 1 : Mathf.LerpUnclamped(.75f, 1, Ease.BackOut(arrival)));
                rewardCard.anchoredPosition = new Vector2(0, ReducedMotion ? 0 : Mathf.Lerp(25, 0, Ease.Out(arrival)));
                audioPlayer.CountTick(Mathf.Clamp01(time / config.countSeconds));
                rewardAmount.text = "+" + Mathf.RoundToInt(amount * Ease.Out(Mathf.Clamp01(time / config.countSeconds))).ToString("N0");
                yield return null;
            }
            rewardGroup.alpha = 1;
            rewardCard.localScale = Vector3.one;
            rewardCard.anchoredPosition = Vector2.zero;
            rewardAmount.text = "+" + amount.ToString("N0");
        }

        public IEnumerator Collect()
        {
            collecting = true;
            Vector3 target = particles[0].parent.InverseTransformPoint(balanceTarget.position);
            Vector2[] origins = new Vector2[particles.Length];
            for (int i = 0; i < particles.Length; i++)
            {
                origins[i] = new Vector2((i % 8 - 3.5f) * 45, -40 + i / 8 * 24);
                particles[i].anchoredPosition = origins[i];
                particles[i].localRotation = Quaternion.identity;
                particles[i].gameObject.SetActive(!ReducedMotion);
                particles[i].GetComponent<Image>().color = gold;
            }
            for (float time = 0; time < config.collectSeconds; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.Clamp01(time / config.collectSeconds);
                if (!ReducedMotion)
                    balanceTarget.localScale = Vector3.one * (1 + .07f * Mathf.Sin(progress * Mathf.PI * 3));
                for (int i = 0; i < particles.Length; i++)
                {
                    float t = Mathf.Clamp01((progress - i * .008f) / .72f);
                    Vector2 point = Vector2.Lerp(origins[i], target, Ease.InOut(t));
                    point.y += Mathf.Sin(t * Mathf.PI) * 100;
                    particles[i].anchoredPosition = point;
                    particles[i].localScale = Vector3.one * Mathf.Lerp(1, .3f, t);
                }
                rewardGroup.alpha = 1 - Ease.In(Mathf.Clamp01((progress - .55f) / .45f));
                yield return null;
            }
            balanceTarget.localScale = Vector3.one;
            HideReward();
        }

        public void HideReward()
        {
            celebrating = collecting = false;
            rewardOverlay.SetActive(false);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].localScale = Vector3.one;
                particles[i].GetComponent<Image>().color = i % 3 == 0 ? gold : (i % 3 == 1 ? new Color(1, .15f, .3f) : new Color(.2f, .85f, 1));
            }
        }
    }
}
