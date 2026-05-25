using Game.GameEngine.Ecs;
using UnityEngine;

namespace SampleProject.Base
{
    public sealed class CommandCenterEntity : Entity
    {

        [Header("Command Center")]
        [SerializeField] private float resourceDropRadius = 2f;

        protected override void Init()
        {
            this.SetData(new TransformComponent
            {
                value = this.transform,
                radius = this.resourceDropRadius
            });
            this.SetData(new CommandCenterComponent());
        }

    }
    public struct CommandCenterComponent
    {
    }
}