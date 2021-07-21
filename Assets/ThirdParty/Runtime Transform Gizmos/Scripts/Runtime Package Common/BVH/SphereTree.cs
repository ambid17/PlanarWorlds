using UnityEngine;
using System.Collections.Generic;

namespace RTG
{
    public class SphereTree<T>
    {
        private SphereTreeNode<T> _root;

        public SphereTree()
        {
            _root = new SphereTreeNode<T>(default(T), new Sphere(Vector3.zero, 1.0f));
        }

        public void DebugDraw()
        {
            // Setup the rendering material
            Material material = MaterialPool.Get.GizmoSolidHandle;
            material.SetInt("_IsLit", 0);
            material.SetColor(ColorEx.KeepAllButAlpha(Color.green, 0.3f));
            material.SetPass(0);

            // Draw the entire tree
            _root.DebugDraw();
        }

        public SphereTreeNode<T> AddNode(T nodeData, Sphere sphere)
        {
            var newNode = new SphereTreeNode<T>(nodeData, sphere);
            InsertNode(newNode);

            return newNode;
        }

        public void RemoveNode(SphereTreeNode<T> node)
        {
            // The root node can never be removed
            if (node == _root) return;
     
            // Keep moving up the hierarchy and remove all nodes which don't
            // have any child nodes any more. There's no point in keeping these
            // around as they're nothing but noise inside the tree.
            SphereTreeNode<T> parent = node.Parent;
            node.SetParent(null);
            while (parent != null && parent.NumChildren == 0 && parent != _root)
            {
                SphereTreeNode<T> newParent = parent.Parent;
                parent.SetParent(null);
                parent = newParent;
            }

            // We have been removing nodes, so we need to make sure that the parent
            // at which we stopped the removal process has its volume recalculated.
            parent.EncapsulateChildrenBottomUp();
        }

        public void OnNodeSphereUpdated(SphereTreeNode<T> node)
        {
            // Just make sure this is a terminal node. Otherwise, ignore.
            if (!node.IsLeaf) return;

            // Check if the node is now outside of its parent
            if(node.IsOutsideParent())
            {
                // The node is outside of its parent. In this case, the first step
                // is to detach it from its parent.
                SphereTreeNode<T> parent = node.Parent;
                node.SetParent(null);

                // Now if the parent no longer has any children, we remove it from the
                // tree. Otherwise, we make sure it properly encapsulates its children.
                if (parent.NumChildren == 0) RemoveNode(parent);
                else parent.EncapsulateChildrenBottomUp();

                // The node needs to be reintegrated inside the tree
                InsertNode(node);
            }
        }

        public bool RaycastAll(Ray ray, List<SphereTreeNodeRayHit<T>> hits)
        {
            hits.Clear();
            if (_root == null) return false;

            float t;
            _root.StackPush(_root);
            while (_root.StackTop != null)
            {
                var node = _root.StackPop();
                if (!node.IsLeaf)
                {
                    if (SphereMath.Raycast(ray, node.Center, node.Radius))
                    {
                        if (SphereMath.Raycast(ray, node.Children[0].Center, node.Children[0].Radius)) _root.StackPush(node.Children[0]);
                        if (node.Children[1] != null && SphereMath.Raycast(ray, node.Children[1].Center, node.Children[1].Radius)) _root.StackPush(node.Children[1]);
                    }
                }
                else
                {
                    if (SphereMath.Raycast(ray, out t, node.Center, node.Radius))
                        hits.Add(new SphereTreeNodeRayHit<T>(ray, node, t));
                }
            }

            return hits.Count != 0;
        }

        public bool OverlapBox(OBB box, List<SphereTreeNode<T>> nodes)
        {
            nodes.Clear();
            if (_root == null) return false;

            _root.StackPush(_root);
            while (_root.StackTop != null)
            {
                var node = _root.StackPop();
                if (!node.IsLeaf)
                {
                    if (SphereMath.ContainsPoint(box.GetClosestPoint(node.Center), node.Center, node.Radius))
                    {
                        if (SphereMath.ContainsPoint(box.GetClosestPoint(node.Children[0].Center), node.Children[0].Center, node.Children[0].Radius)) _root.StackPush(node.Children[0]);
                        if (node.Children[1] != null && SphereMath.ContainsPoint(box.GetClosestPoint(node.Children[1].Center), node.Children[1].Center, node.Children[1].Radius)) _root.StackPush(node.Children[1]);
                    }
                }
                else
                {
                    if (SphereMath.ContainsPoint(box.GetClosestPoint(node.Center), node.Center, node.Radius)) nodes.Add(node);
                }
            }

            return nodes.Count != 0;
        }

        private void InsertNode(SphereTreeNode<T> node)
        {
            var parent = _root;
            while (true)
            {
                if (!parent.IsLeaf)
                {
                    if (parent.NumChildren < 2)
                    {
                        node.SetParent(parent);
                        parent.EncapsulateChildrenBottomUp();
                        break;
                    }
                    else parent = parent.ClosestChild(node);
                }
                else
                {
                    SphereTreeNode<T> newParentNode = new SphereTreeNode<T>();
                    newParentNode.Data = default;
                    newParentNode.Sphere = parent.Sphere;

                    // Note: Detach from current parent to make sure the parent's child 
                    //       buffer can receive 'newParentNode' as child.
                    var oldParent = parent.Parent;
                    parent.SetParent(null);

                    newParentNode.SetParent(oldParent);
                    parent.SetParent(newParentNode);

                    node.SetParent(newParentNode);
                    newParentNode.EncapsulateChildrenBottomUp();

                    if (parent == _root) _root = newParentNode;
                    break;
                }
            }
        }
    }
}
