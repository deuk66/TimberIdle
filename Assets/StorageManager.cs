using UnityEngine;

public class StorageManager : MonoBehaviour
{
    [Header("Storage")]
    [SerializeField] private float storageHeight = 0.85f;
    [SerializeField] private float storageWidth = 0.86f;
    [SerializeField] private float depositRadius = 1.15f;
    [SerializeField] private Color storageColor = new Color(0.43f, 0.48f, 0.54f);

    private GameManager gameManager;
    private PlayerManger player;
    private TreeManager treeManager;
    private Vector2Int storageTile;

    public Vector2Int StorageTile => storageTile;
    public bool IsStorageTile(Vector2Int tile) => tile == storageTile;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            storageTile = new Vector2Int(gameManager.FloorWidth - 1, 0);
        }
    }

    private void Start()
    {
        gameManager = gameManager != null ? gameManager : FindAnyObjectByType<GameManager>();
        player = FindAnyObjectByType<PlayerManger>();
        treeManager = FindAnyObjectByType<TreeManager>();
        if (gameManager == null || gameManager.FloorWidth <= 0 || gameManager.FloorDepth <= 0)
        {
            return;
        }

        storageTile = new Vector2Int(gameManager.FloorWidth - 1, 0);
        GameObject storage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        storage.name = "StorageCube";
        storage.transform.position = gameManager.GetTileCenter(storageTile.x, storageTile.y) + Vector3.up * (storageHeight * 0.5f);
        storage.transform.localScale = new Vector3(gameManager.TileSize * storageWidth, storageHeight, gameManager.TileSize * storageWidth);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        storage.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = storageColor };
    }

    private void Update()
    {
        if (player == null || treeManager == null || !player.IsReturningToStorage || player.CarriedWood <= 0)
        {
            return;
        }

        Vector3 playerPosition = player.transform.position;
        playerPosition.y = 0f;
        Vector3 storagePosition = gameManager.GetTileCenter(storageTile.x, storageTile.y);
        if ((playerPosition - storagePosition).sqrMagnitude > depositRadius * depositRadius)
        {
            return;
        }

        int amount = player.TakeCarriedWood();
        treeManager.AddStoredWood(amount);
        player.CompleteStorageDelivery();
    }
}
