using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotGame
{
    public class ButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public PresentationConfig config;
        public GameAudio audioPlayer;
        public bool ReducedMotion {get;set;}
        private CanvasGroup appearance;
        private Button button;
        private Vector3 startScale;
        private float current = 1, origin = 1, target = 1, elapsed, duration;
        private bool hovered;

        private void Awake() { button = GetComponent<Button>(); appearance = gameObject.AddComponent<CanvasGroup>(); startScale = transform.localScale; }
        private void OnDisable() { current = target = 1; if (startScale != Vector3.zero) transform.localScale = startScale; }
        private void Move(float value, float seconds)
        {
            origin = current;
            target = ReducedMotion ? 1 : value;
            elapsed = 0;
            duration = Mathf.Max(.01f, seconds);
        }
        private void Update()
        {
            if (!button.interactable && target != 1) Move(1, config.releaseSeconds);
            appearance.alpha = button.interactable ? 1 : .42f;
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(.01f,duration));
            current = Mathf.LerpUnclamped(origin, target, target < 1 ? Ease.In(progress) : Ease.Out(progress));
            transform.localScale = startScale * current;
        }
        public void OnPointerEnter(PointerEventData e) { hovered = true; if(button.interactable)Move(1.03f,config.hoverSeconds); }
        public void OnPointerExit(PointerEventData e) { hovered = false; Move(1,config.hoverSeconds); }
        public void OnPointerDown(PointerEventData e) { if(button.interactable){Move(.96f,config.pressSeconds);audioPlayer.Click();} }
        public void OnPointerUp(PointerEventData e) { Move(hovered && button.interactable?1.03f:1,config.releaseSeconds); }
    }
}
