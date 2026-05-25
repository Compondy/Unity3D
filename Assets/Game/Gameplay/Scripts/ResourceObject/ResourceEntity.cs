using Game.GameEngine.Ecs;
using UnityEngine;

namespace SampleProject.ResourceObject
{
    public sealed class ResourceEntity : Entity
    {

        [Header("Resource Settings")]
        [SerializeField] private string resourceType = "Minerals";
        [SerializeField] private int resourceAmount = 10;
        [SerializeField] private float gatherRadius = 1.5f;
        [SerializeField] private float gatherDuration = 3f;

        protected override void Init()
        {

	    this.SetData(new GameObjectComponent
            {
                value = this.gameObject
            });

            this.SetData(new TransformComponent
            {
                value = this.transform,
                radius = this.gatherRadius
            });
            this.SetData(new ResourceComponent
            {
                type = this.resourceType,
                amount = this.resourceAmount,
                gatherDuration = this.gatherDuration
            });
        }

    }
    public struct ResourceComponent
    {
        public string type;
        public int amount;
        public float gatherDuration;
    }
}