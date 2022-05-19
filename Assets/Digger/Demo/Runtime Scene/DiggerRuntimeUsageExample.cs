using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using UnityEngine;

namespace Digger
{
    /// <summary>
    /// This is just a basic example showing how to use DiggerMasterRuntime.
    /// </summary>
    public class DiggerRuntimeUsageExample : MonoBehaviour
    {
        [Header("Async parameters")]
        [Tooltip("Enable to edit the terrain asynchronously and avoid impacting the frame rate too much.")]
        public bool editAsynchronously = true;
        
        [Header("Modification parameters")]
        public BrushType brush = BrushType.Sphere;
        public ActionType action = ActionType.Dig;
        [Range(0, 7)] public int textureIndex;
        [Range(0.5f, 10f)] public float size = 4f;
        [Range(0f, 1f)] public float opacity = 0.5f;

        [Header("Persistence parameters (make sure persistence is enabled in Digger Master Runtime)")]
        public KeyCode keyToPersistData = KeyCode.P;

        public KeyCode keyToDeleteData = KeyCode.K;

        private DiggerMasterRuntime diggerMasterRuntime;

        private void Start()
        {
            diggerMasterRuntime = FindObjectOfType<DiggerMasterRuntime>();
            if (!diggerMasterRuntime) {
                enabled = false;
                Debug.LogWarning(
                    "DiggerRuntimeUsageExample component requires DiggerMasterRuntime component to be setup in the scene. DiggerRuntimeUsageExample will be disabled.");
            }
        }

        private void Update()
        {
            if (Input.GetMouseButton(0)) {
                // Perform a raycast to find terrain surface and call Modify method of DiggerMasterRuntime to edit it
                if (Physics.Raycast(transform.position, transform.forward, out var hit, 2000f)) {
                    if (editAsynchronously) {
                        diggerMasterRuntime.ModifyAsyncBuffured(hit.point, brush, action, textureIndex, opacity, size);
                    } else {
                        diggerMasterRuntime.Modify(hit.point, brush, action, textureIndex, opacity, size);
                    }
                }
            }

            if (Input.GetKeyDown(keyToPersistData)) {
                diggerMasterRuntime.PersistAll();
#if !UNITY_EDITOR
                Debug.Log("Persisted all modified chunks");
#endif
            } else if (Input.GetKeyDown(keyToDeleteData)) {
                diggerMasterRuntime.DeleteAllPersistedData();
#if !UNITY_EDITOR
                Debug.Log("Deleted all persisted data");
#endif
            }
        }
    }
}