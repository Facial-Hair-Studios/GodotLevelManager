using Godot;
using Levels;
using Godot.Collections;
using System.Linq;

public enum LevelLoadMode { Single, Additive }

[Tool]
public partial class LevelManagerData : Resource
{
    private Godot.Collections.Dictionary<string, long> _levels { get; set; } // Contain's _levels, _levels are defined as LevelCommon
    public string CurrentLevel { get; set; }
    [Export] public string NewGameScene { get; set; } // What scene starts when newgame on title screen
    public int Count => _levels.Count;
    public LevelManagerData() => _levels = new Godot.Collections.Dictionary<string, long>();
    public LevelManagerData(Godot.Collections.Dictionary<string, long> lev) => _levels = lev;

    // Returns the instantiated packed scene as a Level.
    public LevelCommon GetLevelByPath(string path) => ResourceLoader.Load<PackedScene>(path).Instantiate<LevelCommon>();
    public LevelCommon GetLevelAt(int idx) => ResourceLoader.Load<PackedScene>(ResourceUid.GetIdPath(_levels.ElementAt(idx).Value)).Instantiate<LevelCommon>();
    public LevelCommon GetLevelByName(string levelName) => ResourceLoader.Load<PackedScene>(ResourceUid.GetIdPath(_levels.First(x => x.Key == levelName).Value)).Instantiate<LevelCommon>();
    public PackedScene GetPackedLevel(string levelName) => ResourceLoader.Load<PackedScene>(ResourceUid.GetIdPath(_levels.First(x => x.Key == levelName).Value));

    public bool Exists(string levelName) => _levels.ContainsKey(levelName);

#if TOOLS

    [Export] public Godot.Collections.Dictionary<string, long> Levels { get => _levels; private set { _levels = value; } }

    public bool Add(string levelName, long uid)
    {
        _levels.Add(levelName, uid);
        return true;
    }

    public bool Remove(string levelName)
    {
        if (_levels.Any(x => x.Key == levelName.ToString()))
        {
            _levels.Remove(_levels.First(x => x.Key == levelName.ToString()).Key);
            return true;
        }
        GD.PrintErr("Level does not exist in manager");
        return false;
    }

    // Set the Level that starts when New game pressed on Title screen
    public void SetNewGameLevel(string levelName)
    {
        LevelCommon l = ResourceLoader.Load<PackedScene>(ResourceUid.GetIdPath(_levels.First(x => x.Key == levelName.ToString()).Value)).Instantiate<LevelCommon>();
        NewGameScene = levelName;
    }
#endif

};
