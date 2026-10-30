using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotGame
{
    public class ButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private const float HoverScale = 1.03f;
        private const float PressScale = .96f;
        private const float DisabledAlpha = .42f;
        private const float MinimumTransitionDuration = .01f;

        [Header("Button animation")]
        public PresentationConfig config;

        [Header("Button sound")]
        public GameAudio audioPlayer;
        public bool ReducedMotion
        {
            get; set;
        }
        private CanvasGroup appearance;
        private Button button;
        private Vector3 startScale;
        private float current = 1;
        private float origin = 1;
        private float target = 1;
        private float elapsed;
        private float duration;
        private bool hovered;

        private void Awake()
        {
            button = GetComponent<Button>();
            appearance = gameObject.AddComponent<CanvasGroup>();
            startScale = transform.localScale;
        }
        private void OnDisable()
        {
            current = target = 1;
            if (startScale != Vector3.zero)
                transform.localScale = startScale;
        }
        private void Move(float value, float seconds)
        {
            origin = current;
            target = ReducedMotion ? 1 : value;
            elapsed = 0;
            duration = Mathf.Max(MinimumTransitionDuration, seconds);
        }
        public void RefreshAppearance()
        {
            if (appearance != null)
                appearance.alpha = button.interactable ? 1 : DisabledAlpha;
        }
        private void Update()
        {
            if (!button.interactable && target != 1)
                Move(1, config.releaseSeconds);
            appearance.alpha = button.interactable ? 1 : DisabledAlpha;
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(.01f, duration));
            float eased = target < 1 ? Ease.In(progress) : Ease.Out(progress);
            current = Mathf.LerpUnclamped(origin, target, eased);
            transform.localScale = startScale * current;
        }
        public void OnPointerEnter(PointerEventData e)
        {
            hovered = true;
            if (button.interactable)
                Move(HoverScale, config.hoverSeconds);
        }
        public void OnPointerExit(PointerEventData e)
        {
            hovered = false;
            Move(1, config.hoverSeconds);
        }
        public void OnPointerDown(PointerEventData e)
        {
            if (button.interactable)
            {
                Move(PressScale, config.pressSeconds);
                audioPlayer.Click();
            }
        }
        public void OnPointerUp(PointerEventData e)
        {
            Move(hovered && button.interactable ? HoverScale : 1, config.releaseSeconds);
        }
    }
}
