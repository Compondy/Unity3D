using Zenject;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private ObstacleType type;
    public ObstacleType Type => type;
    public int ZoneIndex { get; private set; }
    public int EntryIndex { get; private set; }

    public void Init(ObstacleType newType, int zoneIndex, int entryIndex)
    {
        type = newType;
        ZoneIndex = zoneIndex;
        EntryIndex = entryIndex;
    }

    public void SetType(ObstacleType newType)
    {
        type = newType;
    }

    public class Factory : PlaceholderFactory<ObstacleType, int, int, Obstacle> { }
}