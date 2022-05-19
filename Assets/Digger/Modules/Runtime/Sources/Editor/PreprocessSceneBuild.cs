using Digger.Modules.Core.Sources;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Digger.Modules.Runtime.Sources.Editor
{
    public class PreprocessSceneBuild : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (Application.isPlaying)
                return;

            var rootObjects = scene.GetRootGameObjects();

            var includeVoxelData = false;
            foreach (var rootObject in rootObjects) {
                var diggerRuntime = rootObject.GetComponentInChildren<DiggerMasterRuntime>();
                if (diggerRuntime) {
                    includeVoxelData = true;
                    Debug.Log($"DiggerMasterRuntime has been detected in scene '{scene.name}'. Voxel data will be included in build.");
                    break;
                }
            }

            foreach (var rootObject in rootObjects) {
                var diggers = rootObject.GetComponentsInChildren<DiggerSystem>();
                foreach (var digger in diggers) {
                    digger.OnPreprocessBuild(includeVoxelData);
                }
            }
        }
    }
}