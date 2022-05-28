using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
[CreateAssetMenu(fileName = "Hotkeys", menuName = "ScriptableObjects/Hotkeys")]
public class Hotkeys : ScriptableObject
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
    Position,
    Rotation,
    Scale,
    Delete,
    Focus,
    Duplicate,
    CameraForward,
    CameraBack,
    CameraUp,
    CameraDown,
    CameraLeft,
    CameraRight,
    ControlModifier
}

public enum HotkeyFilterType
{
    General,
    Camera,
    Terrain,
    Prefab,
    Encounter
}