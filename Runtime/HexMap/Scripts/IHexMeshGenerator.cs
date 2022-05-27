using UnityEngine;

namespace de.JochenHeckl.Unity.HexMap
{
    public interface IHexMeshGenerator<TileDataType>
    {
        Mesh GenerateMesh( ITileDataStorage<TileDataType> dataSourceIn );
    }
}