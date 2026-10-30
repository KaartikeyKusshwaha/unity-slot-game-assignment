using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame
{
    public class ReelView : MonoBehaviour
    {
        public Image[] cells;
        public Image glow;
        public int Current
        {
            get; private set;
        }
        private const float SymbolSpacing = 125f;
        private const float SettleDuration = .18f;
        private const float SettleBounceCycles = 1.6f;
        private const float SettleDamping = 9f;
        private const float SettleBounceHeight = -15f;
        private const int MinimumSpinSteps = 24;

        public void Show(int symbol, Sprite[] sprites)
        {
            Current = symbol;
            for (int index = 0; index < cells.Length; index++)
            {
                cells[index].sprite = sprites[(symbol + index + 2) % 4];
                cells[index].rectTransform.anchoredPosition =
                    new Vector2(0, (2 - index) * SymbolSpacing);
            }
        }

        public IEnumerator Settle(Sprite[] sprites, bool reduced)
        {
            int symbol = Current;
            for (float time = 0; time < SettleDuration; time += Time.deltaTime)
            {
                Show(symbol, sprites);
                float bouncePhase = time / SettleDuration * Mathf.PI * SettleBounceCycles;
                float bounceEnvelope = Mathf.Exp(-time * SettleDamping);
                float offset = reduced
                    ? 0
                    : Mathf.Sin(bouncePhase) * bounceEnvelope * SettleBounceHeight;
                foreach (var cell in cells)
                    cell.rectTransform.anchoredPosition += Vector2.up * offset;
                glow.color = new Color(1, .8f, .3f, (1 - time / SettleDuration) * .3f);
                yield return null;
            }
            Show(symbol, sprites);
            glow.color = Color.clear;
        }

        public IEnumerator Spin(int target, Sprite[] sprites, float duration, Action onStep = null)
        {
            glow.color = Color.clear;
            float elapsed = 0;
            int start = Current;
            int lastStep = 0;
            int steps = MinimumSpinSteps + ((target - start + 4) % 4);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                // Smoothstep gives zero velocity at both ends of the spin.
                float travel = steps * progress * progress * (3 - 2 * progress);
                int wholeSteps = Mathf.FloorToInt(travel);
                if (wholeSteps > lastStep)
                {
                    onStep?.Invoke();
                    lastStep = wholeSteps;
                }
                float fraction = travel - wholeSteps;
                Show((start + wholeSteps) % 4, sprites);
                foreach (var cell in cells)
                    cell.rectTransform.anchoredPosition += Vector2.up * (fraction * SymbolSpacing);
                yield return null;
            }

            // Snap to an exact symbol centre, eliminating accumulated float drift.
            Show(target, sprites);
        }
    }
}
