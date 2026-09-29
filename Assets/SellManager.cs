using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class SellManager : MonoBehaviour
{
    [Header("Shop")]
    [SerializeField] private float rateChangeInterval = 30f;

    [Header("Gold Counter UI")]
    [SerializeField] private UIElementSettings goldPanelLayout = new UIElementSettings { anchor = new Vector2(0f, 1f), position = new Vector2(28f, -118f), size = new Vector2(230f, 78f) };
    [SerializeField] private UIElementSettings coinIconLayout = new UIElementSettings { anchor = new Vector2(0f, 1f), position = new Vector2(18f, -16f), size = new Vector2(46f, 46f) };
    [SerializeField] private UIElementSettings coinMarkLayout = new UIElementSettings { anchor = new Vector2(0f, 1f), position = new Vector2(18f, -16f), size = new Vector2(46f, 46f), content = "$", fontSize = 25 };
    [SerializeField] private UIElementSettings goldAmountLayout = new UIElementSettings { anchor = new Vector2(0f, 1f), position = new Vector2(78f, -13f), size = new Vector2(130f, 52f), content = "0", fontSize = 38 };

    [Header("Gear Menu UI")]
    [SerializeField] private UIElementSettings gearButtonLayout = new UIElementSettings { anchor = Vector2.one, position = new Vector2(-28f, -28f), size = new Vector2(76f, 76f) };
    [SerializeField] private UIElementSettings gearIconLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.5f), size = new Vector2(48f, 48f) };
    [SerializeField] private UIElementSettings shopToggleButtonLayout = new UIElementSettings { anchor = Vector2.one, position = new Vector2(-28f, -116f), size = new Vector2(76f, 64f), content = "商店", fontSize = 18 };
    [SerializeField] private UIElementSettings gearMenuPanelLayout = new UIElementSettings { anchor = Vector2.one, position = new Vector2(-28f, -198f), size = new Vector2(370f, 260f) };
    [SerializeField] private UIElementSettings continueButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.78f), size = new Vector2(320f, 58f), content = "繼續", fontSize = 24 };
    [SerializeField] private UIElementSettings openSettingsButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.5f), size = new Vector2(320f, 58f), content = "設定", fontSize = 24 };
    [SerializeField] private UIElementSettings menuExitButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.22f), size = new Vector2(320f, 58f), content = "退出遊戲", fontSize = 24 };
    [SerializeField] private UIElementSettings settingsPanelLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.5f), size = new Vector2(560f, 360f) };
    [SerializeField] private UIElementSettings settingsTitleLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.78f), size = new Vector2(500f, 60f), content = "設定", fontSize = 32 };
    [SerializeField] private UIElementSettings settingsBackButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.22f), size = new Vector2(320f, 58f), content = "返回", fontSize = 24 };

    [Header("Skill Tree Buttons UI")]
    [SerializeField] private UIElementSettings treeSkillButtonLayout = new UIElementSettings { anchor = Vector2.zero, position = new Vector2(28f, 28f), size = new Vector2(460f, 116f), content = "樹木技能樹", fontSize = 36 };
    [SerializeField] private UIElementSettings playerSkillButtonLayout = new UIElementSettings { anchor = Vector2.zero, position = new Vector2(28f, 160f), size = new Vector2(460f, 116f), content = "玩家技能樹", fontSize = 36 };

    [Header("Shop UI")]
    [SerializeField] private UIElementSettings shopPanelLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0f), position = new Vector2(0f, 38f), size = new Vector2(750f, 690f) };
    [SerializeField] private UIElementSettings exchangeRateLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.5f), position = new Vector2(0f, 150f), size = new Vector2(690f, 100f), content = "1 WOOD  =  2 GOLD", fontSize = 54 };
    [SerializeField] private UIElementSettings rateTimerLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.5f), position = Vector2.zero, size = new Vector2(690f, 80f), content = "NEXT RATE IN 30s", fontSize = 42 };
    [SerializeField] private string exchangeRateFormat = "1 WOOD  =  {0} GOLD";
    [SerializeField] private string rateTimerFormat = "NEXT RATE IN {0}s";
    [SerializeField] private UIElementSettings exchangeButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0f), position = new Vector2(0f, 45f), size = new Vector2(660f, 168f), content = "EXCHANGE 1 WOOD", fontSize = 48 };

    private TreeManager treeManager;
    private GameObject shopPanel;
    private GameObject gearMenuPanel;
    private GameObject settingsPanel;
    private Text goldAmountText;
    private Text exchangeRateText;
    private Text rateTimerText;
    private Button exchangeButton;
    private int goldCount;
    private int goldPerWood;
    private float rateTimeRemaining;
    private bool shopIsOpen;

    private void Start()
    {
        treeManager = FindAnyObjectByType<TreeManager>();
        goldCount = GameSaveData.Gold;
        goldPerWood = Random.Range(2, 5);
        rateTimeRemaining = Mathf.Max(1f, rateChangeInterval);
        CreateShopUI();
    }

    private void Update()
    {
        rateTimeRemaining -= Time.deltaTime;
        if (rateTimeRemaining <= 0f)
        {
            goldPerWood = Random.Range(2, 5);
            rateTimeRemaining = Mathf.Max(1f, rateChangeInterval);
            UpdateShopUI();
        }

        if (shopIsOpen)
        {
            UpdateShopUI();
        }
    }

    private void CreateShopUI()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        GameObject canvasObject = new GameObject("ShopCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreateGoldCounter(canvas.transform, font);
        CreateShopPanel(canvas.transform, font);
        CreateGearMenu(canvas.transform, font);
        CreateShopToggleButton(canvas.transform, font);
        CreateSkillTreeButtons(canvas.transform, font);
    }

    private void CreateGoldCounter(Transform canvas, Font font)
    {
        GameObject panel = CreatePanel(canvas, "GoldCounter", goldPanelLayout, new Color(0.035f, 0.055f, 0.045f, 0.94f));
        Image coinIcon = CreateImage(panel.transform, "CoinIcon", coinIconLayout, new Color(1f, 0.76f, 0.22f));
        coinIcon.sprite = CreateCoinSprite();
        CreateText(panel.transform, font, "CoinMark", coinMarkLayout, new Color(0.48f, 0.28f, 0.05f), FontStyle.Bold, true);
        goldAmountText = CreateText(panel.transform, font, "GoldAmount", goldAmountLayout, new Color(1f, 0.88f, 0.48f), FontStyle.Bold);
        goldAmountText.text = goldCount.ToString();
    }

    private void CreateGearMenu(Transform canvas, Font font)
    {
        GameObject gearButton = new GameObject("GearButton", typeof(RectTransform), typeof(Image), typeof(Button));
        gearButton.transform.SetParent(canvas, false);
        ApplyLayout(gearButton.GetComponent<RectTransform>(), gearButtonLayout);
        Image gearButtonImage = gearButton.GetComponent<Image>();
        gearButtonImage.color = new Color(0.035f, 0.055f, 0.045f, 0.94f);
        Button gearButtonComponent = gearButton.GetComponent<Button>();
        gearButtonComponent.targetGraphic = gearButtonImage;
        gearButtonComponent.onClick.AddListener(ToggleGearMenu);
        Image gearIcon = CreateImage(gearButton.transform, "GearIcon", gearIconLayout, new Color(0.9f, 0.88f, 0.78f));
        gearIcon.sprite = CreateGearSprite();

        gearMenuPanel = CreatePanel(canvas, "GearMenuPanel", gearMenuPanelLayout, new Color(0.035f, 0.055f, 0.045f, 0.97f));
        CreateMenuButton(gearMenuPanel.transform, font, "ContinueButton", continueButtonLayout, HideGearMenu);
        CreateMenuButton(gearMenuPanel.transform, font, "SettingsButton", openSettingsButtonLayout, ShowSettings);
        CreateMenuButton(gearMenuPanel.transform, font, "ExitButton", menuExitButtonLayout, SaveAndQuit);

        settingsPanel = CreatePanel(canvas, "GameSettingsPanel", settingsPanelLayout, new Color(0.035f, 0.055f, 0.045f, 0.98f));
        CreateText(settingsPanel.transform, font, "SettingsTitle", settingsTitleLayout, new Color(0.95f, 0.83f, 0.56f), FontStyle.Bold, true);
        CreateMenuButton(settingsPanel.transform, font, "SettingsBackButton", settingsBackButtonLayout, BackToGearMenu);
        gearMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void CreateShopToggleButton(Transform canvas, Font font)
    {
        GameObject buttonObject = new GameObject("ShopToggleButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas, false);
        ApplyLayout(buttonObject.GetComponent<RectTransform>(), shopToggleButtonLayout);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.035f, 0.055f, 0.045f, 0.94f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(ToggleShopPanel);
        CreateButtonLabel(buttonObject.transform, font, shopToggleButtonLayout, Color.white);
    }

    private void ToggleShopPanel()
    {
        shopIsOpen = !shopIsOpen;
        gearMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        shopPanel.SetActive(shopIsOpen);
        if (shopIsOpen)
        {
            UpdateShopUI();
        }
    }

    private void ToggleGearMenu()
    {
        bool shouldShow = !gearMenuPanel.activeSelf;
        settingsPanel.SetActive(false);
        gearMenuPanel.SetActive(shouldShow);
        shopPanel.SetActive(false);
        shopIsOpen = false;
        settingsPanel.SetActive(false);
        gearMenuPanel.SetActive(shouldShow);
    }

    private void HideGearMenu()
    {
        gearMenuPanel.SetActive(false);
    }

    private void ShowSettings()
    {
        gearMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void BackToGearMenu()
    {
        settingsPanel.SetActive(false);
        gearMenuPanel.SetActive(true);
    }

    private void CreateMenuButton(Transform parent, Font font, string objectName, UIElementSettings layout, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        ApplyLayout(buttonObject.GetComponent<RectTransform>(), layout);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.32f, 0.25f, 0.15f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateButtonLabel(buttonObject.transform, font, layout, Color.white);
    }

    private void CreateSkillTreeButtons(Transform canvas, Font font)
    {
        CreateSkillTreeButton(canvas, font, "TreeSkillTreeButton", treeSkillButtonLayout, "TreeSkillTree");
        CreateSkillTreeButton(canvas, font, "PlayerSkillTreeButton", playerSkillButtonLayout, "PlayerSkillTree");
    }

    private static void CreateSkillTreeButton(Transform canvas, Font font, string objectName, UIElementSettings layout, string sceneName)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas, false);
        ApplyLayout(buttonObject.GetComponent<RectTransform>(), layout);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.19f, 0.15f, 0.96f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => GameSceneTransition.OpenSkillTree(sceneName));
        CreateButtonLabel(buttonObject.transform, font, layout, Color.white);
    }

    private void CreateShopPanel(Transform canvas, Font font)
    {
        shopPanel = CreatePanel(canvas, "ShopPanel", shopPanelLayout, new Color(0.035f, 0.055f, 0.045f, 0.96f));
        exchangeRateText = CreateText(shopPanel.transform, font, "ExchangeRate", exchangeRateLayout, new Color(0.95f, 0.83f, 0.56f), FontStyle.Bold, true);
        rateTimerText = CreateText(shopPanel.transform, font, "RateTimer", rateTimerLayout, new Color(0.72f, 0.78f, 0.68f), FontStyle.Normal, true);

        GameObject buttonObject = new GameObject("ExchangeButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(shopPanel.transform, false);
        ApplyLayout(buttonObject.GetComponent<RectTransform>(), exchangeButtonLayout);
        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.55f, 0.36f, 0.14f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.colors = new ColorBlock
        {
            normalColor = new Color(0.55f, 0.36f, 0.14f),
            highlightedColor = new Color(0.7f, 0.48f, 0.18f),
            pressedColor = new Color(0.38f, 0.24f, 0.1f),
            selectedColor = new Color(0.7f, 0.48f, 0.18f),
            disabledColor = new Color(0.24f, 0.25f, 0.21f),
            colorMultiplier = 1f,
            fadeDuration = 0.12f
        };
        button.onClick.AddListener(ExchangeOneWood);
        exchangeButton = button;
        CreateButtonLabel(buttonObject.transform, font, exchangeButtonLayout, Color.white);
        shopPanel.SetActive(false);
    }

    private void UpdateShopUI()
    {
        int storedWood = treeManager != null ? treeManager.WoodCount : 0;
        if (exchangeRateText != null)
        {
            exchangeRateText.text = string.Format(exchangeRateFormat, goldPerWood);
        }
        if (rateTimerText != null)
        {
            rateTimerText.text = string.Format(rateTimerFormat, Mathf.CeilToInt(rateTimeRemaining));
        }
        if (exchangeButton != null)
        {
            exchangeButton.interactable = storedWood > 0;
        }
    }

    private void ExchangeOneWood()
    {
        if (treeManager == null || !treeManager.TrySpendWood(1))
        {
            UpdateShopUI();
            return;
        }

        goldCount += goldPerWood;
        goldAmountText.text = goldCount.ToString();
        GameSaveData.SaveGold(goldCount);
        UpdateShopUI();
    }

    private void SaveAndQuit()
    {
        int storedWood = treeManager != null ? treeManager.WoodCount : GameSaveData.Wood;
        GameSaveData.SaveResources(storedWood, goldCount);
        GameSaveData.QuitApplication();
    }

    private void OnApplicationQuit()
    {
        int storedWood = treeManager != null ? treeManager.WoodCount : GameSaveData.Wood;
        GameSaveData.SaveResources(storedWood, goldCount);
    }

    private static GameObject CreatePanel(Transform parent, string name, UIElementSettings layout, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        ApplyLayout(panel.GetComponent<RectTransform>(), layout);
        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        return panel;
    }

    private static Image CreateImage(Transform parent, string name, UIElementSettings layout, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        ApplyLayout(imageObject.GetComponent<RectTransform>(), layout);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateText(Transform parent, Font font, string name, UIElementSettings layout, Color color, FontStyle style, bool centered = false)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        ApplyLayout(textObject.GetComponent<RectTransform>(), layout);
        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.text = layout.content;
        text.fontSize = layout.fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = centered ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void CreateButtonLabel(Transform parent, Font font, UIElementSettings buttonLayout, Color color)
    {
        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.text = buttonLayout.content;
        text.fontSize = buttonLayout.fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
    }

    private static void ApplyLayout(RectTransform rect, UIElementSettings layout)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = layout.anchor;
        rect.anchoredPosition = layout.position;
        rect.sizeDelta = layout.size;
    }

    private static Sprite CreateCoinSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[size * size];
        Vector2 center = new Vector2(31.5f, 31.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance > 30f ? new Color32(0, 0, 0, 0) : distance > 25f ? new Color32(188, 128, 34, 255) : new Color32(255, 202, 74, 255);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
    }

    private static Sprite CreateGearSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[size * size];
        Vector2 center = new Vector2(31.5f, 31.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 offset = new Vector2(x, y) - center;
                float radius = offset.magnitude;
                float angle = Mathf.Atan2(offset.y, offset.x);
                float toothAngle = Mathf.Repeat(angle + Mathf.PI, Mathf.PI / 4f) - Mathf.PI / 8f;
                bool visible = (radius >= 19f && radius <= 25f || Mathf.Abs(toothAngle) < 0.105f && radius < 31f) && radius >= 9f;
                pixels[y * size + x] = visible ? new Color32(235, 225, 200, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
    }
}