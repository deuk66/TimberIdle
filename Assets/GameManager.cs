using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class UIElementSettings
{
    public Vector2 anchor = new Vector2(0.5f, 0.5f);
    public Vector2 position;
    public Vector2 size = new Vector2(300f, 60f);
    [TextArea] public string content;
    public int fontSize = 24;
}

public class GameManager : MonoBehaviour
{
    [Header("Floor")]
    [SerializeField] private int floorWidth = 30;
    [SerializeField] private int floorDepth = 15;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float tileGap = 0.01f;
    [SerializeField] private float tileThickness = 0.7f;
    [SerializeField] private float cameraOrthographicSize = 4.4f;
    [SerializeField] private float cameraMoveSpeed = 10f;

    [Header("Look")]
    [SerializeField] private Color tileColorA = new Color(0.30f, 0.25f, 0.18f);
    [SerializeField] private Color tileColorB = new Color(0.38f, 0.31f, 0.21f);
    [SerializeField] private Color baseColor = new Color(0.08f, 0.07f, 0.06f);

    [Header("Tile Textures")]
    [SerializeField] private Texture2D frontTexture;
    [SerializeField] private Texture2D backTexture;
    [SerializeField] private Texture2D leftTexture;
    [SerializeField] private Texture2D rightTexture;
    [SerializeField] private Texture2D topTexture;

    private Transform floorRoot;
    private Material[] tileMaterialsA;
    private Material[] tileMaterialsB;
    private Material baseMaterial;
    private Mesh tileMesh;
    private Camera gameCamera;
    private Vector3 cameraTarget;
    private Vector3 cameraOffset;

    public float TileSize => tileSize;
    public int FloorWidth => floorWidth;
    public int FloorDepth => floorDepth;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntimeGameManager()
    {
        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            return;
        }

        if (FindAnyObjectByType<GameManager>() != null)
        {
            return;
        }

