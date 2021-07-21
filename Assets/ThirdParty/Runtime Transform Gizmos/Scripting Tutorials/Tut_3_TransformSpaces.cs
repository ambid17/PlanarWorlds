﻿using UnityEngine;
using System.Collections.Generic;

namespace RTG
{
    /// <summary>
    /// The main focus in this tutorial is to learn how to change the transform
    /// space (Global & Local) of a transform gizmo.
    /// </summary>
    public class Tut_3_TransformSpaces : MonoBehaviour
    {
        /// <summary>
        /// A private enum which is used by the class to differentiate between different 
        /// gizmo types. An example where this enum will come in handy is when we use the 
        /// W,E,R,T keys to switch between different types of gizmos. When the W key is 
        /// pressed for example, we will call the 'SetWorkGizmoId' function passing 
        /// GizmoId.Move as the parameter.
        /// </summary>
        private enum GizmoId
        {
            Move = 1,
            Rotate,
            Scale,
            Universal
        }

        /// <summary>
        /// The following 4 variables are references to the ObjectTransformGizmo behaviours
        /// that will be used to move, rotate and scale our objects.
        /// </summary>
        private ObjectTransformGizmo _objectMoveGizmo;
        private ObjectTransformGizmo _objectRotationGizmo;
        private ObjectTransformGizmo _objectScaleGizmo;
        private ObjectTransformGizmo _objectUniversalGizmo;

        /// <summary>
        /// The current work gizmo id. The work gizmo is the gizmo which is currently used
        /// to transform objects. The W,E,R,T keys can be used to change the work gizmo as
        /// needed.
        /// </summary>
        private GizmoId _workGizmoId;
        /// <summary>
        /// A reference to the current work gizmo. If the work gizmo id is GizmoId.Move, then
        /// this will point to '_objectMoveGizmo'. For GizmoId.Rotate, it will point to 
        /// '_objectRotationGizmo' and so on.
        /// </summary>
        private ObjectTransformGizmo _workGizmo;
        /// <summary>
        /// A list of objects which are currently selected. This is also the list that holds
        /// the gizmo target objects. 
        /// </summary>
        private List<GameObject> _selectedObjects = new List<GameObject>();

        /// <summary>
        /// Performs all necessary initializations.
        /// </summary>
        private void Start()
        {
            // Create the 4 gizmos
            _objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
            _objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
            _objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
            _objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

            // Call the 'SetEnabled' function on the parent gizmo to make sure
            // the gizmos are initially hidden in the scene. We want the gizmo
            // to show only when we have a target object available.
            _objectMoveGizmo.Gizmo.SetEnabled(false);
            _objectRotationGizmo.Gizmo.SetEnabled(false);
            _objectScaleGizmo.Gizmo.SetEnabled(false);
            _objectUniversalGizmo.Gizmo.SetEnabled(false);

            // Link the selected objects list to the gizmos
            // Note: The 'SetTargetObjects' function will instruct the gizmo to store
            //       a direct reference to the '_selecteObjects' list. This means that
            //       when you add or remove objects from this list, the gizmos will have
            //       access to the most recent/updated collection. You don't need to call
            //       'SetTargetObjects' again when the list changes.
            _objectMoveGizmo.SetTargetObjects(_selectedObjects);
            _objectRotationGizmo.SetTargetObjects(_selectedObjects);
            _objectScaleGizmo.SetTargetObjects(_selectedObjects);
            _objectUniversalGizmo.SetTargetObjects(_selectedObjects);

            // We initialize the work gizmo to the move gizmo by default. This means
            // that the first time an object is clicked, the move gizmo will appear.
            // You can change the default gizmo, by simply changing these 2 lines of
            // code. For example, if you wanted the scale gizmo to be the default work
            // gizmo, replace '_objectMoveGizmo' with '_objectScaleGizmo' and GizmoId.Move
            // with GizmoId.Scale.
            _workGizmo = _objectMoveGizmo;
            _workGizmoId = GizmoId.Move;
        }

