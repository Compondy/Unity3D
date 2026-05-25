namespace Game.GameEngine.Ecs
{
    public struct GatherCompleteEvent
    {
        public int resourceId;
        public string resourceType;
        public int amount;
    }
}