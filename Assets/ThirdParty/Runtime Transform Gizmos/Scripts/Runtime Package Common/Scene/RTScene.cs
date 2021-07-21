using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace RTG
{
    // TODO: Manual vs Automatic object management;
    public class RTScene : MonoSingleton<RTScene>
    {
        [SerializeField]
        private SceneSettings _settings = new SceneSettings();

        private List<IHoverableSceneEntityContainer> _hoverableSceneEntityContainers = new List<IHoverableSceneEntityContainer>();
        private SceneTree _sceneTree = new SceneTree();

        private HashSet<GameObject> _ignoredRootObjects = new HashSet<GameObject>();
        private List<GameObject> _childrenAndSelfBuffer = new List<GameObject>(100);
        private List<GameObject> _rootGameObjectsBuffer = new List<GameObject>();
        private List<GameObjectRayHit> _objectHitBuffer = new List<GameObjectRayHit>();

        public SceneSettings Settings { get { return _settings; } }

        public void SetRootObjectIgnored(GameObject root, bool ignored)
        {
            if (Settings.PhysicsMode != ScenePhysicsMode.RTG) return;

            if (ignored) _ignoredRootObjects.Add(root);
            else _ignoredRootObjects.Remove(root);
        }

        public void OnGameObjectWillBeDestroyed(GameObject gameObject)
        {
            if (Settings.PhysicsMode != ScenePhysicsMode.RTG) return;
            if (_ignoredRootObjects.Contains(gameObject)) return;

            gameObject.GetAllChildrenAndSelf(_childrenAndSelfBuffer);
            int numObjectsInHierarchy = _childrenAndSelfBuffer.Count;
            for (int objectIndex = 0; objectIndex < numObjectsInHierarchy; ++objectIndex)
                _sceneTree.UnregisterObject(_childrenAndSelfBuffer[objectIndex]);
        }

        public AABB CalculateBounds()
        {
            var activeScene = SceneManager.GetActiveScene();
            var roots = new List<GameObject>(Mathf.Max(10, activeScene.rootCount));
            SceneManager.GetActiveScene().GetRootGameObjects(roots);

            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.NoVolumeSize = Vector3.zero;
            boundsQConfig.ObjectTypes = GameObjectType.Mesh | GameObjectType.Sprite;

            AABB sceneAABB = new AABB();
            foreach(var root in roots)
            {
                var allChildrenAndSelf = root.GetAllChildrenAndSelf();
                foreach(var sceneObject in allChildrenAndSelf)
                {
                    AABB aabb = ObjectBounds.CalcWorldAABB(sceneObject, boundsQConfig);
                    if(aabb.IsValid)
                    {
                        if (sceneAABB.IsValid) sceneAABB.Encapsulate(aabb);
                        else sceneAABB = aabb;
                    }
                }
            }

            return sceneAABB;
        }

        public bool IsAnySceneEntityHovered()
        {
            foreach (var container in _hoverableSceneEntityContainers)
                if (container.HasHoveredSceneEntity) return true;

            return IsAnyUIElementHovered();
        }

        public void RegisterHoverableSceneEntityContainer(IHoverableSceneEntityContainer container)
        {
            if (!_hoverableSceneEntityContainers.Contains(container)) _hoverableSceneEntityContainers.Add(container);
        }

        public bool IsAnyUIElementHovered()
        {
            return GetHoveredUIElements().Count != 0;
        }

        public List<RaycastResult> GetHoveredUIElements()
        {
            // No event system? Return an empty list.
            if (EventSystem.current == null) return new List<RaycastResult>();

            // Get the input device's screen coords. If the coords are not available, return an empty list.
            IInputDevice inputDevice = RTInputDevice.Get.Device;
            if (!inputDevice.HasPointer()) return new List<RaycastResult>();
            Vector2 inputDevicePos = inputDevice.GetPositionYAxisUp();

            // Construct the pointer event data instance needed for the raycast
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(inputDevicePos.x, inputDevicePos.y);

            // Raycast all and return the result
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            results.RemoveAll(item => item.gameObject.GetComponent<RectTransform>() == null);

            return results;
        }

        public GameObject[] GetSceneObjects()
        {
            return GameObject.FindObjectsOfType<GameObject>();
        }

        public bool OverlapBox(OBB obb, List<GameObject> gameObjects)
        {
            if (Settings.PhysicsMode == ScenePhysicsMode.UnityColliders)
            {
                gameObjects.Clear();

                // Retrieve the overlapped 3D objects and store them inside the output list
                Collider[] overlapped3DColliders = Physics.OverlapBox(obb.Center, obb.Extents, obb.Rotation);
                foreach (var collider in overlapped3DColliders) gameObjects.Add(collider.gameObject);

                // Retrieve the overlapped 2D objects. This requires some further calculations
                // because colliders of 2D sprites exist in the XY plane, so we must project the
                // OBB onto the XY plane, and use the min and max projected points to build an 
                // overlap area.
                Plane xyPlane = new Plane(Vector3.forward, 0.0f);
                List<Vector3> projectedCorners = xyPlane.ProjectAllPoints(obb.GetCornerPoints());
                AABB projectedPtsAABB = new AABB(projectedCorners);

                // The OBB has been projected onto the sprite XY plane. We can now use the AABB's min and max
                // extents to construct the overlap area and use the 'OverlapAreaAll' function to retrieve the
                // overlapped 2D colliders.
                Collider2D[] overlapped2DColliders = Physics2D.OverlapAreaAll(projectedPtsAABB.Min, projectedPtsAABB.Max);
                foreach (var collider in overlapped2DColliders) gameObjects.Add(collider.gameObject);
               
                return gameObjects.Count != 0;
            }
            else return _sceneTree.OverlapBox(obb, gameObjects);
        }

        public bool OverlapBox(OBB obb, SceneOverlapFilter overlapFilter, List<GameObject> gameObjects)
        {
            OverlapBox(obb, gameObjects);
            overlapFilter.FilterOverlaps(gameObjects);

            return gameObjects.Count != 0;
        }

        public SceneRaycastHit Raycast(Ray ray, SceneRaycastPrecision rtRaycastPrecision, SceneRaycastFilter raycastFilter)
        {
            RaycastAllObjectsSorted(ray, rtRaycastPrecision, raycastFilter, _objectHitBuffer);
            GameObjectRayHit closestObjectHit = _objectHitBuffer.Count != 0 ? _objectHitBuffer[0] : null;
            XZGridRayHit gridRayHit = RaycastSceneGridIfVisible(ray);

            return new SceneRaycastHit(closestObjectHit, gridRayHit);
        }

        public bool RaycastAllObjects(Ray ray, SceneRaycastPrecision rtRaycastPrecision, List<GameObjectRayHit> hits)
        {
            if (Settings.PhysicsMode == ScenePhysicsMode.UnityColliders)
            {
                hits.Clear();
                RaycastHit[] hits3D = Physics.RaycastAll(ray, float.MaxValue);
                RaycastHit2D[] hits2D = Physics2D.GetRayIntersectionAll(ray, float.MaxValue);
                GameObjectRayHit.Store(ray, hits2D, hits3D, hits);

                return hits.Count != 0;
            }
            else return _sceneTree.RaycastAll(ray, rtRaycastPrecision, hits);
        }

        public bool RaycastAllObjectsSorted(Ray ray, SceneRaycastPrecision raycastPresicion, List<GameObjectRayHit> hits)
        {
            bool anyHit = RaycastAllObjects(ray, raycastPresicion, hits);
            GameObjectRayHit.SortByHitDistance(hits);
  
            return anyHit;
        }

        public bool RaycastAllObjectsSorted(Ray ray, SceneRaycastPrecision rtRaycastPrecision, SceneRaycastFilter raycastFilter, List<GameObjectRayHit> hits)
        {
            hits.Clear();
            if (raycastFilter != null && raycastFilter.AllowedObjectTypes.Count == 0) return false;

            RaycastAllObjectsSorted(ray, rtRaycastPrecision, hits);
            if (raycastFilter != null) raycastFilter.FilterHits(hits);

            return hits.Count != 0;
        }

        public GameObjectRayHit RaycastMeshObject(Ray ray, GameObject meshObject)
        {
            if (Settings.PhysicsMode == ScenePhysicsMode.UnityColliders)
            {
                Collider raycastCollider = null;
                MeshCollider meshCollider = meshObject.GetComponent<MeshCollider>();
                if(meshCollider != null) raycastCollider = meshCollider;
                if(raycastCollider == null) raycastCollider = meshObject.GetComponent<Collider>();

                if(raycastCollider != null)
                {
                    RaycastHit rayHit;
                    if (raycastCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);
                }
                return null;
            }
            else return _sceneTree.RaycastMeshObject(ray, meshObject);
        }

        public GameObjectRayHit RaycastMeshObjectReverseIfFail(Ray ray, GameObject meshObject)
        {
            GameObjectRayHit hit = RaycastMeshObject(ray, meshObject);
            if (hit == null) hit = RaycastMeshObject(new Ray(ray.origin, -ray.direction), meshObject);

            return hit;
        }

        public GameObjectRayHit RaycastSpriteObject(Ray ray, GameObject spriteObject)
        {
            // NOTE: 'ObjectInteractionMode.UnityColliders' must be ignored here as there doesn't seem to
            //       be a way to raycast against a 2D collider.
            return _sceneTree.RaycastSpriteObject(ray, spriteObject);
        }

        public GameObjectRayHit RaycastTerrainObject(Ray ray, GameObject terrainObject)
        {
            TerrainCollider terrainCollider = terrainObject.GetComponent<TerrainCollider>();
            if (terrainCollider == null) return null;

            RaycastHit rayHit;
            if (terrainCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);

            return null;
        }

        public GameObjectRayHit RaycastTerrainObject(Ray ray, GameObject terrainObject, TerrainCollider terrainCollider)
        {
            RaycastHit rayHit;
            if (terrainCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);

            return null;
        }

        public GameObjectRayHit RaycastTerrainObjectReverseIfFail(Ray ray, GameObject terrainObject)
        {
            GameObjectRayHit hit = RaycastTerrainObject(ray, terrainObject);
            if (hit == null) hit = RaycastTerrainObject(new Ray(ray.origin, -ray.direction), terrainObject);

            return hit;
        }

        public XZGridRayHit RaycastSceneGridIfVisible(Ray ray)
        {
            if (!RTSceneGrid.Get.Settings.IsVisible) return null;

            float t;
            if(RTSceneGrid.Get.Raycast(ray, out t))
            {
                XZGridCell hitCell = RTSceneGrid.Get.CellFromWorldPoint(ray.GetPoint(t));
                return new XZGridRayHit(ray, hitCell, t);
            }

            return null;
        }

        public void Update_SystemCall()
        {
            if (Settings.PhysicsMode == ScenePhysicsMode.RTG)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                int numRoots = activeScene.rootCount;
                if (_rootGameObjectsBuffer.Capacity <= numRoots) _rootGameObjectsBuffer.Capacity = numRoots + 100;
                activeScene.GetRootGameObjects(_rootGameObjectsBuffer);

                for (int rootIndex = 0; rootIndex < numRoots; ++rootIndex)
                {
                    GameObject rootObject = _rootGameObjectsBuffer[rootIndex];
                    if (_ignoredRootObjects.Contains(rootObject)) continue;

                    rootObject.GetAllChildrenAndSelf(_childrenAndSelfBuffer);

                    int numObjectsInHierarchy = _childrenAndSelfBuffer.Count;
                    for (int sceneObjIndex = 0; sceneObjIndex < numObjectsInHierarchy; ++sceneObjIndex)
                    {
                        GameObject sceneObject = _childrenAndSelfBuffer[sceneObjIndex];
                        if (!_sceneTree.IsObjectRegistered(sceneObject)) _sceneTree.RegisterObject(sceneObject);
                        else
                        {
                            Transform objectTransform = sceneObject.transform;
                            if (objectTransform.hasChanged)
                            {
                                _sceneTree.OnObjectTransformChanged(objectTransform);
                                objectTransform.hasChanged = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
