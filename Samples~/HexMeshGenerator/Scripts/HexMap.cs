using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace de.JochenHeckl.Unity.HexMap.Samples
{
    [RequireComponent(typeof(MeshFilter))]
    public class HexMap : MonoBehaviour
    {
        public int meshOrder = 3;
        public float tileRadius = 42f;

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
            using var timer = new ScopeTimer(
                (x) => Debug.Log($"Generating the mesh took: {x.TotalMilliseconds, 0:F3} ms.")
            );

            rawTileStorage.CreateHexagonalMap(meshOrder);

            var generator = new HexMeshGenerator<HexMapCell>(
                tileRadius: tileRadius,
                getTileData: GetTileData
            );

            meshFiler.sharedMesh = generator.GenerateMesh(rawTileStorage.Tiles);
        }

        private (Vector3 scale, Vector3 offset) GetTileData(
            AxialCoordinateInt coordinate,
            HexMapCell tileData
        )
        {
            var baseVertex2D = coordinate.ToCartesian(tileRadius);

            return (
                new Vector3(tileRadius, 1f, tileRadius),
                new Vector3(baseVertex2D.x, 0.0f, baseVertex2D.y)
            );
        }
    }
}
