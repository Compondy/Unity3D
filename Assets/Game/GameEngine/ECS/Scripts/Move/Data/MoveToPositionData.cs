using System;
using UnityEngine;
using UnityEngine.AI;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct MoveToPositionData
    {
        public Vector3 destination;
        public float stoppingDistance;
        public bool isReached;

        // NavMesh данные
        [NonSerialized]
        public NavMeshPath path;
        [NonSerialized]
        public int currentCornerIndex;
    }
}