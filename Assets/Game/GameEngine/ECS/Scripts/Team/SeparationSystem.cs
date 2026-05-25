using GameECS;
using UnityEngine;
using SampleProject;

namespace Game.GameEngine.Ecs
{
    public sealed class SeparationSystem : IEcsFixedUpdate
    {
        private const float SEPARATION_RADIUS = 1.5f;
        private const float SEPARATION_WEIGHT = 3f;

        private EcsPool<TransformComponent> transformPool;
        private EcsPool<RigidbodyComponent> rigidbodyPool;
        private EcsPool<TeamComponent> teamPool;

        public void FixedUpdate(int entity)
        {
            if (!this.rigidbodyPool.HasComponent(entity)) return;
            if (!this.teamPool.HasComponent(entity)) return;

            ref var transform = ref this.transformPool.GetComponent(entity).value;
            ref var rigidbody = ref this.rigidbodyPool.GetComponent(entity).value;
            ref var myTeam = ref this.teamPool.GetComponent(entity);

            Vector3 separationForce = Vector3.zero;
            int neighborCount = 0;

            // Используем UnitRegistry
            if (UnitRegistry.Instance == null) return;

            var allUnits = UnitRegistry.Instance.AllUnits;

            foreach (var unit in allUnits)
            {
                if (unit == null || !unit.IsExists()) continue;

                int otherId = unit.Id;
                if (otherId == entity) continue;

                if (!this.transformPool.HasComponent(otherId)) continue;
                if (!this.teamPool.HasComponent(otherId)) continue;

                ref var otherTeam = ref this.teamPool.GetComponent(otherId);

                // Разделяемся только с юнитами из той же команды
                if (myTeam.playerId != otherTeam.playerId) continue;

                ref var otherTransform = ref this.transformPool.GetComponent(otherId);
                Vector3 direction = transform.position - otherTransform.value.position;
                float distance = direction.magnitude;

                if (distance < SEPARATION_RADIUS && distance > 0.01f)
                {
                    float strength = (SEPARATION_RADIUS - distance) / SEPARATION_RADIUS;
                    separationForce += direction.normalized * strength;
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                separationForce /= neighborCount;
                separationForce *= SEPARATION_WEIGHT;
                rigidbody.velocity += separationForce * Time.fixedDeltaTime;

                float maxSpeed = 5f;
                if (rigidbody.velocity.magnitude > maxSpeed)
                {
                    rigidbody.velocity = rigidbody.velocity.normalized * maxSpeed;
                }
            }
        }
    }
}