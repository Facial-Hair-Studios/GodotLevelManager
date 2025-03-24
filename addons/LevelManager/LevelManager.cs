using Godot;
using System;
using Levels;

public partial class LevelManager : Node
{
    // Because Plugin exists outside the SceneTree, we create our own _Tree or refrence to one.
    private static SceneTree s_tree;
    private static LevelManager s_levelManager;

    public static Action ResetLevel;
    public static Action StartNewGame;

    public static Action<PackedScene> LevelLoaded;
    public static Action<LevelCommon, LevelCommon> LevelChanged;


    private LevelCommon _currentLevel;
    private string _levelManagerPath = "res://Assets/Data/LevelManagerData.tres";
    private LevelManagerData _managerData;

    public int LevelCount => _managerData.Count;
    public string NewGameScene => _managerData.NewGameScene;
    public LevelCommon CurrentLevel => _currentLevel;

    public static new SceneTree GetTree() => s_tree;
    public bool Exists(string levelName) => _managerData.Exists(levelName);
    public LevelCommon GetLevelAt(int idx) => _managerData.GetLevelAt(idx);
    public LevelCommon GetLevelByPath(string path) => _managerData.GetLevelByPath(path);
    public LevelCommon GetLevelByName(string levelName) => _managerData.GetLevelByName(levelName);

    public PackedScene LoadLevel(string levelName)
    {
        var packed = _managerData.GetPackedLevel(levelName);
        LevelLoaded.Invoke(packed);
        return packed;
    }

    public bool Load()
    {
        if (ResourceLoader.Exists(_levelManagerPath))
        {
            try
            {
                _managerData = ResourceLoader.Load<LevelManagerData>(_levelManagerPath);
            }
            catch (InvalidCastException)
            {
                GD.PrintErr("LevelManager Data Invalid!");
                return false;
            }
        }
        else
        {
            GD.PrintErr("LevelManager Empty!");
            return false;
        }
        return true;
    }

    public static LevelManager Manager
    {
        get
        {
            if (s_levelManager == null)
            {
                s_levelManager = new LevelManager();
            }
            return s_levelManager;
        }
    }

    // Call this from your TitleScreen or other beginning scripts in game.
    // Because LevelManager exists as a plugin it does not exist in SceneTree
    // As such _Ready will not be called.
    public void Init(SceneTree T)
    {
        s_tree = new SceneTree();
        StartNewGame += NewGame;
        ResetLevel += Reset;
        s_tree = T;
        CreateManagerData();
    }

    void Reset() => CurrentLevel.ResetLevel();

    public async void CreateManagerData()
    {
        if (!ResourceLoader.Exists(_levelManagerPath))
        {
            GD.Print("No LevelManager Data!");
            throw new Exception("Unable to load LevelManager!");
        }
        else
        {
            await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
            if (!Load())
            {
                throw new Exception("Unable to load LevelManager!");
            }
        }
    }

    // Play level changing animation.
    // Load new level and set it as current
    public void SwitchLevel(string levelName, LevelLoadMode mode)
    {
        if (_managerData.Exists(levelName))
        {
            Switch(GetLevelByName(levelName), ref _currentLevel, mode);
        }
        else
        {
            GD.PrintErr("INVALID LEVEL SELECTED: " + levelName);
        }
    }

    public void SwitchLevelPacked(PackedScene packed, LevelLoadMode mode) => Switch(packed.Instantiate<LevelCommon>(), ref _currentLevel, mode);
    private void Switch(LevelCommon toLoad, ref LevelCommon current, LevelLoadMode mode)
    {
        switch (mode)
        {
            case LevelLoadMode.Single:
                if (current != null)
                {
                    s_tree.Root.RemoveChild(current);
                    current.QueueFree();
                }
                s_tree.Root.AddChild(toLoad);
                s_tree.CurrentScene = toLoad;
                toLoad.EnterLevel();
                LevelChanged.Invoke(current, toLoad);
                current = toLoad;
                break;

            case LevelLoadMode.Additive:
                if (!s_tree.Root.HasNode(toLoad.GetPath()))
                {
                    s_tree.Root.AddChild(toLoad);
                }
                break;
        }
    }

    void NewGame()
    {
        SwitchLevel(_managerData.NewGameScene, LevelLoadMode.Single);
        StartNewGame -= NewGame;
    }
}
