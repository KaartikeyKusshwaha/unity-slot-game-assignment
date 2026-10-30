using System;
using UnityEngine;

namespace SlotGame
{
    [Serializable]
    public class SaveData
    {
        public int version = 2;
        public int credits = 1000;
        public int betIndex;
        public bool muted;
        public float volume = 1;
        public bool reducedMotion;
        public int lastWin;
        public bool pendingRound;
        public int[] result;
        public int roundStake;
        public int pendingPayout;
    }

    public interface IGameSaveStore
    {
        SaveData Load();
        void Save(SaveData data);
    }

    public sealed class SaveStore : IGameSaveStore
    {
        public const string Key = "slot.assignment.save.v1";

        public SaveData Load()
        {
            try
            {
                string raw = PlayerPrefs.GetString(Key, "");
                if (string.IsNullOrEmpty(raw))
                    return new SaveData();
                var data = JsonUtility.FromJson<SaveData>(raw);
                if (data == null || data.credits < 0 || data.betIndex < 0 || data.betIndex > 2)
                    return new SaveData();
                if (data.version == 1)
                {
                    // Version 1 had no safe pending-round record, so keep settings and balance only.
                    data.version = 2;
                    data.volume = 1;
                    data.pendingRound = false;
                    Save(data);
                }
                if (data.version != 2)
                    return new SaveData();
                data.volume = float.IsNaN(data.volume) ? 1 : Mathf.Clamp01(data.volume);
                if (data.pendingRound && !ValidRound(data))
                {
                    // Preserve a valid wallet when recovering a damaged pending record.
                    data.pendingRound = false;
                    data.pendingPayout = 0;
                    data.result = null;
                }
                return data;
            }
            catch (Exception)
            {
                return new SaveData();
            }
        }

        private static bool ValidRound(SaveData data)
        {
            // A pending outcome must contain one valid symbol index for each of the three reels.
            if (data.result == null || data.result.Length != 3 || data.roundStake <= 0 || data.pendingPayout < 0)
                return false;
            foreach (int symbol in data.result)
                if (symbol < 0 || symbol >= 4)
                    return false;
            return true;
        }

        public void Save(SaveData data)
        {
            // One JSON record holds both the wallet and pending round, never separate keys.
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            // Flush immediately so a reload cannot refund a stake or award a collected win twice.
            PlayerPrefs.Save();
        }
    }
}
