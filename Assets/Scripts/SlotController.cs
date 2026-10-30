using System.Collections; using UnityEngine; using UnityEngine.UI;
namespace SlotGame {
 public class SlotController:MonoBehaviour {
 public SlotConfig config; public ReelView[] reels; public Text credits,bet,lastWin,status,muteLabel; public Button spinButton,betButton,resetButton,helpButton; public GameObject helpPanel,resetPanel; public GameAudio audioPlayer;
 readonly SaveStore store=new SaveStore(); SaveData data; Wallet wallet; OutcomeGenerator outcomes; bool spinning;
 void Start(){data=store.Load();wallet=new Wallet(data.credits);outcomes=new OutcomeGenerator(new RandomSource());for(int i=0;i<3;i++)reels[i].Show(i,config.symbols);audioPlayer.Mute(data.muted);Refresh();}
 void Save(){data.credits=wallet.Credits;store.Save(data);}
 void Refresh(){credits.text=wallet.Credits.ToString("N0");bet.text="BET  "+config.bets[data.betIndex];muteLabel.text=data.muted?"SOUND OFF":"SOUND ON";spinButton.interactable=!spinning&&wallet.Credits>=config.bets[data.betIndex];betButton.interactable=!spinning;resetButton.interactable=!spinning;helpButton.interactable=!spinning;}
 public void Spin(){if(spinning||helpPanel.activeSelf||resetPanel.activeSelf)return;int stake=config.bets[data.betIndex];if(!wallet.Spend(stake))return;spinning=true;Save();Refresh();StartCoroutine(Resolve(stake));}
 IEnumerator Resolve(int stake){int[] result=outcomes.Spin();status.text="Good luck. Reels in motion...";lastWin.text="0";audioPlayer.Play(audioPlayer.spin);for(int i=0;i<3;i++)StartCoroutine(AnimateReel(i,result[i]));yield return new WaitForSeconds(config.spinSeconds+2*config.stopDelay+.05f);int payout=PayoutEvaluator.Evaluate(result,stake,config.payouts);wallet.Award(payout);Save();lastWin.text=payout.ToString("N0");status.text=payout>0?"MATCH!  +"+payout+" CREDITS":"A new chance with every spin.";if(payout>0){audioPlayer.Play(audioPlayer.win);for(float t=0;t<.8f;t+=Time.deltaTime){foreach(var r in reels)r.glow.color=new Color(1,.73f,.25f,.2f+.15f*Mathf.Sin(t*18));yield return null;}}spinning=false;Refresh();if(!spinButton.interactable)status.text="Choose a smaller bet or reset your credits.";}
 IEnumerator AnimateReel(int index,int target){yield return reels[index].Spin(target,config.symbols,config.spinSeconds+index*config.stopDelay);audioPlayer.Play(audioPlayer.stop);}
 public void ChangeBet(){if(spinning)return;data.betIndex=(data.betIndex+1)%config.bets.Length;Save();Refresh();}
 public void ToggleMute(){data.muted=!data.muted;audioPlayer.Mute(data.muted);Save();Refresh();}
 public void ShowHelp(){helpPanel.SetActive(true);} public void CloseHelp(){helpPanel.SetActive(false);}
 public void AskReset(){if(!spinning)resetPanel.SetActive(true);} public void CancelReset(){resetPanel.SetActive(false);}
 public void ConfirmReset(){wallet=new Wallet(config.startingCredits);Save();resetPanel.SetActive(false);lastWin.text="0";status.text="Fresh credits. Ready when you are.";Refresh();}
 }
}
