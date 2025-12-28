using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Netologia.TowerDefence.Systems
{
    public class GridNavigationSystem : MonoBehaviour
    {
        private Dictionary<Vector2Int, GridNode> _gridNodes = new Dictionary<Vector2Int, GridNode>();
        private Dictionary<GameObject, GridNode> _cellToNode = new Dictionary<GameObject, GridNode>();

        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Vector2 _gridOrigin = Vector2.zero;
        [SerializeField] private string _fieldCellTag = "FieldCell"; // Тег для FieldCell объектов

        public float CellSize => _cellSize;

        private void Awake()
        {
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            // Находим все объекты с тегом FieldCell
            var fieldCells = GameObject.FindGameObjectsWithTag(_fieldCellTag);

            Debug.Log($"Found {fieldCells.Length} FieldCell objects");

            foreach (var cell in fieldCells)
            {
                Vector3 worldPos = cell.transform.position;
                Vector2Int gridPos = WorldToGridPosition(worldPos);

                // Проверяем, можно ли ходить по этой клетке
                // Можно добавить компонент для определения walkable
                bool isWalkable = true;

                var gridNode = new GridNode(cell, gridPos, isWalkable);

                if (!_gridNodes.ContainsKey(gridPos))
                {
                    _gridNodes[gridPos] = gridNode;
                    _cellToNode[cell] = gridNode;
                }
                else
                {
                    Debug.LogWarning($"Duplicate grid position at {gridPos} for cell {cell.name}");
                }
            }

            Debug.Log($"GridNavigationSystem initialized with {_gridNodes.Count} nodes");
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt((worldPosition.x + 0.25f - _gridOrigin.x) / _cellSize);
            int y = Mathf.RoundToInt((worldPosition.y + 0.25f - _gridOrigin.y) / _cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            float x = gridPosition.x * _cellSize + _gridOrigin.x - 0.25f;
            float y = gridPosition.y * _cellSize + _gridOrigin.y - 0.25f;
            return new Vector3(x, y, 0);
        }

        public GridNode GetNodeAtWorldPosition(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGridPosition(worldPosition);
            return GetNodeAtGridPosition(gridPos);
        }

        public GridNode GetNodeAtGridPosition(Vector2Int gridPosition)
        {
            _gridNodes.TryGetValue(gridPosition, out var node);
            return node;
        }

        public GridNode GetNodeForCell(GameObject cellObject)
        {
            _cellToNode.TryGetValue(cellObject, out var node);
            return node;
        }

        public List<GridNode> GetNeighbors(GridNode node)
        {
            var neighbors = new List<GridNode>();
            Vector2Int[] directions =
            {
                new Vector2Int(1, 0),  // вправо
                new Vector2Int(-1, 0), // влево
                new Vector2Int(0, 1),  // вверх
                new Vector2Int(0, -1)  // вниз
            };

            foreach (var dir in directions)
            {
                Vector2Int neighborPos = node.GridPosition + dir;
                if (_gridNodes.TryGetValue(neighborPos, out var neighborNode) && neighborNode.IsWalkable)
                {
                    neighbors.Add(neighborNode);
                }
            }

            return neighbors;
        }

        public List<GridNode> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            GridNode startNode = GetNodeAtWorldPosition(startPos);
            GridNode targetNode = GetNodeAtWorldPosition(targetPos);

            if (startNode == null || targetNode == null || !targetNode.IsWalkable)
                return new List<GridNode>();

            return FindPath(startNode, targetNode);
        }

        public List<GridNode> FindPath(GridNode startNode, GridNode targetNode)
        {
            // Сброс всех узлов
            foreach (var node in _gridNodes.Values)
                node.ResetPathfinding();

            var openSet = new List<GridNode>();
            var closedSet = new HashSet<GridNode>();

            startNode.GCost = 0;
            startNode.HCost = startNode.GetDistance(targetNode);
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // Найти узел с наименьшей F стоимостью
                GridNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentNode.FCost ||
                        (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Если достигли цели
                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                // Проверяем соседей
                foreach (var neighbor in GetNeighbors(currentNode))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    int newMovementCostToNeighbor = currentNode.GCost + currentNode.GetDistance(neighbor);
                    if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newMovementCostToNeighbor;
                        neighbor.HCost = neighbor.GetDistance(targetNode);
                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            // Путь не найден
            return new List<GridNode>();
        }

        private List<GridNode> RetracePath(GridNode startNode, GridNode endNode)
        {
            var path = new List<GridNode>();
            GridNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        public GridNode GetClosestWalkableNode(Vector3 worldPosition)
        {
            Vector2Int centerGridPos = WorldToGridPosition(worldPosition);

            // Сначала проверяем точную позицию
            if (_gridNodes.TryGetValue(centerGridPos, out var node) && node.IsWalkable)
                return node;

            // Поиск в радиусе
            for (int radius = 1; radius <= 5; radius++)
            {
                // Проверяем все клетки на этом "кольце"
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                            continue;

                        Vector2Int gridPos = centerGridPos + new Vector2Int(x, y);
                        if (_gridNodes.TryGetValue(gridPos, out node) && node.IsWalkable)
                        {
                            return node;
                        }
                    }
                }
            }

            return null;
        }

        public GridNode GetRandomWalkableNode()
        {
            var walkableNodes = _gridNodes.Values.Where(n => n.IsWalkable).ToList();
            if (walkableNodes.Count == 0) return null;

            return walkableNodes[Random.Range(0, walkableNodes.Count)];
        }

        private void OnDrawGizmos()
        {
            if (_gridNodes == null || _gridNodes.Count == 0) return;

            // Рисуем сетку
            Gizmos.color = Color.cyan;
            foreach (var node in _gridNodes.Values)
            {
                if (node.IsWalkable)
                {
                    Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * _cellSize * 0.8f);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * _cellSize * 0.8f);
                    Gizmos.color = Color.cyan;
                }
            }
        }
    }
}