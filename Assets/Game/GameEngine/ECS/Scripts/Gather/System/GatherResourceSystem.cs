using GameECS;
using SampleProject.Base;
using SampleProject.ResourceObject;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class GatherResourceSystem : IEcsFixedUpdate
    {
        private EcsPool<GatherTarget> targetResourcePool;
        private EcsPool<GatherState> gatherStatePool;
        private EcsPool<GatherDuration> gatherDurationPool;
        private EcsPool<ResourceBag> resourceBagPool;

        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<ResourceComponent> resourceComponentPool;

        private EcsWorld world;

        private ResourceEntity[] cachedResources;
        private float lastFindTime = 0f;
        private const float FIND_INTERVAL = 2f;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.targetResourcePool.HasComponent(entity))
            {
                return;
            }

            ref var state = ref this.gatherStatePool.GetComponent(entity);
            if (state == GatherState.MOVE_TO_RESOURCE)
            {
                this.UpdateMoveToResourceState(entity);
            }
            else if (state == GatherState.GATHERING)
            {
                this.UpdateGatheringState(entity);
            }
            else if (state == GatherState.MOVE_TO_HOME)
            {
                this.UpdateMoveToBaseState(entity);
            }
        }

        private int FindNearestResource(Vector3 position)
        {
            if (Time.time - this.lastFindTime > FIND_INTERVAL)
            {
                this.cachedResources = GameObject.FindObjectsOfType<ResourceEntity>();
                this.lastFindTime = Time.time;
            }

            if (this.cachedResources == null || this.cachedResources.Length == 0)
            {
                return -1;
            }

            ResourceEntity nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var resource in this.cachedResources)
            {
                if (resource == null || !resource.IsExists())
                    continue;

                if (this.world.HasComponent<ResourceComponent>(resource.Id))
                {
                    ref var resourceComp = ref this.world.GetComponent<ResourceComponent>(resource.Id);
                    if (resourceComp.amount <= 0)
                        continue;
                }

                float distance = Vector3.Distance(position, resource.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = resource;
                }
            }

            return nearest != null ? nearest.Id : -1;
        }

        private void UpdateMoveToResourceState(int entity)
        {
            if (!this.moveToPositionPool.HasComponent(entity))
            {
                this.AddMoveToResourceData(entity);
            }

            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);

            if (!moveData.isReached)
            {
                return;
            }

            if (this.resourceBagPool.HasComponent(entity))
            {
                this.SetMoveToHomeState(entity);
            }
            else
            {
                this.SetGatheringState(entity);
            }
        }

        private void UpdateGatheringState(int entity)
        {
            if (this.gatherDurationPool.HasComponent(entity))
            {
                return;
            }

            ref var target = ref this.targetResourcePool.GetComponent(entity);
            int resourceId = target.targetId;

            if (!this.world.IsEntityExists(resourceId))
            {
                Debug.Log("Resource is not exist, search for another one...");

                ref var myTransform = ref this.transformPool.GetComponent(entity);
                int newResourceId = this.FindNearestResource(myTransform.value.position);

                if (newResourceId != -1)
                {
                    target.targetId = newResourceId;
                    this.SetMoveToResourceState(entity);
                }
                else
                {
                    this.StopGathering(entity);
                }
                return;
            }

            if (!this.resourceComponentPool.HasComponent(resourceId))
            {
                this.StopGathering(entity);
                return;
            }

            ref var resource = ref this.resourceComponentPool.GetComponent(resourceId);

            if (resource.amount <= 0)
            {
                Debug.Log("Resource is not exist, search for another one...");

                ref var myTransform = ref this.transformPool.GetComponent(entity);
                int newResourceId = this.FindNearestResource(myTransform.value.position);

                if (newResourceId != -1)
                {
                    target.targetId = newResourceId;
                    this.SetMoveToResourceState(entity);
                }
                else
                {
                    this.StopGathering(entity);
                }
                return;
            }

            int gatherAmount = Mathf.Min(5, resource.amount);

            this.resourceBagPool.SetComponent(entity, new ResourceBag
            {
                resourceType = resource.type,
                resourceAmount = gatherAmount
            });

            resource.amount -= gatherAmount;

            this.world.SendEvent(entity, new GatherCompleteEvent
            {
                resourceId = resourceId,
                resourceType = resource.type,
                amount = gatherAmount
            });

            Debug.Log($"Unit have gathered {gatherAmount} {resource.type}. Remaining: {resource.amount}");

            if (resource.amount <= 0)
            {
                if (this.world.HasComponent<GameObjectComponent>(resourceId))
                {
                    this.world.SendEvent(resourceId, new DestroyEvent());
                    Debug.Log($"Resource {resource.type} is exhausted and destroyed!");
                }
            }

            this.SetMoveToHomeState(entity);
        }

        private void UpdateMoveToBaseState(int entity)
        {
            if (!this.moveToPositionPool.HasComponent(entity))
            {
                return;
            }

            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);
            if (!moveData.isReached)
            {
                return;
            }

            // Сдача ресурсов на базу
            if (this.resourceBagPool.HasComponent(entity))
            {
                var gatherData = this.resourceBagPool.GetComponent(entity);
                this.resourceBagPool.RemoveComponent(entity);
                Debug.Log($"Dropped at base: {gatherData.resourceAmount} {gatherData.resourceType}");
            }

            ref var resourceId = ref this.targetResourcePool.GetComponent(entity).targetId;
            ref var myTransform = ref this.transformPool.GetComponent(entity);

            // Проверяем, существует ли ещё текущий ресурс
            if (!this.world.IsEntityExists(resourceId))
            {
                // Ищем новый ресурс
                int newResourceId = this.FindNearestResource(myTransform.value.position);

                if (newResourceId != -1)
                {
                    this.targetResourcePool.GetComponent(entity).targetId = newResourceId;
                    Debug.Log($"Found new resource, continue gathering");
                }
                else
                {
                    Debug.Log("No more resources!");
                    this.StopGathering(entity);
                    return;
                }
            }
            else
            {
                // Проверяем, не исчерпан ли ресурс
                if (this.resourceComponentPool.HasComponent(resourceId))
                {
                    ref var resource = ref this.resourceComponentPool.GetComponent(resourceId);
                    if (resource.amount <= 0)
                    {
                        int newResourceId = this.FindNearestResource(myTransform.value.position);

                        if (newResourceId != -1)
                        {
                            this.targetResourcePool.GetComponent(entity).targetId = newResourceId;
                            Debug.Log($"Resource is exhaused, found new one");
                        }
                        else
                        {
                            Debug.Log("No more resources!");
                            this.StopGathering(entity);
                            return;
                        }
                    }
                }
            }

            this.SetMoveToResourceState(entity);
        }

        private void SetMoveToResourceState(int entity)
        {
            this.gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_RESOURCE);
            this.AddMoveToResourceData(entity);
        }

        private void AddMoveToResourceData(int entity)
        {
            ref var resourceId = ref this.targetResourcePool.GetComponent(entity).targetId;

            if (!this.transformPool.HasComponent(resourceId))
                return;

            ref var resourceTransform = ref this.transformPool.GetComponent(resourceId);

            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = resourceTransform.value.position,
                stoppingDistance = resourceTransform.radius
            });
        }

        private void SetGatheringState(int entity)
        {
            ref var target = ref this.targetResourcePool.GetComponent(entity);

            float duration = 3f;
            if (this.resourceComponentPool.HasComponent(target.targetId))
            {
                duration = this.resourceComponentPool.GetComponent(target.targetId).gatherDuration;
            }

            this.gatherStatePool.SetComponent(entity, GatherState.GATHERING);
            this.gatherDurationPool.SetComponent(entity, new GatherDuration
            {
                remainingTime = duration
            });
        }

        private void SetMoveToHomeState(int entity)
        {
            this.gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_HOME);

            var commandCenter = GameObject.FindObjectOfType<CommandCenterEntity>();
            if (commandCenter == null)
            {
                this.StopGathering(entity);
                return;
            }

            if (!this.transformPool.HasComponent(commandCenter.Id))
                return;

            ref var homeTransform = ref this.transformPool.GetComponent(commandCenter.Id);

            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = homeTransform.value.position,
                stoppingDistance = homeTransform.radius
            });
        }

        private void StopGathering(int entity)
        {
            this.moveToPositionPool.RemoveComponent(entity);
            this.gatherStatePool.RemoveComponent(entity);
            this.targetResourcePool.RemoveComponent(entity);
            this.gatherDurationPool.RemoveComponent(entity);
        }
    }
}