        new GameObject("GameManager").AddComponent<GameManager>();
    }

    private void Start()
    {
        CreateMaterials();
        CreateFloor();
        SetupCamera();
        SetupLighting();
    }

    private void Update()
    {
        if (gameCamera == null)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        Vector3 cameraForward = Vector3.ProjectOnPlane(gameCamera.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(gameCamera.transform.right, Vector3.up).normalized;
        Vector3 movement = Vector3.zero;
        if (keyboard.wKey.isPressed)
        {
            movement += cameraForward;
        }
        if (keyboard.sKey.isPressed)
        {
            movement -= cameraForward;
        }
        if (keyboard.aKey.isPressed)
        {
            movement -= cameraRight;
        }
        if (keyboard.dKey.isPressed)
        {
            movement += cameraRight;
        }

        if (movement.sqrMagnitude == 0f)
        {
            return;
        }

        movement.Normalize();
        cameraTarget += movement * cameraMoveSpeed * Time.deltaTime;
        float horizontalLimit = Mathf.Max(0f, floorWidth * tileSize * 0.5f - 2f);
        float verticalLimit = Mathf.Max(0f, floorDepth * tileSize * 0.5f - 2f);
        cameraTarget.x = Mathf.Clamp(cameraTarget.x, -horizontalLimit, horizontalLimit);
        cameraTarget.z = Mathf.Clamp(cameraTarget.z, -verticalLimit, verticalLimit);
        gameCamera.transform.position = cameraTarget + cameraOffset;
        gameCamera.transform.LookAt(cameraTarget);
    }

    public bool TryGetTileCenter(Vector3 worldPoint, out Vector3 tileCenter)
    {
        tileCenter = Vector3.zero;
        if (!TryGetTileCoordinates(worldPoint, out Vector2Int tileCoordinates))
        {
            return false;
        }

        tileCenter = GetTileCenter(tileCoordinates.x, tileCoordinates.y);
        return true;
    }

    public bool TryGetTileCoordinates(Vector3 worldPoint, out Vector2Int tileCoordinates)
    {
        tileCoordinates = default;
        float worldWidth = floorWidth * tileSize;
        float worldDepth = floorDepth * tileSize;
        float localX = worldPoint.x + worldWidth * 0.5f;
        float localZ = worldPoint.z + worldDepth * 0.5f;

        if (localX < 0f || localZ < 0f || localX >= worldWidth || localZ >= worldDepth)
        {
            return false;
        }

        tileCoordinates = new Vector2Int(Mathf.FloorToInt(localX / tileSize), Mathf.FloorToInt(localZ / tileSize));
        return true;
    }

    public bool IsInsideGrid(Vector2Int tileCoordinates)
    {
        return tileCoordinates.x >= 0 && tileCoordinates.x < floorWidth &&
               tileCoordinates.y >= 0 && tileCoordinates.y < floorDepth;
    }

    public Vector3 GetTileCenter(int tileX, int tileZ)
    {
        return new Vector3(
            (tileX + 0.5f) * tileSize - floorWidth * tileSize * 0.5f,
            0f,
            (tileZ + 0.5f) * tileSize - floorDepth * tileSize * 0.5f);
    }

    private void CreateMaterials()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        tileMaterialsA = CreateTileMaterials(shader, tileColorA);
        tileMaterialsB = CreateTileMaterials(shader, tileColorB);
        baseMaterial = CreateMaterial(shader, baseColor);
        tileMesh = CreateTileMesh();
    }

    private Material[] CreateTileMaterials(Shader shader, Color color)
    {
        Texture2D[] textures = { frontTexture, backTexture, leftTexture, rightTexture, topTexture, null };
        Material[] materials = new Material[textures.Length];
        for (int face = 0; face < textures.Length; face++)
        {
            materials[face] = CreateMaterial(shader, textures[face] == null ? color : Color.white);
            if (textures[face] != null)
            {
                materials[face].mainTexture = textures[face];
            }
        }

        return materials;
    }

    private static Material CreateMaterial(Shader shader, Color color)
    {
        Material material = new Material(shader)
        {
            color = color
        };
        material.SetFloat("_Smoothness", 0.15f);
        return material;
    }

    private void CreateFloor()
    {
        floorRoot = new GameObject("MiningFloor").transform;
        floorRoot.SetParent(transform);

        float width = floorWidth * tileSize;
        float depth = floorDepth * tileSize;
        CreateBase(width, depth);

        for (int x = 0; x < floorWidth; x++)
        {
            for (int z = 0; z < floorDepth; z++)
            {
                CreateTile(x, z, width, depth);
            }
        }
    }

    private void CreateBase(float width, float depth)
    {
        GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObject.name = "FloorBase";
        baseObject.transform.SetParent(floorRoot);
        baseObject.transform.position = new Vector3(0f, -tileThickness - 0.16f, 0f);
        baseObject.transform.localScale = new Vector3(width + 0.36f, 0.32f, depth + 0.36f);
        baseObject.GetComponent<Renderer>().sharedMaterial = baseMaterial;
    }

    private void CreateTile(int x, int z, float width, float depth)
    {
        GameObject tileObject = new GameObject($"Tile_{x}_{z}", typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider));
        tileObject.transform.SetParent(floorRoot);
        tileObject.transform.position = new Vector3(
            (x + 0.5f) * tileSize - width * 0.5f,
            -tileThickness * 0.5f,
            (z + 0.5f) * tileSize - depth * 0.5f);
        tileObject.transform.localScale = new Vector3(tileSize - tileGap, tileThickness, tileSize - tileGap);
        tileObject.GetComponent<MeshFilter>().sharedMesh = tileMesh;
        tileObject.GetComponent<MeshRenderer>().sharedMaterials = (x + z) % 2 == 0 ? tileMaterialsA : tileMaterialsB;
    }

    private static Mesh CreateTileMesh()
    {
        Vector3[] vertices =
        {
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f)
        };
        Vector2[] faceUvs =
        {
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f)
        };
        Vector2[] uvs = new Vector2[24];
        for (int face = 0; face < 6; face++)
        {
            for (int vertex = 0; vertex < 4; vertex++)
            {
                uvs[face * 4 + vertex] = faceUvs[vertex];
            }
        }

        Mesh mesh = new Mesh { name = "TexturedFloorTile" };
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.subMeshCount = 6;
        for (int face = 0; face < 6; face++)
        {
            int start = face * 4;
            mesh.SetTriangles(new[] { start, start + 1, start + 2, start, start + 2, start + 3 }, face);
        }
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void SetupCamera()
    {
        gameCamera = Camera.main;
        if (gameCamera == null)
        {
            return;
        }

        gameCamera.orthographic = true;
        gameCamera.orthographicSize = cameraOrthographicSize;
        cameraTarget = new Vector3(0f, -0.7f, 0f);
        gameCamera.transform.position = new Vector3(10f, 12f, -10f);
        cameraOffset = gameCamera.transform.position - cameraTarget;
        gameCamera.transform.LookAt(cameraTarget);
        gameCamera.backgroundColor = new Color(0.055f, 0.07f, 0.08f);
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    private void SetupLighting()
    {
        RenderSettings.ambientLight = new Color(0.24f, 0.27f, 0.3f);

        GameObject lightObject = new GameObject("MiningSun");
        lightObject.transform.SetParent(transform);
        lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.color = new Color(1f, 0.88f, 0.7f);
        light.shadows = LightShadows.Soft;
    }
}

