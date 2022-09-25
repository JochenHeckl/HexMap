/// Thanks Amit! https://www.redblobgames.com/grids/hexagons

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

using static de.JochenHeckl.Unity.HexMap.Constants;

namespace de.JochenHeckl.Unity.HexMap
{
    public class HexWithInsetBordersMeshGenerator<TileDataType> : IHexMeshGenerator<TileDataType>
        where TileDataType : ITileData
    {
        // counter clock wise in the x/z plane
        private Vector3[] MakeNormTileVertices(float normBorderInset, float[] surfaceHeightDeltas)
        {
            var sin30TileRadius = 0.5f;
            var cos30TileRadius = SqrtThreeOverTwo;

            var twoOverThree = 2f / 3f;

            var innerRadius = 1f - (normBorderInset / SqrtThreeOverTwo);
            var sin30InnerRadius = 0.5f * innerRadius;
            var cos30InnerRadius = SqrtThreeOverTwo * innerRadius;

            var sin30BorderInset = 0.5f * normBorderInset;
            var cos30BorderInset = SqrtThreeOverTwo * normBorderInset;

            var one = new Vector3(1f, 0f, 0f);
            var six = new Vector3(sin30TileRadius, 0f, -cos30TileRadius);

            return new Vector3[]
            {
                // center
                Vector3.zero,
                // inner vertices
                new Vector3(sin30InnerRadius, 0f, cos30InnerRadius),
                new Vector3(innerRadius, 0f, 0f),
                new Vector3(sin30InnerRadius, 0f, -cos30InnerRadius),
                new Vector3(-sin30InnerRadius, 0f, -cos30InnerRadius),
                new Vector3(-innerRadius, 0f, 0f),
                new Vector3(-sin30InnerRadius, 0f, cos30InnerRadius),
                // outer vertices 1
                new Vector3(sin30InnerRadius, surfaceHeightDeltas[1], cos30TileRadius),
                new Vector3(
                    sin30TileRadius,
                    (surfaceHeightDeltas[1] + surfaceHeightDeltas[0]) * twoOverThree,
                    cos30TileRadius
                ),
                new Vector3(
                    sin30InnerRadius + cos30BorderInset,
                    surfaceHeightDeltas[0],
                    cos30InnerRadius + sin30BorderInset
                ),
                // outer vertices 2
                new Vector3(
                    innerRadius + cos30BorderInset,
                    surfaceHeightDeltas[0],
                    sin30BorderInset
                ),
                new Vector3(
                    1f,
                    (surfaceHeightDeltas[0] + surfaceHeightDeltas[5]) * twoOverThree,
                    0f
                ),
                new Vector3(
                    innerRadius + cos30BorderInset,
                    surfaceHeightDeltas[5],
                    -sin30BorderInset
                ),
                // outer vertices 3
                new Vector3(
                    sin30InnerRadius + cos30BorderInset,
                    surfaceHeightDeltas[5],
                    -cos30InnerRadius - sin30BorderInset
                ),
                new Vector3(
                    sin30TileRadius,
                    (surfaceHeightDeltas[5] + surfaceHeightDeltas[4]) * twoOverThree,
                    -cos30TileRadius
                ),
                new Vector3(sin30InnerRadius, surfaceHeightDeltas[4], -cos30TileRadius),
                // outer vertices 4
                new Vector3(-sin30InnerRadius, surfaceHeightDeltas[4], -cos30TileRadius),
                new Vector3(
                    -sin30TileRadius,
                    (surfaceHeightDeltas[4] + surfaceHeightDeltas[3]) * twoOverThree,
                    -cos30TileRadius
                ),
                new Vector3(
                    -sin30InnerRadius - cos30BorderInset,
                    surfaceHeightDeltas[3],
                    -cos30InnerRadius - sin30BorderInset
                ),
                // outer vertices 5
                new Vector3(
                    -innerRadius - cos30BorderInset,
                    surfaceHeightDeltas[3],
                    -sin30BorderInset
                ),
                new Vector3(
                    -1f,
                    (surfaceHeightDeltas[3] + surfaceHeightDeltas[2]) * twoOverThree,
                    0f
                ),
                new Vector3(
                    -innerRadius - cos30BorderInset,
                    surfaceHeightDeltas[2],
                    sin30BorderInset
                ),
                // outer vertices 6
                new Vector3(
                    -sin30InnerRadius - cos30BorderInset,
                    surfaceHeightDeltas[2],
                    cos30InnerRadius + sin30BorderInset
                ),
                new Vector3(
                    -sin30TileRadius,
                    (surfaceHeightDeltas[2] + surfaceHeightDeltas[1]) * twoOverThree,
                    cos30TileRadius
                ),
                new Vector3(-sin30InnerRadius, surfaceHeightDeltas[1], cos30TileRadius),
            };
        }

        private Vector2[] MakeTileVertexUVs(float normBorderInset)
        {
            var innerUV = 1f - normBorderInset;

            return new Vector2[]
            {
                // center
                Vector2.zero,
                //inner
                new Vector2(innerUV, 0.5f),
                new Vector2(innerUV, 0.5f),
                new Vector2(innerUV, 0.5f),
                new Vector2(innerUV, 0.5f),
                new Vector2(innerUV, 0.5f),
                new Vector2(innerUV, 0.5f),
                //outer
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f)
            };
        }

