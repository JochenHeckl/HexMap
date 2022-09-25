using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace de.JochenHeckl.Unity.HexMap.Samples
{
    [RequireComponent(typeof(MeshFilter))]
    public class HexMap : MonoBehaviour
    {
        public int meshOrder = 3;
        public float hexRadius = 42f;

        private MeshFilter meshFiler;
        private readonly TileDataStorageAxialCoordinateDictionary<HexMapCell> rawTileStorage =
            new();

        void Awake()
        {
            meshFiler = GetComponent<MeshFilter>();
            GenerateMesh();
        }

        public void GenerateMesh()
        {
            rawTileStorage.CreateHexagonalMap(meshOrder);

            var generator = new HexMeshGenerator<HexMapCell>(
                hexRadius: hexRadius,
                HexMeshGenerator<HexMapCell>.FlatTiles
            );

            meshFiler.sharedMesh = generator.GenerateMesh(rawTileStorage.Tiles);
        }
    }
}
