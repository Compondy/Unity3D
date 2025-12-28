using System.Collections.Generic;
using UnityEngine;

namespace Netologia.TowerDefence.Systems
{
    public class GridNode
    {
        public GameObject CellObject { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public Vector3 WorldPosition => CellObject.transform.position;

        // Для алгоритма A*
        public int GCost; // Расстояние от старта
        public int HCost; // Эвристическое расстояние до цели
        public int FCost => GCost + HCost;
        public GridNode Parent { get; set; }

        public bool IsWalkable { get; set; }

        public GridNode(GameObject cellObject, Vector2Int gridPosition, bool isWalkable = true)
        {
            CellObject = cellObject;
            GridPosition = gridPosition;
            IsWalkable = isWalkable;
        }

        public void ResetPathfinding()
        {
            GCost = int.MaxValue;
            HCost = 0;
            Parent = null;
        }

        public int GetDistance(GridNode other)
        {
            // Манхэттенское расстояние (только по горизонтали/вертикали)
            int xDistance = Mathf.Abs(GridPosition.x - other.GridPosition.x);
            int yDistance = Mathf.Abs(GridPosition.y - other.GridPosition.y);
            return xDistance + yDistance;
        }

        public override string ToString()
        {
            return $"GridNode({GridPosition.x}, {GridPosition.y})";
        }
    }
}