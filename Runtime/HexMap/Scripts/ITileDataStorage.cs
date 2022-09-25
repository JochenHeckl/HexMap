using System;
using System.Collections.Generic;

namespace de.JochenHeckl.Unity.HexMap
{
    public interface ITileDataStorage<TileData>
    {
        TileData this[AxialCoordinateInt coordinate] { get; }

        int TileCount { get; }

        IEnumerable<(AxialCoordinateInt coordinate, TileData data)> Tiles { get; }

        void CreateHexagonalMap(int order);
        void CreateHexagonalMap(int order, Func<AxialCoordinateInt, TileData> creator);

        TileData GetOrCreateTile(AxialCoordinateInt coordinate);
        void RemoveTile(AxialCoordinateInt coordinate);
        void SetTile(AxialCoordinateInt coordinate, TileData data);
        void SetTiles( IEnumerable<(AxialCoordinateInt coordinate, TileData data)> tiles );
        IEnumerable<(AxialCoordinateInt coordinate, TileData data)> GetConnected( AxialCoordinateInt origin );

        ( bool found, TileData tile) TryGetTile(AxialCoordinateInt coordinate);
        bool TileExists(AxialCoordinateInt coordinate);
    }
}
