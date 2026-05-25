using Game.GameEngine.Ecs;
using GameECS;
using SampleProject.ResourceObject;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SampleProject
{
    [RequireComponent(typeof(Entity))]
    public sealed class CharacterBehaviour : EntityBehaviour
    {
        protected override IEnumerable<IEcsSystem> ProvideSystems()
        {
            yield return new IdleStateMachine();

            yield return new CommandStateMachine(
                new CommandState_MoveToPosition(),
                new CommandState_AttackTarget(),
                new CommandState_PatrolByPoints(),
                new CommandState_GatherResource()
            );
            
            yield return new CharacterAnimatorSystem();
            yield return new CharacterRigidbodySystem();
        }

        protected override IEnumerable<(Type, IEcsObserver)> ProvideObservers()
        {
            yield return (typeof(AnimatorEvent), new CharacterAnimatorObserver());
            yield return (typeof(GatherCompleteEvent), new GatherCompleteObserver());
        }

        private sealed class GatherCompleteObserver : IEcsObserver<GatherCompleteEvent>
        {
            private readonly EcsPool<ResourceBag> resourceBagPool;

            public void Handle(int entity, GatherCompleteEvent eventData)
            {
                if (!this.resourceBagPool.HasComponent(entity))
                {
                    this.resourceBagPool.SetComponent(entity, new ResourceBag
                    {
                        resourceType = eventData.resourceType,
                        resourceAmount = 0
                    });
                }

                ref var bag = ref this.resourceBagPool.GetComponent(entity);
                bag.resourceAmount += eventData.amount;
                if (bag.resourceType != eventData.resourceType)
                {
                    Debug.Log($"Warning! Unit have gathered {eventData.resourceType}, while {bag.resourceType} was in bag.");
                }

                Debug.Log($"Gathered: +{eventData.amount} {eventData.resourceType}. " +
                          $"Total in bag: {bag.resourceAmount} {bag.resourceType}");
            }
        }

    }


}