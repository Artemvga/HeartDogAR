using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Shared presentation for the three application scenes; keeps their existing UnityEvents.</summary>
public static class AppPresentation
{
    // Calm textbook palette: paper, blue-green controls and sage selection.
    public static readonly Color Background = new Color32(247, 248, 244, 255);
    public static readonly Color Surface = new Color32(229, 238, 236, 255);
    public static readonly Color Accent = new Color32(42, 94, 114, 255);
    public static readonly Color Ink = new Color32(32, 52, 62, 255);
    public static readonly Color MutedInk = new Color32(87, 109, 118, 255);
    public static readonly Color SelectedSurface = new Color32(202, 224, 215, 255);
    static readonly Color HoverSurface = new Color32(216, 232, 229, 255);
    static readonly Color PressedSurface = new Color32(199, 219, 213, 255);
    static readonly Color AccentHover = new Color32(53, 116, 138, 255);
    static readonly Color AccentPressed = new Color32(32, 74, 91, 255);
    static Sprite rounded;
    static TMP_FontAsset interfaceFont;
    static TMP_FontAsset InterfaceFont
    {
        get
        {
            if (interfaceFont == null)
            {
                interfaceFont = TMP_FontAsset.CreateFontAsset(Resources.Load<Font>("UI/Interface"));
                interfaceFont.name = "HeartDog Interface";
                interfaceFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            }
            return interfaceFont;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply(scene);

    public static void Apply(Scene scene)
    {
        if (scene.name != "Menu" && scene.name != "ModelHeart" && scene.name != "CycleHeart") return;
        var objects = new List<Transform>();
        foreach (var root in scene.GetRootGameObjects()) objects.AddRange(root.GetComponentsInChildren<Transform>(true));
        if (objects.Exists(t => t.name == "App safe area")) return;
        foreach (var t in objects)
        {
            var canvas = t.GetComponent<Canvas>();
            if (canvas != null && canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace)
            {
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                }
                var safe = NewRect("App safe area", t);
                Stretch(safe);
                var children = new List<Transform>();
                foreach (Transform child in t) if (child != safe) children.Add(child);
                foreach (var child in children) child.SetParent(safe, false);
                safe.gameObject.AddComponent<AppSafeArea>();
            }
            var parentCanvas = t.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace) continue;
            var text = t.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.font = InterfaceFont;
                text.fontStyle = FontStyles.Normal;
                text.color = Ink;
                text.raycastTarget = false;
            }
            var button = t.GetComponent<Button>();
            if (button != null && t.name != "ButtonAGAU" && t.name != "ButtonXR" && t.name != "Button22")
                StyleButton(button);
            var slider = t.GetComponent<Slider>();
            if (slider != null)
            {
                if (slider.fillRect != null && slider.fillRect.TryGetComponent<Image>(out var fill)) fill.color = Accent;
                if (slider.handleRect != null && slider.handleRect.TryGetComponent<Image>(out var handle)) handle.color = Ink;
            }
        }
        var flowCatalog = Resources.Load<FlowMeshCatalog>("UI/FlowMeshCatalog");
        if(flowCatalog!=null) foreach(var item in objects)
        {
            var filter=item.GetComponent<MeshFilter>();
            if(filter==null || System.Array.IndexOf(flowCatalog.arrowMeshes,filter.sharedMesh)<0)continue;
            if(item.GetComponent<FlowParticleReplacement>()==null)
                item.gameObject.AddComponent<FlowParticleReplacement>().Initialize(filter.sharedMesh);
        }
        RectTransform Find(string name) => objects.Find(t => t.name.Trim() == name)?.GetComponent<RectTransform>();
        if (scene.name == "Menu")
        {
            var root = Find("InteractableUI"); Stretch(root);
            var background = Find("Panel");
            if (background != null && background.TryGetComponent<Image>(out var bg))
            { bg.sprite = null; bg.color = Background; Stretch(background); bg.raycastTarget = false; }
            var logos = Find("ButtonsURL");
            Place(logos, .5f, .94f, 0, 0, 540, 130);
            Place(Find("ButtonAGAU"), .5f, .5f, -170, 0, 120, 120);
            Place(Find("ButtonXR"), .5f, .5f, 0, 0, 120, 120);
            Place(Find("Button22"), .5f, .5f, 170, 0, 120, 120);
            var menu = Find("MenuUI"); Stretch(menu);
            Header(menu, "HEARTDOG / AR", "Анатомия,\nкоторую можно увидеть", "Изучайте сердце и кровообращение собаки\nв дополненной реальности.");
            Place(Find("StartButton"), .5f, .35f, 0, 0, 900, 154);
            SetLabel(Find("StartButton"), "Начать исследование  →"); Primary(Find("StartButton"));
            Place(Find("InformationButton"), .5f, .35f, 0, -180, 900, 144);
            SetLabel(Find("InformationButton"), "Как пользоваться приложением");
            Place(Find("ExitButton"), .5f, .35f, 0, -348, 900, 132);
            SetLabel(Find("ExitButton"), "Выйти");
            LayoutInformation(Find, objects);
            var choice = Find("ChoiseMode"); Stretch(choice);
            Header(choice, "ВЫБЕРИТЕ РЕЖИМ", "От устройства сердца\nк его работе", "Наведите камеру на учебный маркер,\nчтобы увидеть модель в пространстве.");
            Place(Find("ModelModeButton"), .5f, .35f, 0, 0, 900, 164);
            SetLabel(Find("ModelModeButton"), "01   Строение сердца  →"); Primary(Find("ModelModeButton"));
            Place(Find("ModeCycleButton"), .5f, .35f, 0, -192, 900, 164);
            SetLabel(Find("ModeCycleButton"), "02   Сердечный цикл  →");
            var back = NewRect("Назад", choice); Place(back, .5f, .35f, 0, -382, 900, 132);
            back.gameObject.AddComponent<Image>(); var backButton = back.gameObject.AddComponent<Button>();
            StyleButton(backButton); AddText(back, "Назад", 38, TextAlignmentOptions.Center);
            backButton.onClick.AddListener(() => { choice.gameObject.SetActive(false); menu.gameObject.SetActive(true); });
        }
        else
        {
            var ui = Find("UI"); Stretch(ui); Stretch(Find("ModeUI"));
            var settings = Find("SettingsUI"); Stretch(settings);
            if (settings != null)
            {
                var panel = Find("PanelSettings"); Stretch(panel);
                if (panel != null && panel.TryGetComponent<Image>(out var image)) { image.sprite = null; image.color = Background; }
                var title = NewRect("Заголовок настроек", settings); Place(title, .5f, .65f, 0, 0, 900, 120);
                AddText(title, "Пауза для исследования", 54, TextAlignmentOptions.Center);
                Place(Find("ReturnButton"), .5f, .5f, 0, 80, 900, 152); SetLabel(Find("ReturnButton"), "Продолжить"); Primary(Find("ReturnButton"));
                Place(Find("MenuButton"), .5f, .5f, 0, -100, 900, 144); SetLabel(Find("MenuButton"), "Главное меню");
                Place(Find("ExitButton"), .5f, .5f, 0, -272, 900, 132); SetLabel(Find("ExitButton"), "Выйти");
            }
            Place(Find("SettingsButton"), 1, 1, -135, -100, 220, 112); SetLabel(Find("SettingsButton"), "Меню");
            var hint = NewRect("AR подсказка", Find("ModeUI") ?? ui);
            Place(hint, .5f, 1, -100, -100, 750, 100);
            var hintImage = hint.gameObject.AddComponent<Image>(); hintImage.sprite = Rounded; hintImage.type = Image.Type.Sliced;
            hintImage.color = Surface; hintImage.raycastTarget = false;
            var hintText = AddText(hint, "Наведите камеру на учебный маркер", 32, TextAlignmentOptions.Center);
            var observer = objects.Find(t => t.GetComponent<Vuforia.ObserverBehaviour>() != null);
            if (Application.isPlaying && observer != null)
                hint.gameObject.AddComponent<AppTrackingHint>().Bind(observer.GetComponent<Vuforia.ObserverBehaviour>(), hintText);
            if (scene.name == "ModelHeart") { LayoutModel(Find, objects); LayoutHeart(Find); LayoutLungs(Find, objects); }
        }
        foreach (string name in new[] { "MenuUI", "ChoiseMode", "SettingsUI" })
        {
            var panel = Find(name);
            if (Application.isPlaying && panel != null) panel.gameObject.AddComponent<AppPanelReveal>();
        }
        var loadUI = Find("LoadUI");
        if (loadUI != null)
        {
            Stretch(loadUI); Stretch(Find("LoadPanel"));
            var loadPanel = Find("LoadPanel");
            if (loadPanel != null && loadPanel.TryGetComponent<Image>(out var loadImage))
            { loadImage.sprite = null; loadImage.color = Background; }
        }
        var loading = Find("SliderLoad");
        if (loading != null) Place(loading, .5f, .46f, 0, 0, 900, 28);
        if (loadUI != null)
        {
            var message = NewRect("Загрузка", loadUI); Place(message, .5f, .55f, 0, 0, 900, 100);
            AddText(message, "Открываем исследование…", 42, TextAlignmentOptions.Center);
        }
        Canvas.ForceUpdateCanvases();
        foreach(var rootObject in scene.GetRootGameObjects())
            foreach(var safe in rootObject.GetComponentsInChildren<AppSafeArea>(true))
            {
                var responsive=safe.gameObject.AddComponent<AppResponsiveLayout>();
                responsive.Reflow(((RectTransform)safe.transform).rect.size);
            }
    }

    static void LayoutHeart(System.Func<string, RectTransform> find)
    {
        var root=find("HeartUI");if(root==null)return;Stretch(root);
        foreach(var button in root.GetComponentsInChildren<Button>(true))if(button.name=="ButtonReturn")SetLabel(button.transform as RectTransform,"Назад к выбору органов");
        var view=root.gameObject.AddComponent<LungViewPanel>();view.controls=find("ModeControllerUI");view.panelToggle=find("ControllerUI");
        var background=NewRect("Фон сердцебиения",root);background.SetAsFirstSibling();
        Place(background,.5f,0,0,400,1000,760);
        var image=background.gameObject.AddComponent<Image>();image.sprite=Rounded;image.type=Image.Type.Sliced;image.color=Background;image.raycastTarget=false;
    }

    static void LayoutLungs(System.Func<string, RectTransform> find, List<Transform> objects)
    {
        var root=find("LungUI"); var model=objects.Find(t=>t.name=="Lungs");
        if(root==null || model==null) return;
        Stretch(root);
        foreach(Transform child in root) child.gameObject.SetActive(false);
        var breathing=model.gameObject.GetComponent<LungModelBreathing>();
        if(breathing==null) breathing=model.gameObject.AddComponent<LungModelBreathing>();
        breathing.Initialize();
        var view=root.gameObject.AddComponent<LungViewPanel>(); view.controls=find("ModeControllerUI"); view.panelToggle=find("ControllerUI");
        var panel=NewRect("Управление дыханием",root); Place(panel,.5f,0,0,360,940,320);
        var image=panel.gameObject.AddComponent<Image>();image.sprite=Rounded;image.type=Image.Type.Sliced;image.color=Background;image.raycastTarget=false;
        var caption=NewRect("Подпись",panel);Place(caption,.5f,1,0,-65,880,85);
        AddText(caption,"Дыхание на 3D-модели",42,TextAlignmentOptions.Center);
        var pause=NewRect("Пауза дыхания",panel);Place(pause,.5f,.5f,0,-15,600,108);
        pause.gameObject.AddComponent<Image>();var button=pause.gameObject.AddComponent<Button>();StyleButton(button);
        var label=AddText(pause,"Пауза",36,TextAlignmentOptions.Center);
        button.onClick.AddListener(()=>{breathing.TogglePause();label.text=breathing.paused?"Продолжить":"Пауза";});
        var note=NewRect("Легенда",panel);Place(note,.5f,0,0,40,900,60);
        AddText(note,"Поворот — одним пальцем · Масштаб — двумя",28,TextAlignmentOptions.Center);
        var back=root.GetComponentsInChildren<Button>(true);
        foreach(var control in back) if(control.name=="ButtonReturn")
        {control.gameObject.SetActive(true);Place(control.transform as RectTransform,.5f,0,0,100,900,130);SetLabel(control.transform as RectTransform,"Назад к выбору органов");}
    }

    static void LayoutInformation(System.Func<string, RectTransform> find, List<Transform> objects)
    {
        Stretch(find("Information"));
        var buttons = find("Buttons"); Stretch(buttons);
        Header(buttons, "ПЕРЕД НАЧАЛОМ", "Разберёмся вместе", "Выберите тему, чтобы узнать больше.");
        string[] names = { "InformationPanel1Button", "InformationPanel2Button", "InformationDevelopersButton" };
        string[] labels = { "Строение сердца", "Сосуды и кровообращение", "Об авторах" };
        for (int i = 0; i < names.Length; i++)
        { Place(find(names[i]), .5f, .38f, 0, -i * 168, 900, 144); SetLabel(find(names[i]), labels[i]); }
        foreach (var t in objects)
        {
            if (t.name == "ReturnButton")
            { Place(t as RectTransform, .5f, 0, 0, 130, 900, 136); SetLabel(t as RectTransform, "Назад"); }
        }
        foreach (string name in new[] { "Information1", "Information2", "Developers" }) Stretch(find(name));
        var authors = find("Developers");
        Header(authors, "КОМАНДА ПРОЕКТА", "Люди за HeartDog", "Учебный проект о сердце и кровообращении собаки.");
        var credits = NewRect("Авторы", authors); Place(credits, .5f, .31f, 0, 0, 900, 600);
        AddText(credits, "Разработка\nМицмахер Артем — программист\nМанокова Ольга — 3D-моделлер\nВоронова Таисия — 3D-моделлер\n\nНаставник\nМолодецкая Екатерина Алексеевна", 36, TextAlignmentOptions.Left);
        foreach (var t in objects)
        {
            var scroll = t.GetComponent<ScrollRect>();
            if (scroll == null) continue;
            var rect = t as RectTransform; Stretch(rect);
            rect.offsetMin = new Vector2(70, 250); rect.offsetMax = new Vector2(-70, -200);
            scroll.decelerationRate = .12f; scroll.scrollSensitivity = 32;
            scroll.horizontal = false;
            if (scroll.TryGetComponent<Image>(out var scrollBackground)) { scrollBackground.sprite = null; scrollBackground.color = Surface; }
            Stretch(scroll.viewport);
            scroll.viewport.offsetMax = new Vector2(-24, 0);
            var content = scroll.content;
            content.localScale = Vector3.one; content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(0, 2400);
            var pageImage = content.GetComponentInChildren<Image>(true);
            if (pageImage != null && pageImage.sprite != null)
            {
                Stretch(pageImage.rectTransform); pageImage.color = Color.white;
                var fit = content.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                fit.aspectRatio = pageImage.sprite.rect.width / pageImage.sprite.rect.height;
            }
            scroll.verticalNormalizedPosition = 1;
            if (scroll.verticalScrollbar != null)
            {
                if (scroll.verticalScrollbar.targetGraphic != null) scroll.verticalScrollbar.targetGraphic.color = Accent;
                var barColors = scroll.verticalScrollbar.colors; barColors.normalColor = Accent; scroll.verticalScrollbar.colors = barColors;
            }
        }
    }

    static void LayoutModel(System.Func<string, RectTransform> find, List<Transform> objects)
    {
        foreach (string name in new[] { "ModeUI", "ModeControllerUI", "LevelUI", "ControllerUI", "ButtonsUI", "OrganUI" }) Stretch(find(name));
        var controller = objects.Find(t => t.GetComponent<ModeController>() != null)?.GetComponent<ModeController>();
        if (controller == null) return;
        controller.rotationModeColor = Accent; controller.scaleModeColor = Accent;
        controller.rotationController.normalInvertColor = Surface;
        controller.rotationController.activeInvertColor = SelectedSurface;
        var controls = find("ModeControllerUI");
        var scrim = NewRect("Фон управления", find("ModeUI"));
        scrim.SetAsFirstSibling();
        scrim.anchorMin = Vector2.zero; scrim.anchorMax = new Vector2(1, 0); scrim.pivot = new Vector2(.5f, 0);
        scrim.anchoredPosition = Vector2.zero; scrim.sizeDelta = new Vector2(0, 1000);
        var gradient = new Texture2D(1, 64, TextureFormat.RGBA32, false); gradient.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 64; y++) { var color = Background; color.a = .92f * (1f - Mathf.SmoothStep(0, 1, y / 63f)); gradient.SetPixel(0, y, color); }
        gradient.Apply();
        var scrimImage = scrim.gameObject.AddComponent<RawImage>(); scrimImage.texture = gradient; scrimImage.raycastTarget = false;
        SetLabel(controller.mainModeSwitchButton.transform as RectTransform, "Приближение");
        SetLabel(controller.invertButton.transform as RectTransform, "Инверсия: выкл");
        if (controller.mainModeText != null) controller.mainModeText.text = "Вращение";
        var modePanel = find("PanelTextMode");
        if (modePanel != null && modePanel.TryGetComponent<Image>(out var modeImage))
        { modeImage.sprite = Rounded; modeImage.type = Image.Type.Sliced; modeImage.color = Surface; modeImage.raycastTarget = false; }
        // Keep the gesture receiver alive when organ panels hide the controls.
        var safe = controller.GetComponentInParent<Canvas>().GetComponentInChildren<AppSafeArea>(true);
        if (safe != null) { controller.transform.SetParent(safe.transform, false); controller.transform.SetAsFirstSibling(); Stretch(controller.transform as RectTransform); }
        foreach (var arrow in new[] { controller.rotateUpButton, controller.rotateDownButton, controller.rotateLeftButton, controller.rotateRightButton })
            if (arrow != null) arrow.gameObject.SetActive(false);
        if (find("ButtonsRotation") != null) find("ButtonsRotation").gameObject.SetActive(false);
        controller.scaleController.minScale = .5f;
        controller.scaleController.maxScale = 4f;
        Place(find("ButtonsScale"), 0, 0, 235, 420, 420, 180);
        Place(controller.scaleUpButton.transform as RectTransform, .5f, .5f, -105, 0, 180, 150);
        Place(controller.scaleDownButton.transform as RectTransform, .5f, .5f, 105, 0, 180, 150);
        SetLabel(controller.scaleUpButton.transform as RectTransform, "+"); SetLabel(controller.scaleDownButton.transform as RectTransform, "−");
        Place(controller.mainModeSwitchButton.transform as RectTransform, 1, 0, -250, 580, 420, 132);
        Place(controller.resetButton.transform as RectTransform, 1, 0, -250, 420, 420, 132); SetLabel(controller.resetButton.transform as RectTransform, "Сбросить");
        Place(controller.invertButton.transform as RectTransform, 1, 0, -250, 260, 420, 132);
        Place(find("PanelTextMode"), .5f, 0, 0, 750, 940, 90);
        foreach (string name in new[] { "ButtonHideUI", "ButtonUnHideUI" }) { Place(find(name), 1, 1, -135, -240, 220, 112); SetLabel(find(name), "Панель"); }
        string[] organs = { "ButtonHeart", "ButtonDog", "ButtonVessels", "ButtonLungs" };
        string[] labels = { "Сердце", "Вся модель", "Сосуды", "Лёгкие" };
        for (int i = 0; i < organs.Length; i++) { Place(find(organs[i]), .5f, 0, (i - 1.5f) * 250, 100, 230, 136); SetLabel(find(organs[i]), labels[i]); }
        var tip = NewRect("Подсказка жестов", controls); Place(tip, .5f, 0, 0, 850, 960, 80);
        AddText(tip, "Поворот — одним пальцем · Масштаб — двумя", 30, TextAlignmentOptions.Center);
    }

    public static void StyleButton(Button button)
    {
        if (button.TryGetComponent<Image>(out var image))
        {
            image.sprite = Rounded; image.type = Image.Type.Sliced; image.color = Color.white;
            button.targetGraphic = image;
        }
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.normalColor = Surface; colors.highlightedColor = HoverSurface;
        colors.pressedColor = PressedSurface; colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color32(232, 236, 233, 180); colors.fadeDuration = .12f; button.colors = colors;
        var navigation = button.navigation; navigation.mode = Navigation.Mode.Automatic; button.navigation = navigation;
        foreach (var text in button.GetComponentsInChildren<TMP_Text>(true))
        { text.font = InterfaceFont; text.fontStyle = FontStyles.Normal; text.color = Ink; text.enableAutoSizing = true; text.fontSizeMin = 24; text.fontSizeMax = 42; text.margin = new Vector4(22, 8, 22, 8); text.raycastTarget = false; }
    }

    static void Primary(RectTransform rect)
    {
        if (rect == null || !rect.TryGetComponent<Button>(out var button)) return;
        var colors = button.colors; colors.normalColor = Accent; colors.highlightedColor = AccentHover; colors.pressedColor = AccentPressed; colors.selectedColor = Accent; button.colors = colors;
        foreach (var text in button.GetComponentsInChildren<TMP_Text>(true)) text.color = Background;
    }

    static void Header(RectTransform parent, string eyebrow, string title, string subtitle)
    {
        var label = NewRect("Раздел", parent); Place(label, .5f, .80f, 0, 90, 900, 70); AddText(label, eyebrow, 30, TextAlignmentOptions.Left).color = Accent;
        var heading = NewRect("Заголовок", parent); Place(heading, .5f, .70f, 0, 0, 900, 280); AddText(heading, title, 76, TextAlignmentOptions.Left);
        var description = NewRect("Описание", parent); Place(description, .5f, .54f, 0, 0, 900, 170); AddText(description, subtitle, 36, TextAlignmentOptions.Left).color = MutedInk;
    }

    static TMP_Text AddText(RectTransform parent, string value, float size, TextAlignmentOptions alignment)
    {
        var rect = NewRect("Текст", parent); Stretch(rect);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = InterfaceFont;
        text.text = value; text.fontSize = size; text.color = Ink; text.alignment = alignment; text.raycastTarget = false;
        text.enableAutoSizing = true; text.fontSizeMin = size * .75f; text.fontSizeMax = size;
        return text;
    }

    static void SetLabel(RectTransform rect, string value)
    {
        if (rect == null) return;
        var text = rect.GetComponentInChildren<TMP_Text>(true);
        if (text == null) text = AddText(rect, value, 36, TextAlignmentOptions.Center);
        text.text = value;
    }

    static RectTransform NewRect(string name, Transform parent)
    { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); if(parent != null) rect.gameObject.layer = parent.gameObject.layer; return rect; }
    static void Stretch(RectTransform rect)
    { if (rect == null) return; rect.localScale = Vector3.one; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
    static void Place(RectTransform rect, float ax, float ay, float x, float y, float width, float height)
    { if (rect == null) return; rect.localScale = Vector3.one; rect.anchorMin = rect.anchorMax = new Vector2(ax, ay); rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height); }

    static Sprite Rounded
    {
        get
        {
            if (rounded != null) return rounded;
            const int size = 64; const float radius = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "UI rounded surface"; texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                var q = new Vector2(Mathf.Max(Mathf.Abs(x - 31.5f) - (32 - radius), 0), Mathf.Max(Mathf.Abs(y - 31.5f) - (32 - radius), 0));
                texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(radius - q.magnitude)));
            }
            texture.Apply();
            rounded = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(18, 18, 18, 18));
            return rounded;
        }
    }
}
