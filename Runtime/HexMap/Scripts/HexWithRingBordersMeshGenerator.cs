/// Thanks Amit! https://www.redblobgames.com/grids/hexagons

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace de.JochenHeckl.Unity.HexMap
{
    public class HexWithRingBordersMeshGenerator<TileDataType> : IHexMeshGenerator<TileDataType>
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

        private readonly Vector3[] borderVertexOffsetsNorm = new Vector3[]
        {
            new Vector3(1f, 0f, 0f),
            new Vector3(.5f, 0f, Constants.SqrtThreeOverTwo),
            new Vector3(-.5f, 0f, Constants.SqrtThreeOverTwo),
            new Vector3(-1f, 0f, 0f),
            new Vector3(-.5f, 0f, -Constants.SqrtThreeOverTwo),
            new Vector3(.5f, 0f, -Constants.SqrtThreeOverTwo),
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

        private readonly Vector2[] borderVertexUV = new Vector2[]
        {
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
            1,
            6,
            0,
            6,
            1,
            7,
            7,
            1,
            2,
            7,
            2,
            8,
            8,
            2,
            3,
            8,
            3,
            9,
            9,
            3,
            4,
            9,
            4,
            10,
            10,
            4,
            5,
            10,
            5,
            11,
            11,
            5,
            0,
            11,
            0,
            6
        };

        private readonly float hexRadius;
        private readonly float borderWidth;
        private readonly Func<AxialCoordinateInt, float> getTileHeightLevel;

        public HexWithRingBordersMeshGenerator(
            float hexRadiusIn,
            float borderWidthIn,
            Func<AxialCoordinateInt, float> getTileHeightLevelIn
        )
        {
            hexRadius = hexRadiusIn;
            borderWidth = borderWidthIn;
            getTileHeightLevel = getTileHeightLevelIn;
        }

        public Mesh GenerateMesh(
            IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)> tiles
        )
        {
            var vertexCount =
                tiles.Count() * (tileVertexOffsetsNorm.Length + borderVertexOffsetsNorm.Length);
            var indexFormat =
                (vertexCount < ushort.MaxValue) ? IndexFormat.UInt16 : IndexFormat.UInt32;

            var tileMeshes = tiles.Select(
                x => MakeTileCombineInstance(x.coordinate, hexRadius, borderWidth, indexFormat)
            );
            var borderMeshes = tiles.Select(
                x => MakeBorderCombineInstance(x.coordinate, hexRadius, borderWidth, indexFormat)
            );

            var subMeshes = tileMeshes
                .Concat(borderMeshes)
                .GroupBy(x => x.subMeshIndex)
                .Select(x => MakeSubMesh(x, indexFormat));

            var mesh = new Mesh();

            mesh.CombineMeshes(subMeshes.ToArray(), false, false);
            mesh.Optimize();

            return mesh;
        }

        public Mesh GenerateMesh(
            IEnumerable<IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)>> tileGroups
        )
        {
            throw new NotImplementedException();
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
            var borderMesh = new Mesh();
            borderMesh.indexFormat = indexFormat;

            borderMesh.subMeshCount = GetBorderSubMeshIndex(tile) + 1;

            var baseVertex = GetTilePosition(tile, hexRadius);
            var neighbours = AxialCoordinateInt
                .Neighbours(tile)
                .Select(x => GetTilePosition(x, hexRadius))
                .ToArray();

            var insetRadius = hexRadius - (0.5f * borderWidth);

            var innerBorderVertices = borderVertexOffsetsNorm
                .Select(v => (v * insetRadius) + baseVertex)
                .ToArray();

            var outerbordervertices = new Vector3[]
            {
                (neighbours[1] + neighbours[2] + baseVertex) / 3f,
                (neighbours[2] + neighbours[3] + baseVertex) / 3f,
                (neighbours[3] + neighbours[4] + baseVertex) / 3f,
                (neighbours[4] + neighbours[5] + baseVertex) / 3f,
                (neighbours[5] + neighbours[0] + baseVertex) / 3f,
                (neighbours[0] + neighbours[1] + baseVertex) / 3f
            };

            borderMesh.vertices = innerBorderVertices.Concat(outerbordervertices).ToArray();

            borderMesh.uv = borderVertexUV
                .Select(x => x * (insetRadius / hexRadius))
                .Concat(borderVertexUV)
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
