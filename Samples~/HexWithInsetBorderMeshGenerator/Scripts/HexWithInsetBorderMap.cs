using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Random = UnityEngine.Random;

namespace de.JochenHeckl.Unity.HexMap.Samples
{
    [RequireComponent(typeof(MeshFilter))]
    public class HexWithInsetBorderMap : MonoBehaviour
    {
        public int meshOrder = 3;
        public float tileRadius = 10f;
        public int borderWidth = 2;
        public int blockHeight = 5;
        public int heightVariation = 3;

        private MeshFilter meshFiler;
        private readonly TileDataStorageAxialCoordinateDictionary<HexWithInsetBorderMeshMapCell> rawTileStorage =
            new();

        void Awake()
        {
            meshFiler = GetComponent<MeshFilter>();

            rawTileStorage.CreateHexagonalMap(meshOrder, MakeTile);

            var generator = new HexWithInsetBordersMeshGenerator<HexWithInsetBorderMeshMapCell>(
                tileRadius: tileRadius,
                borderWidth: borderWidth,
                getTileData: GetTileData
            );

            meshFiler.sharedMesh = generator.GenerateMesh(rawTileStorage.Tiles);
        }

        private HexWithInsetBorderMeshMapCell MakeTile(AxialCoordinateInt coordinate)
        {
            var surfaceHeight = Random.Range(
                blockHeight - heightVariation,
                blockHeight + heightVariation
            );

            return new HexWithInsetBorderMeshMapCell(coordinate, surfaceHeight);
        }

        private (Vector3 scale, Vector3 offset, float[] neighbourSurfaceHeights) GetTileData(
            AxialCoordinateInt coordinate,
            HexWithInsetBorderMeshMapCell tileData
        )
        {
            var baseVertex2D = coordinate.ToCartesian(tileRadius);

            return (
                new Vector3(tileRadius, 1f, tileRadius),
                new Vector3(baseVertex2D.x, tileData.SurfaceHeight, baseVertex2D.y),
                AxialCoordinateInt
                    .Neighbours(coordinate)
                    .Select(
                        x =>
                        {
                            var minHeight = blockHeight - heightVariation;
                            var referenceHeight = rawTileStorage.TileExists(x)
                              ? rawTileStorage[x].SurfaceHeight
                              : minHeight;

                            return .5f * (referenceHeight - tileData.SurfaceHeight);
                        }
                    )
                    .ToArray()
            );
        }
    }
}
