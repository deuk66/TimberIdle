using UnityEngine;

public class PlayerHouseManager : MonoBehaviour
{
    [Header("House")]
    [SerializeField] private float houseHeight = 1f;
    [SerializeField] private float houseWidth = 0.82f;
    [SerializeField] private Color houseColor = new Color(0.72f, 0.53f, 0.32f);

    private GameManager gameManager;
    private Vector2Int houseTile;

    public Vector2Int HouseTile => houseTile;
    public bool IsHouseTile(Vector2Int tile) => tile == houseTile;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            houseTile = new Vector2Int(gameManager.FloorWidth - 1, gameManager.FloorDepth - 1);
        }
    }

    private void Start()
    {
        gameManager = gameManager != null ? gameManager : FindAnyObjectByType<GameManager>();
        if (gameManager == null || gameManager.FloorWidth <= 0 || gameManager.FloorDepth <= 0)
        {
            return;
        }

        houseTile = new Vector2Int(gameManager.FloorWidth - 1, gameManager.FloorDepth - 1);
        GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);
        house.name = "PlayerHouseCube";
        house.transform.position = gameManager.GetTileCenter(houseTile.x, houseTile.y) + Vector3.up * (houseHeight * 0.5f);
        house.transform.localScale = new Vector3(gameManager.TileSize * houseWidth, houseHeight, gameManager.TileSize * houseWidth);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        house.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = houseColor };
    }
}
