using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManger : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpHeight = 0.55f;
    [SerializeField] private float chopHopCycle = 0.35f;
    [SerializeField] private int carryCapacity = 10;

    [Header("Appearance")]
    [SerializeField] private float bodyHeight = 0.9f;
    [SerializeField] private float bodyRadius = 0.34f;
    [SerializeField] private float headRadius = 0.24f;
    [SerializeField] private Color bodyColor = new Color(0.86f, 0.32f, 0.12f);
    [SerializeField] private Color headColor = new Color(0.96f, 0.78f, 0.55f);

    private GameManager gameManager;
    private TreeManager treeManager;
    private PlayerHouseManager playerHouseManager;
    private StorageManager storageManager;
    private readonly Queue<Vector3> tilePath = new Queue<Vector3>();
    private readonly List<Vector2Int> treeTargets = new List<Vector2Int>();
    private Vector3 jumpStart;
    private Vector3 jumpDestination;
    private float jumpElapsed;
    private float jumpDuration;
    private bool isJumping;
    private bool isChopping;
    private float chopTimeRemaining;
    private float chopElapsed;
    private float chopHopHeight;
    private Vector3 chopBasePosition;
    private bool isReturningToStorage;
    private int carriedWood;

    public bool IsMoving => isJumping || tilePath.Count > 0;
    public bool CanStartChopping => !IsMoving && !isChopping;
    public bool IsReturningToStorage => isReturningToStorage;
    public int CarriedWood => carriedWood;

    public bool IsTileReserved(Vector2Int tile)
    {
        if (gameManager == null)
        {
            return false;
        }

        if (isJumping && (PositionIsOnTile(jumpStart, tile) || PositionIsOnTile(jumpDestination, tile)))
        {
            return true;
        }

        if (!isJumping && PositionIsOnTile(transform.position, tile))
        {
            return true;
        }

        foreach (Vector3 queuedTile in tilePath)
        {
            if (PositionIsOnTile(queuedTile, tile))
            {
                return true;
            }
        }

        return false;
    }

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        treeManager = FindAnyObjectByType<TreeManager>();
        playerHouseManager = FindAnyObjectByType<PlayerHouseManager>();
        storageManager = FindAnyObjectByType<StorageManager>();
        CreatePlayerModel();

        if (gameManager != null && playerHouseManager != null)
        {
            Vector2Int spawnTile = playerHouseManager.HouseTile;
            transform.position = gameManager.GetTileCenter(spawnTile.x, spawnTile.y);
        }

    }

    private void Update()
    {
        HandleShopModeInput();
        if (isChopping)
        {
            UpdateChopHop();
            return;
        }

        UpdateJumpMovement();
        UpdateAutomaticDestination();
    }

    public bool TryStartChopping(float duration, float hopHeight)
    {
        if (!CanStartChopping)
        {
            return false;
        }

        isChopping = true;
        chopTimeRemaining = duration;
        chopElapsed = 0f;
        chopHopHeight = hopHeight;
        chopBasePosition = transform.position;
        return true;
    }

    private void UpdateChopHop()
    {
        chopTimeRemaining -= Time.deltaTime;
        chopElapsed += Time.deltaTime;

        float cycleProgress = (chopElapsed % chopHopCycle) / chopHopCycle;
        Vector3 position = chopBasePosition;
        position.y += Mathf.Sin(cycleProgress * Mathf.PI) * chopHopHeight;
        transform.position = position;

        if (chopTimeRemaining <= 0f)
        {
            transform.position = chopBasePosition;
            isChopping = false;
        }
    }

    private void HandleShopModeInput()
    {
        if (Keyboard.current == null || !Keyboard.current.gKey.wasPressedThisFrame)
        {
            return;
        }

        if (carriedWood <= 0)
        {
            return;
        }

        isReturningToStorage = !isReturningToStorage;
        tilePath.Clear();
    }

    private void UpdateAutomaticDestination()
    {
        if (IsMoving || gameManager == null || !gameManager.TryGetTileCoordinates(transform.position, out Vector2Int currentTile))
        {
            return;
        }

        if (carriedWood > 0 && carriedWood >= Mathf.Max(1, carryCapacity - 3))
        {
            isReturningToStorage = true;
        }

        if (isReturningToStorage)
        {
            if (storageManager != null)
            {
                Vector2Int storageTile = storageManager.StorageTile;
                QueuePathToTile(gameManager.GetTileCenter(storageTile.x, storageTile.y));
            }

            return;
        }

        if (treeManager == null)
        {
            return;
        }

        treeManager.GetTreeTiles(treeTargets);
        List<Vector2Int> shortestPath = null;
        foreach (Vector2Int treeTile in treeTargets)
        {
            List<Vector2Int> path = FindPathToClickedTile(currentTile, treeTile);
            if (path != null && (shortestPath == null || path.Count < shortestPath.Count))
            {
                shortestPath = path;
            }
        }

        if (shortestPath == null)
        {
            return;
        }

        for (int index = 1; index < shortestPath.Count; index++)
        {
            Vector2Int tile = shortestPath[index];
            tilePath.Enqueue(gameManager.GetTileCenter(tile.x, tile.y));
        }
    }

    private bool PositionIsOnTile(Vector3 position, Vector2Int tile)
    {
        return gameManager.TryGetTileCoordinates(position, out Vector2Int positionTile) && positionTile == tile;
    }

    private void QueuePathToTile(Vector3 destination)
    {
        tilePath.Clear();

        Vector3 currentPosition = isJumping ? jumpDestination : transform.position;
        if (!gameManager.TryGetTileCoordinates(currentPosition, out Vector2Int start))
        {
            return;
        }

        if (!gameManager.TryGetTileCoordinates(destination, out Vector2Int clickedTile))
        {
            return;
        }

        List<Vector2Int> path = FindPathToClickedTile(start, clickedTile);
        if (path == null)
        {
            return;
        }

        for (int index = 1; index < path.Count; index++)
        {
            Vector2Int tile = path[index];
            tilePath.Enqueue(gameManager.GetTileCenter(tile.x, tile.y));
        }
    }

    private List<Vector2Int> FindPathToClickedTile(Vector2Int start, Vector2Int clickedTile)
    {
        if (!IsBlocked(clickedTile))
        {
            return FindPath(start, clickedTile);
        }

        List<Vector2Int> shortestPath = null;
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (Vector2Int direction in directions)
        {
            Vector2Int adjacentTile = clickedTile + direction;
            if (!gameManager.IsInsideGrid(adjacentTile) || IsBlocked(adjacentTile))
            {
                continue;
            }

            List<Vector2Int> path = FindPath(start, adjacentTile);
            if (path != null && (shortestPath == null || path.Count < shortestPath.Count))
            {
                shortestPath = path;
            }
        }

        return shortestPath;
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        if (!gameManager.IsInsideGrid(target) || IsBlocked(target))
        {
            return null;
        }

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        frontier.Enqueue(start);
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            if (current == target)
            {
                break;
            }

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;
                if (!gameManager.IsInsideGrid(next) || cameFrom.ContainsKey(next) || IsBlocked(next))
                {
                    continue;
                }

                cameFrom[next] = current;
                frontier.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(target))
        {
            return null;
        }

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int step = target;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }

    private bool IsBlocked(Vector2Int tile)
    {
        return (treeManager != null && treeManager.IsTileBlocked(tile)) ||
               (playerHouseManager != null && playerHouseManager.IsHouseTile(tile)) ||
               (storageManager != null && storageManager.IsStorageTile(tile));
    }

    public int AddCarriedWood(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int added = Mathf.Min(amount, Mathf.Max(0, carryCapacity - carriedWood));
        carriedWood += added;
        if (carriedWood >= Mathf.Max(1, carryCapacity - 3))
        {
            isReturningToStorage = true;
            tilePath.Clear();
        }

        return added;
    }

    public int TakeCarriedWood()
    {
        int amount = carriedWood;
        carriedWood = 0;
        return amount;
    }

    public void CompleteStorageDelivery()
    {
        carriedWood = 0;
        isReturningToStorage = false;
        tilePath.Clear();
    }

    private void UpdateJumpMovement()
    {
        if (!isJumping)
        {
            if (tilePath.Count == 0)
            {
                return;
            }

            jumpStart = transform.position;
            jumpDestination = tilePath.Dequeue();
            jumpElapsed = 0f;
            jumpDuration = gameManager.TileSize / Mathf.Max(moveSpeed, 0.01f);
            isJumping = true;

            Vector3 direction = jumpDestination - jumpStart;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        jumpElapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(jumpElapsed / jumpDuration);
        Vector3 position = Vector3.Lerp(jumpStart, jumpDestination, progress);
        position.y += Mathf.Sin(progress * Mathf.PI) * jumpHeight;
        transform.position = position;

        if (progress >= 1f)
        {
            transform.position = jumpDestination;
            isJumping = false;
        }
    }

    private void CreatePlayerModel()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        GameObject body = new GameObject("ConeBody");
        body.transform.SetParent(transform, false);
        body.AddComponent<MeshFilter>().sharedMesh = CreateConeMesh(bodyRadius, bodyHeight, 24);
        body.AddComponent<MeshRenderer>().sharedMaterial = CreateMaterial(shader, bodyColor);

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "RoundHead";
        head.transform.SetParent(transform, false);
        head.transform.localPosition = new Vector3(0f, bodyHeight + headRadius * 0.65f, 0f);
        head.transform.localScale = Vector3.one * (headRadius * 2f);
        Destroy(head.GetComponent<Collider>());
        head.GetComponent<Renderer>().sharedMaterial = CreateMaterial(shader, headColor);
    }

    private static Material CreateMaterial(Shader shader, Color color)
    {
        Material material = new Material(shader)
        {
            color = color
        };
        material.SetFloat("_Smoothness", 0.2f);
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
            name = "PlayerCone"
        };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