        private readonly int[] tileIndices = new int[]
        {
            0,
            1,
            2,
            0,
            2,
            3,
            0,
            3,
            4,
            0,
            4,
            5,
            0,
            5,
            6,
            0,
            6,
            1,
        };

        private readonly int[] borderIndices = new int[]
        {
            // outer 1-2
            1,
            8,
            9,
            1,
            9,
            10,
            2,
            1,
            10,
            2,
            10,
            11,
            // outer 2-3
            2,
            11,
            12,
            2,
            12,
            13,
            3,
            2,
            13,
            3,
            13,
            14,
            // outer 3-4
            3,
            14,
            15,
            3,
            15,
            16,
            4,
            3,
            16,
            4,
            16,
            17,
            // outer 4-5
            4,
            17,
            18,
            4,
            18,
            19,
            5,
            4,
            19,
            5,
            19,
            20,
            // outer 5-6
            5,
            20,
            21,
            5,
            21,
            22,
            6,
            5,
            22,
            6,
            22,
            23,
            // outer 6-1
            6,
            23,
            24,
            6,
            24,
            7,
            1,
            6,
            7,
            1,
            7,
            8,
        };

        private readonly float tileRadius;
        private readonly float borderWidth;
        private readonly Func<
            AxialCoordinateInt,
            TileDataType,
            (Vector3 scale, Vector3 offset, float[] surfaceHeightDeltas)
        > getTileData;

        private Vector2[] tileVertexUV;

        public HexWithInsetBordersMeshGenerator(
            float tileRadius,
            float borderWidth,
            Func<
                AxialCoordinateInt,
                TileDataType,
                (Vector3 scale, Vector3 offset, float[] surfaceHeightDeltas)
            > getTileData
        )
        {
            this.tileRadius = tileRadius;
            this.borderWidth = borderWidth;
            this.getTileData = getTileData;

            tileVertexUV = MakeTileVertexUVs(borderWidth / tileRadius);
        }

        public Mesh GenerateMesh(
            IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)> tiles
        )
        {
            return GenerateMesh(
                new IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)>[] { tiles }
            );
        }

        public Mesh GenerateMesh(
            IEnumerable<IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)>> tileGroups
        )
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var subMeshes = new List<int[]>();

            foreach (var tileGroup in tileGroups)
            {
                var startIndex = vertices.Count();
                var tileTriangleIndices = new List<int>();

                foreach (var tileData in tileGroup)
                {
                    AppendTileMeshData(
                        tileData.coordinate,
                        tileData.data,
                        vertices,
                        uvs,
                        tileTriangleIndices
                    );
                }

                subMeshes.Add((tileTriangleIndices.ToArray()));
            }

            var mesh = new Mesh();
            mesh.indexFormat =
                (vertices.Count < ushort.MaxValue) ? IndexFormat.UInt16 : IndexFormat.UInt32;

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.subMeshCount = subMeshes.Count;

            for (
                var subMeshDescriptorIndex = 0;
                subMeshDescriptorIndex < mesh.subMeshCount;
                subMeshDescriptorIndex++
            )
            {
                mesh.SetIndices(
                    subMeshes[subMeshDescriptorIndex],
                    MeshTopology.Triangles,
                    subMeshDescriptorIndex
                );
            }

            return mesh;
        }

        private void AppendTileMeshData(
            AxialCoordinateInt coordinate,
            TileDataType tileData,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangleIndices
        )
        {
            var tileBaseIndex = vertices.Count;
            var (vertexScale, vertexOffset, surfaceHeightDeltas) = getTileData(
                coordinate,
                tileData
            );

            var tileVertexOffsetsNorm = MakeNormTileVertices(
                borderWidth / tileRadius,
                surfaceHeightDeltas
            );
            var tileVertices = tileVertexOffsetsNorm.Select(
                x => Vector3.Scale(x, vertexScale) + vertexOffset
            );

            vertices.AddRange(tileVertices);
            uvs.AddRange(MakeTileVertexUVs(borderWidth / tileRadius));

            triangleIndices.AddRange(
                tileIndices.Concat(borderIndices).Select(x => tileBaseIndex + x)
            );
        }
    }
}