        /// <summary>
        /// Called every frame to perform all necessary updates. In this tutorial,
        /// we listen to user input and take action. 
        /// </summary>
        private void Update()
        {
            // Check if the left mouse button was pressed in the current frame.
            // Note: Something that was left out of the video tutorial by mistake. We 
            //       only take the mouse click into account if no gizmo is currently 
            //       hovered. When a gizmo is hovered, we ignore clicks because in that
            //       case a click usually represents the intent of clicking and dragging
            //       the gizmo handles. If we didn't perform this check, clicking on a
            //       gizmo might actually disable it instead if the click does not hover
            //       a game object (i.e. thin air click).
            if (RTInput.WasLeftMouseButtonPressedThisFrame() &&
                RTGizmosEngine.Get.HoveredGizmo == null)
            {
                // Pick a game object
                GameObject pickedObject = PickGameObject();
                if (pickedObject != null)
                {
                    // Is the CTRL key pressed?
                    if (RTInput.IsKeyPressed(KeyCode.LeftControl))
                    {
                        // The CTRL key is pressed; it means we find ourselves in 2 possible situations:
                        // a) the picked object is already selected, in which case we deselect it;
                        // b) the picked object is not selected, in which case we append it to the selection.
                        if (_selectedObjects.Contains(pickedObject)) _selectedObjects.Remove(pickedObject);
                        else _selectedObjects.Add(pickedObject);

                        // The selection has changed
                        OnSelectionChanged();
                    }
                    else
                    {
                        // The CTRL key is not pressed; in this case we just clear the selection and
                        // select only the object that we clicked on.
                        _selectedObjects.Clear();
                        _selectedObjects.Add(pickedObject);

                        // The selection has changed
                        OnSelectionChanged();
                    }
                }
                else
                {
                    // If we reach this point, it means no object was picked. This means that we clicked
                    // in thin air, so we just clear the selected objects list.
                    _selectedObjects.Clear();
                    OnSelectionChanged();

                    // The selection has changed
                    OnSelectionChanged();
                }
            }

            // If the G key was pressed, we change the transform space to Global. Otherwise,
            // if the L key was pressed, we change it to Local.
            if (RTInput.WasKeyPressedThisFrame(KeyCode.G)) SetTransformSpace(GizmoSpace.Global);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.L)) SetTransformSpace(GizmoSpace.Local);

