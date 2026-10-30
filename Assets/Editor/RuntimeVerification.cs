using System; using System.IO; using System.Reflection; using UnityEditor; using UnityEditor.SceneManagement; using UnityEngine; using SlotGame;
[InitializeOnLoad]
public static class RuntimeVerification
{
    const string Running="SlotVerification.Running";
    const string SaveKey="slot.assignment.save.v1";
    static int stage;
    static double next;
    static SlotController game;
    sealed class FixedRandom : IRandomSource { public int Next(int count) => 3; }
    static RuntimeVerification() { EditorApplication.update += Tick; }
    public static void Run()
    {
        SessionState.SetString("SlotVerification.Backup", PlayerPrefs.GetString(SaveKey, ""));
        PlayerPrefs.DeleteKey(SaveKey);
        SessionState.SetBool(Running,true);
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        EditorApplication.isPlaying=true;
    }
    static void Require(bool condition,string message) { if(!condition) throw new Exception(message); }
    static void Capture(string name)
    {
        Canvas.ForceUpdateCanvases();
        var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();
        var target=new RenderTexture(1280,800,24);
        camera.targetTexture=target;camera.Render();RenderTexture.active=target;
        var texture=new Texture2D(1280,800,TextureFormat.RGB24,false);
        texture.ReadPixels(new Rect(0,0,1280,800),0,0);texture.Apply();
        Directory.CreateDirectory("Artifacts");File.WriteAllBytes("Artifacts/"+name+".png",texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=null;
        UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        try
        {
            if(stage==0)
            {
                game=UnityEngine.Object.FindFirstObjectByType<SlotController>();
                if(game==null||game.credits.text!="1,000")return;
                Require(new SaveStore().Load().credits==1000,"Initial credits");
                game.ShowHelp();Capture("payout-panel");game.CloseHelp();
                typeof(SlotController).GetField("outcomes",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(game,new OutcomeGenerator(new FixedRandom()));
                game.Spin();game.Spin();game.ChangeBet();
                Require(new SaveStore().Load().credits==990,"Duplicate spin deducted twice");
                Require(new SaveStore().Load().betIndex==0,"Bet changed while spinning");
                Require(!game.spinButton.interactable,"Spin not locked");
                stage=1;next=EditorApplication.timeSinceStartup+.6;
            }
            else if(stage==1){Capture("reels-in-motion");stage=2;next=EditorApplication.timeSinceStartup+4;}
            else if(stage==2)
            {
                Require(new SaveStore().Load().credits==1240,"Winning payout not credited exactly once");
                foreach(var reel in game.reels)Require(reel.Current==3,"Wrong landing symbol");
                Require(game.lastWin.text=="250","Wrong win label");
                Capture("winning-spin");
                game.ToggleMute();Require(new SaveStore().Load().muted,"Mute not persisted");
                game.AskReset();Capture("reset-confirmation");game.CancelReset();
                Require(new SaveStore().Load().credits==1240,"Cancel changed balance");
                game.AskReset();game.ConfirmReset();Require(new SaveStore().Load().credits==1000,"Reset failed");
                game.ChangeBet();Require(new SaveStore().Load().betIndex==1,"Bet not saved");
                PlayerPrefs.SetString(SaveKey,"broken json");Require(new SaveStore().Load().credits==1000,"Corrupt save not recovered");
                var wallet=new Wallet(5);Require(!wallet.Spend(10)&&wallet.Credits==5,"Insufficient credits");
                Finish(0,"PASS: repeated-click protection, bet lock, saved stake, exact winning payout, reel landing, mute, reset/cancel, saved bet, corrupt save, insufficient funds.");
            }
        }
        catch(Exception error){Finish(1,error.ToString());}
    }
    static void Finish(int code,string message)
    {
        Directory.CreateDirectory("Artifacts");File.WriteAllText("Artifacts/runtime-verification.txt",message);
        var backup=SessionState.GetString("SlotVerification.Backup","");
        if(backup=="")PlayerPrefs.DeleteKey(SaveKey);else PlayerPrefs.SetString(SaveKey,backup);
        PlayerPrefs.Save();SessionState.SetBool(Running,false);Debug.Log(message);EditorApplication.Exit(code);
    }
}
