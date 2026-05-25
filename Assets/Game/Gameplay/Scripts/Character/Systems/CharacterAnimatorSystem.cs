using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

namespace SampleProject
{
    public sealed class CharacterAnimatorSystem : IEcsUpdate
    {
        private EcsPool<AnimatorComponent> animatorPool;

        private EcsPool<MoveStepData> moveStep;
        private EcsPool<HitDuration> attackPool;
        private EcsPool<GatherDuration> gatherPool;
        private EcsPool<MoveToPositionData> moveToPosition;
        private EcsPool<HitPointsComponent> hitPointsPool;

        void IEcsUpdate.Update(int entity)
        {
            if (!this.animatorPool.HasComponent(entity))
                return;

            ref var animatorComponent = ref this.animatorPool.GetComponent(entity);
            if (animatorComponent.value == null)
                return;
            
            var animatorState = this.ResolveState(entity);
            animatorComponent.value.ChangeState(animatorState);
        }

        private int ResolveState(int entity)
        {
            if (this.hitPointsPool.HasComponent(entity))
            {
                ref var hp = ref this.hitPointsPool.GetComponent(entity);
                if (hp.current <= 0)
                {
                    return AnimatorStateId.DEATH;
                }
            }



            if (this.attackPool.HasComponent(entity))
            {
                return AnimatorStateId.ATTACK;
            }

            if (this.gatherPool.HasComponent(entity))
            {
                return AnimatorStateId.GATHERING;
            }

        if (this.moveStep.HasComponent(entity) || this.moveToPosition.HasComponent(entity))
    {
        return AnimatorStateId.MOVE;
    }
            return AnimatorStateId.IDLE;
        }
    }
}