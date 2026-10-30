using System;

namespace SlotGame
{
    /// <summary>Owns accounting and pending-round persistence, independent of animation.</summary>
    public sealed class RoundSession
    {
        public SaveData Data
        {
            get;
        }
        private readonly IGameSaveStore store;
        private readonly OutcomeGenerator outcomes;
        private readonly int[] bets;
        private readonly int[] payouts;
        private readonly Wallet wallet;

        public RoundSession(IGameSaveStore store, IRandomSource random, int[] bets, int[] payouts)
        {
            this.store = store;
            this.bets = bets;
            this.payouts = payouts;
            outcomes = new OutcomeGenerator(random);
            Data = store.Load();
            wallet = new Wallet(Data.credits);
            // The result and stake are the saved round facts. Recompute the payout so a stale
            // derived amount cannot change what the player receives after a reload.
            if (Data.pendingRound)
            {
                if (Array.IndexOf(bets, Data.roundStake) < 0)
                {
                    ClearRound();
                    Save();
                }
                else
                    Data.pendingPayout = PayoutEvaluator.Evaluate(Data.result, Data.roundStake, payouts);
            }
        }

        public bool TryBegin()
        {
            if (Data.pendingRound || Data.credits < bets[Data.betIndex])
                return false;

            // Save the selected outcome with the deducted stake before the controller starts animation.
            int[] result = outcomes.Spin();
            int stake = bets[Data.betIndex];
            if (!wallet.Spend(stake))
                return false;
            Data.credits = wallet.Credits;
            Data.result = result;
            Data.roundStake = stake;
            Data.pendingPayout = PayoutEvaluator.Evaluate(result, stake, payouts);
            Data.lastWin = 0;
            Data.pendingRound = true;
            Save();
            return true;
        }

        public int Collect()
        {
            if (!Data.pendingRound || Data.pendingPayout <= 0)
                return 0;
            int reward = Data.pendingPayout;
            wallet.Award(reward);
            Data.credits = wallet.Credits;
            Data.lastWin = reward;
            ClearRound();
            // Award and clear the pending round in the same save before coin animation can start.
            Save();
            return reward;
        }

        public void FinishLoss()
        {
            if (!Data.pendingRound || Data.pendingPayout != 0)
                return;
            ClearRound();
            Save();
        }

        private void ClearRound()
        {
            Data.pendingRound = false;
            Data.pendingPayout = 0;
            Data.result = null;
            Data.roundStake = 0;
        }

        public void ChangeBet(int direction = 1)
        {
            if (Data.pendingRound)
                return;
            Data.betIndex = (Data.betIndex + direction + bets.Length) % bets.Length;
            Save();
        }

        public void Reset(int startingCredits)
        {
            if (Data.pendingRound)
                return;

            // Use the same wallet methods as normal play so credit changes have one implementation.
            if (wallet.Credits > 0)
                wallet.Spend(wallet.Credits);
            wallet.Award(startingCredits);
            Data.credits = wallet.Credits;
            Data.lastWin = 0;
            Save();
        }

        public void Save() => store.Save(Data);
    }
}
