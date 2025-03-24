#if TOOLS
using Godot;

[Tool]
public partial class LevelManagerPlugin : EditorPlugin
{
    private LevelDockScript _editorDock;

    bool isLoaded;

    // Initialization of the plugin goes here.
    public override void _EnterTree()
    {
        base._EnterTree();
        if (!isLoaded)
        {
            _editorDock = GD.Load<PackedScene>("res://addons/LevelManager/LevelDock.tscn").Instantiate<LevelDockScript>();
            if (_editorDock == null)
            {
                GD.PrintErr("Editor failed to initalize");
                return;
            }

            _editorDock.Name = "Levels";
            AddControlToDock(DockSlot.LeftUl, _editorDock);
            isLoaded = true;
        }
    }

    public override void _ExitTree()
    {
        if (_editorDock != null)
        {
            RemoveControlFromDocks(_editorDock);
            _editorDock.Free();
            isLoaded = false;
        }
    }
}


#endif
