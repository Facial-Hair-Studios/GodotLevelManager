#if TOOLS
using Godot;
using Levels;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

[Tool]
public partial class LevelDockScript : Panel
{
    private Button _save;
    private Button _refresh;
    private Button _newLevelButton;
    private RichTextLabel _pathLable;

    private List<string> _names = new();
    private LevelManagerData _managerData;
    private string _levelManagerPath = "res://Assets/Data/LevelManagerData.tres";

    // ItemList Properties
    private ItemList _levelList;
    private Button _removeLevelButton;
    private Vector2 _currentListPostion;
    private int _currentIndex;

    // File selector for the Level at the start of a new game
    private OptionButton _newGameLevelSelector;
    private FileDialog _dialog;

    // Called when the node enters the scene tree for the first time.
    public override void _EnterTree()
    {
        base._EnterTree();
        _names = new List<string>();

        _dialog = GetNode<FileDialog>("FileDialog");
        _managerData = new LevelManagerData();
        _save = GetNode<Button>("ManagerLabelControl/ManagerLabel/ControlButtonsContainer/Save");
        _refresh = GetNode<Button>("ManagerLabelControl/ManagerLabel/ControlButtonsContainer/Refresh");

        _levelList = GetNode<ItemList>("LevelDockControl/ContentControl/VBoxContainer/LevelList");
        _newLevelButton = GetNode<Button>("BottomControl/AnchorSupportControl/LevelAdditionRemovalControl/Control3/HBoxContainer/NewLevel");
        _removeLevelButton = GetNode<Button>("BottomControl/AnchorSupportControl/LevelAdditionRemovalControl/Control3/HBoxContainer/RemoveLevel");

        _dialog.InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen;
        _dialog.RootSubfolder = "res://";

        _levelList.ItemClicked += ItemClicked;
        _removeLevelButton.Pressed += Remove;

        _pathLable = GetNode<RichTextLabel>("BottomControl/AnchorSupportControl/LevelDetailControl/FilePickerContainer/FilePickerBackground/VBoxContainer/Control/DataFilePath");
        _pathLable.Text = "Manager Data: " + _levelManagerPath;

        _save.Pressed += Save;
        _refresh.Pressed += RefreshUI;
        _newLevelButton.Pressed += NewLevel_Button;
        _newGameLevelSelector = GetNode<OptionButton>("BottomControl/AnchorSupportControl/LevelDetailControl/FilePickerContainer/FilePickerBackground/VBoxContainer/NewGameLevel");
        _newGameLevelSelector.Select(-1);
        _newGameLevelSelector.ItemSelected += SetNewGameLevelInLevelManager;

        if (!Load())
        {
            GD.Print("Level Manager Empty");
        }
        RefreshUI();
    }

    void Remove()
    {
        DirAccess.RemoveAbsolute(ResourceUid.GetIdPath(_managerData.Levels.First(x => x.Key == _names[_currentIndex].ToString()).Value));
        _managerData.Remove(_names[_currentIndex]);
        _levelList.RemoveItem(_currentIndex);
        _currentIndex -= 1;
        EditorInterface.Singleton.GetResourceFilesystem().Scan();

        Save();
        RefreshUI();
    }

    void AddScene(string path)
    {
        var uid = ResourceLoader.GetResourceUid(path);
        if (!ResourceUid.HasId(uid))
        {
            var newUid = ResourceUid.CreateId();
            ResourceUid.AddId(newUid, path);
            uid = newUid;
        }

        var packed = (PackedScene)ResourceLoader.Load(path);
        var level = packed.Instantiate<LevelCommon>();

        if (level.ID == Guid.Empty)
        {
            level.ID = Guid.NewGuid();
            packed.Pack(level);
            ResourceSaver.Save(packed, path);
            ResourceUid.SetId(uid, path);
        }
        if (_managerData.Add(level.LevelName, uid))
        {
            _names.Add(level.LevelName);
            _levelList.AddItem(level.LevelName);
        }
        _dialog.FileSelected -= AddScene;
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
        _newGameLevelSelector.Clear();

        foreach (var entry in _managerData.Levels)
        {
            if (ResourceUid.HasId(entry.Value))
            {
                var packed = ResourceLoader.Load<PackedScene>(ResourceUid.GetIdPath(entry.Value));
                var temp = packed.Instantiate<LevelCommon>();

                if (temp == null)
                {
                    GD.PrintErr("Failed to due to bad cast! Check if object is [Tool]");
                    return false;
                }

                _newGameLevelSelector.AddItem(temp.LevelName);
                _names.Add(entry.Key);
            }
            else
            {
                GD.PrintErr("Uid Path not registered: ", entry.Value);
            }
        }

        _newGameLevelSelector.Select(_names.FindIndex(x => x == _managerData.NewGameScene));
        return true;
    }

