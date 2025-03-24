using Godot;
using System;

[Tool]
public partial class FileSelector : Node
{
    public Action FileSelected;

    private string _path;
    private string[] _filters;
    FileDialog Dialog;
    Button _browseButton;
    RichTextLabel label;
    RichTextLabel PathField;

    public string Path { get => _path; }

    public void SetPathField(string path)
    {
        if (PathField != null)
        {
            PathField.Text = path;
        }
    }

    public void Init(string path, string[] filters)
    {
        Dialog = GetNodeOrNull<FileDialog>("FileDialog");
        _browseButton = GetNodeOrNull<Button>("HBoxContainer/Browse");
        label = GetNodeOrNull<RichTextLabel>("SelectLabel");
        PathField = GetNodeOrNull<RichTextLabel>("HBoxContainer/PathLabel");

        _path = path;
        _filters = filters;
        _browseButton.Pressed += Open;
    }

    public bool IsOpen() => Dialog.Visible;

    public void GetPath(string p)
    {
        _path = p;
        PathField.Text = _path;
        Dialog.Hide();
        FileSelected.Invoke();
    }

    public void Open()
    {
        if (_filters != null)
        {
            Dialog.Filters = _filters;
        }
        if (!string.IsNullOrEmpty(_path))
        {
            Dialog.RootSubfolder = _path;
        }
        Dialog.Show();
        Dialog.FileSelected += GetPath;
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        Dialog = null;
        _browseButton = null;
        label = null;
        PathField = null;
    }
}
