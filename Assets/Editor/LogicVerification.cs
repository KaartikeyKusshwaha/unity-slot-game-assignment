using System;
using SlotGame;
using UnityEngine;

public static class LogicVerification
{
    public sealed class MemoryStore : IGameSaveStore
    {
        public string json = JsonUtility.ToJson(new SaveData());
        public SaveData Load() => JsonUtility.FromJson<SaveData>(json);
        public void Save(SaveData data)
        {
            json = JsonUtility.ToJson(data);
        }
    }
    public sealed class SequenceRandom : IRandomSource
    {
        readonly int[] sequence; int index;
        public SequenceRandom(params int[] sequence)
        {
            this.sequence = sequence;
        }
        public int Next(int count) => sequence[index++ % sequence.Length];
    }
    public static void Require(bool condition, string reason)
    {
        if (!condition)
            throw new Exception(reason);
    }
    public static void Run()
    {
        int[] bets = { 10, 25, 50 }, payouts = { 5, 10, 20, 25 };
        int wins = 0;
        for (int a = 0; a < 4; a++)
            for (int b = 0; b < 4; b++)
                for (int c = 0; c < 4; c++)
                {
                    int expected = a == b && b == c ? 10 * payouts[a] : 0;
                    Require(PayoutEvaluator.Evaluate(new[] { a, b, c }, 10, payouts) == expected, "Payout mismatch");
                    if (expected > 0)
                        wins++;
                    var store = new MemoryStore();
                    var session = new RoundSession(store, new SequenceRandom(a, b, c), bets, payouts);
                    Require(session.TryBegin() && !session.TryBegin(), "Duplicate begin");
                    Require(session.Data.credits == 990, "Stake accounting");
                    var resumed = new RoundSession(store, new SequenceRandom(0), bets, payouts);
                    Require(resumed.Data.pendingRound && resumed.Data.pendingPayout == expected && !resumed.TryBegin(), "Reload during spin");
                    if (expected > 0)
                    {
                        Require(resumed.Collect() == expected && resumed.Collect() == 0, "Duplicate collect");
                        var after = new RoundSession(store, new SequenceRandom(0), bets, payouts);
                        Require(after.Data.credits == 990 + expected && !after.Data.pendingRound && after.Collect() == 0, "Reload during collection");
                    }
                    else
                    {
                        resumed.FinishLoss();
                        Require(!store.Load().pendingRound && store.Load().credits == 990, "Loss finalization");
                    }
                }
        Require(wins == 4, "Exactly four winning combinations");
        var poor = new MemoryStore();
        poor.Save(new SaveData { credits = 5 });
        var insufficient = new RoundSession(poor, new SequenceRandom(0), bets, payouts);
        Require(!insufficient.TryBegin() && insufficient.Data.credits == 5, "Insufficient credits");
        foreach (int bet in bets)
            foreach (int symbol in new[] { 0, 1, 2, 3 })
                Require(PayoutEvaluator.Evaluate(new[] { symbol, symbol, symbol }, bet, payouts) == bet * payouts[symbol], "Stake multiplier");
        string backup = PlayerPrefs.GetString(SaveStore.Key, "");
        try
        {
            PlayerPrefs.SetString(SaveStore.Key, "{\"version\":1,\"credits\":777,\"betIndex\":2,\"muted\":true}");
            var migrated = new SaveStore().Load();
            Require(migrated.version == 2 && migrated.credits == 777 && migrated.betIndex == 2 && migrated.muted && migrated.volume == 1, "Save migration");
            PlayerPrefs.SetString(SaveStore.Key, "invalid json");
            Require(new SaveStore().Load().credits == 1000, "Corrupt save fallback");
        }
        finally { if (backup == "") PlayerPrefs.DeleteKey(SaveStore.Key); else PlayerPrefs.SetString(SaveStore.Key, backup); PlayerPrefs.Save(); }
        Debug.Log("LOGIC PASS: 64 outcomes, four wins, all stakes, duplicate locks, reload before reveal/collect/after collect, save migration and corrupt-save recovery.");
    }
}
