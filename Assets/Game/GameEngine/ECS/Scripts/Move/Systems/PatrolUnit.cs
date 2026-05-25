using System.Collections.Generic;
using UnityEngine;
using Game.GameEngine.Ecs;

namespace SampleProject.Character
{
    public class PatrolUnit : MonoBehaviour
    {
        [Header("Patrol Points")]
        [SerializeField] private Transform[] patrolPoints;

        [Header("Settings")]
        [SerializeField] private bool startOnStart = true;
        [SerializeField] private float startDelay = 0.5f;

        private void Start()
        {
            if (this.startOnStart && this.patrolPoints.Length > 0)
            {
                this.Invoke(nameof(StartPatrol), this.startDelay);
            }
        }

        [ContextMenu("Start Patrol")]
        public void StartPatrol()
        {
            if (this.patrolPoints.Length == 0)
            {
                Debug.LogError($"No patrol points assigned to {this.name}!");
                return;
            }

            // Преобразуем Transform[] в List<Vector3>
            List<Vector3> points = new List<Vector3>();
            foreach (var point in this.patrolPoints)
            {
                if (point != null)
                {
                    points.Add(point.position);
                }
            }

            if (points.Count == 0)
            {
                Debug.LogError($"No valid patrol points for {this.name}!");
                return;
            }

            // Отправляем команду на патрулирование
            var command = new CommandRequest
            {
                type = CommandType.PATROL_BY_POINTS,
                status = CommandStatus.IDLE,
                args = points
            };

            this.GetComponent<Entity>().SetData(command);
            Debug.Log($"{this.name} started patrolling between {points.Count} points");
        }

        [ContextMenu("Stop Patrol")]
        public void StopPatrol()
        {
            this.GetComponent<Entity>().RemoveData<CommandRequest>();
            Debug.Log($"{this.name} stopped patrolling");
        }

        // Обновить точки патрулирования во время игры
        public void SetPatrolPoints(Transform[] newPoints)
        {
            this.patrolPoints = newPoints;

            // Если уже патрулирует - обновляем команду
            if (this.GetComponent<Entity>().HasData<CommandRequest>())
            {
                this.StartPatrol();
            }
        }
    }
}