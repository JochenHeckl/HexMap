/// Thanks Amit! https://www.redblobgames.com/grids/hexagons

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace de.JochenHeckl.Unity.HexMap
{
    /// <summary>
    /// Generates a simple hexagonal mesh
    /// </summary>
    /// <typeparam name="TileDataType"></typeparam>
    public class HexMeshGenerator<TileDataType> : HexMeshGeneratorBase<TileDataType>
        where TileDataType : ITileData
    {
        Dictionary<AxialCoordinateInt, int[]> tileVertexIndices = new();

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

        private readonly Vector2[] tileUvs = new Vector2[]
        {
            Vector2.zero,
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f)
        };

        private float tileRadius;
        private Func<AxialCoordinateInt, TileDataType, (Vector3 scale, Vector3 offset)> getTileData;

        public HexMeshGenerator(
            float tileRadius,
            Func<AxialCoordinateInt, TileDataType, (Vector3 scale, Vector3 offset)> getTileData
        )
        {
            this.tileRadius = tileRadius;
            this.getTileData = getTileData;
        }

        protected override void PrepareGeneration()
        {
            tileVertexIndices = new();
        }

        protected override Mesh FinalizeGeneration(Mesh mesh)
        {
            return mesh;
        }

        protected override void AppendTileMeshData(
            AxialCoordinateInt coordinate,
            TileDataType tileData,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangleIndices
        )
        {
            var (scale, offset) = getTileData(coordinate, tileData);
            var neighbourCoordinates = AxialCoordinateInt.Neighbours(coordinate);

            var currentTileVertexIndices = new int[7];

            // add center vertex
            currentTileVertexIndices[0] = vertices.Count();
            vertices.Add(Vector3.Scale(tileVertexOffsetsNorm[0], scale) + offset);
            uvs.Add(tileUvs[0]);

            for (var outerVertex = 1; outerVertex < 7; outerVertex++)
            {
                var firstNeighbour = neighbourCoordinates[(outerVertex + 4) % 6];

                int[] sharedIndices;

                if (tileVertexIndices.TryGetValue(firstNeighbour, out sharedIndices))
                {
                    var matchingVertexIndex = 1 + ((outerVertex + 1) % 6);
                    currentTileVertexIndices[outerVertex] = sharedIndices[matchingVertexIndex];

                    var existingVertexPos = vertices[sharedIndices[matchingVertexIndex]];
                    var curVertexPos =
                        Vector3.Scale(tileVertexOffsetsNorm[outerVertex], scale) + offset;

                    if (Vector3.Distance(existingVertexPos, curVertexPos) > 1f)
                    {
                        throw new InvalidProgramException();
                    }

                    continue;
                }

                var secondNeighbour = neighbourCoordinates[outerVertex - 1];

                if (tileVertexIndices.TryGetValue(secondNeighbour, out sharedIndices))
                {
                    var matchingVertexIndex = 1 + ((outerVertex + 3) % 6);
                    currentTileVertexIndices[outerVertex] = sharedIndices[matchingVertexIndex];

                    var existingVertexPos = vertices[sharedIndices[matchingVertexIndex]];
                    var curVertexPos =
                        Vector3.Scale(tileVertexOffsetsNorm[outerVertex], scale) + offset;

                    if (Vector3.Distance(existingVertexPos, curVertexPos) > 1f)
                    {
                        throw new InvalidProgramException();
                    }

                    continue;
                }

                currentTileVertexIndices[outerVertex] = vertices.Count();
                vertices.Add(Vector3.Scale(tileVertexOffsetsNorm[outerVertex], scale) + offset);
                uvs.Add(tileUvs[outerVertex]);
            }

            triangleIndices.AddRange(tileIndices.Select(x => currentTileVertexIndices[x]));
            tileVertexIndices[coordinate] = currentTileVertexIndices;
        }
    }
}
