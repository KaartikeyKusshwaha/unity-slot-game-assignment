using UnityEngine;

namespace SlotGame
{
    [CreateAssetMenu(menuName = "Slot Game/Configuration")]
    public class SlotConfig : ScriptableObject
    {
        public Sprite[] symbols;
        public int[] payouts = { 5, 10, 20, 25 };
        public int[] bets = { 10, 25, 50 };
        public int startingCredits = 1000;
        public float spinSeconds = 2.2f;
        public float stopDelay = .35f;
    }
}
