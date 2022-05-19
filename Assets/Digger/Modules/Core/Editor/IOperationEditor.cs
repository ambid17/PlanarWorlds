using UnityEditor;

namespace Digger.Modules.Core.Editor
{
    public interface IOperationEditor
    {
        void OnEnable();
        void OnDisable();
        void OnInspectorGUI();
        void OnSceneGUI();
        void OnScene(UnityEditor.Editor editor, SceneView sceneview);
    }
}