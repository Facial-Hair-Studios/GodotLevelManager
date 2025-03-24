using Godot;

namespace Levels
{
    // This can be used to store level data
    // Save game data for games like platformer: Coins collected, Secrets found, times died etc.
    // Data for RPG games: EnemySpawns, CanBattle, EventTriggers and locations, MapState, etc.
    public partial class LevelDataCommon : Resource
    {
        public virtual void Create() { }
    }
}