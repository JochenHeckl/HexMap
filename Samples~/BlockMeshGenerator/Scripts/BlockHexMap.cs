using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace de.JochenHeckl.Unity.HexMap.Samples
{
    [RequireComponent(typeof(MeshFilter))]
    public class BlockHexMap : MonoBehaviour
    {
        public int meshOrder = 3;
        public float tileRadius = 42f;
        public float blockHeight = 10f;
        public float heightVariation = 20f;

        private MeshFilter meshFiler;
        private readonly TileDataStorageAxialCoordinateDictionary<BlockHexMapCell> rawTileStorage =
            new();

        void Awake()
        {
            meshFiler = GetComponent<MeshFilter>();

            rawTileStorage.CreateHexagonalMap(meshOrder);

            var generator = new BlockHexMeshGenerator<BlockHexMapCell>(
                tileRadius: tileRadius,
                getBlockData: GetBlockData
            );

            meshFiler.sharedMesh = generator.GenerateMesh(rawTileStorage);
        }

        public (Vector3 scale, Vector3 offset) GetBlockData(
            AxialCoordinateInt coordinate,
            BlockHexMapCell tileData
        )
        {
            var baseVertex2D = coordinate.ToCartesian(tileRadius);
            var blockdata = new BlockHexMeshGenerator<BlockHexMapCell>.BlockData()
            {
                Height = blockHeight,
                VerticalOffset = Random.Range(0f, heightVariation)
            };

            return (
                new Vector3(tileRadius, blockdata.Height, tileRadius),
                new Vector3(baseVertex2D.x, blockdata.VerticalOffset, baseVertex2D.y)
            );
        }
    }
}
