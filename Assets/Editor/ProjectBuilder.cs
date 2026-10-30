using System;
using System.IO;
using SlotGame;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class ProjectBuilder
{
    static Font font;
    static Sprite rounded,circle;
    static PresentationConfig presentation;
    static GameAudio audio;
    static Color gold=new Color(1,.79f,.35f), burgundy=new Color(.3f,.025f,.065f), ink=new Color(.055f,.02f,.045f);
    static RectTransform Rect(string name,Transform p,float x,float y,float w,float h)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;
    }
    static Image Pic(string name,Transform p,float x,float y,float w,float h,Color color,Sprite sprite=null)
    {
        var im=Rect(name,p,x,y,w,h).gameObject.AddComponent<Image>();im.sprite=sprite;im.color=color;im.raycastTarget=false;if(sprite==rounded)im.type=Image.Type.Sliced;return im;
    }
    static Text Label(string value,Transform p,float x,float y,float w,float h,int size,Color color)
    {
        var t=Rect(value,p,x,y,w,h).gameObject.AddComponent<Text>();t.text=value;t.font=font;t.fontSize=size;t.fontStyle=size>=20?FontStyle.Bold:FontStyle.Normal;t.alignment=TextAnchor.MiddleCenter;t.color=color;t.raycastTarget=false;return t;
    }
    static Button Button(string text,Transform p,float x,float y,float w,float h,UnityEngine.Events.UnityAction action,bool primary=false)
    {
        var im=Pic(text,p,x,y,w,h,primary?gold:new Color(.55f,.31f,.15f),rounded);
        Pic("Button face",im.transform,0,0,w-4,h-4,primary?new Color(.66f,.045f,.11f):burgundy,rounded);im.raycastTarget=true;
        var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;var c=b.colors;c.highlightedColor=new Color(1,1,.8f);c.disabledColor=new Color(.42f,.4f,.4f,.7f);b.colors=c;
        UnityEventTools.AddPersistentListener(b.onClick,action);Label(text,im.transform,0,0,w-10,h,primary?26:21,Color.white);
        var motion=im.gameObject.AddComponent<ButtonMotion>();motion.config=presentation;motion.audioPlayer=audio;return b;
    }
    static CanvasGroup Overlay(string name,Transform p)
    {
        var im=Pic(name,p,0,0,1280,800,new Color(.025f,.008f,.02f,.94f));im.raycastTarget=true;return im.gameObject.AddComponent<CanvasGroup>();
    }
    static void Card(Transform p,float w,float h)
    {
        Pic("Gold border",p,0,0,w,h,gold,rounded);Pic("Burgundy panel",p,0,0,w-4,h-4,burgundy,rounded);Pic("Inner panel",p,0,0,w-20,h-20,ink,rounded);
    }
    static Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/"+name+".png");
    static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/"+name+".wav");
    static T Upsert<T>(T asset,string path) where T:UnityEngine.Object
    {
        var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(asset,path);return asset;
    }
    static Sprite Shape(bool round)
    {
        string name=round?"Circle":"RoundedPanel";string path="Assets/UI/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old)return old;
        var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);tex.name=name;tex.filterMode=FilterMode.Bilinear;
        for(int y=0;y<64;y++)for(int x=0;x<64;x++)
        {
            float dx=Mathf.Max(0,Mathf.Max(12-x,x-51)),dy=Mathf.Max(0,Mathf.Max(12-y,y-51));
            float d=round?Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32))-30:Mathf.Sqrt(dx*dx+dy*dy)-12;
            tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(-d)));
        }
        tex.Apply();AssetDatabase.CreateAsset(tex,"Assets/UI/"+name+"Texture.asset");
        var sprite=Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,round?Vector4.zero:new Vector4(14,14,14,14));AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    static Sprite Crop(string source,Rect rect,string name)
    {
        string path="Assets/UI/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old)return old;
        var sprite=Sprite.Create(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UI/"+source+".png"),rect,new Vector2(.5f,.5f),100);AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    public static void Create()
    {
        foreach(string path in Directory.GetFiles("Assets/UI","*.png")){var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.alphaIsTransparency=true;i.mipmapEnabled=false;i.SaveAndReimport();}
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");rounded=Shape(false);circle=Shape(true);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ink;camera.transform.position=new Vector3(0,0,-10);camera.tag="MainCamera";
        var canvas=new GameObject("Game Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,800);scaler.matchWidthOrHeight=.5f;new GameObject("Event System",typeof(EventSystem),typeof(StandaloneInputModule));
        var asset=ScriptableObject.CreateInstance<SlotConfig>();asset.symbols=new[]{Art("slot-symbol1"),Art("slot-symbol2"),Art("slot-symbol3"),Art("slot-symbol4")};var config=Upsert(asset,"Assets/Resources/GameConfig.asset");presentation=Upsert(ScriptableObject.CreateInstance<PresentationConfig>(),"Assets/Resources/Presentation.asset");
        var game=new GameObject("Game Flow").AddComponent<SlotController>();game.config=config;game.presentation=presentation;
        audio=game.gameObject.AddComponent<GameAudio>();game.audioPlayer=audio;audio.source=game.gameObject.AddComponent<AudioSource>();audio.source.playOnAwake=false;audio.loopSource=game.gameObject.AddComponent<AudioSource>();audio.loopSource.loop=true;audio.loopSource.playOnAwake=false;
        audio.click=Clip("click");audio.entry=Clip("entry");audio.lever=Clip("lever");audio.spin=Clip("spin");audio.stop=Clip("stop");audio.win=Clip("win");audio.collect=Clip("collect");audio.loopSource.clip=Clip("rumble");
        var face=Crop("slot-machine4",new Rect(137,0,550,624),"CabinetFace");
        var scenery=Rect("Ambient scenery",canvas.transform,0,0,1500,1100);
        Pic("Room",scenery,0,0,1600,1100,new Color(.08f,.012f,.035f));Pic("Pattern",scenery,0,30,1500,1100,new Color(.36f,.1f,.22f,.24f),Art("bg_gradient"));
        for(int side=-1;side<=1;side+=2){Pic("Distant cabinet",scenery,side*535,-35,330,390,new Color(.25f,.12f,.22f,.45f),face);for(int n=0;n<6;n++)Pic("Velvet fold",scenery,side*(600-n*17),45,20,1000,new Color(.12f+n*.008f,.012f,.035f,.85f));Pic("Floor light",scenery,side*400,-282,330,5,new Color(.8f,.32f,.08f,.3f));}
        Pic("Floor",canvas.transform,0,-375,1500,120,new Color(.025f,.008f,.02f));Pic("Shadow",canvas.transform,0,-304,600,26,new Color(0,0,0,.6f),rounded);
        var cabinet=Rect("Cabinet assembly",canvas.transform,0,-20,550,624);cabinet.localScale=Vector3.one*.88f;Pic("Cabinet body",cabinet,0,0,550,624,Color.white,face);Pic("Nameplate",cabinet,0,-280,450,43,burgundy,rounded);Label("THE LUCKY LINE",cabinet,0,-280,430,38,24,gold);
        game.reels=new ReelView[3];for(int n=0;n<3;n++)
        {
            var panel=Pic("Reel "+(n+1),cabinet,-128+n*130,-38,108,174,new Color(.94f,.95f,.87f));panel.gameObject.AddComponent<RectMask2D>();var reel=panel.gameObject.AddComponent<ReelView>();reel.cells=new Image[5];for(int j=0;j<5;j++){reel.cells[j]=Pic("Symbol "+j,panel.transform,0,(2-j)*125,94,106,Color.white,config.symbols[(n+j)%4]);reel.cells[j].preserveAspect=true;}reel.glow=Pic("Reveal highlight",panel.transform,0,0,108,174,Color.clear);reel.Show(n,config.symbols);game.reels[n]=reel;
            if(n==0){PrefabUtility.SaveAsPrefabAsset(reel.cells[2].gameObject,"Assets/Prefabs/Symbol.prefab");PrefabUtility.SaveAsPrefabAsset(panel.gameObject,"Assets/Prefabs/Reel.prefab");}
        }
        Label(">",cabinet,-213,-38,25,35,24,gold);Label("<",cabinet,216,-38,25,35,24,gold);
        Pic("Lever bracket",cabinet,281,-152,48,56,new Color(.4f,.23f,.08f),rounded);Pic("Lever hub rim",cabinet,303,-152,54,54,gold,circle);Pic("Lever hub",cabinet,303,-152,40,40,new Color(.14f,.08f,.04f),circle);
        var pivot=Rect("Lever pivot",cabinet,303,-152,60,170);var lever=Pic("Pull lever",pivot,0,84,57,168,Color.white,Crop("slot-machine2",new Rect(673,58,92,270),"LeverTexture"));lever.preserveAspect=true;lever.raycastTarget=true;game.leverButton=lever.gameObject.AddComponent<Button>();game.leverButton.targetGraphic=lever;UnityEventTools.AddPersistentListener(game.leverButton.onClick,game.Spin);Label("PULL",cabinet,370,-134,110,35,22,gold);Label("or SPACE",cabinet,370,-163,120,25,16,Color.white);
        var effects=game.gameObject.AddComponent<CasinoEffects>();game.effects=effects;effects.config=presentation;effects.ambientLayer=scenery;effects.lights=new Image[34];for(int n=0;n<34;n++)effects.lights[n]=Pic("Marquee bulb "+n,cabinet,-248+(n%17)*31,n<17?303:-307,8,8,gold,circle);
        effects.sideLights=new Image[2];for(int n=0;n<2;n++)effects.sideLights[n]=Pic("Side sweep "+n,canvas.transform,n==0?-540:540,0,160,600,Color.clear,rounded);
        var hud=Rect("Game HUD",canvas.transform,0,0,1280,800);game.hud=hud.gameObject;Pic("HUD background",hud,0,336,1210,104,new Color(.045f,.012f,.025f,.97f),rounded);
        Label("BALANCE",hud,-387,363,240,27,17,gold);game.credits=Label("1,000",hud,-387,325,280,48,36,Color.white);effects.balanceTarget=game.credits.rectTransform;game.betButton=Button("BET  10",hud,0,337,245,58,game.ChangeBet);game.bet=game.betButton.GetComponentInChildren<Text>();Label("LAST WIN",hud,387,363,240,27,17,gold);game.lastWin=Label("0",hud,387,325,260,48,32,Color.white);
        Pic("Status backdrop",hud,0,-335,760,48,ink,rounded);game.status=Label("PULL THE LEVER  /  PRESS SPACE",hud,0,-335,740,46,22,gold);game.helpButton=Button("PAYOUTS",hud,-400,-375,180,42,game.ShowHelp);game.resetButton=Button("RESET",hud,0,-375,180,42,game.AskReset);game.menuButton=Button("MENU",hud,400,-375,180,42,game.MainMenu);
        var menu=Overlay("Main menu",canvas.transform);game.menuGroup=menu;menu.GetComponent<Image>().color=new Color(.025f,.008f,.02f,.86f);Pic("Title rule",menu.transform,0,232,600,2,gold);Label("THE LUCKY LINE",menu.transform,0,156,1100,90,66,gold);Label("THREE REELS. ONE PERFECT MATCH.",menu.transform,0,83,1000,40,23,Color.white);Button("PLAY",menu.transform,0,-15,350,80,game.Play,true);Button("SETTINGS",menu.transform,0,-116,350,62,game.ShowSettings);Button("EXIT",menu.transform,0,-200,350,62,game.Exit);Label("Pull the lever. Match three. Collect your reward.",menu.transform,0,-288,1050,36,22,new Color(.86f,.77f,.65f));Label("SIMULATED CREDITS  /  NO REAL MONEY",menu.transform,0,-345,1000,28,16,new Color(.75f,.65f,.53f));
        var reward=Overlay("Reward presentation",canvas.transform);reward.GetComponent<Image>().color=new Color(.025f,.008f,.02f,.7f);effects.rewardOverlay=reward.gameObject;effects.rewardGroup=reward;effects.particles=new RectTransform[32];for(int n=0;n<32;n++)effects.particles[n]=Pic("Reward particle "+n,reward.transform,0,0,17,17,n%3==0?gold:(n%3==1?new Color(1,.15f,.3f):new Color(.2f,.85f,1)),circle).rectTransform;
        var card=Rect("Reward card",reward.transform,0,0,690,450);effects.rewardCard=card;Card(card,690,450);card.Find("Inner panel").GetComponent<Image>().color=new Color(.36f,.012f,.055f);Label("BINGO!",card,0,155,630,76,58,gold);game.rewardLabel=Label("Congratulations - you won",card,0,92,640,42,25,Color.white);effects.rewardAmount=Label("+250",card,0,5,620,110,86,gold);Label("CREDITS",card,0,-65,600,34,22,Color.white);game.collectButton=Button("COLLECT REWARD",card,0,-152,470,70,game.CollectReward,true);reward.gameObject.SetActive(false);
        var help=Overlay("Payout help",canvas.transform);game.helpPanel=help.gameObject;Card(help.transform,830,640);Label("THE PAYOUTS",help.transform,0,242,760,60,38,gold);for(int n=0;n<4;n++){Pic("Payout symbol",help.transform,-220,145-n*72,48,54,Color.white,config.symbols[n]).preserveAspect=true;Label("3 MATCHING    "+config.payouts[n]+" x BET",help.transform,65,145-n*72,440,52,25,Color.white);}Label("Only the centre line pays. Payout is the total credits awarded.\nEach symbol has an equal 1 in 4 chance per reel.",help.transform,0,-160,780,70,19,Color.white);Button("BACK",help.transform,0,-253,300,56,game.CloseHelp);help.gameObject.SetActive(false);
        var reset=Overlay("Reset confirmation",canvas.transform);game.resetPanel=reset.gameObject;Card(reset.transform,790,350);Label("START FRESH?",reset.transform,0,100,710,60,38,gold);Label("Replace your balance with 1,000 credits.",reset.transform,0,18,720,50,24,Color.white);Button("RESET",reset.transform,-175,-88,260,60,game.ConfirmReset,true);Button("CANCEL",reset.transform,175,-88,260,60,game.CancelReset);reset.gameObject.SetActive(false);
        var exit=Overlay("Session finished",canvas.transform);game.exitPanel=exit.gameObject;Card(exit.transform,790,350);Label("THANKS FOR PLAYING",exit.transform,0,80,730,70,36,gold);Label("Your credits and settings are saved.",exit.transform,0,4,720,45,23,Color.white);Button("RETURN TO MENU",exit.transform,0,-90,400,62,game.ReturnFromExit);exit.gameObject.SetActive(false);
        var settings=Overlay("Settings",canvas.transform);game.settingsPanel=settings.gameObject;Card(settings.transform,830,520);Label("SETTINGS",settings.transform,0,194,760,60,40,gold);Label("MASTER VOLUME",settings.transform,0,112,700,40,22,Color.white);
        var sliderRoot=Rect("Volume slider",settings.transform,0,45,570,44);var track=Pic("Track",sliderRoot,0,0,570,10,new Color(.3f,.15f,.15f),rounded);track.raycastTarget=true;var knob=Pic("Volume handle",sliderRoot,0,0,30,38,gold,rounded);knob.raycastTarget=true;var slider=sliderRoot.gameObject.AddComponent<Slider>();slider.minValue=0;slider.maxValue=1;slider.value=1;slider.handleRect=knob.rectTransform;slider.targetGraphic=knob;game.volumeSlider=slider;UnityEventTools.AddPersistentListener(slider.onValueChanged,game.SetVolume);
        game.motionButton=Button("REDUCED MOTION: OFF",settings.transform,0,-53,590,60,game.ToggleMotion);game.motionLabel=game.motionButton.GetComponentInChildren<Text>();Button("BACK",settings.transform,0,-169,320,62,game.CloseSettings);settings.gameObject.SetActive(false);
        var mute=Button("SOUND ON",canvas.transform,485,262,200,40,game.ToggleMute);game.muteLabel=mute.GetComponentInChildren<Text>();Button("AUDIO",canvas.transform,-485,262,180,40,game.ShowSettings);
        var view=game.gameObject.AddComponent<CabinetView>();game.view=view;view.cabinet=cabinet;view.lever=pivot;view.config=presentation;view.transitionFade=Pic("Transition fade",canvas.transform,0,0,1280,800,Color.clear);hud.gameObject.SetActive(false);
        PlayerSettings.companyName="KaartikeyKusshwaha";PlayerSettings.productName="The Lucky Line";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Main.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Main.unity",true)};AssetDatabase.SaveAssets();VerifyLogic();Render();
    }
    public static void CreateAndVerify(){Create();RuntimeVerification.Run();}
    public static void VerifyLogic(){LogicVerification.Run();}
    public static void BuildWeb(){VerifyLogic();var r=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Build/WebGL",BuildTarget.WebGL,BuildOptions.None);if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Web build failed");WebBuildPresentation.Apply();}
    public static void Render(){EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");Capture("first-screen");}
    public static void Capture(string name)
    {
        var camera=UnityEngine.Object.FindAnyObjectByType<Camera>();Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1280,800,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,800),0,0);image.Apply();Directory.CreateDirectory("Artifacts");File.WriteAllBytes("Artifacts/"+name+".png",image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
    }
}