            // Switch between different gizmo types using the W,E,R,T keys.
            // Note: We use the 'SetWorkGizmoId' function to perform the switch.
            if (RTInput.WasKeyPressedThisFrame(KeyCode.W)) SetWorkGizmoId(GizmoId.Move);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.E)) SetWorkGizmoId(GizmoId.Rotate);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.R)) SetWorkGizmoId(GizmoId.Scale);
            else if (RTInput.WasKeyPressedThisFrame(KeyCode.T)) SetWorkGizmoId(GizmoId.Universal);
        }

        /// <summary>
        /// An implementatio of the OnGUI function which shows the current transform
        /// space in the top left corner of the screen.
        /// </summary>
        private void OnGUI()
        {
            // We will use a style with a green text color
            var guiStyle = new GUIStyle();
            guiStyle.normal.textColor = Color.green;

            // Draw the label.
            // Note: In order to get access to the current transform space, we use the 'TransformSpace'
            //       property of the move gizmo. Any gizmo would do because all of them use the same
            //       transform space. We make sure of that inside the 'SetTransformSpace' function.
            GUILayout.Label("Transform Space: " + _objectMoveGizmo.TransformSpace.ToString(), guiStyle);
        }

        /// <summary>
        /// Uses the mouse position to pick a game object in the scene. Returns
        /// the picked game object or null if no object is picked.
        /// </summary>
        /// <remarks>
        /// Objects must have colliders attached.
        /// </remarks>
        private GameObject PickGameObject()
        {
            // Build a ray using the current mouse cursor position
            Ray ray = Camera.main.ScreenPointToRay(RTInput.MousePosition);

            // Check if the ray intersects a game object. If it does, return it
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, float.MaxValue))
                return rayHit.collider.gameObject;

            // No object is intersected by the ray. Return null.
            return null;
        }

        /// <summary>
        /// This function is called to change the type of work gizmo. This is
        /// used in the 'Update' function in response to the user pressing the
        /// W,E,R,T keys to switch between different gizmo types.
        /// </summary>
        private void SetWorkGizmoId(GizmoId gizmoId)
        {
            // If the specified gizmo id is the same as the current id, there is nothing left to do
            if (gizmoId == _workGizmoId) return;

            // Start with a clean slate and disable all gizmos
            _objectMoveGizmo.Gizmo.SetEnabled(false);
            _objectRotationGizmo.Gizmo.SetEnabled(false);
            _objectScaleGizmo.Gizmo.SetEnabled(false);
            _objectUniversalGizmo.Gizmo.SetEnabled(false);

            // At this point all gizmos are disabled. Now we need to check the gizmo id
            // and adjust the '_workGizmo' variable.
            _workGizmoId = gizmoId;
            if (gizmoId == GizmoId.Move) _workGizmo = _objectMoveGizmo;
            else if (gizmoId == GizmoId.Rotate) _workGizmo = _objectRotationGizmo;
            else if (gizmoId == GizmoId.Scale) _workGizmo = _objectScaleGizmo;
            else if (gizmoId == GizmoId.Universal) _workGizmo = _objectUniversalGizmo;

            // If we have any selected objects, we need to make sure the work gizmo is enabled
            if (_selectedObjects.Count != 0)
            {
                // Make sure the work gizmo is enabled. There is no need to check if the gizmo is already
                // enabled. The 'SetEnabled' call will simply be ignored if that is the case.
                _workGizmo.Gizmo.SetEnabled(true);

                // When working with transform spaces, the gizmos need to know about the pivot object. This piece 
                // of information is necessary when the transform space is set to local because in that case the
                // gizmo will have its rotation synchronized with the target objects rotation. But because there
                // is more than one target object, we need to tell the gizmo which object to use. This is the role
                // if the pivot object in this case. 
                _workGizmo.SetTargetPivotObject(_selectedObjects[_selectedObjects.Count - 1]);
            }
        }

        /// <summary>
        /// Called from the 'Update' function whenever the '_selectedObjects' list
        /// changes. It is responsible for updating the gizmos accordingly.
        /// </summary>
        private void OnSelectionChanged()
        {
            // If we have any selected objects, we need to make sure the work gizmo is enabled
            if (_selectedObjects.Count != 0)
            {
                // Make sure the work gizmo is enabled. There is no need to check if the gizmo is already
                // enabled. The 'SetEnabled' call will simply be ignored if that is the case.
                _workGizmo.Gizmo.SetEnabled(true);

                // When working with transform spaces, the gizmos need to know about the pivot object. This piece 
                // of information is necessary when the transform space is set to local because in that case the
                // gizmo will have its rotation synchronized with the target objects rotation. But because there
                // is more than one target object, we need to tell the gizmo which object to use. This is the role
                // if the pivot object in this case. 
                _workGizmo.SetTargetPivotObject(_selectedObjects[_selectedObjects.Count - 1]);

                // The last step we need to perform is to make sure that the work gizmo is positioned
                // and rotated correctly. This is because the gizmos (as will become more clear in 
                // later tutorials) have 2 properties such as transform space and transform pivot and
                // when the selected objects change, these 2 properties will dictate how the gizmo should
                // be positioned and rotated. In order to ensure that the correct position and rotation
                // are used, we need to call 'RefreshPositionAndRotation'.
                _workGizmo.RefreshPositionAndRotation();
            }
            else
            {
                // The target object is null. In this case, we don't want any gizmos to be visible
                // in the scene, so we disable all of them.
                _objectMoveGizmo.Gizmo.SetEnabled(false);
                _objectRotationGizmo.Gizmo.SetEnabled(false);
                _objectScaleGizmo.Gizmo.SetEnabled(false);
                _objectUniversalGizmo.Gizmo.SetEnabled(false);
            }
        }

        /// <summary>
        /// Called from the 'Update' function in response to user input in order
        /// to change the transform space for all gizmos. The parameter represents
        /// the desired transform space and is of type 'GizmoSpace'. This is an enum
        /// with 2 mamebers: Global and Local.
        /// </summary>
        private void SetTransformSpace(GizmoSpace transformSpace)
        {
            // In order to change the transform space for a gizmo, we need to call the 
            // gizmo's 'SetTransformSpace' function. We do this for all gizmos and pass
            // the specified transform space as parameter.
            _objectMoveGizmo.SetTransformSpace(transformSpace);
            _objectRotationGizmo.SetTransformSpace(transformSpace);
            _objectScaleGizmo.SetTransformSpace(transformSpace);
            _objectUniversalGizmo.SetTransformSpace(transformSpace);
        }
    }
}
