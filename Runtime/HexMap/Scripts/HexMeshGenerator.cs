/// Thanks Amit! https://www.redblobgames.com/grids/hexagons

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace de.JochenHeckl.Unity.HexMap
{
    public class HexMeshGenerator<TileDataType> : IHexMeshGenerator<TileDataType>
        where TileDataType : ITileData
    {
        private readonly Vector3[] tileVertexOffsetsNorm = new Vector3[]
        {
            Vector3.zero,
            new Vector3(1f, 0f, 0f),
            new Vector3(.5f, 0f, Constants.SqrtThreeOverTwo),
            new Vector3(-.5f, 0f, Constants.SqrtThreeOverTwo),
            new Vector3(-1f, 0f, 0f),
            new Vector3(-.5f, 0f, -Constants.SqrtThreeOverTwo),
            new Vector3(.5f, 0f, -Constants.SqrtThreeOverTwo)
        };

        private readonly int[] tileIndices = new int[]
        {
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
            6
        };

        private float hexRadius;
        private Func<Vector2, float> getTileHeight;

        public HexMeshGenerator(float hexRadius, Func<Vector2, float> getTileHeight)
        {
            this.hexRadius = hexRadius;
            this.getTileHeight = getTileHeight;
        }

        public Mesh GenerateMesh(ITileDataStorage<TileDataType> dataSourceIn)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();

            var generatedTiles = new Dictionary<AxialCoordinateInt, int[]>();

            foreach (var tile in dataSourceIn.Tiles.Select(x => x.coordinate))
            {
                MakeTileMeshData(hexRadius, tile, vertices, uvs, generatedTiles);
            }

            var mesh = new Mesh();
            mesh.indexFormat =
                (vertices.Count < ushort.MaxValue) ? IndexFormat.UInt16 : IndexFormat.UInt32;

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.subMeshCount = 1;
            mesh.triangles = generatedTiles
                .SelectMany(t => tileIndices.Select(x => t.Value[x]))
                .ToArray();

            return mesh;
        }

        private void MakeTileMeshData(
            float hexRadius,
            AxialCoordinateInt tileCoordinate,
            List<Vector3> vertices,
            List<Vector2> uvs,
            Dictionary<AxialCoordinateInt, int[]> tileTriangleIndices
        )
        {
            var baseVertex = GetTilePosition(tileCoordinate, hexRadius);

            tileTriangleIndices[tileCoordinate] = Enumerable.Range(vertices.Count, 7).ToArray();

            vertices.Add(baseVertex);
            uvs.Add(new Vector2(0, 1));

            var missingOuterVertices = new bool[6] { true, true, true, true, true, true };

            // test lower right
            if (
                tileTriangleIndices.TryGetValue(
                    tileCoordinate + (1, 0),
                    out var lowerRightTileIndices
                )
            )
            {
                tileTriangleIndices[tileCoordinate][1] = lowerRightTileIndices[5];
                missingOuterVertices[0] = false;

                tileTriangleIndices[tileCoordinate][2] = lowerRightTileIndices[4];
                missingOuterVertices[1] = false;
            }

            if (tileTriangleIndices.TryGetValue(tileCoordinate + (0, 1), out var lowerTileIndices))
            {
                tileTriangleIndices[tileCoordinate][2] = lowerTileIndices[6];
                missingOuterVertices[1] = false;

                tileTriangleIndices[tileCoordinate][3] = lowerTileIndices[5];
                missingOuterVertices[2] = false;
            }

            if (
                tileTriangleIndices.TryGetValue(
                    tileCoordinate + (-1, 1),
                    out var lowerLeftTileIndices
                )
            )
            {
                tileTriangleIndices[tileCoordinate][3] = lowerLeftTileIndices[1];
                missingOuterVertices[2] = false;

                tileTriangleIndices[tileCoordinate][4] = lowerLeftTileIndices[6];
                missingOuterVertices[3] = false;
            }

            if (
                tileTriangleIndices.TryGetValue(
                    tileCoordinate + (-1, 0),
                    out var upperLeftTileIndices
                )
            )
            {
                tileTriangleIndices[tileCoordinate][4] = upperLeftTileIndices[2];
                missingOuterVertices[3] = false;

                tileTriangleIndices[tileCoordinate][5] = upperLeftTileIndices[1];
                missingOuterVertices[4] = false;
            }

            if (tileTriangleIndices.TryGetValue(tileCoordinate + (0, -1), out var upperTileIndices))
            {
                tileTriangleIndices[tileCoordinate][5] = upperTileIndices[3];
                missingOuterVertices[4] = false;

                tileTriangleIndices[tileCoordinate][6] = upperTileIndices[2];
                missingOuterVertices[5] = false;
            }

            if (
                tileTriangleIndices.TryGetValue(
                    tileCoordinate + (1, -1),
                    out var upperRightTileIndices
                )
            )
            {
                tileTriangleIndices[tileCoordinate][6] = upperRightTileIndices[4];
                missingOuterVertices[5] = false;

                tileTriangleIndices[tileCoordinate][1] = upperRightTileIndices[3];
                missingOuterVertices[0] = false;
            }

            for (
                var missingOuterVertexIdx = 0;
                missingOuterVertexIdx < missingOuterVertices.Length;
                missingOuterVertexIdx++
            )
            {
                if (missingOuterVertices[missingOuterVertexIdx])
                {
                    tileTriangleIndices[tileCoordinate][missingOuterVertexIdx + 1] = vertices.Count;

                    var vertexOffset = hexRadius * tileVertexOffsetsNorm[missingOuterVertexIdx + 1];

                    vertices.Add(
                        baseVertex
                            + new Vector3(
                                vertexOffset.x,
                                getTileHeight(new Vector2(vertexOffset.x, vertexOffset.z)),
                                vertexOffset.z
                            )
                    );
                    uvs.Add(new Vector2(1, 0));
                }
            }
        }

        public Vector3 GetTilePosition(AxialCoordinateInt tileCoordinate, float hexRadius)
        {
            var baseVertex2D = tileCoordinate.ToCartesian(hexRadius);
            return new Vector3(baseVertex2D.x, getTileHeight(baseVertex2D), baseVertex2D.y);
        }
    }
}
