using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultHotkeys : MonoBehaviour
{
    public static Hotkeys GetDefaultHotkeys()
    {
        Hotkeys keys = new Hotkeys()
        {
            hotkeys = GetHotKeys()
        };
        return keys;
    }

    public static Hotkey[] GetHotKeys()
    {
        List<Hotkey> hotkeys = new List<Hotkey>();

        #region General Hotkeys
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.FileSave,
            defaultKeyCode = KeyCode.S,
            savedKeyCode = KeyCode.S,
            modifier = KeyCode.LeftControl,
            readableName = "Save Campaign",
            tooltip = "Saves campaign to the last file used",
            filterType = HotkeyFilterType.General
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.FileSaveAs,
            defaultKeyCode = KeyCode.S,
            savedKeyCode = KeyCode.S,
            modifier = KeyCode.LeftControl,
            secondModifier = KeyCode.LeftShift,
            readableName = "Save Campaign As",
            tooltip = "Opens the folder dialog to choose campaign file location",
            filterType = HotkeyFilterType.General
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.FileNew,
            defaultKeyCode = KeyCode.N,
            savedKeyCode = KeyCode.N,
            modifier = KeyCode.LeftControl,
            readableName = "New Campaign",
            tooltip = "Opens dialog to save a file for a new campaign",
            filterType = HotkeyFilterType.General
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.FileOpen,
            defaultKeyCode = KeyCode.O,
            savedKeyCode = KeyCode.O,
            modifier = KeyCode.LeftControl,
            readableName = "Open Campaign",
            tooltip = "Opens a saved campaign",
            filterType = HotkeyFilterType.General
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.ControlModifier,
            defaultKeyCode = KeyCode.LeftControl,
            savedKeyCode = KeyCode.LeftControl,
            modifier = KeyCode.LeftControl,
            readableName = "",
            tooltip = "",
            filterType = HotkeyFilterType.Hidden
        });
        #endregion
        
        #region Camera Hotkeys
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.CameraForward,
            defaultKeyCode = KeyCode.W,
            savedKeyCode = KeyCode.W,
            modifier = KeyCode.Mouse1,
            readableName = "Camera Forward",
            tooltip = "While holding right click, moves the camera Forward",
            filterType = HotkeyFilterType.Camera
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.CameraBack,
            defaultKeyCode = KeyCode.S,
            savedKeyCode = KeyCode.S,
            modifier = KeyCode.Mouse1,
            readableName = "Camera Back",
            tooltip = "While holding right click, moves the camera Back",
            filterType = HotkeyFilterType.Camera
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.CameraLeft,
            defaultKeyCode = KeyCode.A,
            savedKeyCode = KeyCode.A,
            modifier = KeyCode.Mouse1,
            readableName = "Camera Left",
            tooltip = "While holding right click, moves the camera Left",
            filterType = HotkeyFilterType.Camera
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.CameraRight,
            defaultKeyCode = KeyCode.D,
            savedKeyCode = KeyCode.D,
            modifier = KeyCode.Mouse1,
            readableName = "Camera Right",
            tooltip = "While holding right click, moves the camera Right",
            filterType = HotkeyFilterType.Camera
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.CameraUp,
            defaultKeyCode = KeyCode.E,
            savedKeyCode = KeyCode.E,
            modifier = KeyCode.Mouse1,
            readableName = "Camera Up",
            tooltip = "While holding right click, moves the camera Up",
            filterType = HotkeyFilterType.Camera
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.CameraDown,
            defaultKeyCode = KeyCode.Q,
            savedKeyCode = KeyCode.Q,
            modifier = KeyCode.Mouse1,
            readableName = "Camera Down",
            tooltip = "While holding right click, moves the camera Down",
            filterType = HotkeyFilterType.Camera
        });
        #endregion
        
        #region Terrain Hotkeys
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.TerrainRaise,
            defaultKeyCode = KeyCode.Z,
            savedKeyCode = KeyCode.Z,
            readableName = "Raise Terrain",
            tooltip = "Changes terrain mode to Raise the terrain",
            filterType = HotkeyFilterType.Terrain
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.TerrainLower,
            defaultKeyCode = KeyCode.X,
            savedKeyCode = KeyCode.X,
            readableName = "Lower Terrain",
            tooltip = "Changes terrain mode to Lower the terrain",
            filterType = HotkeyFilterType.Terrain
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.TerrainSmooth,
            defaultKeyCode = KeyCode.C,
            savedKeyCode = KeyCode.C,
            readableName = "Smooth Terrain",
            tooltip = "Changes terrain mode to Smooth the terrain",
            filterType = HotkeyFilterType.Terrain
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.TerrainSetHeight,
            defaultKeyCode = KeyCode.V,
            savedKeyCode = KeyCode.V,
            readableName = "Set Terrain Height",
            tooltip = "Changes terrain mode to Set the height",
            filterType = HotkeyFilterType.Terrain
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.TerrainPaint,
            defaultKeyCode = KeyCode.B,
            savedKeyCode = KeyCode.B,
            readableName = "Paint Terrain",
            tooltip = "Changes terrain mode to Paint the terrain",
            filterType = HotkeyFilterType.Terrain
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.TerrainTree,
            defaultKeyCode = KeyCode.N,
            savedKeyCode = KeyCode.N,
            readableName = "Add Terrain Trees",
            tooltip = "Changes terrain mode to paint trees",
            filterType = HotkeyFilterType.Terrain
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.TerrainFoliage,
            defaultKeyCode = KeyCode.M,
            savedKeyCode = KeyCode.M,
            readableName = "Add Terrain Foliage",
            tooltip = "Changes terrain mode to paint foliage",
            filterType = HotkeyFilterType.Terrain
        });
        #endregion
        
        #region Prefab Hotkeys
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.PrefabPosition,
            defaultKeyCode = KeyCode.Z,
            savedKeyCode = KeyCode.Z,
            readableName = "Position mode",
            tooltip = "Sets the prefab gizmo to edit the current object's Position",
            filterType = HotkeyFilterType.Prefab
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.PrefabRotation,
            defaultKeyCode = KeyCode.X,
            savedKeyCode = KeyCode.X,
            readableName = "Rotation mode",
            tooltip = "Sets the prefab gizmo to edit the current object's Rotation",
            filterType = HotkeyFilterType.Prefab
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.PrefabScale,
            defaultKeyCode = KeyCode.C,
            savedKeyCode = KeyCode.C,
            readableName = "Scale mode",
            tooltip = "Sets the prefab gizmo to edit the current object's Scale",
            filterType = HotkeyFilterType.Prefab
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.PrefabDelete,
            defaultKeyCode = KeyCode.Backspace,
            savedKeyCode = KeyCode.Backspace,
            readableName = "Delete Object",
            tooltip = "Deletes the currently selected object",
            filterType = HotkeyFilterType.Prefab
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.PrefabFocus,
            defaultKeyCode = KeyCode.Z,
            savedKeyCode = KeyCode.Z,
            readableName = "Focus",
            tooltip = "Moves the camera to focus on the currently selected object",
            filterType = HotkeyFilterType.Prefab
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.PrefabDuplicate,
            defaultKeyCode = KeyCode.D,
            savedKeyCode = KeyCode.D,
            modifier = KeyCode.LeftControl,
            readableName = "Duplicate",
            tooltip = "Sets the prefab gizmo to edit position",
            filterType = HotkeyFilterType.Prefab
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.PrefabMultiSelect,
            defaultKeyCode = KeyCode.Mouse0,
            savedKeyCode = KeyCode.Mouse0,
            modifier = KeyCode.LeftShift,
            readableName = "Multi Select",
            tooltip = "Allows the user to select multiple objects. Hold shift and left click multiple objects to edit them",
            filterType = HotkeyFilterType.Prefab
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.PrefabSnap,
            defaultKeyCode = KeyCode.Mouse0,
            savedKeyCode = KeyCode.Mouse0,
            modifier = KeyCode.LeftControl,
            readableName = "Snap object",
            tooltip = "Holding CTRL while moving/rotation/scaling and the object will snap the object to the grid",
            filterType = HotkeyFilterType.Prefab
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.PrefabVertexSnap,
            defaultKeyCode = KeyCode.Mouse0,
            savedKeyCode = KeyCode.Mouse0,
            modifier = KeyCode.V,
            readableName = "Vertex Snap",
            tooltip = "While holding V in position mode, you can snap a vertex of one object to another. Use this to snap objects together",
            filterType = HotkeyFilterType.Prefab
        });
        #endregion
        
        #region Encounter Hotkeys
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.EncounterFocus,
            defaultKeyCode = KeyCode.F,
            savedKeyCode = KeyCode.F,
            readableName = "Focus",
            tooltip = "Moves the camera to focus on the currently selected object",
            filterType = HotkeyFilterType.Encounter
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.EncounterSelect,
            defaultKeyCode = KeyCode.Mouse0,
            savedKeyCode = KeyCode.Mouse0,
            readableName = "Select Character",
            tooltip = "Left click on a character to select them.",
            filterType = HotkeyFilterType.Encounter
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.EncounterMove,
            defaultKeyCode = KeyCode.Mouse2,
            savedKeyCode = KeyCode.Mouse2,
            readableName = "Move Character",
            tooltip = "Press your mouse wheel to start moving a selected character",
            filterType = HotkeyFilterType.Encounter
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.EncounterPlace,
            defaultKeyCode = KeyCode.Mouse0,
            savedKeyCode = KeyCode.Mouse0,
            readableName = "Place Character",
            tooltip = "Press left click to place a moved character",
            filterType = HotkeyFilterType.Encounter
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.EncounterCancel,
            defaultKeyCode = KeyCode.Mouse1,
            savedKeyCode = KeyCode.Mouse1,
            readableName = "Cancel Character Place",
            tooltip = "Press right click to cancel the current player movement",
            filterType = HotkeyFilterType.Encounter
        });
        
        hotkeys.Add(new Hotkey()
        {
            hotkeyName = HotKeyName.EncounterIgnoreSnap,
            defaultKeyCode = KeyCode.LeftControl,
            savedKeyCode = KeyCode.LeftControl,
            readableName = "Ignore Snapping",
            tooltip = "Hold control to move a player without snapping to the grid",
            filterType = HotkeyFilterType.Encounter
        });
        #endregion

        return hotkeys.ToArray();
    }
}
