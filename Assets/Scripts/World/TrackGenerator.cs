using UnityEngine;
using System.Collections.Generic;
using Zenject;
using static GameConstants;

public class TrackGenerator : MonoBehaviour
{
    [Inject] private GameManager _gameManager;
    [Inject] private DiContainer _container;
    [Inject] private Obstacle.Factory _obstacleFactory;
    [Inject] private MetaProgress _meta;

    [SerializeField] private GameObject menuSegments;

    [Header("Zones Configuration")]
    [SerializeField] private ZoneConfig[] zones;
    [SerializeField] private int segmentsPerZone = 10;
    [SerializeField] private Light mainDirectionalLight;

    [Header("Default Fallback Prefabs")]
    [SerializeField] private GameObject[] segmentPrefabs;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField] private Transform poolRoot;

    [Header("Settings")]
    [SerializeField] private int segmentsCount = 8;
    [SerializeField] private float laneOffset = 1.5f;

    [Range(0, 1)][SerializeField] private float obstacleChance = 0.3f;
    [Range(0, 1)][SerializeField] private float coinChance = 0.5f;
    [Range(0, 1)][SerializeField] private float powerupChance = 0.05f;

    private List<GameObject> _activeSegments = new List<GameObject>();
    private Dictionary<int, Dictionary<int, Queue<Obstacle>>> _zoneObstaclePools = new Dictionary<int, Dictionary<int, Queue<Obstacle>>>();
    private Dictionary<int, Queue<GameObject>> _zoneSegmentPools = new Dictionary<int, Queue<GameObject>>();
    private Queue<GameObject> _coinPool = new Queue<GameObject>();
    private Queue<GameObject> _powerupPool = new Queue<GameObject>();

    private float _nextSpawnZ;
    private bool _isGenerating;
    private const int PoolSize = 50;
    private const int PowerupPoolSize = 20;

    private int _currentZoneIndex = 0;
    private int _segmentsSpawnedInCurrentZone = 0;

    private int AvailableZonesCount => (zones != null && zones.Length > 1 && (_meta == null || _meta.Level >= 2)) ? zones.Length : 1;

    public int CurrentZoneIndex => (zones != null && zones.Length > 0) ? (_currentZoneIndex % AvailableZonesCount) : 0;

    public ZoneConfig CurrentZone
    {
        get
        {
            if (zones == null || zones.Length == 0) return null;
            int count = AvailableZonesCount;
            return zones[_currentZoneIndex % count];
        }
    }

    private void Start()
    {
        if (poolRoot == null)
        {
            var go = new GameObject("[PoolRoot]");
            poolRoot = go.transform;
        }

        CreatePools();
    }

    public GameObject GetObstaclePrefab(ObstacleType type, int zoneIndex)
    {
        if (zones != null && zoneIndex >= 0 && zoneIndex < zones.Length && zones[zoneIndex] != null)
        {
            var p = zones[zoneIndex].GetObstaclePrefab(type);
            if (p != null) return p;
        }
        return obstaclePrefab;
    }

    public GameObject GetObstaclePrefab(int zoneIndex, int entryIndex)
    {
        if (zones != null && zoneIndex >= 0 && zoneIndex < zones.Length && zones[zoneIndex] != null)
        {
            var p = zones[zoneIndex].GetObstaclePrefabByIndex(entryIndex);
            if (p != null) return p;
        }
        return obstaclePrefab;
    }

    public GameObject GetObstaclePrefab(ObstacleType type)
    {
        return GetObstaclePrefab(type, CurrentZoneIndex);
    }

    private void CreatePools()
    {
        int totalZones = (zones != null && zones.Length > 0) ? zones.Length : 1;

        for (int z = 0; z < totalZones; z++)
        {
            if (!_zoneObstaclePools.ContainsKey(z))
                _zoneObstaclePools[z] = new Dictionary<int, Queue<Obstacle>>();

            if (!_zoneSegmentPools.ContainsKey(z))
                _zoneSegmentPools[z] = new Queue<GameObject>();

            ZoneConfig zone = (zones != null && z < zones.Length) ? zones[z] : null;
            int entriesCount = (zone != null && zone.ObstaclesCount > 0) ? zone.ObstaclesCount : 1;
            int countPerEntry = Mathf.Max(3, PoolSize / entriesCount);

            for (int entryIdx = 0; entryIdx < entriesCount; entryIdx++)
            {
                if (!_zoneObstaclePools[z].ContainsKey(entryIdx))
                    _zoneObstaclePools[z][entryIdx] = new Queue<Obstacle>();

                ObstaclePrefabEntry entry = (zone != null && zone.ObstaclesCount > 0)
                    ? zone.GetObstacleEntry(entryIdx)
                    : new ObstaclePrefabEntry { type = ObstacleType.Lane, prefab = obstaclePrefab };

                for (int i = 0; i < countPerEntry; i++)
                {
                    var obs = _obstacleFactory.Create(entry.type, z, entryIdx);
                    if (obs != null)
                    {
                        obs.gameObject.SetActive(false);
                        _zoneObstaclePools[z][entryIdx].Enqueue(obs);
                    }
                }
            }
        }

        if (coinPrefab == null)
        {
            Debug.LogWarning("[TrackGenerator] coinPrefab is not assigned; skipping coin pool creation");
        }
        else
        {
            for (int i = 0; i < PoolSize; i++)
            {
                var coin = _container.InstantiatePrefab(coinPrefab);
                coin.SetActive(false);
                _coinPool.Enqueue(coin);
            }
        }

        if (powerupPrefab == null)
        {
            Debug.LogWarning("[TrackGenerator] powerupPrefab is not assigned; skipping powerup pool creation");
        }
        else
        {
            for (int i = 0; i < PowerupPoolSize; i++)
            {
                var powerup = _container.InstantiatePrefab(powerupPrefab);
                powerup.SetActive(false);
                _powerupPool.Enqueue(powerup);
            }
        }
    }

    public void StartGeneration()
    {
        if (menuSegments == null)
        {
            Debug.LogError("[TrackGenerator] menuSegments is not assigned");
            return;
        }

        menuSegments.SetActive(false);
        _isGenerating = true;
        _nextSpawnZ = 0;
        _currentZoneIndex = 0;
        _segmentsSpawnedInCurrentZone = 0;
        ApplyZoneVisuals(CurrentZone);

        for (int i = 0; i < segmentsCount; i++)
        {
            SpawnSegment(_nextSpawnZ);
            if (_activeSegments.Count == 0) break;
            _nextSpawnZ += GetSegmentLength(_activeSegments[_activeSegments.Count - 1]);
        }
    }

    public void StopGeneration()
    {
        if (menuSegments == null)
        {
            Debug.LogError("[TrackGenerator] menuSegments is not assigned");
            return;
        }

        if (!_isGenerating && _activeSegments.Count == 0)
            return;

        menuSegments.SetActive(true);
        _isGenerating = false;

        foreach (var seg in _activeSegments)
        {
            ReturnSegmentToPool(seg);
        }
        _activeSegments.Clear();
        _nextSpawnZ = 0;
    }

    private void Update()
    {
        if (!_isGenerating || _gameManager.CurrentState != GameManager.State.Playing) return;

        float speed = _gameManager.Speed;
        float moveDelta = speed * Time.deltaTime;

        for (int i = _activeSegments.Count - 1; i >= 0; i--)
        {
            GameObject seg = _activeSegments[i];
            seg.transform.position -= Vector3.forward * moveDelta;

            float segLength = GetSegmentLength(seg);
            if (seg.transform.position.z < -segLength - 5f)
            {
                ReturnSegmentToPool(seg);
                _activeSegments.RemoveAt(i);

                if (_activeSegments.Count > 0)
                {
                    GameObject lastSeg = _activeSegments[_activeSegments.Count - 1];
                    float lastSegLength = GetSegmentLength(lastSeg);
                    _nextSpawnZ = lastSeg.transform.position.z + lastSegLength;
                }
                else
                {
                    _nextSpawnZ = 0;
                }
            }
        }

        while (_activeSegments.Count < segmentsCount)
        {
            SpawnSegment(_nextSpawnZ);
            if (_activeSegments.Count == 0) break;
            float segLength = GetSegmentLength(_activeSegments[_activeSegments.Count - 1]);
            _nextSpawnZ += segLength;
        }
    }

    private void SpawnSegment(float z)
    {
        _segmentsSpawnedInCurrentZone++;
        int availableZones = AvailableZonesCount;

        if (availableZones > 1 && _segmentsSpawnedInCurrentZone >= segmentsPerZone)
        {
            _segmentsSpawnedInCurrentZone = 0;
            _currentZoneIndex = (_currentZoneIndex + 1) % availableZones;
            ApplyZoneVisuals(CurrentZone);
        }

        int curZoneIndex = _currentZoneIndex % availableZones;

        GameObject[] prefabsToUse = (CurrentZone != null && CurrentZone.segmentPrefabs != null && CurrentZone.segmentPrefabs.Length > 0)
            ? CurrentZone.segmentPrefabs
            : segmentPrefabs;

        if (prefabsToUse == null || prefabsToUse.Length == 0)
        {
            Debug.LogError("[TrackGenerator] No segment prefabs available for spawning");
            return;
        }

        if (!_zoneSegmentPools.ContainsKey(curZoneIndex))
            _zoneSegmentPools[curZoneIndex] = new Queue<GameObject>();

        GameObject segment = GetFromPool(_zoneSegmentPools[curZoneIndex]);
        if (segment == null)
        {
            var prefab = prefabsToUse[Random.Range(0, prefabsToUse.Length)];
            segment = Instantiate(prefab);
        }

        var segComponent = segment.GetComponent<TrackSegment>();
        if (segComponent != null)
        {
            segComponent.ZoneIndex = curZoneIndex;
        }

        segment.transform.position = new Vector3(0, 0, z);
        PopulateSegment(segment, curZoneIndex);
        segment.SetActive(true);
        _activeSegments.Add(segment);
    }

    private void ApplyZoneVisuals(ZoneConfig zone)
    {
        if (zone == null) return;

        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.color = zone.lightColor;
            mainDirectionalLight.intensity = zone.lightIntensity;
        }
    }

    private void ReturnSegmentToPool(GameObject segment)
    {
        var items = segment.GetComponent<TrackSegment>();
        int segZoneIndex = items != null ? items.ZoneIndex : 0;

        if (items != null)
        {
            foreach (var item in items.powerupsInSegment)
            {
                item.gameObject.SetActive(false);
                item.transform.SetParent(poolRoot, false);
                _powerupPool.Enqueue(item.gameObject);
            }
            items.powerupsInSegment.Clear();

            foreach (var item in items.coinsInSegment)
            {
                item.gameObject.SetActive(false);
                item.transform.SetParent(poolRoot, false);
                _coinPool.Enqueue(item.gameObject);
            }
            items.coinsInSegment.Clear();

            foreach (var item in items.obstaclesInSegment)
            {
                item.gameObject.SetActive(false);
                var obsComponent = item.GetComponent<Obstacle>();
                int obsZone = obsComponent != null ? obsComponent.ZoneIndex : segZoneIndex;
                int entryIdx = obsComponent != null ? obsComponent.EntryIndex : 0;

                if (!_zoneObstaclePools.ContainsKey(obsZone))
                    _zoneObstaclePools[obsZone] = new Dictionary<int, Queue<Obstacle>>();

                if (!_zoneObstaclePools[obsZone].ContainsKey(entryIdx))
                    _zoneObstaclePools[obsZone][entryIdx] = new Queue<Obstacle>();

                if (obsComponent != null)
                    _zoneObstaclePools[obsZone][entryIdx].Enqueue(obsComponent);
            }
            items.obstaclesInSegment.Clear();
        }

        segment.SetActive(false);
        segment.transform.SetParent(poolRoot, false);
        if (!_zoneSegmentPools.ContainsKey(segZoneIndex))
            _zoneSegmentPools[segZoneIndex] = new Queue<GameObject>();

        _zoneSegmentPools[segZoneIndex].Enqueue(segment);
    }

    private Obstacle GetObstacleFromPool(ObstacleType type, int zoneIndex, int entryIndex)
    {
        if (_zoneObstaclePools.TryGetValue(zoneIndex, out var zonePools) &&
            zonePools.TryGetValue(entryIndex, out var pool) && pool.Count > 0)
        {
            while (pool.Count > 0)
            {
                var obs = pool.Dequeue();
                if (obs != null) return obs;
            }
        }

        return _obstacleFactory.Create(type, zoneIndex, entryIndex);
    }

    private Obstacle GetObstacleFromPool(ObstacleType type, int zoneIndex)
    {
        if (_zoneObstaclePools.TryGetValue(zoneIndex, out var zonePools))
        {
            foreach (var pool in zonePools.Values)
            {
                while (pool.Count > 0)
                {
                    var obs = pool.Dequeue();
                    if (obs != null) return obs;
                }
            }
        }

        return _obstacleFactory.Create(type, zoneIndex, 0);
    }

    private Obstacle GetObstacleFromPool(ObstacleType type)
    {
        return GetObstacleFromPool(type, CurrentZoneIndex);
    }

    private void PopulateSegment(GameObject segment, int zoneIndex)
    {
        TrackSegment segComponent = segment.GetComponent<TrackSegment>();
        if (segComponent == null) return;

        float curObstacleChance = CurrentZone != null ? CurrentZone.obstacleChance : obstacleChance;
        float curCoinChance = CurrentZone != null ? CurrentZone.coinChance : coinChance;
        float curPowerupChance = CurrentZone != null ? CurrentZone.powerupChance : powerupChance;

        List<int> availableLanes = new List<int> { -1, 0, 1 };

        for (float t = 0.05f; t < 0.95f; t += Random.Range(0.25f, 0.4f))
        {
            if (availableLanes.Count == 0) break;

            Vector3 pos;
            Quaternion rot;
            segComponent.GetPointAt(t, out pos, out rot);

            if (Random.value < curObstacleChance)
            {
                int entryIndex = CurrentZone != null ? CurrentZone.GetRandomObstacleIndex() : 0;
                ObstaclePrefabEntry entry = CurrentZone != null
                    ? CurrentZone.GetObstacleEntry(entryIndex)
                    : new ObstaclePrefabEntry { type = ObstacleType.Lane, prefab = obstaclePrefab };

                ObstacleType obsType = entry.type;
                Obstacle obs = GetObstacleFromPool(obsType, zoneIndex, entryIndex);

                if (obs != null)
                {
                    Transform parentRoot = segComponent.ObstaclesRoot != null ? segComponent.ObstaclesRoot : segComponent.transform;
                    parentRoot.localScale = Vector3.one;
                    obs.transform.SetParent(parentRoot, false);
                    obs.transform.rotation = rot;
                    obs.transform.localScale = Vector3.one;

                     if (obsType == ObstacleType.Jump)
                     {
                         obs.transform.position = new Vector3(pos.x, SurfaceY + 0.5f, pos.z);
                         obs.gameObject.SetActive(true);
                         segComponent.obstaclesInSegment.Add(obs.gameObject);
                         availableLanes.Clear();
                         break;
                     }
                     else if (obsType == ObstacleType.Slide)
                     {
                         obs.transform.position = new Vector3(pos.x, 0.0f, pos.z);
                         obs.gameObject.SetActive(true);
                         segComponent.obstaclesInSegment.Add(obs.gameObject);
                         availableLanes.Clear();
                         break;
                     }
                     else if (obsType == ObstacleType.Wall)
                     {
                         int openLane = availableLanes[Random.Range(0, availableLanes.Count)];
                          obs.transform.position = new Vector3(pos.x, SurfaceY + 0.5f, pos.z);
                         obs.gameObject.SetActive(true);
                         segComponent.obstaclesInSegment.Add(obs.gameObject);
                         availableLanes.Clear();
                         availableLanes.Add(openLane);
                      }
                      else
                      {
                        int laneIndex = Random.Range(0, availableLanes.Count);
                        int lane = availableLanes[laneIndex];
                        availableLanes.RemoveAt(laneIndex);
                        Vector3 spawnPos = pos + GetLaneOffset(lane, rot);
                        obs.transform.position = new Vector3(spawnPos.x, SurfaceY + 1.0f, spawnPos.z);
                        obs.gameObject.SetActive(true);
                        segComponent.obstaclesInSegment.Add(obs.gameObject);
                    }
                }
            }

            if (availableLanes.Count > 0 && Random.value < curCoinChance)
            {
                int laneIndex = Random.Range(0, availableLanes.Count);
                int lane = availableLanes[laneIndex];
                availableLanes.RemoveAt(laneIndex);

                var coin = GetFromPool(_coinPool);
                if (coin != null)
                {
                    Transform coinRoot = segComponent.CoinsRoot != null ? segComponent.CoinsRoot : segComponent.transform;
                    coinRoot.localScale = Vector3.one;
                    Vector3 coinPos = pos + GetLaneOffset(lane, rot);
                    coin.transform.SetParent(coinRoot, false);
                     coin.transform.position = new Vector3(
                         coinPos.x,
                         SurfaceY + 1f,
                         coinPos.z
                     );
                    coin.transform.rotation = rot;
                    coin.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                    coin.SetActive(true);
                    segComponent.coinsInSegment.Add(coin);
                }
            }

            if (availableLanes.Count > 0 && Random.value < curPowerupChance * 3f)
            {
                int bonusLaneIndex = Random.Range(0, availableLanes.Count);
                int bonusLane = availableLanes[bonusLaneIndex];
                availableLanes.RemoveAt(bonusLaneIndex);

                var powerup = GetFromPool(_powerupPool);
                if (powerup != null)
                {
                    Transform pRoot = segComponent.PowerupsRoot != null ? segComponent.PowerupsRoot : segComponent.transform;
                    pRoot.localScale = Vector3.one;
                    Vector3 powerupPos = pos + GetLaneOffset(bonusLane, rot);
                    powerup.transform.SetParent(pRoot, false);
                     powerup.transform.position = new Vector3(
                         powerupPos.x,
                         SurfaceY + 1f,
                         powerupPos.z
                     );
                    powerup.transform.rotation = rot;
                    powerup.transform.localScale = Vector3.one;
                    powerup.SetActive(true);
                    segComponent.powerupsInSegment.Add(powerup);
                }
            }
        }
    }

    private Vector3 GetLaneOffset(int lane, Quaternion rotation)
    {
        float offset = lane * laneOffset;
        return rotation * new Vector3(offset, 0, 0);
    }

    private float GetSegmentLength(GameObject segment)
    {
        TrackSegment segComponent = segment.GetComponent<TrackSegment>();
        return segComponent != null ? segComponent.WorldLength : 27f;
    }

    private GameObject GetFromPool(Queue<GameObject> pool)
    {
        while (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            if (obj != null)
                return obj;
        }
        return null;
    }
}