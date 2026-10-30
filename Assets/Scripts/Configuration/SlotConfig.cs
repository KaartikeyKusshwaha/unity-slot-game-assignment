using UnityEngine;

namespace SlotGame
{
    [CreateAssetMenu(menuName = "Slot Game/Configuration")]
    public class SlotConfig : ScriptableObject
    {
        [Header("Symbols and payouts")]
        public Sprite[] symbols;
        public int[] payouts = { 5, 10, 20, 25 };

        [Header("Player settings")]
        public int[] bets = { 10, 25, 50 };
        public int startingCredits = 1000;
    }
}
