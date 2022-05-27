/// Thanks Amit! https://www.redblobgames.com/grids/hexagons

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace de.JochenHeckl.Unity.HexMap
{
    public class HexWithFlatCornerBordersMeshGenerator<TileDataType>
        : IHexMeshGenerator<TileDataType> where TileDataType : ITileData
    {
        // counter clock wise in the x/z plane
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

        private readonly Vector2[] tileVertexUV = new Vector2[]
        {
            Vector3.zero,
            new Vector3(1f, 0f),
            new Vector3(1f, 0f),
            new Vector3(1f, 0f),
            new Vector3(1f, 0f),
            new Vector3(1f, 0f),
            new Vector3(1f, 0f)
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

        private readonly int[] borderIndices = new int[]
        {
            7,
            6,
            0,
            7,
            0,
            1,
            7,
            1,
            8,
            9,
            8,
            1,
            9,
            1,
            2,
            9,
            2,
            10,
            11,
            10,
            2,
            11,
            2,
            3,
            11,
            3,
            12,
            13,
            12,
            3,
            13,
            3,
            4,
            13,
            4,
            14,
            15,
            14,
            4,
            15,
            4,
            5,
            15,
            5,
            16,
            17,
            16,
            5,
            17,
            5,
            0,
            17,
            0,
            6
        };

        private readonly float hexRadius;
        private readonly float borderWidth;
        private readonly Func<AxialCoordinateInt, float> getTileHeightLevel;

        public HexWithFlatCornerBordersMeshGenerator(
            float hexRadiusIn,
            float borderWidthIn,
            Func<AxialCoordinateInt, float> getTileHeightLevelIn
        )
        {
            hexRadius = hexRadiusIn;
            borderWidth = borderWidthIn;
            getTileHeightLevel = getTileHeightLevelIn;
        }

        public Mesh GenerateMesh(ITileDataStorage<TileDataType> dataSourceIn)
        {
            var vertexCount = dataSourceIn.TileCount * 18;
            var indexFormat =
                (vertexCount < ushort.MaxValue) ? IndexFormat.UInt16 : IndexFormat.UInt32;

            var tileMeshes = dataSourceIn.Tiles.Select(
                x => MakeTileCombineInstance(x.coordinate, hexRadius, borderWidth, indexFormat)
            );
            var borderMeshes = dataSourceIn.Tiles.Select(
                x => MakeBorderCombineInstance(x.coordinate, hexRadius, borderWidth, indexFormat)
            );

            var subMeshes = tileMeshes
                .Concat(borderMeshes)
                .GroupBy(x => x.subMeshIndex)
                .Select(x => MakeSubMesh(x, indexFormat));

            var mesh = new Mesh();

            mesh.CombineMeshes(subMeshes.ToArray(), false, false);
            mesh.OptimizeIndexBuffers();
            mesh.Optimize();

            return mesh;
        }

        private CombineInstance MakeTileCombineInstance(
            AxialCoordinateInt tile,
            float hexRadius,
            float borderWidth,
            IndexFormat indexFormat
        )
        {
            var tileMesh = new Mesh();
            tileMesh.indexFormat = indexFormat;

            tileMesh.subMeshCount = GetTileSubMeshIndex(tile) + 1;

            var baseVertex = GetTilePosition(tile, hexRadius);

            var insetRadius = hexRadius - (0.5f * borderWidth);

            tileMesh.vertices = tileVertexOffsetsNorm
                .Select(v => (v * insetRadius) + baseVertex)
                .ToArray();
            tileMesh.uv = tileVertexUV.Select(x => x * (insetRadius / hexRadius)).ToArray();
            tileMesh.SetTriangles(tileIndices, GetTileSubMeshIndex(tile));
            tileMesh.RecalculateNormals();

            return new CombineInstance()
            {
                mesh = tileMesh,
                subMeshIndex = GetTileSubMeshIndex(tile)
            };
        }

        private CombineInstance MakeBorderCombineInstance(
            AxialCoordinateInt tile,
            float hexRadius,
            float borderWidth,
            IndexFormat indexFormat
        )
        {
            var borderVertexDirectionShift01 = (
                tileVertexOffsetsNorm[1] + tileVertexOffsetsNorm[2]
            ).normalized;
            var borderVertexDirectionShift12 = (
                tileVertexOffsetsNorm[2] + tileVertexOffsetsNorm[3]
            ).normalized;
            var borderVertexDirectionShift23 = (
                tileVertexOffsetsNorm[3] + tileVertexOffsetsNorm[4]
            ).normalized;
            var borderVertexDirectionShift34 = (
                tileVertexOffsetsNorm[4] + tileVertexOffsetsNorm[5]
            ).normalized;
            var borderVertexDirectionShift45 = (
                tileVertexOffsetsNorm[5] + tileVertexOffsetsNorm[6]
            ).normalized;
            var borderVertexDirectionShift50 = (
                tileVertexOffsetsNorm[6] + tileVertexOffsetsNorm[1]
            ).normalized;

            var borderMesh = new Mesh();
            borderMesh.indexFormat = indexFormat;
            borderMesh.subMeshCount = GetBorderSubMeshIndex(tile) + 1;

            var neighbours = AxialCoordinateInt
                .Neighbours(tile)
                .Select(x => GetTilePosition(x, hexRadius))
                .ToArray();
            var baseVertex = GetTilePosition(tile, hexRadius);
            var insetRadius = hexRadius - (0.5f * borderWidth);

            var innerBorderVertices = tileVertexOffsetsNorm
                .Skip(1)
                .Select(x => baseVertex + (x * insetRadius))
                .ToArray();

            var outerBorderVertices = new Vector3[]
            {
                innerBorderVertices[0]
                    + borderVertexDirectionShift01 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[1]
                    + borderVertexDirectionShift01 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[1]
                    + borderVertexDirectionShift12 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[2]
                    + borderVertexDirectionShift12 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[2]
                    + borderVertexDirectionShift23 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[3]
                    + borderVertexDirectionShift23 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[3]
                    + borderVertexDirectionShift34 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[4]
                    + borderVertexDirectionShift34 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[4]
                    + borderVertexDirectionShift45 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[5]
                    + borderVertexDirectionShift45 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[5]
                    + borderVertexDirectionShift50 * borderWidth * Constants.SqrtThreeOverTwo,
                innerBorderVertices[0]
                    + borderVertexDirectionShift50 * borderWidth * Constants.SqrtThreeOverTwo
            };

            outerBorderVertices[0].y = neighbours[2].y;
            outerBorderVertices[1].y = neighbours[2].y;
            outerBorderVertices[2].y = neighbours[3].y;
            outerBorderVertices[3].y = neighbours[3].y;
            outerBorderVertices[4].y = neighbours[4].y;
            outerBorderVertices[5].y = neighbours[4].y;
            outerBorderVertices[6].y = neighbours[5].y;
            outerBorderVertices[7].y = neighbours[5].y;
            outerBorderVertices[8].y = neighbours[0].y;
            outerBorderVertices[9].y = neighbours[0].y;
            outerBorderVertices[10].y = neighbours[1].y;
            outerBorderVertices[11].y = neighbours[1].y;

            borderMesh.vertices = innerBorderVertices.Concat(outerBorderVertices).ToArray();

            borderMesh.uv = Enumerable
                .Repeat(new Vector2(0, 0), 6)
                .Concat(Enumerable.Repeat(new Vector2(0, 1), 12))
                .ToArray();

            borderMesh.SetTriangles(borderIndices, GetBorderSubMeshIndex(tile));

            borderMesh.RecalculateNormals();

            return new CombineInstance()
            {
                mesh = borderMesh,
                subMeshIndex = GetBorderSubMeshIndex(tile)
            };
        }

        private CombineInstance MakeSubMesh(
            IGrouping<int, CombineInstance> meshGroup,
            IndexFormat indexFormat
        )
        {
            var groupMesh = new Mesh();
            groupMesh.indexFormat = indexFormat;

            groupMesh.CombineMeshes(meshGroup.Select(x => x).ToArray(), true, false, false);

            return new CombineInstance() { mesh = groupMesh, subMeshIndex = 0 };
        }

        public Vector3 GetTilePosition(AxialCoordinateInt tileCoordinate, float hexRadius)
        {
            var baseVertex2D = tileCoordinate.ToCartesian(hexRadius);

            var heightLevel =
                (getTileHeightLevel != null) ? getTileHeightLevel(tileCoordinate) : 0f;
            return new Vector3(baseVertex2D.x, heightLevel, baseVertex2D.y);
        }

        private int GetTileSubMeshIndex(AxialCoordinateInt tile)
        {
            return 0;
        }

        private int GetBorderSubMeshIndex(AxialCoordinateInt tile)
        {
            return 1;
        }
    }
}
