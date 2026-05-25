using Game.GameEngine.Ecs;
using UnityEngine;

namespace SampleProject
{
    public sealed class Faction1Entity : Entity
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int damage = 15;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1f;

        protected override void Init()
        {
            // Здоровье
            this.SetData(new HitPointsComponent
            {
                current = this.maxHealth,
                max = this.maxHealth
            });

            // Боевые характеристики
            this.SetData(new CombatComponent
            {
                damage = this.damage,
                minDistance = this.attackRange,
                timeBetweenAttack = this.attackCooldown,
                animationTime = 0.5f,
                damageType = DamageType.MELEE
            });

            // Сторона: playerId = 0 для синих
            this.SetData(new TeamComponent
            {
                playerId = 0
            });

            // Transform
            this.SetData(new TransformComponent
            {
                value = this.transform,
                radius = this.attackRange
            });

            // GameObject
            this.SetData(new GameObjectComponent
            {
                value = this.gameObject
            });

            var rb = this.GetComponent<Rigidbody>();
            if (rb != null)
            {
                this.SetData(new RigidbodyComponent { value = rb });
            }
            else
            {
                Debug.LogError($"No Rigidbody on {this.name}!");
            }

            // Animator
            var animator = this.GetComponentInChildren<AnimatorMachine>();
            if (animator != null)
            {
                this.SetData(new AnimatorComponent { value = animator });
            }
            else
            {
                Debug.LogError($"No AnimatorMachine found on {this.name}!");
            }

        }



        

    }
}