public static class GameSceneTransition
{
    private static readonly List<Behaviour> PausedBehaviours = new List<Behaviour>();
    private static readonly List<Canvas> HiddenCanvases = new List<Canvas>();
    private static readonly List<AudioListener> PausedAudioListeners = new List<AudioListener>();
    private static Camera gameCamera;
    private static SceneTransitionRunner transitionRunner;
    private static bool gameCameraWasEnabled;
    private static int gameCameraCullingMask;
    private static CameraClearFlags gameCameraClearFlags;
    private static float gameCameraDepth;
    private static bool sessionPaused;
    private static float previousTimeScale = 1f;

    internal static bool IsGamePaused => sessionPaused;

    public static void LoadScene(string sceneName)
    {
        GetTransitionRunner().LoadScene(sceneName);
    }

    public static void OpenSkillTree(string sceneName)
    {
        GetTransitionRunner().OpenSkillTree(sceneName);
    }

    public static void ReturnToGame(string skillTreeSceneName)
    {
        GetTransitionRunner().ReturnToGame(skillTreeSceneName);
    }

    private static SceneTransitionRunner GetTransitionRunner()
    {
        if (transitionRunner == null)
        {
            transitionRunner = Object.FindAnyObjectByType<SceneTransitionRunner>();
            if (transitionRunner == null)
            {
                GameObject runnerObject = new GameObject("SceneTransitionRunner");
                transitionRunner = runnerObject.AddComponent<SceneTransitionRunner>();
            }
        }

        return transitionRunner;
    }

    internal static void PauseGameScene(Scene gameScene)
    {
        previousTimeScale = Time.timeScale;
        PausedBehaviours.Clear();
        HiddenCanvases.Clear();
        PausedAudioListeners.Clear();

        foreach (Camera camera in Object.FindObjectsByType<Camera>())
        {
            if (camera.gameObject.scene == gameScene && camera.CompareTag("MainCamera"))
            {
                gameCamera = camera;
                gameCameraWasEnabled = camera.enabled;
                gameCameraCullingMask = camera.cullingMask;
                gameCameraClearFlags = camera.clearFlags;
                gameCameraDepth = camera.depth;
                camera.cullingMask = 0;
                camera.clearFlags = CameraClearFlags.Depth;
                camera.depth = Mathf.Min(gameCameraDepth, -2f);
                break;
            }
        }

        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>())
        {
            if (canvas.gameObject.scene == gameScene && canvas.enabled)
            {
                HiddenCanvases.Add(canvas);
                canvas.enabled = false;
            }
        }

        foreach (AudioListener audioListener in Object.FindObjectsByType<AudioListener>())
        {
            if (audioListener.gameObject.scene == gameScene && audioListener.enabled)
            {
                PausedAudioListeners.Add(audioListener);
                audioListener.enabled = false;
            }
        }

        PauseBehaviours(Object.FindObjectsByType<GameManager>(), gameScene);
        PauseBehaviours(Object.FindObjectsByType<TreeManager>(), gameScene);
        PauseBehaviours(Object.FindObjectsByType<PlayerManger>(), gameScene);
        PauseBehaviours(Object.FindObjectsByType<SellManager>(), gameScene);
        PauseBehaviours(Object.FindObjectsByType<PlayerHouseManager>(), gameScene);
        PauseBehaviours(Object.FindObjectsByType<StorageManager>(), gameScene);
        sessionPaused = true;
        Time.timeScale = 0f;
    }

    private static void PauseBehaviours<T>(T[] behaviours, Scene gameScene) where T : Behaviour
    {
        foreach (T behaviour in behaviours)
        {
            if (behaviour.gameObject.scene == gameScene && behaviour.enabled)
            {
                PausedBehaviours.Add(behaviour);
                behaviour.enabled = false;
            }
        }
    }

    internal static void RestoreGameScene()
    {
        Scene gameScene = SceneManager.GetSceneByName("GameScene");
        if (gameScene.isLoaded)
        {
            SceneManager.SetActiveScene(gameScene);
        }

        if (gameCamera != null)
        {
            gameCamera.cullingMask = gameCameraCullingMask;
            gameCamera.clearFlags = gameCameraClearFlags;
            gameCamera.depth = gameCameraDepth;
            gameCamera.enabled = gameCameraWasEnabled;
        }

        foreach (Behaviour behaviour in PausedBehaviours)
        {
            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        foreach (Canvas canvas in HiddenCanvases)
        {
            if (canvas != null)
            {
                canvas.enabled = true;
            }
        }

        foreach (AudioListener audioListener in PausedAudioListeners)
        {
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
        }

        Time.timeScale = previousTimeScale;
        gameCamera = null;
        PausedBehaviours.Clear();
        HiddenCanvases.Clear();
        PausedAudioListeners.Clear();
        sessionPaused = false;
    }
}

