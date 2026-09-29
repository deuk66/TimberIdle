using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Main Menu Layout")]
    [SerializeField] private UIElementSettings mainPanelLayout = new UIElementSettings { size = new Vector2(560f, 620f) };
    [SerializeField] private UIElementSettings titleLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.83f), size = new Vector2(500f, 90f), content = "TREE IDLE", fontSize = 56 };
    [SerializeField] private UIElementSettings startButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.59f), size = new Vector2(390f, 76f), content = "開始遊戲", fontSize = 25 };
    [SerializeField] private UIElementSettings settingsButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.41f), size = new Vector2(390f, 76f), content = "設定", fontSize = 25 };
    [SerializeField] private UIElementSettings exitButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.23f), size = new Vector2(390f, 76f), content = "結束遊戲", fontSize = 25 };

    [Header("Settings Layout")]
    [SerializeField] private UIElementSettings settingsPanelLayout = new UIElementSettings { size = new Vector2(700f, 410f) };
    [SerializeField] private UIElementSettings settingsTitleLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.82f), size = new Vector2(620f, 70f), content = "設定", fontSize = 38 };
    [SerializeField] private UIElementSettings resetButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.52f), size = new Vector2(500f, 74f), content = "重置遊戲資料", fontSize = 25 };
    [SerializeField] private UIElementSettings settingsStatusLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.35f), size = new Vector2(600f, 40f), fontSize = 18 };
    [SerializeField] private UIElementSettings settingsBackButtonLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.18f), size = new Vector2(500f, 64f), content = "返回", fontSize = 25 };
    [SerializeField] private UIElementSettings resetConfirmPanelLayout = new UIElementSettings { size = new Vector2(620f, 250f) };
    [SerializeField] private UIElementSettings resetConfirmTextLayout = new UIElementSettings { anchor = new Vector2(0.5f, 0.7f), size = new Vector2(570f, 60f), content = "清除已保存的木頭與金幣？", fontSize = 23 };
    [SerializeField] private UIElementSettings confirmResetButtonLayout = new UIElementSettings { anchor = new Vector2(0.3f, 0.28f), size = new Vector2(240f, 58f), content = "確認重置", fontSize = 25 };
    [SerializeField] private UIElementSettings cancelResetButtonLayout = new UIElementSettings { anchor = new Vector2(0.7f, 0.28f), size = new Vector2(240f, 58f), content = "取消", fontSize = 25 };

    private GameObject mainPanel;
    private GameObject settingsPanel;
    private GameObject resetConfirmPanel;
    private Text settingsStatusText;

    private void Start()
    {
        Camera menuCamera = GetComponent<Camera>();
        if (menuCamera != null)
        {
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            menuCamera.backgroundColor = new Color(0.055f, 0.09f, 0.075f);
        }

        EnsureEventSystem();
        CreateMenuUI();
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    private void CreateMenuUI()
    {
        GameObject canvasObject = new GameObject("MenuCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreateBackdrop(canvas.transform);

        mainPanel = CreatePanel(canvas.transform, "MainMenu", mainPanelLayout);
        CreateText(mainPanel.transform, font, "GameTitle", titleLayout, new Color(0.93f, 0.79f, 0.49f), FontStyle.Bold);
        CreateButton(mainPanel.transform, font, "StartButton", startButtonLayout, StartGame);
        CreateButton(mainPanel.transform, font, "SettingsButton", settingsButtonLayout, ShowSettings);
        CreateButton(mainPanel.transform, font, "ExitButton", exitButtonLayout, ExitGame);

        settingsPanel = CreatePanel(canvas.transform, "SettingsPanel", settingsPanelLayout);
        CreateText(settingsPanel.transform, font, "SettingsTitle", settingsTitleLayout, new Color(0.93f, 0.79f, 0.49f), FontStyle.Bold);
        CreateButton(settingsPanel.transform, font, "ResetDataButton", resetButtonLayout, ShowResetConfirmation);
        settingsStatusText = CreateText(settingsPanel.transform, font, "SettingsStatus", settingsStatusLayout, new Color(0.74f, 0.83f, 0.7f), FontStyle.Normal);
        CreateButton(settingsPanel.transform, font, "SettingsBackButton", settingsBackButtonLayout, ShowMainMenu);

        resetConfirmPanel = CreatePanel(settingsPanel.transform, "ResetConfirmPanel", resetConfirmPanelLayout);
        CreateText(resetConfirmPanel.transform, font, "ResetConfirmText", resetConfirmTextLayout, Color.white, FontStyle.Normal);
        CreateButton(resetConfirmPanel.transform, font, "ConfirmResetButton", confirmResetButtonLayout, ResetGameData);
        CreateButton(resetConfirmPanel.transform, font, "CancelResetButton", cancelResetButtonLayout, HideResetConfirmation);

        settingsPanel.SetActive(false);
        resetConfirmPanel.SetActive(false);
    }

    private static void CreateBackdrop(Transform parent)
    {
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        backdrop.transform.SetParent(parent, false);
        RectTransform rect = backdrop.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = backdrop.GetComponent<Image>();
        image.color = new Color(0.055f, 0.09f, 0.075f);
        image.raycastTarget = false;
    }

    private static GameObject CreatePanel(Transform parent, string name, UIElementSettings layout)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        ApplyLayout(rect, layout);
        panel.GetComponent<Image>().color = new Color(0.035f, 0.065f, 0.052f, 0.96f);
        return panel;
    }

    private static Text CreateText(Transform parent, Font font, string name, UIElementSettings layout, Color color, FontStyle style)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        ApplyLayout(rect, layout);
        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.text = layout.content;
        text.fontSize = layout.fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
    }

    private static void CreateButton(Transform parent, Font font, string name, UIElementSettings layout, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        ApplyLayout(rect, layout);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.35f, 0.28f, 0.16f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text label = labelObject.GetComponent<Text>();
        label.font = font;
        label.text = layout.content;
        label.fontSize = layout.fontSize;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(0.96f, 0.94f, 0.85f);
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;
    }

    private static void ApplyLayout(RectTransform rect, UIElementSettings layout)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = layout.anchor;
        rect.anchoredPosition = layout.position;
        rect.sizeDelta = layout.size;
    }

    private void StartGame()
    {
        GameSceneTransition.LoadScene("GameScene");
    }

    private void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void ShowMainMenu()
    {
        resetConfirmPanel.SetActive(false);
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    private void ShowResetConfirmation()
    {
        resetConfirmPanel.SetActive(true);
    }

    private void HideResetConfirmation()
    {
        resetConfirmPanel.SetActive(false);
    }

    private void ResetGameData()
    {
        GameSaveData.ResetResources();
        settingsStatusText.text = "遊戲資料已重置";
        resetConfirmPanel.SetActive(false);
    }

    private void ExitGame()
    {
        GameSaveData.QuitApplication();
    }
}