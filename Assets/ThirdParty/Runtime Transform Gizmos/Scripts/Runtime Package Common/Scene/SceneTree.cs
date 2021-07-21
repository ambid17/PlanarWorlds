using UnityEngine;
using System.Collections.Generic;

namespace RTG
{
    public class SceneTree
    {
        private static float _nonMeshObjectSize = 1e-4f;
        private SphereTree<GameObject> _objectTree = new SphereTree<GameObject>();
        private List<SphereTreeNodeRayHit<GameObject>> _nodeHitBuffer = new List<SphereTreeNodeRayHit<GameObject>>();
        private List<SphereTreeNode<GameObject>> _nodeBuffer = new List<SphereTreeNode<GameObject>>();
        private Dictionary<GameObject, SphereTreeNode<GameObject>> _objectToNode = new Dictionary<GameObject, SphereTreeNode<GameObject>>();

        public GameObjectRayHit RaycastMeshObject(Ray ray, GameObject gameObject)
        {
            Mesh objectMesh = gameObject.GetMesh();
            RTMesh rtMesh = RTMeshDb.Get.GetRTMesh(objectMesh);

            if (rtMesh != null)
            {
                MeshRayHit meshRayHit = rtMesh.Raycast(ray, gameObject.transform.localToWorldMatrix);
                if (meshRayHit != null) return new GameObjectRayHit(ray, gameObject, meshRayHit);
            }
            else
            {
                // If no RTMesh instance is available, we will cast a ray against
                // the object's MeshCollider as a last resort. This is actually useful
                // when dealing with static mesh objects. These objects' meshes have
                // their 'isReadable' flag set to false and can not be used to create
                // an RTMesh instance. Thus a mesh collider is the next best choice.
                MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    RaycastHit rayHit;
                    if (meshCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);
                }
            }

            return null;
        }

        public GameObjectRayHit RaycastSpriteObject(Ray ray, GameObject gameObject)
        {
            float t;
            OBB worldOBB = ObjectBounds.CalcSpriteWorldOBB(gameObject);
            if (!worldOBB.IsValid) return null;

            if (BoxMath.Raycast(ray, out t, worldOBB.Center, worldOBB.Size, worldOBB.Rotation)) 
                return new GameObjectRayHit(ray, gameObject, worldOBB.GetPointFaceNormal(ray.GetPoint(t)), t);

            return null;
        }

        public bool RaycastAll(Ray ray, SceneRaycastPrecision raycastPresicion, List<GameObjectRayHit> hits)
        {
            hits.Clear();
            if (!_objectTree.RaycastAll(ray, _nodeHitBuffer)) return false;

            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectTypeHelper.AllCombined;
            boundsQConfig.NoVolumeSize = Vector3Ex.FromValue(_nonMeshObjectSize);

            Vector3 camLook = RTFocusCamera.Get.Look;
            if (raycastPresicion == SceneRaycastPrecision.BestFit)
            {
                foreach (var nodeHit in _nodeHitBuffer)
                {
                    GameObject sceneObject = nodeHit.HitNode.Data;
                    if (sceneObject == null || !sceneObject.activeInHierarchy) continue;

                    Renderer renderer = sceneObject.GetComponent<Renderer>();
                    if (renderer != null && !renderer.isVisible) continue;

                    GameObjectType objectType = sceneObject.GetGameObjectType();
                    if (objectType == GameObjectType.Mesh)
                    {
                        GameObjectRayHit objectHit = RaycastMeshObject(ray, sceneObject);
                        if (objectHit != null) hits.Add(objectHit);
                    }
                    else
                    if (objectType == GameObjectType.Terrain)
                    {
                        TerrainCollider terrainCollider = sceneObject.GetComponent<TerrainCollider>();
                        if(terrainCollider != null)
                        {
                            RaycastHit hitInfo;
                            if (terrainCollider.Raycast(ray, out hitInfo, float.MaxValue)) hits.Add(new GameObjectRayHit(ray, hitInfo));
                        }
                    }
                    else
                    if(objectType == GameObjectType.Sprite)
                    {
                        GameObjectRayHit objectHit = RaycastSpriteObject(ray, sceneObject);
                        if (objectHit != null) hits.Add(objectHit);
                    }
                    else
                    {
                        OBB worldOBB = ObjectBounds.CalcWorldOBB(sceneObject, boundsQConfig);
                        if (worldOBB.IsValid)
                        {
                            float t;
                            if (BoxMath.Raycast(ray, out t, worldOBB.Center, worldOBB.Size, worldOBB.Rotation))
                            {
                                var faceDesc = BoxMath.GetFaceClosestToPoint(ray.GetPoint(t), worldOBB.Center, worldOBB.Size, worldOBB.Rotation, camLook);
                                var hit = new GameObjectRayHit(ray, sceneObject, faceDesc.Plane.normal, t);
                                hits.Add(hit);
                            }
                        }
                    }
                }
            }
            else
            if (raycastPresicion == SceneRaycastPrecision.Box)
            {
                foreach (var nodeHit in _nodeHitBuffer)
                {
                    GameObject sceneObject = nodeHit.HitNode.Data;
                    if (sceneObject == null || !sceneObject.activeInHierarchy) continue;

                    Renderer renderer = sceneObject.GetComponent<Renderer>();
                    if (renderer != null && !renderer.isVisible) continue;

                    OBB worldOBB = ObjectBounds.CalcWorldOBB(sceneObject, boundsQConfig);
                    if (worldOBB.IsValid)
                    {
                        float t;
                        if (BoxMath.Raycast(ray, out t, worldOBB.Center, worldOBB.Size, worldOBB.Rotation))
                        {
                            var faceDesc = BoxMath.GetFaceClosestToPoint(ray.GetPoint(t), worldOBB.Center, worldOBB.Size, worldOBB.Rotation, camLook);
                            var hit = new GameObjectRayHit(ray, sceneObject, faceDesc.Plane.normal, t);
                            hits.Add(hit);
                        }
                    }
                }
            }

            return hits.Count != 0;
        }

