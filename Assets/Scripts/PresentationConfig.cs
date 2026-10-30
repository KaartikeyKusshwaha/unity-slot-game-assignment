using UnityEngine;

namespace SlotGame
{
    [CreateAssetMenu(menuName = "Slot Game/Presentation")]
    public class PresentationConfig : ScriptableObject
    {
        public float entrySeconds = .65f;
        public float zoomSeconds = .7f;
        public float pullSeconds = .22f;
        public float leverReturnSeconds = .35f;
        public float firstStopSeconds = 2.2f;
        public float stopGapSeconds = .45f;
        public float lossPauseSeconds = 1.2f;
        public float cardSeconds = .35f;
        public float countSeconds = .9f;
        public float collectSeconds = .8f;
        public float hoverSeconds = .12f;
        public float pressSeconds = .06f;
        public float releaseSeconds = .14f;
    }

    public static class Ease
    {
        public static float In(float t) => t * t * t;
        public static float Out(float t) => 1 - Mathf.Pow(1 - t, 3);
        public static float InOut(float t) => t < .5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
        public static float BackOut(float t)
        {
            float p = t - 1;
            return 1 + 2.70158f * p * p * p + 1.70158f * p * p;
        }
    }
}
