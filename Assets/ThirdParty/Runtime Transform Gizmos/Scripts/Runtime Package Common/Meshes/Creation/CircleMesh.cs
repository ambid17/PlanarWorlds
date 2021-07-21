﻿using UnityEngine;
using System.Collections.Generic;

namespace RTG
{
    public static class CircleMesh
    {
        public static Mesh CreateCircleXY(float circleRadius, int numBorderPoints, Color color)
        {
            if (circleRadius < 1e-4f || numBorderPoints < 4) return null;

            int numVerts = numBorderPoints + 1;
            int numTriangles = numBorderPoints - 1;

            Vector3[] positions = new Vector3[numBorderPoints + 1];
            Vector3[] normals = new Vector3[positions.Length];
            int[] indices = new int[numTriangles * 3];

            int indexPtr = 0;
            positions[0] = Vector3.zero;
            float angleStep = 360.0f / (numBorderPoints - 1);
            for (int ptIndex = 0; ptIndex < numBorderPoints; ++ptIndex)
            {
                float angle = angleStep * ptIndex * Mathf.Deg2Rad;

                int vertIndex = ptIndex + 1;
                positions[vertIndex] = new Vector3(Mathf.Sin(angle) * circleRadius, Mathf.Cos(angle) * circleRadius, 0.0f);
                normals[vertIndex] = Vector3.forward;
            }

            for (int vertIndex = 1; vertIndex < numVerts - 1; ++vertIndex)
            {
                indices[indexPtr++] = 0;
                indices[indexPtr++] = vertIndex;
                indices[indexPtr++] = vertIndex + 1;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = positions;
            mesh.colors = ColorEx.GetFilledColorArray(positions.Length, color);
            mesh.normals = normals;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            return mesh;
        }

        public static Mesh CreateWireCircleXY(float circleRadius, int numBorderPoints, Color color)
        {
            if (circleRadius < 1e-4f || numBorderPoints < 4) return null;

            Vector3[] positions = new Vector3[numBorderPoints];
            int[] indices = new int[numBorderPoints];

            float angleStep = 360.0f / (numBorderPoints - 1);
            for (int ptIndex = 0; ptIndex < numBorderPoints; ++ptIndex)
            {
                float angle = angleStep * ptIndex * Mathf.Deg2Rad;
                positions[ptIndex] = new Vector3(Mathf.Sin(angle) * circleRadius, Mathf.Cos(angle) * circleRadius, 0.0f);
                indices[ptIndex] = ptIndex;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = positions;
            mesh.colors = ColorEx.GetFilledColorArray(numBorderPoints, color);
            mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
            mesh.UploadMeshData(false);

            return mesh;
        }
    }
}
