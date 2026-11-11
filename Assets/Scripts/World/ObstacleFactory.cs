using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ObstacleFactory : IFactory<ObstacleType, int, int, Obstacle>
{
    private readonly DiContainer _container;
    private readonly TrackGenerator _trackGenerator;

    public ObstacleFactory(DiContainer container, TrackGenerator trackGenerator)
    {
        _container = container;
        _trackGenerator = trackGenerator;
    }

    public Obstacle Create(ObstacleType type, int zoneIndex, int entryIndex)
    {
        GameObject prefab = _trackGenerator.GetObstaclePrefab(zoneIndex, entryIndex);
        if (prefab == null)
        {
            Debug.LogError($"[ObstacleFactory] No prefab found for zone: {zoneIndex}, entryIndex: {entryIndex}");
            return null;
        }

        GameObject instance = _container.InstantiatePrefab(prefab);
        var obstacle = instance.GetComponent<Obstacle>();
        if (obstacle == null)
        {
            obstacle = instance.AddComponent<Obstacle>();
        }
        obstacle.Init(type, zoneIndex, entryIndex);
        return obstacle;
    }
}