using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class TrackGenerator : MonoBehaviour
{
    [Inject] private GameManager _gameManager;

    [SerializeField] private GameObject menuSegments;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] segmentPrefabs;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject powerupPrefab;

    [Header("Settings")]
    [SerializeField] private int segmentsCount = 8;
    [SerializeField] private float laneOffset = 1.5f;

    [Range(0, 1)][SerializeField] private float obstacleChance = 0.3f;
    [Range(0, 1)][SerializeField] private float coinChance = 0.5f;
    [Range(0, 1)][SerializeField] private float powerupChance = 0.05f;

    private Queue<GameObject> _segmentPool = new Queue<GameObject>();
    private List<GameObject> _activeSegments = new List<GameObject>();
    private Queue<GameObject> _obstaclePool = new Queue<GameObject>();
    private Queue<GameObject> _coinPool = new Queue<GameObject>();
    private Queue<GameObject> _powerupPool = new Queue<GameObject>();

    private float _nextSpawnZ;
    private bool _isGenerating;
    private const int PoolSize = 50;
    private const int PowerupPoolSize = 20;
    private const float SURFACE_Y = 0.5f;

    private void Start()
    {
        CreatePools();
    }

    private void CreatePools()
    {
        for (int i = 0; i < segmentsCount + 2; i++)
        {
            var prefab = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];
            var seg = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            seg.SetActive(false);
            _segmentPool.Enqueue(seg);
        }

        for (int i = 0; i < PoolSize; i++)
        {
            var obs = Instantiate(obstaclePrefab);
            obs.SetActive(false);
            _obstaclePool.Enqueue(obs);

            var coin = Instantiate(coinPrefab);
            coin.SetActive(false);
            _coinPool.Enqueue(coin);
        }

        for (int i = 0; i < PowerupPoolSize; i++)
        {
            var powerup = Instantiate(powerupPrefab);
            powerup.SetActive(false);
            _powerupPool.Enqueue(powerup);
        }
    }

    public void StartGeneration()
    {
        menuSegments.SetActive(false);
        _isGenerating = true;
        _nextSpawnZ = 0;

        for (int i = 0; i < segmentsCount; i++)
        {
            SpawnSegment(_nextSpawnZ);
            _nextSpawnZ += GetSegmentLength(_activeSegments[_activeSegments.Count - 1]);
        }
    }

    public void StopGeneration()
    {
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
            float segLength = GetSegmentLength(_activeSegments[_activeSegments.Count - 1]);
            _nextSpawnZ += segLength;
        }
    }

    private void SpawnSegment(float z)
    {
        GameObject segment = GetFromPool(_segmentPool);
        if (segment == null)
        {
            var prefab = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];
            segment = Instantiate(prefab);
        }

        segment.transform.position = new Vector3(0, 0, z);
        segment.SetActive(true);
        _activeSegments.Add(segment);

        PopulateSegment(segment);
    }

    private void ReturnSegmentToPool(GameObject segment)
    {
        foreach (Transform child in segment.transform)
        {
            if (child.TryGetComponent<Coin>(out _))
            {
                child.gameObject.SetActive(false);
                _coinPool.Enqueue(child.gameObject);
                continue;
            }

            if (child.TryGetComponent<Obstacle>(out _))
            {
                child.gameObject.SetActive(false);
                _obstaclePool.Enqueue(child.gameObject);
                continue;
            }

            if (child.TryGetComponent<Powerup>(out _))
            {
                child.gameObject.SetActive(false);
                _powerupPool.Enqueue(child.gameObject);
                continue;
            }
        }

        segment.SetActive(false);
        _segmentPool.Enqueue(segment);
    }

    private void PopulateSegment(GameObject segment)
    {
        TrackSegment segComponent = segment.GetComponent<TrackSegment>();
        if (segComponent == null) return;

        List<int> availableLanes = new List<int> { -1, 0, 1 };

        for (float t = 0.05f; t < 0.95f; t += Random.Range(0.05f, 0.15f))
        {
            if (availableLanes.Count == 0) break;

            Vector3 pos;
            Quaternion rot;
            segComponent.GetPointAt(t, out pos, out rot);

            int laneIndex = Random.Range(0, availableLanes.Count);
            int lane = availableLanes[laneIndex];
            availableLanes.RemoveAt(laneIndex);

            Vector3 spawnPos = pos + GetLaneOffset(lane, rot);
            spawnPos.y = SURFACE_Y;

            if (Random.value < obstacleChance)
            {
                var obs = GetFromPool(_obstaclePool);
                if (obs != null)
                {
                    spawnPos.y = SURFACE_Y + 0.5f;
                    obs.transform.position = spawnPos;
                    obs.transform.rotation = rot;
                    var obstacleTransform = segment.transform.GetChild(0).transform.GetChild(2).transform;
                    obs.transform.SetParent(obstacleTransform);
                    obs.transform.localScale = Vector3.one;
                    obs.SetActive(true);
                }
            }
            else if (Random.value < coinChance)
            {
                var coin = GetFromPool(_coinPool);
                if (coin != null)
                {
                    spawnPos.y = SURFACE_Y + 0.5f + 0.3f;
                    coin.transform.position = spawnPos;
                    coin.transform.rotation = rot;
                    var coinsTransform = segment.transform.GetChild(0).transform.GetChild(1).transform;
                    coin.transform.SetParent(coinsTransform);
                    coin.transform.localScale = Vector3.one;
                    coin.SetActive(true);
                }
            }

            if (Random.value < powerupChance * 3f && availableLanes.Count > 0)
            {
                int bonusLaneIndex = Random.Range(0, availableLanes.Count);
                int bonusLane = availableLanes[bonusLaneIndex];
                availableLanes.RemoveAt(bonusLaneIndex);

                var powerup = GetFromPool(_powerupPool);
                if (powerup != null)
                {
                    Vector3 powerupPos = pos + GetLaneOffset(bonusLane, rot);
                    powerupPos.y = SURFACE_Y + 1f;
                    powerup.transform.position = powerupPos;
                    powerup.transform.rotation = rot;
                    var powerupsTransform = segment.transform.GetChild(0).transform.GetChild(0).transform;
                    powerup.transform.SetParent(powerupsTransform);
                    powerup.transform.localScale = Vector3.one;
                    powerup.SetActive(true);
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
        return segComponent != null ? segComponent.WorldLength : 20f;
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