using Game.GameEngine.Ecs;

namespace SampleProject
{
    public static class TeamHelper
    {
        public static bool IsEnemy(this TeamComponent team, TeamComponent other)
        {
            return team.playerId != other.playerId;
        }

        public static bool IsAlly(this TeamComponent team, TeamComponent other)
        {
            return team.playerId == other.playerId;
        }
    }
}