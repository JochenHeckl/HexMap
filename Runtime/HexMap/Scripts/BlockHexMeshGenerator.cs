/// Thanks Amit! https://www.redblobgames.com/grids/hexagons

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace de.JochenHeckl.Unity.HexMap
{
    /// <summary>
    /// Generates a mesh made out of hexagonal blocks
    /// </summary>
    /// <typeparam name="TileDataType"></typeparam>
    public class BlockHexMeshGenerator<TileDataType> : IHexMeshGenerator<TileDataType>
        where TileDataType : ITileData
    {
        public struct BlockData
        {
            public float VerticalOffset;
            public float Height;
        }

        private readonly Vector3[] blockVertexOffsetsNorm = new Vector3[]
        {
            // top
            new Vector3(0f, 1f, 0f),
            new Vector3(1f, 1f, 0f),
            new Vector3(.5f, 1f, Constants.SqrtThreeOverTwo),
            new Vector3(-.5f, 1f, Constants.SqrtThreeOverTwo),
            new Vector3(-1f, 1f, 0f),
            new Vector3(-.5f, 1f, -Constants.SqrtThreeOverTwo),
            new Vector3(.5f, 1f, -Constants.SqrtThreeOverTwo),
            // bottom
            new Vector3(0f, -1f, 0f),
            new Vector3(1f, -1f, 0f),
            new Vector3(.5f, -1f, Constants.SqrtThreeOverTwo),
            new Vector3(-.5f, -1f, Constants.SqrtThreeOverTwo),
            new Vector3(-1f, -1f, 0f),
            new Vector3(-.5f, -1f, -Constants.SqrtThreeOverTwo),
            new Vector3(.5f, -1f, -Constants.SqrtThreeOverTwo)
        };

        private readonly Vector2[] blockUVs = new Vector2[]
        {
            // top
            new Vector3(0f, 1f),
            new Vector3(1f, 1f),
            new Vector3(1f, 1f),
            new Vector3(1f, 1f),
            new Vector3(1f, 1f),
            new Vector3(1f, 1f),
            new Vector3(1f, 1f),
            // bottom
            new Vector3(0f, 0f),
            new Vector3(1f, 0f),
            new Vector3(1f, 0f),
            new Vector3(1f, 0f),
            new Vector3(1f, 0f),
            new Vector3(1f, 0f),
            new Vector3(1f, 0f),
        };

        private readonly int[] blockIndices = new int[]
        {
            // upper cap
            0,
            2,
            1,
            0,
            3,
            2,
            0,
            4,
            3,
            0,
            5,
            4,
            0,
            6,
            5,
            0,
            1,
            6,
            // lowe cap
            7,
            8,
            9,
            7,
            9,
            10,
            7,
            10,
            11,
            7,
            11,
            12,
            7,
            12,
            13,
            7,
            13,
            8,
            // sides
            1,
            9,
            8,
            1,
            2,
            9,
            2,
            10,
            9,
            2,
            3,
            10,
            3,
            11,
            10,
            3,
            4,
            11,
            4,
            12,
            11,
            4,
            5,
            12,
            5,
            13,
            12,
            5,
            6,
            13,
            6,
            8,
            13,
            6,
            1,
            8,
        };

        private float hexRadius;
        private Func<
            AxialCoordinateInt,
            TileDataType,
            (Vector3 scale, Vector3 offset)
        > getBlockData;

        public BlockHexMeshGenerator(
            float tileRadius,
            Func<AxialCoordinateInt, TileDataType, (Vector3 scale, Vector3 offset)> getBlockData
        )
        {
            this.hexRadius = tileRadius;
            this.getBlockData = getBlockData;
        }

        public Mesh GenerateMesh(ITileDataStorage<TileDataType> dataSourceIn)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();

            var blockTriangleIndices = new List<int>();

            foreach (var tileData in dataSourceIn.Tiles)
            {
                AppendBlockMeshData(
                    tileData.coordinate,
                    tileData.data,
                    vertices,
                    uvs,
                    blockTriangleIndices
                );
            }

            var mesh = new Mesh();
            mesh.indexFormat =
                (vertices.Count < ushort.MaxValue) ? IndexFormat.UInt16 : IndexFormat.UInt32;

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.subMeshCount = 1;
            mesh.triangles = blockTriangleIndices.ToArray();

            return mesh;
        }

        private void AppendBlockMeshData(
            AxialCoordinateInt coordinate,
            TileDataType tileData,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangleIndices
        )
        {
            var blockBaseIndex = vertices.Count;
            var (vertexScale, vertexOffset) = getBlockData(coordinate, tileData);

            var blockVertices = blockVertexOffsetsNorm.Select(
                x =>
                    new Vector3((x.x * vertexScale.x), (x.y * vertexScale.y), (x.z * vertexScale.z))
                    + vertexOffset
            );

            vertices.AddRange(blockVertices);
            uvs.AddRange(blockUVs);

            triangleIndices.AddRange(blockIndices.Select(x => blockBaseIndex + x));
        }
    }
}
