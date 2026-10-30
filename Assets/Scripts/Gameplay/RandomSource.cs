using System;

namespace SlotGame
{
    /// <summary>Production random source used to choose reel symbols.</summary>
    public sealed class RandomSource : IRandomSource
    {
        private readonly Random random = new Random();

        public int Next(int count)
        {
            return random.Next(count);
        }
    }
}
