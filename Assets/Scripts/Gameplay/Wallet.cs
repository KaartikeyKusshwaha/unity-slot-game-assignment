using System;

namespace SlotGame
{
    /// <summary>Applies credit changes and prevents negative balances.</summary>
    public sealed class Wallet
    {
        public int Credits
        {
            get; private set;
        }

        public Wallet(int credits)
        {
            Credits = Math.Max(0, credits);
        }

        public bool Spend(int amount)
        {
            if (amount <= 0 || Credits < amount)
                return false;

            Credits -= amount;
            return true;
        }

        public void Award(int amount)
        {
            Credits = checked(Credits + Math.Max(0, amount));
        }
    }
}
