using Digger.Modules.Runtime.Sources;
using UnityEngine;

namespace Digger
{
    /// <summary>
    /// This is just a basic example showing how to use DiggerNavMeshRuntime.
    /// </summary>
    public class DiggerNavMeshUsageExample : MonoBehaviour
    {
        public KeyCode keyToUpdateNavMesh = KeyCode.N;
        private DiggerNavMeshRuntime diggerNavMeshRuntime;

        private void Start()
        {
            diggerNavMeshRuntime = FindObjectOfType<DiggerNavMeshRuntime>();
            if (!diggerNavMeshRuntime) {
                enabled = false;
                Debug.LogWarning("DiggerNavMeshUsageExample requires DiggerNavMeshRuntime component to be setup in the scene. DiggerNavMeshUsageExample will be disabled.");
                return;
            }

            // this is mandatory and should be called only once in a Start method
            diggerNavMeshRuntime.CollectNavMeshSources();
        }

        private void Update()
        {
            if (Input.GetKeyDown(keyToUpdateNavMesh)) {
                // this will start updating the NavMesh over several frames, asynchronously
                diggerNavMeshRuntime.UpdateNavMeshAsync(() => Debug.Log("NavMesh has been updated."));
                Debug.Log("NavMesh is updating...");
            }
        }
    }
}