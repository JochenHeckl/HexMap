using System.Collections.Generic;

using UnityEngine;

namespace de.JochenHeckl.Unity.HexMap
{
    public interface IHexMeshGenerator<TileDataType>
    {
        Mesh GenerateMesh(IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)> tiles);
        Mesh GenerateMesh(
            IEnumerable<IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)>> tileGroups
        );
    }
}
