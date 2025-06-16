using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntitySpawn : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject kamikazeeSlimePrefab;
    [SerializeField] private GameObject undeadArcherPrefab;
    [SerializeField] private GameObject zombiePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float difficultyIncreaseInterval = 30f;
    [SerializeField] private int maxEnemies = 30;
    
    private BoxCollider2D spawnArea;
    private float timeSinceLastDifficultyIncrease;
    private int enemiesPerSpawn = 1;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        if (spawnArea == null)
        {
            Debug.LogError("No BoxCollider2D found on spawn area!");
            enabled = false;
            return;
        }

        timeSinceLastDifficultyIncrease = 0f;
        StartCoroutine(SpawnRoutine());
        CleanupDestroyedEnemies();
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Bounds bounds = spawnArea.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(randomX, randomY);
    }

    private GameObject GetRandomEnemyPrefab()
    {
        int randomEnemy = Random.Range(0, 3);
        return randomEnemy switch
        {
            0 => kamikazeeSlimePrefab,
            1 => undeadArcherPrefab,
            _ => zombiePrefab
        };
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            CleanupDestroyedEnemies();
            
            for (int i = 0; i < enemiesPerSpawn; i++)
            {
                if (activeEnemies.Count < maxEnemies)
                {
                    SpawnEnemy();
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPosition = GetRandomSpawnPosition();
        GameObject enemyPrefab = GetRandomEnemyPrefab();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    private void CleanupDestroyedEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    private void Update()
    {
        timeSinceLastDifficultyIncrease += Time.deltaTime;
        if (timeSinceLastDifficultyIncrease >= difficultyIncreaseInterval)
        {
            IncreaseDifficulty();
            timeSinceLastDifficultyIncrease = 0f;
        }
    }

    private void IncreaseDifficulty()
    {
        enemiesPerSpawn++;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