    public void Save()
    {
        var dir = DirAccess.Open("res://");

        if (!dir.DirExists("res://Assets/Data"))
        {
            dir.MakeDirRecursive("res://Assets/Data");
        }

        var err = ResourceSaver.Save(_managerData, _levelManagerPath);
        if (err != Error.Ok)
        {
            GD.PrintErr("LevelManager Failed to save: ", err);
        }
    }

    public void RefreshUI()
    {
        _levelList.Clear();
        _newGameLevelSelector.Clear();

        foreach (var Level in _managerData.Levels)
        {
            if (ResourceUid.HasId(Level.Value))
            {
                var packed = ResourceLoader.Load<PackedScene>(ResourceUid.GetIdPath(Level.Value));
                var temp = packed.Instantiate<LevelCommon>(); ;

                _levelList.AddItem(temp.LevelName);
                _newGameLevelSelector.AddItem(temp.LevelName);
                if (_newGameLevelSelector.ItemCount == 1)
                {
                    // To ensure first item is selectable
                    _newGameLevelSelector.Select(-1);
                }
            }
        }
    }
    public void SetNewGameLevelInLevelManager(long id) => _managerData.SetNewGameLevel(_names[(int)id]);
    public void ChangeSceneInEditor(string levelName)
    {
        if (_managerData.Levels != null)
        {
            if (ResourceUid.HasId(_managerData.Levels.First(x => x.Key == levelName.ToString()).Value))
            {
                EditorInterface.Singleton.OpenSceneFromPath(ResourceUid.GetIdPath(_managerData.Levels.First(x => x.Key == levelName.ToString()).Value));
            }
        }
    }

    void NewLevel_Button()
    {
        _dialog.FileSelected += CreateLevel;
        _dialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        _dialog.Popup();
    }

    void AddScene_Button()
    {
        _dialog.FileSelected += AddScene;
        _dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        _dialog.Popup();
    }

    void NewGameSceneSelected(long index)
    {
        var levelGuid = _names[(int)index];
        if (_managerData.NewGameScene == levelGuid)
        {
            return;
        }
        _managerData.NewGameScene = levelGuid;
    }

    void ItemClicked(long i, Vector2 positon, long mousePos)
    {
        _currentIndex = (int)i;
        _currentListPostion = positon;
        ChangeSceneInEditor(_names[_currentIndex]);
    }

    // Note the below functions are describing and creating Godot packed scene's in code
    // Generate Level Type node with script attached. Attach nodes, set properties like Name
    // Generate .tscn file, generate refrence to tscn to be added to Level Manager
    public void CreateLevel(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var level = new LevelCommon(name);

        level.ID = Guid.NewGuid();
        var packedLevel = new PackedScene();
        packedLevel.Pack(level);
        packedLevel.ResourceName = level.LevelName;
        var uid = ResourceUid.CreateId();

        ResourceSaver.Save(packedLevel, path);
        ResourceUid.AddId(uid, path);
        EditorInterface.Singleton.GetResourceFilesystem().Scan();

        AddScene(path);
        _dialog.FileSelected -= CreateLevel;

        RefreshUI();
        Save();
    }

    void RemoveDeferred() => _levelList.Clear();

    public override void _ExitTree()
    {
        base._ExitTree();
        _levelList.Clear();
        _save.Pressed -= Save;
        _refresh.Pressed -= RefreshUI;
        _newLevelButton.Pressed -= NewLevel_Button;
        _newGameLevelSelector.ItemSelected -= SetNewGameLevelInLevelManager;
    }
}
#endif
