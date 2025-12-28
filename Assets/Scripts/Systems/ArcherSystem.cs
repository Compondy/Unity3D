using Netologia.Behaviours;
using Netologia.TowerDefence;
using Netologia.TowerDefence.Behaviors;
using UnityEngine;

namespace Netologia.Systems
{
    public class ArcherSystem : GameObjectPoolContainer<Archer>, Director.IManualUpdate
    {
        public void ManualUpdate()
        {
            foreach (var pool in _pools.Values)
            {
                foreach (var archer in pool)
                {
                    if (archer.gameObject.activeSelf)
                    {
                        archer.ManualUpdate(TimeManager.DeltaTime);
                    }
                }
            }
        }

        protected override bool IncorrectPrefabParameters(Archer prefab, out string message)
        {
            message = string.Empty;
            if (prefab == null)
            {
                message = "Prefab is null!";
                return true;
            }
            return false;
        }
    }
}