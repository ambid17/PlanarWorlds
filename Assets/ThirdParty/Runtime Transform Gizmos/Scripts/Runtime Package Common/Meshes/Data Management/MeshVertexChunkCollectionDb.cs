﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTG
{
    public class MeshVertexChunkCollectionDb : Singleton<MeshVertexChunkCollectionDb>
    {
        private Dictionary<Mesh, MeshVertexChunkCollection> _meshToVChunkCollection = new Dictionary<Mesh, MeshVertexChunkCollection>();

        public MeshVertexChunkCollection this[Mesh mesh] 
        { 
            get 
            {
                if (!HasChunkCollectionForMesh(mesh) && !CreateMeshVertChunkCollection(mesh)) return null;
                return _meshToVChunkCollection[mesh]; 
            } 
        }

        public void SetMeshDirty(Mesh mesh)
        {
            MeshVertexChunkCollection chunkCollection = null;
            if (_meshToVChunkCollection.TryGetValue(mesh, out chunkCollection))
            {
                chunkCollection.FromMesh(mesh);
            }
        }

        public bool HasChunkCollectionForMesh(Mesh mesh)
        {
            return _meshToVChunkCollection.ContainsKey(mesh);
        }

        private bool CreateMeshVertChunkCollection(Mesh mesh)
        {
            var meshVertexChunkCollection = new MeshVertexChunkCollection();
            if(!meshVertexChunkCollection.FromMesh(mesh)) return false;

            _meshToVChunkCollection.Add(mesh, meshVertexChunkCollection);
            return true;
        }
    }
}
