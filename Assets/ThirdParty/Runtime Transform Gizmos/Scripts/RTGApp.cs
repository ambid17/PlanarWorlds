using UnityEngine;
using UnityEngine.Rendering;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTG
{
    public delegate void RTGAppInitializedHandler();

    public class RTGApp : MonoSingleton<RTGApp>, IRLDApplication
    {
        public event RTGAppInitializedHandler Initialized;

        private Camera _renderCamera;
        private RenderPipelineId _renderPipelineId;

        public RenderPipelineId RenderPipelineId { get { return _renderPipelineId; } }
        public Camera RenderCamera { get { return _renderCamera; } }

        private void OnCanCameraUseScrollWheel(YesNoAnswer answer)
        {
            if (RTScene.Get.IsAnyUIElementHovered()) answer.No();
            else answer.Yes();
        }

        private void OnCanCameraProcessInput(YesNoAnswer answer)
        {
            if (RTGizmosEngine.Get.DraggedGizmo != null) answer.No();
            else answer.Yes();
        }

        private void OnCanUndoRedo(UndoRedoOpType undoRedoOpType, YesNoAnswer answer)
        {
            if (RTGizmosEngine.Get.DraggedGizmo == null) answer.Yes();
            else answer.No();
        }

        private void OnCanDoGizmoHoverUpdate(YesNoAnswer answer)
        {
            answer.Yes();
        }

        private void OnViewportsCameraAdded(Camera camera)
        {
            RTGizmosEngine.Get.AddRenderCamera(camera);
        }

        private void OnViewportCameraRemoved(Camera camera)
        {
            RTGizmosEngine.Get.RemoveRenderCamera(camera);
        }

        private void Start()
        {
            DetectRenderPipeline();

            // Undo/Redo
            RTUndoRedo.Get.CanUndoRedo += OnCanUndoRedo;

            // Camera
            RTFocusCamera.Get.CanProcessInput += OnCanCameraProcessInput;
            RTFocusCamera.Get.CanUseScrollWheel += OnCanCameraUseScrollWheel;
            RTCameraViewports.Get.CameraAdded += OnViewportsCameraAdded;
            RTCameraViewports.Get.CameraRemoved += OnViewportCameraRemoved;

            // Scene
            RTScene.Get.RegisterHoverableSceneEntityContainer(RTGizmosEngine.Get);
            RTSceneGrid.Get.Initialize_SystemCall();

            // Gizmo engine
            RTGizmosEngine.Get.CanDoHoverUpdate += OnCanDoGizmoHoverUpdate;
            if (_renderPipelineId == RenderPipelineId.Standard) RTGizmosEngine.Get.CreateSceneGizmo(RTFocusCamera.Get.TargetCamera);
            RTGizmosEngine.Get.AddRenderCamera(RTFocusCamera.Get.TargetCamera);

            RTMeshCompiler.CompileEntireScene();
            if (_renderPipelineId != RenderPipelineId.Standard)
            {
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
                RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            }

            if (Initialized != null) Initialized();
        }

        private void DetectRenderPipeline()
        {
            _renderPipelineId = RenderPipelineId.Standard;
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("Universal"))
                    _renderPipelineId = RenderPipelineId.URP;
                // else
                // if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HDRenderPipelineAsset"))
                //     _renderPipelineId = RenderPipelineId.HDRP;
                else
                {
                    Debug.LogError("RLD: Unsupported render pipeline. Only Standard and URP pipelines are supported.");
                    Debug.Break();
                }
            }
        }

        private void Update()
        {
            // Note: Don't change the order :)
            RTInputDevice.Get.Update_SystemCall();
            RTFocusCamera.Get.Update_SystemCall();
            RTScene.Get.Update_SystemCall();
            RTSceneGrid.Get.Update_SystemCall();
            RTGizmosEngine.Get.Update_SystemCall();
            RTUndoRedo.Get.Update_SystemCall();
        }

        private void OnRenderObject()
        {
            if (_renderPipelineId == RenderPipelineId.Standard) _renderCamera = Camera.current;

            if (RTGizmosEngine.Get.IsSceneGizmoCamera(_renderCamera))
            {
                RTGizmosEngine.Get.Render_SystemCall(_renderCamera);
            }
            else
            {
                // Note: Don't change the order :)
                if (RTCameraBackground.Get != null) RTCameraBackground.Get.Render_SystemCall(_renderCamera);
                RTSceneGrid.Get.Render_SystemCall(_renderCamera);
                RTGizmosEngine.Get.Render_SystemCall(_renderCamera);
            }
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            _renderCamera = camera;
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            // HDRP not currently supported.
            if (_renderPipelineId == RenderPipelineId.HDRP)
            {
            }
        }

        private void OnDisable()
        {
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
                RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            }
        }

        #if UNITY_EDITOR
        [MenuItem("Tools/Runtime Transform Gizmos/Initialize")]
        public static void Initialize()
        {
            DestroyAppAndModules();
            RTGApp gizmosApp = CreateAppModuleObject<RTGApp>(null);
            Transform appTransform = gizmosApp.transform;

            CreateAppModuleObject<RTGizmosEngine>(appTransform);

            CreateAppModuleObject<RTScene>(appTransform);
            CreateAppModuleObject<RTSceneGrid>(appTransform);

            var focusCamera = CreateAppModuleObject<RTFocusCamera>(appTransform);
            focusCamera.SetTargetCamera(Camera.main);
            CreateAppModuleObject<RTCameraBackground>(appTransform);

            CreateAppModuleObject<RTInputDevice>(appTransform);
            CreateAppModuleObject<RTUndoRedo>(appTransform);
        }

        private static DataType CreateAppModuleObject<DataType>(Transform parentTransform) where DataType : MonoBehaviour
        {
            string objectName = typeof(DataType).ToString();
            int dotIndex = objectName.IndexOf(".");
            if (dotIndex >= 0) objectName = objectName.Remove(0, dotIndex + 1);

            GameObject moduleObject = new GameObject(objectName);
            moduleObject.transform.parent = parentTransform;

            return moduleObject.AddComponent<DataType>();
        }

        private static void DestroyAppAndModules()
        {
            Type[] allModuleTypes = GetAppModuleTypes();
            foreach (var moduleType in allModuleTypes)
            {
                var allModulesInScene = MonoBehaviour.FindObjectsOfType(moduleType);
                foreach(var module in allModulesInScene)
                {
                    MonoBehaviour moduleMono = module as MonoBehaviour;
                    if (moduleMono != null) DestroyImmediate(moduleMono.gameObject);
                }
            }
        }

        private static Type[] GetAppModuleTypes()
        {
            return new Type[]
            {
                typeof(RTGApp), typeof(RTFocusCamera), typeof(RTCameraBackground), 
                typeof(RTScene), typeof(RTSceneGrid), 
                typeof(RTInputDevice), typeof(RTUndoRedo), typeof(RTGizmosEngine),
            };
        }
        #endif
    }
}
