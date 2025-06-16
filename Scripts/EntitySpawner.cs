using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class EntitySpawner : MonoBehaviour
{
    [Header("Entity Prefabs")]
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject skeletonPrefab;
    [SerializeField] private GameObject slimePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float initialSpawnInterval = 5f;
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float spawnIntervalDecreaseRate = 0.1f; // Decrease per minute
    
    [Header("Spawn Limits")]
    [SerializeField] private int initialEntitiesPerSpawn = 3;
    [SerializeField] private int maxEntitiesPerSpawn = 8;
    [SerializeField] private float entitiesIncreaseRate = 1f; // Increase per minute
    [SerializeField] private int maxEntities = 30;

    [Header("Tile Settings")]
    [SerializeField] private Tilemap spawnableTilemap;

    private List<GameObject> activeEntities = new List<GameObject>();
    private List<Vector3Int> spawnableTilePositions;
    private float currentSpawnInterval;
    private int currentEntitiesPerSpawn;
    private float gameTimer;

    private void Start()
    {
        if (spawnableTilemap == null)
        {
            Debug.LogError("No Tilemap assigned for spawning!");
            enabled = false;
            return;
        }

        currentSpawnInterval = initialSpawnInterval;
        currentEntitiesPerSpawn = initialEntitiesPerSpawn;
        gameTimer = 0f;

        CacheSpawnableTilePositions();
        StartCoroutine(SpawnRoutine());
    }
    private void Update()
    {
        gameTimer += Time.deltaTime;
        UpdateDifficulty();
    }

    private void UpdateDifficulty()
    {
        float minutesElapsed = gameTimer / 60f;
        
        // Update spawn interval
        currentSpawnInterval = Mathf.Max(
            minSpawnInterval,
            initialSpawnInterval - (spawnIntervalDecreaseRate * minutesElapsed)
        );

        // Update entities per spawn
        currentEntitiesPerSpawn = Mathf.Min(
            maxEntitiesPerSpawn,
            initialEntitiesPerSpawn + Mathf.FloorToInt(entitiesIncreaseRate * minutesElapsed)
        );
    }

    private void CacheSpawnableTilePositions()
    {
        spawnableTilePositions = new List<Vector3Int>();
        BoundsInt bounds = spawnableTilemap.cellBounds;

        // Cache all tile positions that have a tile
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (spawnableTilemap.HasTile(tilePosition))
                {
                    spawnableTilePositions.Add(tilePosition);
                }
            }
        }

        if (spawnableTilePositions.Count == 0)
        {
            Debug.LogWarning("No spawnable tiles found in tilemap!");
        }
    }

    private Vector2 GetRandomTilePosition()
    {
        if (spawnableTilePositions.Count == 0)
            return Vector2.zero;

        // Get a random tile position from our cached list
        int randomIndex = Random.Range(0, spawnableTilePositions.Count);
        Vector3Int tilePosition = spawnableTilePositions[randomIndex];
        
        // Convert the tile position to world position
        Vector3 worldPosition = spawnableTilemap.GetCellCenterWorld(tilePosition);
        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private GameObject GetRandomEntityPrefab()
    {
        int randomIndex = Random.Range(0, 3);
        return randomIndex switch
        {
            0 => zombiePrefab,
            1 => skeletonPrefab,
            _ => slimePrefab
        };
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            CleanupDestroyedEntities();
            
            for (int i = 0; i < currentEntitiesPerSpawn; i++)
            {
                if (activeEntities.Count < maxEntities)
                {
                    SpawnEntity(GetRandomEntityPrefab());
                }
            }
            
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    private void SpawnEntity(GameObject entityPrefab)
    {
        Vector2 spawnPosition = GetRandomTilePosition();
        GameObject entity = Instantiate(entityPrefab, spawnPosition, Quaternion.identity);
        activeEntities.Add(entity);
    }

    private void CleanupDestroyedEntities()
    {
        activeEntities.RemoveAll(entity => entity == null);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}