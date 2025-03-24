using Godot;
using System;

namespace Levels
{
    /* Inherit from this and define your own level types. 
    * Platformer Levels: which can have exits, items to collect, enemies, etc.
    * RPG levels: Type of monster spawns, Event triggers and their locations, Other maps to enter like a town, Map state, etc.
    * FPS levels: Player Spawn locations, Item and Weapon spawn locations, etc.
    * Level types must also have [Tool] Attribute to work in EditorPlugin! */

    [Tool]
    public partial class LevelCommon : Node2D
    {
        public LevelDataCommon Data { get; protected set; }

        public Guid ID;
        private bool _isActive; // Used for Pause game

        [Export] private Resource _audioFile;
        private AudioStreamPlayer _backgroundPlayer;
        [Export] public string LevelName;

        public void SetLevelData(LevelDataCommon level) => Data = level;
        protected void SetIsActive(bool state) => _isActive = state;

        public LevelCommon() { }
        public LevelCommon(string name)
        {
            // Name of node in tree and Name of Level
            Name = name;
            LevelName = name;
            Data = new LevelDataCommon();
        }


        public virtual void EnterLevel()
        {
            _backgroundPlayer = (AudioStreamPlayer)GetNode("BackgroundAudio");
            CreateAudioStream();
            SetIsActive(true);
        }

        public void CreateAudioStream()
        {
            if (_audioFile != null)
            {
                _backgroundPlayer.Stream = GD.Load<AudioStream>(_audioFile.ResourcePath);
                _backgroundPlayer.Play();
            }
        }

        public virtual void Update(double delta)
        {

        }

        public virtual void FixedUpdate(double delta)
        {

        }

        public override void _Process(double delta)
        {
            if (_isActive)
            {
                base._Process(delta);
                if (!Engine.IsEditorHint())
                {
                    Update(delta);
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isActive)
            {
                base._PhysicsProcess(delta);
                if (!Engine.IsEditorHint())
                {
                    FixedUpdate(delta);
                }
            }
        }

        public virtual void ExitLevel()
        {

        }

        public virtual void CalledDefferedExitLevel()
        {
            // Implement async call in here
        }

        public virtual void ResetLevel()
        {

        }
    }
}
