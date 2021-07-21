using UnityEngine;

namespace RTG
{
    public class SphereTreeNode<T> 
    {
        #region Stack Optimization
        private SphereTreeNode<T> _stackTop = null;
        private SphereTreeNode<T> _stackPrevious = null;
        #endregion

        private Sphere _sphere;
        private T _data;
        private SphereTreeNode<T> _parent;
        private SphereTreeNode<T>[] _children = new SphereTreeNode<T>[2];
        private int _numChildren = 0;

        public SphereTreeNode<T>[] Children { get { return _children; } }
        public int NumChildren { get { return _numChildren; } }
        public bool IsLeaf { get { return _data != null; } }

        public SphereTreeNode()
        {
            _sphere = new Sphere(Vector3.zero, 1.0f);
            _data = default(T);
        }

        public SphereTreeNode(T data, Sphere sphere)
        {
            _sphere = sphere;
            _data = data;
        }

        public Sphere Sphere { get { return _sphere; } set { _sphere = value; } }
        public Vector3 Center { get { return _sphere.Center; } set { _sphere.Center = value; } }
        public float Radius { get { return _sphere.Radius; } set { _sphere.Radius = value; } }
        public SphereTreeNode<T> Parent { get { return _parent; } }
        public T Data { get { return _data; } set { _data = value; } }

        #region Stack Optimization
        public SphereTreeNode<T> StackTop { get { return _stackTop; } }
        public void StackPush(SphereTreeNode<T> node)
        {
            node._stackPrevious = _stackTop;
            _stackTop = node;     
        }
        public SphereTreeNode<T> StackPop()
        {
            SphereTreeNode<T> top = _stackTop;
            _stackTop = top._stackPrevious;

            return top;
        }
        #endregion

        /// <summary>
        /// Checks if the node's sphere is outside of its parent's 
        /// sphere. The method returns false if the node doesn't have
        /// a parent.
        /// </summary>
        public bool IsOutsideParent()
        {
            // No parent?
            if (_parent == null) return false;

            // Calculate the exit distance. If this is larger than the parent's
            // radius, it means the node is outside its parent.
            float exitDistance = (Center - _parent.Center).magnitude + Radius;
            if (exitDistance > _parent.Radius) return true;

            return false;
        }

        /// <summary>
        /// Finds the child which is closest to 'node'.
        /// </summary>
        /// <returns>
        /// The child closest to 'node' or null if the no children
        /// are available.
        /// </returns>
        public SphereTreeNode<T> ClosestChild(SphereTreeNode<T> node)
        {
            if (_numChildren == 0) return null;

            SphereTreeNode<T> closestChild = _children[0];
            float minDistSq = (node.Center - _children[0].Center).magnitude;
            if (_children[1] != null)
            {
                float d = (node.Center - _children[1].Center).magnitude;
                if (d < minDistSq) closestChild = _children[1];
            }

            return closestChild;
        }

        /// <summary>
        /// Sets the node's parent. This call is ignored if the specified parent
        /// is the node itself or if it's the same as the current parent.
        /// </summary>
        public void SetParent(SphereTreeNode<T> newParent)
        {
            // Ignore parent?
            if (newParent == this || newParent == _parent) return;

            // If we already have a parent, detach the node from it
            if (_parent != null)
            {
                if (_parent._children[0] == this)
                {
                    _parent._children[0] = _parent._children[1];
                    _parent._children[1] = null;
                }
                else _parent._children[1] = null;

                --_parent._numChildren;
                _parent = null;
            }

            if (newParent != null)
            {
                _parent = newParent;
                _parent._children[_parent._numChildren++] = this;
            }
            else _parent = null;
        }

        /// <summary>
        /// This method will recalculate the node's center and radius
        /// so that it encapsulates all children. This is a recursive
        /// call which propagates up the hierarchy towards the root.
        /// </summary>
        public void EncapsulateChildrenBottomUp()
        {
            // Nothing to do if the node doesn't have any children
            if (NumChildren != 0)
            {
                SphereTreeNode<T> parent = this;
                while (parent != null)
                {
                    // First, we will calculate the new sphere center as the average
                    // of all child node centers.
                    Vector3 centerSum = parent._children[0].Center;
                    if (parent._children[1] != null) centerSum += parent._children[1].Center;
                    parent.Center = centerSum * (1.0f / parent.NumChildren);

                    // Now we will calculate the radius which the node must have so that
                    // it can encapsulate all its children.
                    float maxRadius = (parent._children[0].Center - parent.Center).magnitude + parent._children[0].Radius;
                    if (parent._children[1] != null)
                    {
                        float r = (parent._children[1].Center - parent.Center).magnitude + parent._children[1].Radius;
                        if (r > maxRadius) maxRadius = r;
                    }

                    parent.Radius = maxRadius;
                    parent = parent.Parent;
                }
            }
        }

        /// <summary>
        /// Allows the node to render itself for debugging purposes. The client
        /// code is responsible for setting up the rendering material.
        /// </summary>
        /// <remarks>
        /// This method is recursive and will draw the node's children also. Thus,
        /// it is enough to call this method for the root of a sphere tree in order
        /// to draw the entire tree.
        /// </remarks>
        public void DebugDraw()
        {
            // Draw the node
            Matrix4x4 nodeTransform = Matrix4x4.TRS(_sphere.Center, Quaternion.identity, Vector3Ex.FromValue(_sphere.Radius));
            Graphics.DrawMeshNow(MeshPool.Get.UnitSphere, nodeTransform);

            // Draw the node's children
            foreach (var child in _children) child.DebugDraw();
        }
    }
}