public class SceneTransitionRunner : MonoBehaviour
{
    private const float WipeDuration = 0.5f;
    private Canvas wipeCanvas;
    private RectTransform wipeRect;
    private Image wipeImage;
    private bool isTransitioning;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        CreateWipeOverlay();
    }

    public void LoadScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }
    }

    public void OpenSkillTree(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(OpenSkillTreeRoutine(sceneName));
        }
    }

    public void ReturnToGame(string skillTreeSceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(ReturnToGameRoutine(skillTreeSceneName));
        }
    }

    private void CreateWipeOverlay()
    {
        GameObject canvasObject = new GameObject("SceneTransitionCanvas");
        canvasObject.transform.SetParent(transform, false);
        wipeCanvas = canvasObject.AddComponent<Canvas>();
        wipeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        wipeCanvas.sortingOrder = 32760;

        GameObject wipeObject = new GameObject("RightToLeftBlackWipe", typeof(RectTransform), typeof(Image));
        wipeObject.transform.SetParent(canvasObject.transform, false);
        wipeRect = wipeObject.GetComponent<RectTransform>();
        wipeRect.anchorMin = new Vector2(1f, 0f);
        wipeRect.anchorMax = Vector2.one;
        wipeRect.pivot = new Vector2(1f, 0.5f);
        wipeRect.offsetMin = Vector2.zero;
        wipeRect.offsetMax = Vector2.zero;
        wipeImage = wipeObject.GetComponent<Image>();
        wipeImage.color = Color.black;
        wipeImage.raycastTarget = false;
        wipeImage.enabled = false;
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isTransitioning = true;
        yield return AnimateWipe(true);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (loadOperation != null)
        {
            while (!loadOperation.isDone)
            {
                yield return null;
            }
        }

        yield return null;
        yield return AnimateWipe(false);
        isTransitioning = false;
    }

    private IEnumerator OpenSkillTreeRoutine(string sceneName)
    {
        Scene skillTreeScene = SceneManager.GetSceneByName(sceneName);
        if (skillTreeScene.isLoaded)
        {
            yield break;
        }

        isTransitioning = true;
        yield return AnimateWipe(true);

        Scene gameScene = SceneManager.GetSceneByName("GameScene");
        if (!gameScene.isLoaded)
        {
            yield return LoadSceneRoutine(sceneName);
            yield break;
        }

        GameSceneTransition.PauseGameScene(gameScene);
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOperation == null)
        {
            GameSceneTransition.RestoreGameScene();
            yield return AnimateWipe(false);
            isTransitioning = false;
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        skillTreeScene = SceneManager.GetSceneByName(sceneName);
        if (skillTreeScene.isLoaded)
        {
            SceneManager.SetActiveScene(skillTreeScene);
        }

        yield return null;
        yield return AnimateWipe(false);
        isTransitioning = false;
    }

    private IEnumerator ReturnToGameRoutine(string skillTreeSceneName)
    {
        isTransitioning = true;
        yield return AnimateWipe(true);

        if (!GameSceneTransition.IsGamePaused)
        {
            yield return LoadSceneRoutine("GameScene");
            yield break;
        }

        Scene skillTreeScene = SceneManager.GetSceneByName(skillTreeSceneName);
        if (skillTreeScene.isLoaded)
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(skillTreeScene);
            if (unloadOperation != null)
            {
                while (!unloadOperation.isDone)
                {
                    yield return null;
                }
            }
        }

        GameSceneTransition.RestoreGameScene();
        yield return null;
        yield return AnimateWipe(false);
        isTransitioning = false;
    }

    private IEnumerator AnimateWipe(bool coverScreen)
    {
        wipeImage.enabled = true;
        float elapsed = 0f;
        while (elapsed < WipeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / WipeDuration);
            float easedProgress = progress * progress * (3f - 2f * progress);
            SetWipeCoverage(coverScreen ? easedProgress : 1f - easedProgress);
            yield return null;
        }

        SetWipeCoverage(coverScreen ? 1f : 0f);
        if (!coverScreen)
        {
            wipeImage.enabled = false;
        }
    }

    private void SetWipeCoverage(float coverage)
    {
        wipeRect.anchorMin = new Vector2(1f - coverage, 0f);
        wipeRect.anchorMax = Vector2.one;
        wipeRect.offsetMin = Vector2.zero;
        wipeRect.offsetMax = Vector2.zero;
    }
}

public static class SkillTreeReturnButton
{
    public static void Create(Component owner, string sceneName, UIElementSettings layout, Color color)
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystemObject, owner.gameObject.scene);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        GameObject canvasObject = new GameObject("SkillTreeCanvas");
        SceneManager.MoveGameObjectToScene(canvasObject, owner.gameObject.scene);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject buttonObject = new GameObject("ReturnToGameButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot = layout.anchor;
        buttonRect.anchoredPosition = layout.position;
        buttonRect.sizeDelta = layout.size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => GameSceneTransition.ReturnToGame(sceneName));

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = layout.content;
        label.fontSize = layout.fontSize;
        label.fontStyle = FontStyle.Bold;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;
    }
}
