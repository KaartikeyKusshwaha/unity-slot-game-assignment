namespace SlotGame
{
    /// <summary>Chooses each reel result independently before the reels animate.</summary>
    public sealed class OutcomeGenerator
    {
        private const int ReelCount = 3;
        private const int SymbolCount = 4;

        private readonly IRandomSource random;

        public OutcomeGenerator(IRandomSource random)
        {
            this.random = random;
        }

        public int[] Spin()
        {
            var result = new int[ReelCount];
            for (int reel = 0; reel < result.Length; reel++)
                result[reel] = random.Next(SymbolCount);

            return result;
        }
    }
}
