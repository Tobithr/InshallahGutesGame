using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public Transform[] spawnPoints;
    public float minDistanceFromEnemies = 10f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RespawnPlayer(PlayerHealth player)
    {
        Transform spawnPoint = GetBestSpawnPoint();
        if (spawnPoint == null) return;

        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;
    }

    public void RespawnBot(BotController bot)
    {
        Transform spawnPoint = GetBestSpawnPoint();
        if (spawnPoint == null) return;

        bot.transform.position = spawnPoint.position;
        bot.transform.rotation = spawnPoint.rotation;
    }

    // Pick the spawn point furthest from all living enemies/players
    Transform GetBestSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        if (spawnPoints.Length == 1) return spawnPoints[0];

        // Collect enemy/bot positions
        var enemies = new List<Vector3>();
        foreach (var bot in FindObjectsByType<BotController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (!bot.IsDead) enemies.Add(bot.transform.position);

        Transform best = spawnPoints[0];
        float bestDist = 0f;

        foreach (var sp in spawnPoints)
        {
            float minDist = float.MaxValue;
            foreach (var e in enemies)
            {
                float d = Vector3.Distance(sp.position, e);
                if (d < minDist) minDist = d;
            }
            if (enemies.Count == 0) minDist = float.MaxValue;
            if (minDist > bestDist) { bestDist = minDist; best = sp; }
        }

        return best;
    }

    // Create spawn points at runtime if none are assigned
    public void GenerateDefaultSpawnPoints(Bounds mapBounds, int count = 8)
    {
        spawnPoints = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"SpawnPoint_{i}");
            go.transform.SetParent(transform);
            float angle = (360f / count) * i;
            float radius = Mathf.Min(mapBounds.extents.x, mapBounds.extents.z) * 0.7f;
            float x = mapBounds.center.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float z = mapBounds.center.z + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            go.transform.position = new Vector3(x, mapBounds.min.y + 1f, z);
            spawnPoints[i] = go.transform;
        }
    }
}
