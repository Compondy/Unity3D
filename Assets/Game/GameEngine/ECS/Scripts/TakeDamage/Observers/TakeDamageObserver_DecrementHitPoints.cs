using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class TakeDamageObserver_DecrementHitPoints : IEcsObserver<TakeDamageEvent>
    {
        private readonly EcsPool<HitPointsComponent> hitPointsPool;
        private EcsPool<AttackTarget> attackTargetPool;
        private EcsPool<HitRequest> hitRequestPool;

        void IEcsObserver<TakeDamageEvent>.Handle(int entity, TakeDamageEvent takeDamageEvent)
        {
            if (!this.hitPointsPool.HasComponent(entity))
                return;

            ref var hitPoints = ref this.hitPointsPool.GetComponent(entity);
            hitPoints.current -= takeDamageEvent.damage;

            if (hitPoints.current <= 0)
            {
                // Очищаем команду атаки умирающего юнита
                if (this.attackTargetPool.HasComponent(entity))
                {
                    this.attackTargetPool.RemoveComponent(entity);
                }

                // Очищаем HitRequest
                if (this.hitRequestPool.HasComponent(entity))
                {
                    this.hitRequestPool.RemoveComponent(entity);
                }

                // Отправляем событие смерти
                EcsModule.World.SendEvent(entity, new DestroyEvent());
            }
        }
    }
}