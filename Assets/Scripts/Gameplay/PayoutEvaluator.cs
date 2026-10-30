namespace SlotGame
{
    /// <summary>
    /// Calculates the total credit award when all three reels match.
    /// The stake is deducted separately when the round begins.
    /// </summary>
    public static class PayoutEvaluator
    {
        public static int Evaluate(int[] result, int bet, int[] multipliers)
        {
            bool allSymbolsMatch = result[0] == result[1] && result[1] == result[2];
            return allSymbolsMatch ? checked(bet * multipliers[result[0]]) : 0;
        }
    }
}
