using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Hotkeys
{
    [SerializeField]
    public Hotkey[] hotkeys;
}

[Serializable]
public class Hotkey
{
    [SerializeField]
    public HotKeyName hotkeyName;
    [SerializeField]
    public KeyCode defaultKeyCode;
    [SerializeField]
    public KeyCode savedKeyCode;
    [SerializeField]
    public KeyCode modifier;
    [SerializeField]
    public string readableName;
    [SerializeField]
    public string tooltip;
    [SerializeField] 
    public HotkeyFilterType filterType;
}

public enum HotKeyName
{
    // General Hotkeys
    ControlModifier,
    FileSave,
    FileSaveAs,
    FileNew,
    FileOpen,

    // Camera Hotkeys
    CameraMovement,
    CameraForward,
    CameraBack,
    CameraUp,
    CameraDown,
    CameraLeft,
    CameraRight,
    
    // Terrain Hotkeys
    TerrainRaise,
    TerrainLower,
    TerrainSmooth,
    TerrainSetHeight,
    TerrainPaint,
    TerrainTree,
    TerrainFoliage,
    
    // Prefab Hotkeys
    PrefabPosition,
    PrefabRotation,
    PrefabScale,
    PrefabDelete,
    PrefabFocus,
    PrefabDuplicate,
    PrefabMultiSelect,
    PrefabSnap,
    PrefabVertexSnap,
    
    // Encounter Hotkeys
    EncounterFocus,
    EncounterSelect,
    EncounterMove,
    EncounterPlace,
    EncounterCancel,
    EncounterIgnoreSnap,
}

public enum HotkeyFilterType
{
    General,
    Camera,
    Terrain,
    Prefab,
    Encounter,
    Hidden
}