        public bool OverlapBox(OBB obb, List<GameObject> gameObjects)
        {
            gameObjects.Clear();
            if (!_objectTree.OverlapBox(obb, _nodeBuffer)) return false;

            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectTypeHelper.AllCombined;
            boundsQConfig.NoVolumeSize = Vector3Ex.FromValue(_nonMeshObjectSize);

            foreach (SphereTreeNode<GameObject> node in _nodeBuffer)
            {
                GameObject sceneObject = (GameObject)node.Data;
                if (sceneObject == null || !sceneObject.activeInHierarchy) continue;

                OBB worldOBB = ObjectBounds.CalcWorldOBB(sceneObject, boundsQConfig);
                if (obb.IntersectsOBB(worldOBB)) gameObjects.Add(sceneObject);
            }

            return gameObjects.Count != 0;
        }

        public bool IsObjectRegistered(GameObject gameObject)
        {
            return _objectToNode.ContainsKey(gameObject);
        }

        public bool RegisterObject(GameObject gameObject)
        {
            if (!CanRegisterObject(gameObject)) return false;

            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectTypeHelper.AllCombined;
            boundsQConfig.NoVolumeSize = Vector3Ex.FromValue(_nonMeshObjectSize);

            AABB worldAABB = ObjectBounds.CalcWorldAABB(gameObject, boundsQConfig);
            Sphere worldSphere = new Sphere(worldAABB);

            SphereTreeNode<GameObject> objectNode = _objectTree.AddNode(gameObject, worldSphere);
            _objectToNode.Add(gameObject, objectNode);

            RTFocusCamera.Get.SetObjectVisibilityDirty();
            return true;
        }

        public bool UnregisterObject(GameObject gameObject)
        {
            if (!IsObjectRegistered(gameObject)) return false;

            _objectTree.RemoveNode(_objectToNode[gameObject]);
            _objectToNode.Remove(gameObject);

            RTFocusCamera.Get.SetObjectVisibilityDirty();
            return true;
        }

        public void OnObjectTransformChanged(Transform objectTransform)
        {
            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectTypeHelper.AllCombined;
            boundsQConfig.NoVolumeSize = Vector3Ex.FromValue(_nonMeshObjectSize);

            AABB worldAABB = ObjectBounds.CalcWorldAABB(objectTransform.gameObject, boundsQConfig);
            Sphere worldSphere = new Sphere(worldAABB);

            SphereTreeNode<GameObject> objectNode = _objectToNode[objectTransform.gameObject];
            objectNode.Sphere = worldSphere;

            _objectTree.OnNodeSphereUpdated(objectNode);
            RTFocusCamera.Get.SetObjectVisibilityDirty();
        }

        public void RemoveNodesWithNullObjects()
        {
            var newObjectToNodeDictionary = new Dictionary<GameObject, SphereTreeNode<GameObject>>();
            foreach (var pair in _objectToNode)
            {
                if (pair.Key == null)  _objectTree.RemoveNode(pair.Value);
                else newObjectToNodeDictionary.Add(pair.Key, pair.Value);
            }

            _objectToNode.Clear();
            _objectToNode = newObjectToNodeDictionary;
        }

        public void DebugDraw()
        {
            _objectTree.DebugDraw();
        }

        private bool CanRegisterObject(GameObject gameObject)
        {
            if (gameObject == null || IsObjectRegistered(gameObject)) return false;
            if (gameObject.IsRTGAppObject()) return false;
            if (gameObject.GetComponent<RectTransform>() != null) return false;

            return true;
        }
    }
}
