using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreeManager : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int spawnRegionSize = 15;

    [Header("Tree")]
    [SerializeField] private float trunkHeight = 1.5f;
    [SerializeField] private float trunkRadius = 0.18f;
    [SerializeField] private float leavesHeight = 1.5f;
    [SerializeField] private float leavesRadius = 0.75f;
    [SerializeField] private Color trunkColor = new Color(0.35f, 0.19f, 0.09f);
    [SerializeField] private Color leavesColor = new Color(0.16f, 0.48f, 0.2f);

    [Header("Wood Counter UI")]
    [SerializeField] private UIElementSettings woodPanelLayout = new UIElementSettings { anchor = new Vector2(0f, 1f), position = new Vector2(28f, -28f), size = new Vector2(230f, 78f) };
    [SerializeField] private UIElementSettings woodIconLayout = new UIElementSettings { anchor = new Vector2(0f, 1f), position = new Vector2(18f, -16f), size = new Vector2(46f, 22f) };
    [SerializeField] private UIElementSettings woodGrainLayout = new UIElementSettings { anchor = new Vector2(0f, 1f), position = new Vector2(33f, -16f), size = new Vector2(4f, 18f) };
    [SerializeField] private UIElementSettings woodCountLayout = new UIElementSettings { anchor = new Vector2(0f, 1f), position = new Vector2(78f, -13f), size = new Vector2(130f, 52f), content = "0", fontSize = 38 };
    [SerializeField] private string woodCountFormat = "{0}";

    [Header("Chopping")]
    [SerializeField] private float chopDuration = 1.5f;
    [SerializeField] private float chopDistance = 1.15f;
    [SerializeField] private float playerHopHeight = 0.16f;
    [SerializeField] private float shakeDistance = 0.12f;
    [SerializeField] private float shakeFrequency = 18f;

    private sealed class TreeInstance
    {
        public Transform Root;
        public Vector2Int GridPosition;
        public Vector3 RestPosition;
        public Vector3 ShakeAxis;
        public float ChopElapsed;
    }

    private readonly List<TreeInstance> trees = new List<TreeInstance>();
    private GameManager gameManager;
    private PlayerManger player;
    private PlayerHouseManager playerHouseManager;
    private StorageManager storageManager;
    private Material trunkMaterial;
    private Material leavesMaterial;
    private float spawnTimer;
    private TreeInstance activeTree;
    private int woodCount;
    private Text woodCountText;

    public int WoodCount => woodCount;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        player = FindAnyObjectByType<PlayerManger>();
        playerHouseManager = FindAnyObjectByType<PlayerHouseManager>();
        storageManager = FindAnyObjectByType<StorageManager>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        trunkMaterial = CreateMaterial(shader, trunkColor);
        leavesMaterial = CreateMaterial(shader, leavesColor);
        spawnTimer = Mathf.Max(0.1f, spawnInterval);
        woodCount = GameSaveData.Wood;
        CreateWoodCounterUI();
        UpdateWoodCounter();
    }

    private void Update()
    {
        if (gameManager == null || player == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
            player = FindAnyObjectByType<PlayerManger>();
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnTreeOnRandomTile();
            spawnTimer = Mathf.Max(0.1f, spawnInterval);
        }

        if (activeTree != null)
        {
            UpdateChoppingAnimation();
        }
        else
        {
            TryStartChoppingNearbyTree();
        }
    }

    private void SpawnTreeOnRandomTile()
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        int regionWidth = Mathf.Min(spawnRegionSize, gameManager.FloorWidth);
        int regionDepth = Mathf.Min(spawnRegionSize, gameManager.FloorDepth);
        int firstTopRow = gameManager.FloorDepth - regionDepth;

        for (int x = 0; x < regionWidth; x++)
        {
            for (int z = firstTopRow; z < gameManager.FloorDepth; z++)
            {
                Vector2Int tileCoordinates = new Vector2Int(x, z);
                if (!IsTileOccupied(tileCoordinates))
                {
                    availableTiles.Add(tileCoordinates);
                }
            }
        }

        if (availableTiles.Count == 0)
        {
            return;
        }

        Vector2Int gridPosition = availableTiles[Random.Range(0, availableTiles.Count)];
        Vector3 spawnPosition = gameManager.GetTileCenter(gridPosition.x, gridPosition.y);
        GameObject treeRoot = new GameObject("Tree");
        treeRoot.transform.position = spawnPosition;
        BoxCollider treeCollider = treeRoot.AddComponent<BoxCollider>();
        treeCollider.center = new Vector3(0f, (trunkHeight * 0.75f + leavesHeight) * 0.5f, 0f);
        treeCollider.size = new Vector3(leavesRadius * 2f, trunkHeight * 0.75f + leavesHeight, leavesRadius * 2f);
        CreateTrunk(treeRoot.transform);
        CreateLeaves(treeRoot.transform);
        trees.Add(new TreeInstance
        {
            Root = treeRoot.transform,
            GridPosition = gridPosition,
            RestPosition = spawnPosition,
            ShakeAxis = GetScreenHorizontalAxis()
        });
    }

    private bool IsTileOccupied(Vector2Int tileCoordinates)
    {
        if (player.IsTileReserved(tileCoordinates))
        {
            return true;
        }

        if ((playerHouseManager != null && playerHouseManager.IsHouseTile(tileCoordinates)) ||
            (storageManager != null && storageManager.IsStorageTile(tileCoordinates)))
        {
            return true;
        }

        Vector3 tileCenter = gameManager.GetTileCenter(tileCoordinates.x, tileCoordinates.y);
        float occupancyRadius = gameManager.TileSize * 0.45f;
        Vector3 playerPosition = player.transform.position;
        playerPosition.y = 0f;
        if ((playerPosition - tileCenter).sqrMagnitude < occupancyRadius * occupancyRadius)
        {
            return true;
        }

        foreach (TreeInstance tree in trees)
        {
            if (tree.Root != null && tree.GridPosition == tileCoordinates)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetTreeTile(Collider hitCollider, out Vector2Int tileCoordinates)
    {
        tileCoordinates = default;
        if (hitCollider == null)
        {
            return false;
        }

        foreach (TreeInstance tree in trees)
        {
            if (tree.Root != null &&
                (hitCollider.transform == tree.Root || hitCollider.transform.IsChildOf(tree.Root)))
            {
                tileCoordinates = tree.GridPosition;
                return true;
            }
        }

        return false;
    }

    public bool IsTileBlocked(Vector2Int tileCoordinates)
    {
        foreach (TreeInstance tree in trees)
        {
            if (tree.Root != null && tree.GridPosition == tileCoordinates)
            {
                return true;
            }
        }

        return false;
    }

    public void GetTreeTiles(List<Vector2Int> tileCoordinates)
    {
        tileCoordinates.Clear();
        foreach (TreeInstance tree in trees)
        {
            if (tree.Root != null)
            {
                tileCoordinates.Add(tree.GridPosition);
            }
        }
    }

    public bool TrySpendWood(int amount)
    {
        if (amount <= 0 || woodCount < amount)
        {
            return false;
        }

        woodCount -= amount;
        GameSaveData.SaveWood(woodCount);
        UpdateWoodCounter();
        return true;
    }

    public void AddStoredWood(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        woodCount += amount;
        GameSaveData.SaveWood(woodCount);
        UpdateWoodCounter();
    }

    private void CreateTrunk(Transform treeRoot)
    {
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(treeRoot, false);
        trunk.transform.localPosition = new Vector3(0f, trunkHeight * 0.5f, 0f);
        trunk.transform.localScale = new Vector3(trunkRadius * 2f, trunkHeight * 0.5f, trunkRadius * 2f);
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMaterial;
        Destroy(trunk.GetComponent<Collider>());
    }

    private void CreateLeaves(Transform treeRoot)
    {
        GameObject leaves = new GameObject("ConeLeaves");
        leaves.transform.SetParent(treeRoot, false);
        leaves.transform.localPosition = new Vector3(0f, trunkHeight * 0.75f, 0f);
        leaves.AddComponent<MeshFilter>().sharedMesh = CreateConeMesh(leavesRadius, leavesHeight, 24);
        leaves.AddComponent<MeshRenderer>().sharedMaterial = leavesMaterial;
    }

    private void TryStartChoppingNearbyTree()
    {
        if (!player.CanStartChopping || player.IsReturningToStorage)
        {
            return;
        }

        float nearestDistance = chopDistance * chopDistance;
        TreeInstance nearestTree = null;
        foreach (TreeInstance tree in trees)
        {
            if (tree.Root == null)
            {
                continue;
            }

            Vector3 offset = player.transform.position - tree.RestPosition;
            offset.y = 0f;
            float distance = offset.sqrMagnitude;
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestTree = tree;
            }
        }

        if (nearestTree != null && player.TryStartChopping(chopDuration, playerHopHeight))
        {
            activeTree = nearestTree;
            activeTree.ChopElapsed = 0f;
        }
    }

    private void UpdateChoppingAnimation()
    {
        activeTree.ChopElapsed += Time.deltaTime;
        float sway = Mathf.Sin(activeTree.ChopElapsed * shakeFrequency) * shakeDistance;
        activeTree.Root.position = activeTree.RestPosition + activeTree.ShakeAxis * sway;

        if (activeTree.ChopElapsed >= chopDuration)
        {
            Transform choppedTree = activeTree.Root;
            choppedTree.position = activeTree.RestPosition;
            trees.Remove(activeTree);
            activeTree = null;
            Destroy(choppedTree.gameObject);
            player.AddCarriedWood(Random.Range(2, 4));
        }
    }

    private void CreateWoodCounterUI()
    {
        GameObject canvasObject = new GameObject("WoodCounterCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("WoodCounterPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        ApplyLayout(panel.GetComponent<RectTransform>(), woodPanelLayout);
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.035f, 0.055f, 0.045f, 0.94f);
        panelImage.raycastTarget = false;

        Image logIcon = CreateBlock(panel.transform, "WoodIcon", woodIconLayout, new Color(0.48f, 0.27f, 0.12f));
        logIcon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -35f);
        Image woodGrain = CreateBlock(panel.transform, "WoodGrain", woodGrainLayout, new Color(0.76f, 0.52f, 0.25f));
        woodGrain.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -35f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        woodCountText = CreateText(panel.transform, font, "WoodCount", woodCountLayout, new Color(0.98f, 0.91f, 0.72f), FontStyle.Bold);
    }

    private void UpdateWoodCounter()
    {
        if (woodCountText != null)
        {
            woodCountText.text = string.Format(woodCountFormat, woodCount);
        }
    }

    private static Image CreateBlock(Transform parent, string objectName, UIElementSettings layout, Color color)
    {
        GameObject block = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        block.transform.SetParent(parent, false);
        ApplyLayout(block.GetComponent<RectTransform>(), layout);
        Image image = block.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateText(Transform parent, Font font, string objectName, UIElementSettings layout, Color color, FontStyle style)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        ApplyLayout(textObject.GetComponent<RectTransform>(), layout);

        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.text = layout.content;
        text.fontSize = layout.fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void ApplyLayout(RectTransform rect, UIElementSettings layout)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = layout.anchor;
        rect.anchoredPosition = layout.position;
        rect.sizeDelta = layout.size;
    }

    private static Vector3 GetScreenHorizontalAxis()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return Vector3.right;
        }

        Vector3 axis = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
        return axis.sqrMagnitude > 0f ? axis : Vector3.right;
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

    private static Mesh CreateConeMesh(float radius, float height, int segments)
    {
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 6];
        vertices[0] = new Vector3(0f, height, 0f);
        vertices[1] = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices[i + 2] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        int triangleIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            int current = i + 2;
            int next = (i + 1) % segments + 2;
            triangles[triangleIndex++] = 0;
            triangles[triangleIndex++] = next;
            triangles[triangleIndex++] = current;
            triangles[triangleIndex++] = 1;
            triangles[triangleIndex++] = current;
            triangles[triangleIndex++] = next;
        }

        Mesh mesh = new Mesh
        {
            name = "TreeCone"
        };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
