using System.Collections.Generic;
using Game.GameEngine.Ecs;
using UnityEngine;

namespace SampleProject
{
    public class UnitRegistry : MonoBehaviour
    {
        public static UnitRegistry Instance { get; private set; }

        private List<Entity> allUnits = new List<Entity>();
        private float lastUpdateTime;
        private float updateInterval = 0.5f;

        public IReadOnlyList<Entity> AllUnits => this.allUnits;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void Update()
        {
            // Обновляем список каждые 0.5 секунды
            if (Time.time - this.lastUpdateTime > this.updateInterval)
            {
                this.RefreshUnitList();
                this.lastUpdateTime = Time.time;
            }
        }

        private void RefreshUnitList()
        {
            this.allUnits.Clear();

            // Находим всех юнитов на сцене
            var faction1Units = FindObjectsOfType<Faction1Entity>();
            var faction2Units = FindObjectsOfType<Faction2Entity>();

            foreach (var unit in faction1Units)
            {
                if (unit != null && unit.IsExists())
                    this.allUnits.Add(unit);
            }

            foreach (var unit in faction2Units)
            {
                if (unit != null && unit.IsExists())
                    this.allUnits.Add(unit);
            }
        }

        public void RegisterUnit(Entity unit)
        {
            if (!this.allUnits.Contains(unit))
                this.allUnits.Add(unit);
        }

        public void UnregisterUnit(Entity unit)
        {
            this.allUnits.Remove(unit);
        }
    }
}