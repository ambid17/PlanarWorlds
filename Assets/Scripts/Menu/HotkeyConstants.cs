using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HotkeyConstants
{
    #region General Hotkeys
    public static string ModifierKey = "Modifier Key";
    public static string ModifierKeyDefault = "LeftControl";
    public static string ModifierKeyTooltip = "When pressed at the same time, modifies the action of other keys";
    #endregion
    
    #region Prefab Mode Hotkeys
    public static string SelectPosition = "Position Mode";
    public static string SelectPositionDefault = "Z";
    public static string SelectPositionTooltip = "Selects the position manipulation type in the inspector";

    public static string SelectRotation = "Rotation Mode";
    public static string SelectRotationDefault = "X";
    public static string SelectRotationTooltip = "Selects the rotation manipulation type in the inspector";

    public static string SelectScale = "Scale Mode";
    public static string SelectScaleDefault = "C";
    public static string SelectScaleTooltip = "Selects the scale manipulation type in the inspector";

    public static string DeletePrefab = "Delete Prefab";
    public static string DeletePrefabDefault = "Delete";
    public static string DeletePrefabTooltip = "Deletes the currently selected object";

    public static string Focus = "Focus";
    public static string FocusDefault = "F";
    public static string FocusTooltip = "Focuses the camera on the currently selected object";

    public static string Duplicate = "Duplicate";
    public static string DuplicateDefault = "D";
    public static string DuplicateTooltip = "*** Duplicate the currently selected object";
    #endregion
    

    #region Camera hotkeys
    public static string CameraMoveForward = "Move forward";
    public static string CameraMoveBack = "Move back";
    
    public static string CameraStrafeLeft = "Strafe left";
    public static string CameraStrafeRight = "Strafe right";
    
    public static string CameraMoveUp = "Move up";
    public static string CameraMoveDown = "Move down";
    
    public static string CameraPan = "Pan";
    public static string CameraLookAround = "Look around";
    public static string CameraOrbit = "Orbit";
    #endregion


    
    #region Gizmos hotkeys
    public static string GizmosEnable2DMode = "Enable 2D mode";
    public static string GizmosEnableSnapping = "Enable snapping";
    public static string GizmosEnableVertexSnapping = "Enable vertex snapping";
    #endregion 
    
}
