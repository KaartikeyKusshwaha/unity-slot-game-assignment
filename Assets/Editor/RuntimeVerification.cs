using System;
using System.IO;
using System.Reflection;
using SlotGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class RuntimeVerification
{
    const string Running="SlotVerification.Running";
    static int stage;static double deadline;static SlotController game;
    static RuntimeVerification(){EditorApplication.update+=Tick;}
    public static void Run()
    {
        SessionState.SetString("SlotVerification.Backup",PlayerPrefs.GetString(SaveStore.Key,""));
        PlayerPrefs.DeleteKey(SaveStore.Key);SessionState.SetBool(Running,true);
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");EditorApplication.isPlaying=true;
    }
    static void Assert(bool condition,string reason)=>LogicVerification.Require(condition,reason);
    static void Choose(params int[] symbols)
    {
        var session=new RoundSession(new SaveStore(),new LogicVerification.SequenceRandom(symbols),game.config.bets,game.config.payouts);
        typeof(SlotController).GetField("session",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(game,session);
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying)return;
        try
        {
            if(deadline==0)deadline=EditorApplication.timeSinceStartup+75;
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Runtime verification timed out at stage "+stage);
            game=UnityEngine.Object.FindAnyObjectByType<SlotController>();if(game==null||game.Session==null)return;
            if(stage==0)
            {
                Assert(game.State==GameState.Menu,"Starts at menu");ProjectBuilder.Capture("menu");Choose(3,3,3);game.Play();game.Play();stage=1;
            }
            else if(stage==1&&game.State==GameState.Ready)
            {
                ProjectBuilder.Capture("standing-view");game.leverButton.onClick.Invoke();game.Spin();game.ChangeBet();game.MainMenu();
                Assert(game.Session.Data.credits==990&&game.Session.Data.betIndex==0&&!game.leverButton.interactable,"Input lock");stage=2;
            }
            else if(stage==2&&game.State==GameState.ZoomingIn){ProjectBuilder.Capture("lever-zoom");stage=3;}
            else if(stage==3&&game.State==GameState.Spinning){ProjectBuilder.Capture("reel-closeup");stage=4;}
            else if(stage==4&&game.State==GameState.AwaitingCollect)
            {
                Assert(game.Session.Data.credits==990&&game.Session.Data.pendingPayout==250,"Pending reward remains uncredited");
                foreach(var reel in game.reels)Assert(reel.Current==3,"Exact reel landing");
                ProjectBuilder.Capture("collect-reward");game.CollectReward();game.CollectReward();Assert(game.Session.Data.credits==1240&&!game.Session.Data.pendingRound,"Collect exactly once");stage=5;
            }
            else if(stage==5&&game.State==GameState.Ready)
            {
                Assert(Vector2.Distance(game.view.cabinet.anchoredPosition,game.view.standingPosition)<.01f,"Standing position restored");ProjectBuilder.Capture("collected-standing");
                game.ToggleMute();Assert(new SaveStore().Load().muted,"Saved mute");game.SetVolume(.35f);Assert(Mathf.Approximately(new SaveStore().Load().volume,.35f),"Saved volume");Choose(0,1,2);game.Spin();stage=6;
            }
            else if(stage==6&&game.State==GameState.Ready)
            {
                Assert(game.Session.Data.credits==1230&&!game.Session.Data.pendingRound,"Automatic loss return");ProjectBuilder.Capture("loss-return");
                game.ToggleMotion();Assert(game.Session.Data.reducedMotion,"Reduced motion");game.Spin();stage=7;
            }
            else if(stage==7&&game.State==GameState.Ready)
            {
                Assert(game.Session.Data.credits==1220&&!game.Session.Data.pendingRound,"Reduced-motion loss flow");
                game.ConfirmReset();Assert(game.Session.Data.credits==1000,"Reset");game.MainMenu();stage=8;
            }
            else if(stage==8&&game.State==GameState.Menu)
            {
                Finish(0,"RUNTIME PASS: menu/play, attached lever, input locks, zoom, exact reel landing, pending payout, collect once, return transform, loss auto-return, reduced motion, saved volume/mute, reset and menu.");
            }
        }
        catch(Exception error){Finish(1,error.ToString());}
    }
    static void Finish(int code,string report)
    {
        Directory.CreateDirectory("Artifacts");File.WriteAllText("Artifacts/runtime-verification.txt",report);
        string backup=SessionState.GetString("SlotVerification.Backup","");if(backup=="")PlayerPrefs.DeleteKey(SaveStore.Key);else PlayerPrefs.SetString(SaveStore.Key,backup);
        PlayerPrefs.Save();SessionState.SetBool(Running,false);Debug.Log(report);EditorApplication.Exit(code);
    }
}
