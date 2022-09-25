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
    public abstract class HexMeshGeneratorBase<TileDataType> : IHexMeshGenerator<TileDataType>
        where TileDataType : ITileData
    {
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
            PrepareGeneration();

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

            return FinalizeGeneration(mesh);
        }

        protected virtual void PrepareGeneration() { }

        protected virtual Mesh FinalizeGeneration(Mesh mesh) => mesh;

        protected abstract void AppendTileMeshData(
            AxialCoordinateInt coordinate,
            TileDataType tileData,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangleIndices
        );
    }
}
