using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

namespace SampleProject
{
    [RequireComponent(typeof(Entity))]
    public sealed class UnitBehaviour : EntityBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float detectionRange = 15f;

        protected override IEnumerable<IEcsSystem> ProvideSystems()
        {
            yield return new UnitDetectionSystem(this.detectionRange);
            yield return new NavMeshMoveSystem();
            yield return new SeparationSystem();

            yield return new CommandStateMachine(
                new CommandState_MoveToPosition(),
                new CommandState_AttackTarget()
            );

            yield return new IdleStateMachine();

            yield return new CharacterAnimatorSystem();
            yield return new CharacterRigidbodySystem();
        }

        protected override IEnumerable<(Type, IEcsObserver)> ProvideObservers()
        {
            return new (Type, IEcsObserver)[]
            {
                (typeof(TakeDamageEvent), new TakeDamageObserver_DecrementHitPoints()),
                (typeof(TakeDamageEvent), new TakeDamageObserver_ChangeColor()),
                (typeof(DestroyEvent), new DestroyObserver_DisableGameObject())
            };
        }
    }
}