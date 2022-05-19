using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Digger.Modules.Core.Sources
{
    public class DiggerMaster : ADiggerMonoBehaviour
    {
        public const string ParentFolder = "DiggerData";
        public const string ScenesBaseFolder = "Scenes";


        [SerializeField] private int chunkSize = 33;
        public int SizeOfMesh => chunkSize - 1;
        public int SizeVox => chunkSize + 1;

        [SerializeField] private string sceneDataFolder;
        [SerializeField] private float screenRelativeTransitionHeightLod0 = 0.1f;
        [SerializeField] private float screenRelativeTransitionHeightLod1 = 0.05f;
        [SerializeField] private int colliderLodIndex = 0;
        [SerializeField] private bool createLODs = true;
        [SerializeField] private bool showUnderlyingObjects = false;
        [SerializeField] private int resolutionMult = 1;
        [SerializeField] private float voxelHeight = 1f;
        [SerializeField] private bool autoVoxelHeight = false;
        [SerializeField] private int layer = 0;
        [SerializeField] private string chunksTag = "Untagged";
        [SerializeField] private bool enableOcclusionCulling = false;

        private static string ParentPath {
            get {
                var projectDir = new DirectoryInfo(Application.dataPath);
                if (projectDir.Exists && projectDir.Parent != null && projectDir.Parent.Exists) {
                    Directory.SetCurrentDirectory(projectDir.Parent.FullName);
                }

                return Path.Combine("Assets", ParentFolder);
            }
        }

        private static string ScenesBasePath => Path.Combine(ParentPath, ScenesBaseFolder);
        public string SceneDataPath => Path.Combine(ScenesBasePath, sceneDataFolder);

        public string SceneDataFolder {
            get => sceneDataFolder;
            set => sceneDataFolder = value;
        }

        public float ScreenRelativeTransitionHeightLod0 {
            get => screenRelativeTransitionHeightLod0;
            set => screenRelativeTransitionHeightLod0 = value;
        }

        public float ScreenRelativeTransitionHeightLod1 {
            get => screenRelativeTransitionHeightLod1;
            set => screenRelativeTransitionHeightLod1 = value;
        }

        public int ColliderLodIndex {
            get => colliderLodIndex;
            set => colliderLodIndex = value;
        }

        public int ResolutionMult {
            get => resolutionMult;
            set => resolutionMult = value;
        }

        public int ChunkSize {
            get => chunkSize;
            set => chunkSize = value;
        }

        public bool CreateLODs {
            get => createLODs;
            set => createLODs = value;
        }

        public bool ShowUnderlyingObjects {
            get => showUnderlyingObjects;
            set => showUnderlyingObjects = value;
        }

        public int Layer {
            get => layer;
            set => layer = value;
        }

        public string ChunksTag {
            get => chunksTag;
            set => chunksTag = value ?? "Untagged";
        }

        public bool EnableOcclusionCulling {
            get => enableOcclusionCulling;
            set => enableOcclusionCulling = value;
        }

        public float VoxelHeight {
            get => voxelHeight;
            set => voxelHeight = value;
        }

        public bool AutoVoxelHeight {
            get => autoVoxelHeight;
            set => autoVoxelHeight = value;
        }

        public void CreateDirs()
        {
#if UNITY_EDITOR
            if (!Directory.Exists(ParentPath)) {
                AssetDatabase.CreateFolder("Assets", ParentFolder);
            }

            if (!Directory.Exists(ScenesBasePath)) {
                AssetDatabase.CreateFolder(ParentPath, ScenesBaseFolder);
            }

            if (string.IsNullOrEmpty(sceneDataFolder)) {
                var sceneName = SceneManager.GetActiveScene().name;
                if (string.IsNullOrEmpty(sceneName)) {
                    sceneName = "Untitled";
                }

                sceneDataFolder = sceneName;
                var i = 0;
                while (Directory.Exists(SceneDataPath)) {
                    sceneDataFolder = $"{sceneName}-{i++}";
                }
            }

            if (!Directory.Exists(SceneDataPath)) {
                AssetDatabase.CreateFolder(ScenesBasePath, sceneDataFolder);
            }
#endif
        }
    }
}