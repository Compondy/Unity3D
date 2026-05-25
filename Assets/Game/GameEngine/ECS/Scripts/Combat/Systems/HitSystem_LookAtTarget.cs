using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class HitSystem_LookAtTarget : IEcsFixedUpdate
    {
        private EcsPool<HitRequest> hitRequestPool;
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<HitPointsComponent> hitPointsPool;
        private EcsEmitter<SmoothRotateEvent> rotateEmitter;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.hitRequestPool.HasComponent(entity))
                return;

            // Проверяем, жив ли атакующий
            if (this.hitPointsPool.HasComponent(entity))
            {
                ref var hp = ref this.hitPointsPool.GetComponent(entity);
                if (hp.current <= 0)
                {
                    this.hitRequestPool.RemoveComponent(entity);
                    return;
                }
            }

            ref var request = ref this.hitRequestPool.GetComponent(entity);

            // Проверяем, существует ли цель
            if (!this.transformPool.HasComponent(request.targetId))
            {
                this.hitRequestPool.RemoveComponent(entity);
                return;
            }

            // Проверяем, жива ли цель
            if (this.hitPointsPool.HasComponent(request.targetId))
            {
                ref var targetHp = ref this.hitPointsPool.GetComponent(request.targetId);
                if (targetHp.current <= 0)
                {
                    this.hitRequestPool.RemoveComponent(entity);
                    return;
                }
            }

            ref var myTransform = ref this.transformPool.GetComponent(entity).value;
            ref var targetTransform = ref this.transformPool.GetComponent(request.targetId).value;

            // Проверяем, не уничтожены ли Transform'ы
            if (myTransform == null || targetTransform == null)
            {
                this.hitRequestPool.RemoveComponent(entity);
                return;
            }

            var direction = (targetTransform.position - myTransform.position).normalized;
            this.rotateEmitter.SendEvent(entity, new SmoothRotateEvent
            {
                direction = direction
            });
        }
    }
}