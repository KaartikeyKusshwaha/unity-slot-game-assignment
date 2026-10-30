namespace SlotGame
{
    /// <summary>Calculates a payout when all three reels show the same symbol.</summary>
    public static class PayoutEvaluator
    {
        public static int Evaluate(int[] result, int bet, int[] multipliers)
        {
            bool allSymbolsMatch = result[0] == result[1] && result[1] == result[2];
            return allSymbolsMatch ? checked(bet * multipliers[result[0]]) : 0;
        }
    }
}
