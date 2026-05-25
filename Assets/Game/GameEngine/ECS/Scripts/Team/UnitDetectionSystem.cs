using GameECS;
using UnityEngine;
using SampleProject;

namespace Game.GameEngine.Ecs
{
    public sealed class UnitDetectionSystem : IEcsFixedUpdate
    {
        private readonly float detectionRange;
        private readonly EcsPool<AttackTarget> attackTargetPool;
        private readonly EcsPool<TransformComponent> transformPool;
        private readonly EcsPool<HitPointsComponent> hitPointsPool;
        private readonly EcsPool<TeamComponent> teamPool;

        private float lastFindTime;

        public UnitDetectionSystem(float detectionRange)
        {
            this.detectionRange = detectionRange;
        }

        public void FixedUpdate(int entity)
        {
            // Проверяем каждые 0.5 секунды
            if (Time.fixedTime - this.lastFindTime < 0.5f)
                return;

            this.lastFindTime = Time.fixedTime;

            // Если уже есть цель и она жива - не ищем новую
            if (this.attackTargetPool.HasComponent(entity))
            {
                ref var target = ref this.attackTargetPool.GetComponent(entity);
                if (this.hitPointsPool.HasComponent(target.targetId))
                {
                    ref var targetHp = ref this.hitPointsPool.GetComponent(target.targetId);
                    if (targetHp.current > 0)
                        return;
                }
            }

            // Ищем ближайшего врага через UnitRegistry
            int nearestEnemy = this.FindNearestEnemy(entity);

            if (nearestEnemy != -1)
            {
                this.attackTargetPool.SetComponent(entity, new AttackTarget
                {
                    targetId = nearestEnemy
                });
            }
            else
            {
                this.attackTargetPool.RemoveComponent(entity);
            }
        }

        private int FindNearestEnemy(int entity)
        {
            if (!this.transformPool.HasComponent(entity)) return -1;
            if (!this.teamPool.HasComponent(entity)) return -1;

            ref var myTransform = ref this.transformPool.GetComponent(entity);
            ref var myTeam = ref this.teamPool.GetComponent(entity);

            Vector3 myPosition = myTransform.value.position;
            int nearestId = -1;
            float nearestDistance = this.detectionRange;

            // Используем UnitRegistry для получения всех юнитов
            if (UnitRegistry.Instance == null)
            {
                Debug.LogWarning("UnitRegistry not found on scene!");
                return -1;
            }

            var allUnits = UnitRegistry.Instance.AllUnits;

            foreach (var unit in allUnits)
            {
                if (unit == null || !unit.IsExists()) continue;

                int targetId = unit.Id;
                if (targetId == entity) continue;

                if (!this.teamPool.HasComponent(targetId)) continue;
                if (!this.transformPool.HasComponent(targetId)) continue;
                if (!this.hitPointsPool.HasComponent(targetId)) continue;

                ref var targetTeam = ref this.teamPool.GetComponent(targetId);
                ref var targetHp = ref this.hitPointsPool.GetComponent(targetId);

                // Проверяем враг ли (разные playerId) и жив ли
                if (myTeam.playerId == targetTeam.playerId) continue;
                if (targetHp.current <= 0) continue;

                ref var targetTransform = ref this.transformPool.GetComponent(targetId);
                float distance = Vector3.Distance(myPosition, targetTransform.value.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestId = targetId;
                }
            }

            return nearestId;
        }
    }
}