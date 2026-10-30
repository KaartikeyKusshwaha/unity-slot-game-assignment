using System;
using UnityEngine;

namespace SlotGame
{
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int credits = 1000;
        public int betIndex;
        public bool muted;
    }

    public sealed class SaveStore
    {
        private const string Key = "slot.assignment.save.v1";

        public SaveData Load()
        {
            try
            {
                string raw = PlayerPrefs.GetString(Key, "");
                if (string.IsNullOrEmpty(raw))
                    return new SaveData();

                var data = JsonUtility.FromJson<SaveData>(raw);
                // Treat invalid or incompatible storage as a new local session.
                if (data == null || data.version != 1 || data.credits < 0 ||
                    data.credits > 100000000 || data.betIndex < 0 || data.betIndex > 2)
                    return new SaveData();

                return data;
            }
            catch
            {
                return new SaveData();
            }
        }

        public void Save(SaveData data)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
