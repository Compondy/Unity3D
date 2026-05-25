using GameECS;
using UnityEngine;
using UnityEngine.AI;

namespace Game.GameEngine.Ecs
{
    public sealed class NavMeshMoveSystem : IEcsFixedUpdate
    {
        private EcsPool<MoveToPositionData> movePool;
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<RigidbodyComponent> rigidbodyPool;
        private EcsPool<CommandRequest> commandPool;
        public void FixedUpdate(int entity)
        {

            if (!this.rigidbodyPool.HasComponent(entity))
            {
                return;
            }

            if (!this.movePool.HasComponent(entity))
                return;

            ref var moveData = ref this.movePool.GetComponent(entity);

            if (!this.transformPool.HasComponent(entity))
                return;

            ref var transformComp = ref this.transformPool.GetComponent(entity);
            Vector3 currentPosition = transformComp.value.position;

            // Проверка на достижение цели
            float distanceToTarget = Vector3.Distance(currentPosition, moveData.destination);
            if (distanceToTarget <= moveData.stoppingDistance)
            {
                moveData.isReached = true;

                // Останавливаем движение
                if (this.rigidbodyPool.HasComponent(entity))
                {
                    ref var rigidbody = ref this.rigidbodyPool.GetComponent(entity);
                    rigidbody.value.velocity = Vector3.zero;
                }

                this.movePool.RemoveComponent(entity);
                return;
            }

            moveData.isReached = false;

            // Создаём путь если его нет
            if (moveData.path == null)
            {
                moveData.path = new NavMeshPath();
                moveData.currentCornerIndex = 1;
                NavMesh.CalculatePath(currentPosition, moveData.destination, NavMesh.AllAreas, moveData.path);
            }

            // Если путь пустой или цель изменилась - пересчитываем
            if (moveData.path.corners.Length == 0)
            {
                NavMesh.CalculatePath(currentPosition, moveData.destination, NavMesh.AllAreas, moveData.path);
                moveData.currentCornerIndex = 1;
            }

            // Проверяем, не устарел ли путь
            if (moveData.path.corners.Length > 0)
            {
                Vector3 lastCorner = moveData.path.corners[moveData.path.corners.Length - 1];
                if (Vector3.Distance(lastCorner, moveData.destination) > moveData.stoppingDistance)
                {
                    NavMesh.CalculatePath(currentPosition, moveData.destination, NavMesh.AllAreas, moveData.path);
                    moveData.currentCornerIndex = 1;
                }
            }

            // Движение по точкам пути
            if (moveData.path.corners.Length > moveData.currentCornerIndex)
            {
                Vector3 targetCorner = moveData.path.corners[moveData.currentCornerIndex];
                Vector3 direction = (targetCorner - currentPosition).normalized;

                float speed = 5f;

                if (this.rigidbodyPool.HasComponent(entity))
                {
                    ref var rigidbody = ref this.rigidbodyPool.GetComponent(entity);
                    rigidbody.value.velocity = new Vector3(direction.x * speed, rigidbody.value.velocity.y, direction.z * speed);

                    // Поворот в сторону движения
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        rigidbody.value.rotation = Quaternion.Slerp(rigidbody.value.rotation, targetRotation, Time.fixedDeltaTime * 10f);
                    }
                }

                // Переход к следующей точке
                if (Vector3.Distance(currentPosition, targetCorner) <= 0.5f)
                {
                    moveData.currentCornerIndex++;
                }
            }
            else
            {
                // Останавливаем, если нет пути
                if (this.rigidbodyPool.HasComponent(entity))
                {
                    ref var rigidbody = ref this.rigidbodyPool.GetComponent(entity);
                    rigidbody.value.velocity = Vector3.zero;
                }
            }
           
        }
    }
}