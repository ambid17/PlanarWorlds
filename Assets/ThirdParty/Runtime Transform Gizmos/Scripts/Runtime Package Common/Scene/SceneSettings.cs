using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTG
{
    [Serializable]
    public class SceneSettings : Settings
    {
        [SerializeField]
        private ScenePhysicsMode _physicsMode = ScenePhysicsMode.RTG;

        public ScenePhysicsMode PhysicsMode { get { return _physicsMode; } set { if (!Application.isPlaying) _physicsMode = value; } }

        #if UNITY_EDITOR
        protected override void RenderContent(UnityEngine.Object undoRecordObject)
        {
            ScenePhysicsMode newPhysicsMode;
            var content = new GUIContent();

            content.text = "Physics mode";
            content.tooltip = "Controls the way in which raycasts, overlap tests etc are performed. It is recommended to leave this to \'RTG\'. Otherwise, some " +
                              "plugin features might not work as expected (e.g. vertex snapping for the move gizmo). You should select \'UnityColliders\' if you " +
                              "are experiencing slow frame rates which can happen for heavily populated scenes. In that case, you will need to attach colliders for " +
                              "all objects that you would like to interact with.";
            newPhysicsMode = (ScenePhysicsMode)EditorGUILayout.EnumPopup(content, PhysicsMode);
            if (newPhysicsMode != PhysicsMode)
            {
                EditorUndoEx.Record(undoRecordObject);
                PhysicsMode = newPhysicsMode;
            }
        }
        #endif
    }
}
