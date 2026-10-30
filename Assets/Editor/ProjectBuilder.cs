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
    static Sprite rounded, circle;
    static PresentationConfig presentation;
    static GameAudio audio;
    static Color gold = new Color(1, .79f, .35f), burgundy = new Color(.3f, .025f, .065f), ink = new Color(.055f, .02f, .045f);
    static RectTransform Rect(string name, Transform p, float x, float y, float w, float h)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(p, false);
        r.anchoredPosition = new Vector2(x, y);
        r.sizeDelta = new Vector2(w, h);
        return r;
    }
    static Image Pic(string name, Transform p, float x, float y, float w, float h, Color color, Sprite sprite = null)
    {
        var im = Rect(name, p, x, y, w, h).gameObject.AddComponent<Image>();
        im.sprite = sprite;
        im.color = color;
        im.raycastTarget = false;
        if (sprite == rounded)
            im.type = Image.Type.Sliced;
        return im;
    }
    static Text Label(string value, Transform p, float x, float y, float w, float h, int size, Color color)
    {
        var t = Rect(value, p, x, y, w, h).gameObject.AddComponent<Text>();
        t.text = value;
        t.font = font;
        t.fontSize = size;
        t.fontStyle = size >= 20 ? FontStyle.Bold : FontStyle.Normal;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.raycastTarget = false;
        if (size >= 38)
        {
            var shadow = t.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(.12f, .045f, .015f, .9f);
            shadow.effectDistance = new Vector2(0, -4);
            var edge = t.gameObject.AddComponent<Outline>();
            edge.effectColor = new Color(.2f, .075f, .015f, .7f);
            edge.effectDistance = new Vector2(1, -1);
        }
        return t;
    }
    static Button Button(string text, Transform p, float x, float y, float w, float h, UnityEngine.Events.UnityAction action, bool primary = false)
    {
        var im = Pic(text, p, x, y, w, h, Color.white, SlicedArt("slot_machine_Middle_box", new Rect(51, 39, 555, 199), "SuppliedButtonPanel", new Vector4(28, 30, 28, 30)));
        im.type = Image.Type.Simple;
        im.raycastTarget = true;
        var b = im.gameObject.AddComponent<Button>();
        b.targetGraphic = im;
        var c = b.colors;
        c.highlightedColor = new Color(1, 1, .8f);
        c.disabledColor = new Color(.42f, .4f, .4f, .7f);
        b.colors = c;
        UnityEventTools.AddPersistentListener(b.onClick, action);
        Label(text, im.transform, 0, 0, w - 10, h, primary ? 26 : 21, Color.white);
        var motion = im.gameObject.AddComponent<ButtonMotion>();
        motion.config = presentation;
        motion.audioPlayer = audio;
        return b;
    }
    static CanvasGroup Overlay(string name, Transform p)
    {
        var im = Pic(name, p, 0, 0, 1280, 800, new Color(.025f, .008f, .02f, .94f));
        im.raycastTarget = true;
        return im.gameObject.AddComponent<CanvasGroup>();
    }
    static void Card(Transform p, float w, float h)
    {
        var frame = Pic("Supplied popup frame", p, 0, 0, w, h, Color.white, SlicedArt("popup", new Rect(89, 72, 1551, 729), "SuppliedPopup", new Vector4(72, 72, 72, 72)));
        frame.type = Image.Type.Sliced;
    }
    static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/" + name + ".png");
    static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/" + name + ".wav");
    static T Upsert<T>(T asset, string path) where T : UnityEngine.Object
    {
        var old = AssetDatabase.LoadAssetAtPath<T>(path);
        if (old)
        {
            EditorUtility.CopySerialized(asset, old);
            UnityEngine.Object.DestroyImmediate(asset);
            EditorUtility.SetDirty(old);
            return old;
        }
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
    static Sprite Shape(bool round)
    {
        string name = round ? "Circle" : "RoundedPanel";
        string path = "Assets/UI/" + name + ".asset";
        var old = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (old)
            return old;
        var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        tex.name = name;
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float dx = Mathf.Max(0, Mathf.Max(12 - x, x - 51)), dy = Mathf.Max(0, Mathf.Max(12 - y, y - 51));
                float d = round ? Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(32, 32)) - 30 : Mathf.Sqrt(dx * dx + dy * dy) - 12;
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(-d)));
            }
        tex.Apply();
        AssetDatabase.CreateAsset(tex, "Assets/UI/" + name + "Texture.asset");
        var sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, round ? Vector4.zero : new Vector4(14, 14, 14, 14));
        AssetDatabase.CreateAsset(sprite, path);
        return sprite;
    }
    static Sprite Crop(string source, Rect rect, string name)
    {
        string path = "Assets/UI/" + name + ".asset";
        var old = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (old)
            return old;
        var sprite = Sprite.Create(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UI/" + source + ".png"), rect, new Vector2(.5f, .5f), 100);
        AssetDatabase.CreateAsset(sprite, path);
        return sprite;
    }
    static Sprite SlicedArt(string source, Rect rect, string name, Vector4 border)
    {
        string path = "Assets/UI/" + name + ".asset";
        var old = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (old)
            return old;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UI/" + source + ".png");
        var sprite = Sprite.Create(texture, rect, new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, border);
        AssetDatabase.CreateAsset(sprite, path);
        return sprite;
    }
    static Button AtlasButton(string name, Transform parent, float x, float y, float size, string sheet, UnityEngine.Events.UnityAction action)
    {
        var image = Pic(name, parent, x, y, size, size, Color.white, Crop(sheet, new Rect(0, 768, 256, 256), sheet + "Normal"));
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = new SpriteState { highlightedSprite = Crop(sheet, new Rect(0, 512, 256, 256), sheet + "Hover"), pressedSprite = Crop(sheet, new Rect(0, 256, 256, 256), sheet + "Pressed"), disabledSprite = Crop(sheet, new Rect(0, 0, 256, 256), sheet + "Disabled") };
        UnityEventTools.AddPersistentListener(button.onClick, action);
        var motion = button.gameObject.AddComponent<ButtonMotion>();
        motion.config = presentation;
        motion.audioPlayer = audio;
        return button;
    }
    static Button ConfirmButton(bool yes, Transform parent, float x, float y, UnityEngine.Events.UnityAction action)
    {
        int left = yes ? 70 : 549;
        string label = yes ? "Yes" : "No";
        var image = Pic(label, parent, x, y, 260, 94, Color.white, Crop("Yes_No_Btn", new Rect(left, 465, 370, 142), label + "Normal"));
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = new SpriteState { highlightedSprite = Crop("Yes_No_Btn", new Rect(left, 276, 370, 139), label + "Hover"), pressedSprite = Crop("Yes_No_Btn", new Rect(left, 84, 370, 141), label + "Pressed") };
        UnityEventTools.AddPersistentListener(button.onClick, action);
        var motion = button.gameObject.AddComponent<ButtonMotion>();
        motion.config = presentation;
        motion.audioPlayer = audio;
        return button;
    }
    public static void Create()
    {
        foreach (string path in Directory.GetFiles("Assets/UI", "*.png"))
        {
            var i = (TextureImporter)AssetImporter.GetAtPath(path);
            i.textureType = TextureImporterType.Sprite;
            i.spriteImportMode = SpriteImportMode.Single;
            i.alphaIsTransparency = true;
            i.mipmapEnabled = false;
            i.SaveAndReimport();
        }
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rounded = Shape(false);
        circle = Shape(true);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = ink;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.tag = "MainCamera";
        var canvas = new GameObject("Game Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 10;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 800);
        scaler.matchWidthOrHeight = .5f;
        new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
        var asset = ScriptableObject.CreateInstance<SlotConfig>();
        asset.symbols = new[] { Art("slot-symbol1"), Art("slot-symbol2"), Art("slot-symbol3"), Art("slot-symbol4") };
        var config = Upsert(asset, "Assets/Resources/GameConfig.asset");
        presentation = Upsert(ScriptableObject.CreateInstance<PresentationConfig>(), "Assets/Resources/Presentation.asset");
        var game = new GameObject("Game Flow").AddComponent<SlotController>();
        game.config = config;
        game.presentation = presentation;
        audio = game.gameObject.AddComponent<GameAudio>();
        game.audioPlayer = audio;
        audio.source = game.gameObject.AddComponent<AudioSource>();
        audio.source.playOnAwake = false;
        audio.loopSource = game.gameObject.AddComponent<AudioSource>();
        audio.loopSource.loop = true;
        audio.loopSource.playOnAwake = false;
        audio.latch = Clip("latch");
        audio.tick = Clip("tick");
        audio.count = Clip("count");
        audio.click = Clip("click");
        audio.entry = Clip("entry");
        audio.lever = Clip("lever");
        audio.spin = Clip("spin");
        audio.stop = Clip("stop");
        audio.win = Clip("win");
        audio.collect = Clip("collect");
        audio.loopSource.clip = Clip("rumble");
        var face = Crop("slot-machine4", new Rect(137, 0, 550, 624), "CabinetFace");
        var scenery = Rect("Ambient scenery", canvas.transform, 0, 0, 1500, 1100);
        Pic("Illustrated casino room", scenery, 0, 0, 1400, 880, Color.white, Art("CasinoRoom"));
        Pic("Supplied atmosphere gradient", scenery, 0, 30, 1400, 1000, new Color(.5f, .4f, .8f, .12f), Art("bg_gradient"));
        Pic("Floor", canvas.transform, 0, -375, 1500, 120, new Color(.025f, .008f, .02f, .38f));
        Pic("Shadow", canvas.transform, 0, -304, 600, 26, new Color(0, 0, 0, .6f), rounded);
        var cabinet = Rect("Cabinet assembly", canvas.transform, 0, -20, 550, 624);
        cabinet.localScale = Vector3.one * .95f;
        Pic("Cabinet body", cabinet, 0, 0, 550, 624, Color.white, face);
        Pic("Nameplate", cabinet, 0, -280, 450, 43, Color.white, SlicedArt("slot_machine_Middle_box", new Rect(51, 39, 555, 199), "SuppliedButtonPanel", new Vector4(28, 30, 28, 30))).type = Image.Type.Sliced;
        Label("THE LUCKY LINE", cabinet, 0, -280, 430, 38, 24, gold);
        Pic("Supplied reel glass", cabinet, 1, -39, 368, 210, Color.white, Crop("slot-machine5", new Rect(229, 168, 368, 210), "SuppliedReelGlass"));
        game.reels = new ReelView[3];
        for (int n = 0; n < 3; n++)
        {
            var panel = Pic("Reel " + (n + 1), cabinet, -128 + n * 130, -38, 108, 174, new Color(.94f, .95f, .87f));
            panel.gameObject.AddComponent<RectMask2D>();
            var reel = panel.gameObject.AddComponent<ReelView>();
            reel.cells = new Image[5];
            for (int j = 0; j < 5; j++)
            {
                reel.cells[j] = Pic("Symbol " + j, panel.transform, 0, (2 - j) * 125, 94, 106, Color.white, config.symbols[(n + j) % 4]);
                reel.cells[j].preserveAspect = true;
            }
            reel.glow = Pic("Reveal highlight", panel.transform, 0, 0, 108, 174, Color.clear);
            reel.Show(n, config.symbols);
            game.reels[n] = reel;
            if (n == 0)
            {
                PrefabUtility.SaveAsPrefabAsset(reel.cells[2].gameObject, "Assets/Prefabs/Symbol.prefab");
                PrefabUtility.SaveAsPrefabAsset(panel.gameObject, "Assets/Prefabs/Reel.prefab");
            }
        }
        Label(">", cabinet, -213, -38, 25, 35, 24, gold);
        Label("<", cabinet, 216, -38, 25, 35, 24, gold);
        Pic("Lever bracket", cabinet, 281, -152, 48, 56, new Color(.4f, .23f, .08f), rounded);
        Pic("Lever hub rim", cabinet, 303, -152, 54, 54, gold, circle);
        Pic("Lever hub", cabinet, 303, -152, 40, 40, new Color(.14f, .08f, .04f), circle);
        var pivot = Rect("Lever pivot", cabinet, 303, -152, 60, 170);
        var lever = Pic("Pull lever", pivot, 0, 84, 57, 168, Color.white, Crop("slot-machine2", new Rect(673, 58, 92, 270), "LeverTexture"));
        lever.preserveAspect = true;
        lever.raycastTarget = true;
        game.leverButton = lever.gameObject.AddComponent<Button>();
        game.leverButton.targetGraphic = lever;
        UnityEventTools.AddPersistentListener(game.leverButton.onClick, game.Spin);
        Label("PULL", cabinet, 370, -134, 110, 35, 22, gold);
        Label("or SPACE", cabinet, 370, -163, 120, 25, 16, Color.white);
        var effects = game.gameObject.AddComponent<CasinoEffects>();
        game.effects = effects;
        effects.config = presentation;
        effects.audioPlayer = audio;
        effects.ambientLayer = scenery;
        effects.lights = new Image[34];
        for (int n = 0; n < 34; n++)
            effects.lights[n] = Pic("Marquee bulb " + n, cabinet, -248 + (n % 17) * 31, n < 17 ? 303 : -307, 8, 8, gold, circle);
        effects.sideLights = new Image[2];
        for (int n = 0; n < 2; n++)
        {
            effects.sideLights[n] = Pic("Side sweep " + n, canvas.transform, n == 0 ? -600 : 600, 0, 72, 470, Color.clear, rounded);
            effects.sideLights[n].transform.SetSiblingIndex(1);
        }
        var hud = Rect("Game HUD", canvas.transform, 0, 0, 1280, 800);
        game.hud = hud.gameObject;
        Pic("HUD background", hud, 0, 336, 1210, 104, new Color(.045f, .012f, .025f, .97f), rounded);
        // Credit chip and reset remain together in the fixed top HUD during zoom.
        Pic("Casino coin stack", hud, -548, 335, 100, 86, Color.white, Art("CasinoCoins")).preserveAspect = true;
        Label("CREDIT BALANCE", hud, -398, 363, 240, 27, 17, gold);
        game.credits = Label("1,000", hud, -398, 325, 240, 48, 36, Color.white);
        effects.balanceTarget = game.credits.rectTransform;
        game.betButton = Button("BET  10", hud, -75, 337, 215, 58, game.ChangeBet);
        game.bet = game.betButton.GetComponentInChildren<Text>();
        game.previousBetButton = AtlasButton("Decrease bet", hud, -222, 337, 48, "slot_machine_buttons-04", game.PreviousBet);
        game.nextBetButton = AtlasButton("Increase bet", hud, 73, 337, 48, "slot_machine_buttons-03", game.ChangeBet);
        Label("LAST WIN", hud, 230, 363, 180, 27, 17, gold);
        game.lastWin = Label("0", hud, 230, 325, 180, 48, 32, Color.white);
        game.resetButton = Button("RESET", hud, 488, 337, 176, 58, game.AskReset);
        Pic("Status backdrop", hud, 0, -335, 760, 48, ink, rounded);
        game.status = Label("PULL THE LEVER  /  PRESS SPACE", hud, 0, -335, 740, 46, 22, gold);
        game.helpButton = Button("PAYOUTS", hud, -180, -375, 220, 42, game.ShowHelp);
        game.menuButton = Button("MENU", hud, 180, -375, 220, 42, game.MainMenu);
        var menu = Overlay("Main menu", canvas.transform);
        game.menuGroup = menu;
        menu.GetComponent<Image>().color = new Color(.035f, .015f, .11f, .25f);
        Pic("Menu cabinet hero", menu.transform, 300, -5, 440, 500, Color.white, Crop("slot-machine1", new Rect(137, 0, 615, 624), "SuppliedMenuCabinet")).preserveAspect = true;
        for (int i = 0; i < 3; i++)
            Pic("Menu reel symbol " + i, menu.transform, 185 + i * 92, -32, 64, 80, Color.white, config.symbols[i]).preserveAspect = true;
        Pic("Menu coin treasure", menu.transform, 455, -230, 160, 130, Color.white, Art("CasinoCoins")).preserveAspect = true;
        Label("THE LUCKY", menu.transform, -280, 200, 620, 80, 60, gold);
        Label("LINE", menu.transform, -280, 115, 620, 120, 76, gold);
        Button("PLAY", menu.transform, -280, 0, 370, 95, game.Play, true);
        Button("SETTINGS", menu.transform, -280, -102, 300, 64, game.ShowSettings);
        Button("EXIT", menu.transform, -280, -181, 300, 64, game.Exit);
        for (int i = 0; i < 4; i++)
            Pic("Menu symbol jewel " + i, menu.transform, -415 + i * 90, -280, 64, 64, Color.white, config.symbols[i]).preserveAspect = true;
        Label("THREE REELS. ONE PERFECT MATCH.", menu.transform, 0, -355, 1000, 32, 21, Color.white);
        var reward = Overlay("Reward presentation", canvas.transform);
        reward.GetComponent<Image>().color = new Color(.025f, .008f, .02f, .7f);
        effects.rewardOverlay = reward.gameObject;
        effects.rewardGroup = reward;
        effects.particles = new RectTransform[32];
        for (int n = 0; n < 32; n++)
            effects.particles[n] = Pic("Reward particle " + n, reward.transform, 0, 0, 17, 17, n % 3 == 0 ? gold : (n % 3 == 1 ? new Color(1, .15f, .3f) : new Color(.2f, .85f, 1)), circle).rectTransform;
        var card = Rect("Reward card", reward.transform, 0, 0, 690, 450);
        effects.rewardCard = card;
        Card(card, 690, 450);

        Label("BINGO!", card, 0, 155, 630, 76, 58, gold);
        game.rewardLabel = Label("Congratulations - you won", card, 0, 92, 640, 42, 25, Color.white);
        effects.rewardAmount = Label("+250", card, 0, 5, 620, 110, 86, gold);
        Label("CREDITS", card, 0, -65, 600, 34, 22, Color.white);
        game.collectButton = Button("COLLECT REWARD", card, 0, -152, 470, 70, game.CollectReward, true);
        reward.gameObject.SetActive(false);
        var help = Overlay("Payout help", canvas.transform);
        game.helpPanel = help.gameObject;
        Card(help.transform, 830, 640);
        Label("THE PAYOUTS", help.transform, 0, 242, 760, 60, 38, gold);
        for (int n = 0; n < 4; n++)
        {
            Pic("Payout symbol", help.transform, -220, 145 - n * 72, 48, 54, Color.white, config.symbols[n]).preserveAspect = true;
            Label("3 MATCHING    " + config.payouts[n] + " x BET", help.transform, 65, 145 - n * 72, 440, 52, 25, Color.white);
        }
        Label("Only the centre line pays. Payout is the total credits awarded.\nEach symbol has an equal 1 in 4 chance per reel.", help.transform, 0, -160, 780, 70, 19, Color.white);
        Button("BACK", help.transform, 0, -253, 300, 56, game.CloseHelp);
        AtlasButton("Close payouts", help.transform, 365, 270, 54, "slot_machine_buttons-02", game.CloseHelp);
        help.gameObject.SetActive(false);
        var reset = Overlay("Reset confirmation", canvas.transform);
        game.resetPanel = reset.gameObject;
        Card(reset.transform, 790, 350);
        Label("START FRESH?", reset.transform, 0, 100, 710, 60, 38, gold);
        Label("Replace your balance with 1,000 credits.", reset.transform, 0, 18, 720, 50, 24, Color.white);
        ConfirmButton(true, reset.transform, -175, -88, game.ConfirmReset);
        ConfirmButton(false, reset.transform, 175, -88, game.CancelReset);
        reset.gameObject.SetActive(false);
        var exit = Overlay("Session finished", canvas.transform);
        game.exitPanel = exit.gameObject;
        Card(exit.transform, 790, 350);
        Label("THANKS FOR PLAYING", exit.transform, 0, 80, 730, 70, 36, gold);
        Label("Your credits and settings are saved.", exit.transform, 0, 4, 720, 45, 23, Color.white);
        Button("RETURN TO MENU", exit.transform, 0, -90, 400, 62, game.ReturnFromExit);
        exit.gameObject.SetActive(false);
        var settings = Overlay("Settings", canvas.transform);
        game.settingsPanel = settings.gameObject;
        Card(settings.transform, 830, 520);
        Label("SETTINGS", settings.transform, 0, 194, 760, 60, 40, gold);
        Label("MASTER VOLUME", settings.transform, 0, 112, 700, 40, 22, Color.white);
        var sliderRoot = Rect("Volume slider", settings.transform, 0, 45, 570, 44);
        var track = Pic("Track", sliderRoot, 0, 0, 570, 10, new Color(.3f, .15f, .15f), rounded);
        track.raycastTarget = true;
        var knob = Pic("Volume handle", sliderRoot, 0, 0, 30, 38, gold, rounded);
        knob.raycastTarget = true;
        var slider = sliderRoot.gameObject.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
        slider.handleRect = knob.rectTransform;
        slider.targetGraphic = knob;
        game.volumeSlider = slider;
        UnityEventTools.AddPersistentListener(slider.onValueChanged, game.SetVolume);
        game.motionButton = Button("REDUCED MOTION: OFF", settings.transform, 0, -53, 590, 60, game.ToggleMotion);
        game.motionLabel = game.motionButton.GetComponentInChildren<Text>();
        Button("BACK", settings.transform, 0, -169, 320, 62, game.CloseSettings);
        AtlasButton("Close settings", settings.transform, 365, 205, 54, "slot_machine_buttons-02", game.CloseSettings);
        settings.gameObject.SetActive(false);
        var mute = Button("SOUND ON", canvas.transform, 485, 262, 200, 40, game.ToggleMute);
        game.muteLabel = mute.GetComponentInChildren<Text>();
        Button("AUDIO", canvas.transform, -485, 262, 180, 40, game.ShowSettings);
        var view = game.gameObject.AddComponent<CabinetView>();
        game.view = view;
        view.cabinet = cabinet;
        view.lever = pivot;
        view.pressedLeverTip = Pic("Supplied lowered lever cap", cabinet, 303, -152, 57, 38, Color.white, Crop("slot-machine3", new Rect(673, 0, 92, 60), "SuppliedLoweredLever"));
        view.pressedLeverTip.gameObject.SetActive(false);
        view.config = presentation;
        view.audioPlayer = audio;
        view.transitionFade = Pic("Transition fade", canvas.transform, 0, 0, 1280, 800, Color.clear);
        hud.gameObject.SetActive(false);
        PlayerSettings.companyName = "KaartikeyKusshwaha";
        PlayerSettings.productName = "The Lucky Line";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 800;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
        AssetDatabase.SaveAssets();
        VerifyLogic();
        VerifyAssetUse();
        Render();
    }
    public static void VerifyAssetUse()
    {
        var dependencies = new System.Collections.Generic.HashSet<string>(AssetDatabase.GetDependencies("Assets/Scenes/Main.unity", true));
        string[] supplied = { "bg_gradient", "popup", "slot-machine1", "slot-machine2", "slot-machine3", "slot-machine4", "slot-machine5", "slot-symbol1", "slot-symbol2", "slot-symbol3", "slot-symbol4", "slot_machine_buttons-02", "slot_machine_buttons-03", "slot_machine_buttons-04", "slot_machine_Middle_box", "Yes_No_Btn" };
        foreach (string name in supplied)
            if (!dependencies.Contains("Assets/UI/" + name + ".png"))
                throw new Exception("Supplied asset missing from scene dependencies: " + name);
        Debug.Log("ASSET USE PASS: all 16 supplied PNGs are real scene dependencies.");
    }
    public static void CreateAndVerify()
    {
        Create();
        RuntimeVerification.Run();
    }
    public static void VerifyLogic()
    {
        LogicVerification.Run();
    }
    public static void BuildWeb()
    {
        VerifyLogic();
        VerifyAssetUse();
        var r = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Build/WebGL", BuildTarget.WebGL, BuildOptions.None);
        if (r.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("Web build failed");
        WebBuildPresentation.Apply();
    }
    public static void Render()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        Capture("first-screen");
    }
    public static void Capture(string name)
    {
        var camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
        Canvas.ForceUpdateCanvases();
        var rt = new RenderTexture(1280, 800, 24);
        camera.targetTexture = rt;
        camera.Render();
        RenderTexture.active = rt;
        var image = new Texture2D(1280, 800, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0);
        image.Apply();
        Directory.CreateDirectory("Artifacts");
        File.WriteAllBytes("Artifacts/" + name + ".png", image.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(rt);
    }
}
