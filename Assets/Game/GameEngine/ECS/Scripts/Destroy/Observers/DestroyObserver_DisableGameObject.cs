using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class DestroyObserver_DisableGameObject : IEcsObserver<DestroyEvent>
    {
        private readonly EcsPool<GameObjectComponent> gameObjectPool;
        private readonly EcsPool<AnimatorComponent> animatorPool;
        private readonly EcsPool<RigidbodyComponent> rigidbodyPool;

        void IEcsObserver<DestroyEvent>.Handle(int entity, DestroyEvent destroyEvent)
        {
            if (!this.gameObjectPool.HasComponent(entity))
                return;

            ref var goComponent = ref this.gameObjectPool.GetComponent(entity);
            if (goComponent.value == null)
                return;

            GameObject obj = goComponent.value;


            if (this.animatorPool.HasComponent(entity))
            {
                ref var animatorComp = ref this.animatorPool.GetComponent(entity);
                if (animatorComp.value != null)
                {
                    animatorComp.value.ChangeState(AnimatorStateId.DEATH);
                }
            }

            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            if (this.rigidbodyPool.HasComponent(entity))
            {
                ref var rbComp = ref this.rigidbodyPool.GetComponent(entity);
                if (rbComp.value != null)
                {
                    rbComp.value.velocity = Vector3.zero;
                    //rbComp.value.isKinematic = true;
                }
            }
        }
    }
}