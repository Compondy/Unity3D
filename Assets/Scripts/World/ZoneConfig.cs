using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewZoneConfig", menuName = "Runner/ZoneConfig")]
public class ZoneConfig : ScriptableObject
{
    [Header("Visual")]
    public string zoneName = "Zone";
    public Material roadMaterial;
    public Material environmentMaterial;
    public Color lightColor = Color.white;
    public float lightIntensity = 1f;

    [Header("Prefabs")]
    public GameObject[] segmentPrefabs;

    [Header("Obstacles for this zone")]
    public ObstaclePrefabEntry[] obstaclePrefabs;

    [Header("Spawn Probabilities")]
    [Range(0, 1)] public float obstacleChance = 0.35f;
    [Range(0, 1)] public float coinChance = 0.5f;
    [Range(0, 1)] public float powerupChance = 0.05f;

    public int ObstaclesCount => obstaclePrefabs != null ? obstaclePrefabs.Length : 0;

    public ObstaclePrefabEntry GetObstacleEntry(int index)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return new ObstaclePrefabEntry { type = ObstacleType.Lane, prefab = null };

        if (index >= 0 && index < obstaclePrefabs.Length)
            return obstaclePrefabs[index];

        return obstaclePrefabs[0];
    }

    public GameObject GetObstaclePrefabByIndex(int index)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return null;
        if (index >= 0 && index < obstaclePrefabs.Length)
            return obstaclePrefabs[index].prefab;
        return obstaclePrefabs[0].prefab;
    }

    public int GetRandomObstacleIndex()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return 0;
        return Random.Range(0, obstaclePrefabs.Length);
    }

    public GameObject GetObstaclePrefab(ObstacleType type)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return null;
        for (int i = 0; i < obstaclePrefabs.Length; i++)
        {
            if (obstaclePrefabs[i].type == type)
                return obstaclePrefabs[i].prefab;
        }
        return obstaclePrefabs[0].prefab;
    }

    public ObstacleType GetRandomObstacleType()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return ObstacleType.Lane;
        int idx = Random.Range(0, obstaclePrefabs.Length);
        return obstaclePrefabs[idx].type;
    }
}

[System.Serializable]
public struct ObstaclePrefabEntry
{
    public ObstacleType type;
    public GameObject prefab;
}