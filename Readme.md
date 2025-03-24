# Godot Level Manager
<p> Godot Level Manager is A Unity based Level Manager that will allow Godot to recognize and manage what is a level vs a player or non level / Level GameObject since all GameObject's are viewed as Scene's in Godot aything can technically be loaded as a "Level". This allows a distinct seperation between levels and game objects as well as categorization of levels. </p>

### Features:
- Add, Remove Level
- Various GetLevel Functions
- Dock Plugin for Level Creation
- Simple inheritable class that allows definition of any level types
- Simple inheritable class that allows definition of any level data to be saved or run active in the scene
- Level API for Enter, Exit, Reset that can be implemented per game
- Several Events like LevelLoaded, LevelChanged, etc. That can be subscribed to 

### ToDo:
- Add Scene / Node copy functions
- Add Level Merge functions
- Add better background Level loading and SetActive functionality, current implementation will net you a PackedScene, but Switch will always instantly load a level with no Caching

!["Screenshot:"](https://raw.githubusercontent.com/Facial-Hair-Studios/GodotLevelManager/refs/heads/main/addons/Screenshot/Dock.png)

