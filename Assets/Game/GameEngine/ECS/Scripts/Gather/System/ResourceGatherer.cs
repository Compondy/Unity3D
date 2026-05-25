using UnityEngine;
using Game.GameEngine.Ecs;
using SampleProject.ResourceObject;

namespace SampleProject.Character
{
    public class ResourceGatherer : MonoBehaviour
    {
        [Header("Target Resource")]
        [SerializeField] private ResourceEntity targetResource;

        [Header("Settings")]
        [SerializeField] private bool startOnStart = true;
        [SerializeField] private float startDelay = 1f;

        private void Start()
        {
            if (this.startOnStart)
            {
                this.Invoke(nameof(StartGathering), this.startDelay);
            }
        }

        [ContextMenu("Start Gathering")]
        public void StartGathering()
        {
            if (this.targetResource == null)
            {
                Debug.LogError($"No target resource assigned to {this.name}!");
                return;
            }

            var command = new CommandRequest
            {
                type = CommandType.GATHER_RESOURCE,
                status = CommandStatus.IDLE,
                args = this.targetResource
            };

            this.GetComponent<Entity>().SetData(command);
            Debug.Log($"{this.name} started gathering {this.targetResource.name}");
        }

        [ContextMenu("Stop Gathering")]
        public void StopGathering()
        {
            this.GetComponent<Entity>().RemoveData<CommandRequest>();
            Debug.Log($"{this.name} stopped gathering");
        }

        // Опционально: выбор ресурса по типу
        public void SetTargetResource(ResourceEntity resource)
        {
            this.targetResource = resource;
        }
    }
}