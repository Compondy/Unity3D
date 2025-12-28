using Netologia.Behaviours;
using Netologia.Systems;
using Netologia.TowerDefence.Behaviors;
using Netologia.TowerDefence.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Netologia.TowerDefence
{
    public class Archer : MonoBehaviour, IPoolElement<Archer>
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 2f;

        [Header("Combat Settings")]
        [SerializeField] private float _attackRange = 3f;
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private int _damage = 10;
        [SerializeField] private ElementalType _attackElemental = ElementalType.Physic;
        [SerializeField] private Projectile _projectilePrefab;
        private SpriteRenderer _spriteRenderer;

        private GridNavigationSystem _gridNavigation;
        private ProjectileSystem _projectiles;
        private UnitSystem _units;

        private GridNode _currentNode;
        private Unit _currentTarget;
        private float _attackTimer;
        private List<GridNode> _currentPath = new List<GridNode>();
        private int _currentPathIndex = 0;
        private Vector2Int? _lastTargetGridPos;

        public Archer Ref { get; set; }
        public int ID { get; set; }
        public event Action<Archer> OnReturnToPool;

        private void Awake()
        {
            if (Director.Instance != null)
            {
                _gridNavigation = Director.Instance._gridNavigation;
                _projectiles = Director.Instance._projectiles;
                _units = Director.Instance._units;
            }
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }


        public void Initialize(Vector3 startPosition)
        {
            GridNode startNode = _gridNavigation.GetClosestWalkableNode(startPosition);

            if (startNode != null)
            {
                _currentNode = startNode;
                transform.position = startNode.WorldPosition;
            }
            else
            {
                transform.position = startPosition;
                _currentNode = null;
            }

            _attackTimer = UnityEngine.Random.Range(0f, _attackCooldown * 0.5f);
            _currentPath.Clear();
            _currentPathIndex = 0;
            _lastTargetGridPos = null;
            gameObject.SetActive(true);
        }

        public void ManualUpdate(float deltaTime)
        {
            if (_currentTarget == null || !_currentTarget.gameObject.activeSelf)
            {
                FindTarget();
            }

            if (_currentTarget != null && _currentTarget.gameObject.activeSelf)
            {
                float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.transform.position);

                if (distanceToTarget <= _attackRange)
                {
                    StopMovement();

                    FaceTarget(_currentTarget.transform.position);

                    _attackTimer -= deltaTime;
                    if (_attackTimer <= 0)
                    {
                        Attack();
                        _attackTimer = _attackCooldown;
                    }
                }
                else
                {
                    MoveToTarget(deltaTime);
                }
            }
            else
            {
                Patrol(deltaTime);
            }
        }

        private void FindTarget()
        {
            if (_units != null && _units.Count() > 0)
            {
                float closestDistance = float.MaxValue;
                Unit closestUnit = null;
                float searchRange = _attackRange * 3f;
                float searchRangeSquared = searchRange * searchRange;

                foreach (var pool in _units)
                {
                    foreach (var unit in pool)
                    {
                        if (unit.gameObject.activeSelf)
                        {
                            float distanceSquared = Vector3.SqrMagnitude(unit.transform.position - transform.position);
                            if (distanceSquared < searchRangeSquared && distanceSquared < closestDistance)
                            {
                                closestDistance = distanceSquared;
                                closestUnit = unit;
                            }
                        }
                    }
                }

                _currentTarget = closestUnit;
                _lastTargetGridPos = null;

                if (_currentTarget != null)
                {
                    UpdatePathToTarget();
                }
            }
        }

        private void UpdatePathToTarget()
        {
            if (_currentTarget == null || _currentNode == null) return;

            Vector2Int targetGridPos = _gridNavigation.WorldToGridPosition(_currentTarget.transform.position);

            if (_lastTargetGridPos.HasValue && _lastTargetGridPos.Value == targetGridPos)
                return;

            _lastTargetGridPos = targetGridPos;

            GridNode targetNode = _gridNavigation.GetClosestWalkableNode(_currentTarget.transform.position);
            if (targetNode == null) return;

            _currentPath = _gridNavigation.FindPath(_currentNode, targetNode);
            _currentPathIndex = 0;
        }

        private void MoveToTarget(float deltaTime)
        {
            UpdatePathToTarget();

            if (_currentPathIndex >= _currentPath.Count)
            {
                if (_currentPath.Count > 0 && _currentPathIndex == _currentPath.Count)
                {
                    MoveTowards(_currentTarget.transform.position, deltaTime);
                }
                else
                {
                    Patrol(deltaTime);
                }
                return;
            }

            GridNode targetNode = _currentPath[_currentPathIndex];
            MoveTowards(targetNode.WorldPosition, deltaTime);

            if (Vector3.Distance(transform.position, targetNode.WorldPosition) < 0.1f)
            {
                _currentNode = targetNode;
                _currentPathIndex++;
            }
        }

        private void MoveTowards(Vector3 targetPosition, float deltaTime)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * _moveSpeed * deltaTime;

            // Поворачиваем спрайт в сторону движения
            UpdateSpriteFlip(direction.x);

            UpdateCurrentNode();
        }

        private void UpdateSpriteFlip(float directionX)
        {
            if (_spriteRenderer == null) return;

            // Простая логика:
            // directionX > 0 - цель/движение вправо, не зеркалим (смотрим вправо)
            // directionX < 0 - цель/движение влево, зеркалим (смотрим влево)

            if (directionX > 0)
            {
                _spriteRenderer.flipX = false;
            }
            else if (directionX < 0)
            {
                _spriteRenderer.flipX = true;
            }
        }

        private void UpdateCurrentNode()
        {
            if (_gridNavigation == null) return;

            GridNode closestNode = _gridNavigation.GetClosestWalkableNode(transform.position);
            if (closestNode != null && Vector3.Distance(transform.position, closestNode.WorldPosition) < 0.5f)
            {
                _currentNode = closestNode;
            }
        }

        private void Patrol(float deltaTime)
        {
            if (_currentNode == null) return;

            if (_currentPathIndex >= _currentPath.Count)
            {
                var neighbors = _gridNavigation.GetNeighbors(_currentNode);
                if (neighbors.Count > 0)
                {
                    GridNode randomNeighbor = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
                    _currentPath = new List<GridNode> { randomNeighbor };
                    _currentPathIndex = 0;
                }
            }

            if (_currentPathIndex < _currentPath.Count)
            {
                MoveTowards(_currentPath[_currentPathIndex].WorldPosition, deltaTime);

                if (Vector3.Distance(transform.position, _currentPath[_currentPathIndex].WorldPosition) < 0.1f)
                {
                    _currentNode = _currentPath[_currentPathIndex];
                    _currentPathIndex++;
                }
            }
        }

        private void StopMovement()
        {
            _currentPath.Clear();
            _currentPathIndex = 0;
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            // Определяем направление к цели
            float directionX = targetPosition.x - transform.position.x;

            // Поворачиваем спрайт лицом к цели
            UpdateSpriteFlip(directionX);
        }

        private void Attack()
        {
            if (_currentTarget == null || _projectilePrefab == null) return;

            var projectile = _projectiles[_projectilePrefab].Get;
            projectile.PrepareData(transform.position, _currentTarget, _damage, _attackElemental);
        }

        public void ReturnToPool()
        {
            _currentNode = null;
            _currentTarget = null;
            _currentPath.Clear();
            _currentPathIndex = 0;
            _lastTargetGridPos = null;

            OnReturnToPool?.Invoke(this);
            gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}