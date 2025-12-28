using Netologia.Systems;
using Netologia.TowerDefence.Systems;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Netologia.TowerDefence.Behaviors
{
    public class ArcherTower : MonoBehaviour
    {
        [Header("Archer Settings")]
        [SerializeField] private Archer _archerPrefab;
        [SerializeField] private float _spawnInterval = 5f;
        [SerializeField] private int _maxArchers = 3;
        [SerializeField] private float _spawnRadius = 2f;

        private float _spawnTimer;
        private ArcherSystem _archers;
        private GridNavigationSystem _gridNavigation;
        private List<Archer> _spawnedArchers = new List<Archer>();

        [Inject]
        private void Construct(ArcherSystem archers, GridNavigationSystem gridNavigation)
        {
            _archers = archers;
            _gridNavigation = gridNavigation;
        }

        private void Start()
        {
            _spawnTimer = _spawnInterval * 0.5f;

            // Спавним начальных лучников
            int initialArchers = Mathf.Min(1, _maxArchers);
            for (int i = 0; i < initialArchers; i++)
            {
                SpawnArcher();
            }
        }

        private void Update()
        {
            if (!TimeManager.IsGame) return;

            _spawnTimer -= TimeManager.DeltaTime;
            if (_spawnTimer <= 0)
            {
                if (_spawnedArchers.Count < _maxArchers)
                {
                    SpawnArcher();
                }
                _spawnTimer = _spawnInterval;
            }
        }

        private void SpawnArcher()
        {
            if (_archerPrefab == null)
            {
                Debug.LogError("Archer prefab is not assigned!");
                return;
            }

            var archer = _archers[_archerPrefab].Get;

            if (archer == null)
            {
                Debug.LogError("Failed to get archer from pool!");
                return;
            }

            // Подписываемся на событие возврата в пул
            archer.OnReturnToPool += HandleArcherReturned;

            Vector3 spawnPosition = CalculateSpawnPosition();
            archer.Initialize(spawnPosition);

            _spawnedArchers.Add(archer);
        }

        private Vector3 CalculateSpawnPosition()
        {
            Vector3 towerPosition = transform.position;

            // Пытаемся найти свободную клетку вокруг башни
            GridNode spawnNode = FindSpawnNodeNearTower();

            if (spawnNode != null)
            {
                return spawnNode.WorldPosition;
            }

            // Если не нашли свободную клетку, спавним в радиусе от башни
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(0.5f, _spawnRadius);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0
            );

            return towerPosition + offset;
        }

        private GridNode FindSpawnNodeNearTower()
        {
            if (_gridNavigation == null) return null;

            Vector3 towerPosition = transform.position;
            Vector2Int towerGridPos = _gridNavigation.WorldToGridPosition(towerPosition);

            // Проверяем узлы по спирали от центра
            for (int radius = 0; radius <= Mathf.CeilToInt(_spawnRadius); radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                            continue;

                        Vector2Int gridPos = towerGridPos + new Vector2Int(x, y);
                        GridNode node = _gridNavigation.GetNodeAtGridPosition(gridPos);

                        if (node != null && node.IsWalkable)
                        {
                            float distance = Vector3.Distance(towerPosition, node.WorldPosition);
                            if (distance <= _spawnRadius)
                            {
                                return node;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void HandleArcherReturned(Archer archer)
        {
            _spawnedArchers.Remove(archer);

            if (archer != null)
            {
                archer.OnReturnToPool -= HandleArcherReturned;
            }
        }

        private void OnDestroy()
        {
            foreach (var archer in _spawnedArchers.ToArray())
            {
                if (archer != null && archer.gameObject.activeSelf)
                {
                    archer.OnReturnToPool -= HandleArcherReturned;
                    archer.ReturnToPool();
                }
            }

            _spawnedArchers.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            // Радиус спавна лучников
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _spawnRadius);

            // Позиции активных лучников (в редакторе во время игры)
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                foreach (var archer in _spawnedArchers)
                {
                    if (archer != null && archer.gameObject.activeSelf)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(transform.position, archer.transform.position);
                        Gizmos.DrawWireSphere(archer.transform.position, 0.2f);
                    }
                }
            }
#endif
        }
    }
}