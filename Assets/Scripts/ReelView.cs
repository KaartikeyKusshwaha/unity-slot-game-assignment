using System.Collections; using UnityEngine; using UnityEngine.UI;
namespace SlotGame {
 public class ReelView:MonoBehaviour {
 public Image[] cells; public Image glow; public int Current {get;private set;}
 public void Show(int symbol,Sprite[] sprites){Current=symbol; for(int i=0;i<cells.Length;i++){cells[i].sprite=sprites[(symbol+i+2)%4];cells[i].rectTransform.anchoredPosition=new Vector2(0,(2-i)*125);}}
 public IEnumerator Spin(int target,Sprite[] sprites,float duration){glow.color=Color.clear;float elapsed=0,travel=0;int steps=24+((target-Current+4)%4);int start=Current;
 while(elapsed<duration){elapsed+=Time.deltaTime;float t=Mathf.Clamp01(elapsed/duration); // Smoothstep integrates a velocity that starts and ends at zero.
 travel=steps*(t*t*(3-2*t));int whole=Mathf.FloorToInt(travel);float fraction=travel-whole;Show((start+whole)%4,sprites);foreach(var cell in cells)cell.rectTransform.anchoredPosition+=Vector2.up*(fraction*125);yield return null;}
 Show(target,sprites); }
 }
}
