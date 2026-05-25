using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class AttackTargetSystem : IEcsFixedUpdate
    {
        private EcsPool<AttackTarget> targetPool;
        private EcsPool<HitRequest> hitRequestPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;

        private EcsPool<CombatComponent> combatPool;
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<HitPointsComponent> hitPointsPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.targetPool.HasComponent(entity))
                return;

            if (this.hitPointsPool.HasComponent(entity))
            {
                ref var myHp = ref this.hitPointsPool.GetComponent(entity);
                if (myHp.current <= 0)
                {
                    this.targetPool.RemoveComponent(entity);
                    this.hitRequestPool.RemoveComponent(entity);
                    return;
                }
            }

            ref var targetId = ref this.targetPool.GetComponent(entity).targetId;

            // Проверяем, жива ли цель
            if (this.hitPointsPool.HasComponent(targetId))
            {
                ref var targetHp = ref this.hitPointsPool.GetComponent(targetId);
                if (targetHp.current <= 0)
                {
                    this.targetPool.RemoveComponent(entity);
                    this.hitRequestPool.RemoveComponent(entity);
                    this.moveToPositionPool.RemoveComponent(entity);
                    return;
                }
            }

            // Проверяем, существует ли Transform цели
            if (!this.transformPool.HasComponent(targetId))
            {
                this.targetPool.RemoveComponent(entity);
                this.hitRequestPool.RemoveComponent(entity);
                this.moveToPositionPool.RemoveComponent(entity);
                return;
            }

            var isnull = this.transformPool.GetComponent(entity).value;
            if (isnull == null) 
                return;



            var myPosition = this.transformPool.GetComponent(entity).value.position;
            var targetPosition = this.transformPool.GetComponent(targetId).value.position;
            ref var minDistance = ref this.combatPool.GetComponent(entity).minDistance;
            
            if (Vector3.Distance(myPosition, targetPosition) <= minDistance)
            {
                //Attack target:
                this.moveToPositionPool.RemoveComponent(entity);
                this.hitRequestPool.SetComponent(entity, new HitRequest
                {
                    targetId = targetId
                });
            }
            else
            {
                //Move to target:
                this.hitRequestPool.RemoveComponent(entity);
                this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    destination = targetPosition,
                    stoppingDistance = minDistance
                });    
            }
        }
    }
}