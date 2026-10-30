using System; using UnityEngine;
namespace SlotGame {
 [Serializable] public class SaveData { public int version=1; public int credits=1000; public int betIndex; public bool muted; }
 public sealed class SaveStore {
 const string Key="slot.assignment.save.v1";
 public SaveData Load(){try{var raw=PlayerPrefs.GetString(Key,"");if(string.IsNullOrEmpty(raw))return new SaveData();var d=JsonUtility.FromJson<SaveData>(raw);if(d==null||d.version!=1||d.credits<0||d.credits>100000000||d.betIndex<0||d.betIndex>2) return new SaveData();return d;}catch{return new SaveData();}}
 public void Save(SaveData data){PlayerPrefs.SetString(Key,JsonUtility.ToJson(data));PlayerPrefs.Save();}
 }
}
