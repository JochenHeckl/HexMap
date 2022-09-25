using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace de.JochenHeckl.Unity.HexMap.Samples
{
    [RequireComponent(typeof(MeshFilter))]
    public class HexBlockMap : MonoBehaviour
    {
        public int meshOrder = 3;
        public float tileRadius = 1f;
        public int blockHeight = 5;
        public int heightVariation = 2;

        private MeshFilter meshFiler;
        private readonly TileDataStorageAxialCoordinateDictionary<HexBlockMapCell> rawTileStorage =
            new();

        void Awake()
        {
            meshFiler = GetComponent<MeshFilter>();

            rawTileStorage.CreateHexagonalMap(meshOrder);

            var generator = new HexBlockMeshGenerator<HexBlockMapCell>(
                tileRadius: tileRadius,
                getTileData: GetTileData
            );

            meshFiler.sharedMesh = generator.GenerateMesh(rawTileStorage.Tiles);
        }

        public (Vector3 scale, Vector3 offset) GetTileData(
            AxialCoordinateInt coordinate,
            HexBlockMapCell tileData
        )
        {
            var baseVertex2D = coordinate.ToCartesian(tileRadius);
            var Height = Random.Range(blockHeight - heightVariation, blockHeight + heightVariation);

            var blockdata = new HexBlockMeshGenerator<HexBlockMapCell>.BlockData()
            {
                Height = Height,
                VerticalOffset = Height
            };

            return (
                new Vector3(tileRadius, blockdata.Height, tileRadius),
                new Vector3(baseVertex2D.x, blockdata.VerticalOffset, baseVertex2D.y)
            );
        }
    }
}
