using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace de.JochenHeckl.Unity.HexMap
{
    public class TileDataStorageAxialCoordinateDictionary<TileDataType>
        : ITileDataStorage<TileDataType> where TileDataType : ITileData
    {
        private Dictionary<AxialCoordinateInt, TileDataType> storage = new();

        public IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)> Tiles =>
            storage.Select(x => (x.Key, x.Value));

        public int TileCount => storage.Count;

        public TileDataType this[AxialCoordinateInt coordinate]
        {
            get => storage[coordinate];
            set => storage[coordinate] = value;
        }

        public TileDataType GetOrCreateTile(AxialCoordinateInt coordinate)
        {
            return GetOrCreateTile(coordinate, _ => Activator.CreateInstance<TileDataType>());
        }

        public TileDataType GetOrCreateTile(
            AxialCoordinateInt coordinate,
            Func<AxialCoordinateInt, TileDataType> creator
        )
        {
            TileDataType tile;

            if (storage.TryGetValue(coordinate, out tile))
            {
                return tile;
            }

            var newTile = creator(coordinate);
            storage[coordinate] = newTile;

            return newTile;
        }

        public IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)> GetConnected(
            AxialCoordinateInt origin
        )
        {
            var connected = new List<(AxialCoordinateInt coordinate, TileDataType data)>();

            var closedTiles = new HashSet<AxialCoordinateInt>() { origin };
            var openTiles = new AxialCoordinateInt[] { origin };

            while (openTiles.Length > 0)
            {
                var newOpenTiles = openTiles
                    .SelectMany(x => AxialCoordinateInt.Neighbours(x))
                    .Distinct()
                    .Where(x => storage.ContainsKey(x) && !closedTiles.Contains(x))
                    .ToArray();

                closedTiles.UnionWith(openTiles);
                openTiles = newOpenTiles;
            }

            return closedTiles.Distinct().Select(x => (x, storage[x])).ToArray();
        }

        public void RemoveTile(AxialCoordinateInt coordinate)
        {
            storage.Remove(coordinate);
        }

        public void SetTile(AxialCoordinateInt coordinate, TileDataType data)
        {
            storage[coordinate] = data;
        }

        public void SetTiles(IEnumerable<(AxialCoordinateInt coordinate, TileDataType data)> tiles)
        {
            storage = tiles.ToDictionary(x => x.coordinate, x => x.data);
        }

        public bool TileExists(AxialCoordinateInt coordinate)
        {
            return storage.Keys.Contains(coordinate);
        }

        public void CreateHexagonalMap(int order)
        {
            CreateHexagonalMap(order, _ => default);
        }

        public void CreateHexagonalMap(int order, Func<AxialCoordinateInt, TileDataType> creator)
        {
            foreach (
                var coordinate in AxialCoordinateInt.TilesInRange(
                    new AxialCoordinateInt(0, 0),
                    order
                )
            )
            {
                storage[coordinate] = creator(coordinate);
            }
        }

        public void ConditionalCreateHexagonalMap(
            int order,
            Func<AxialCoordinateInt, TileDataType> creator,
            Func<AxialCoordinateInt, bool> condition
        )
        {
            foreach (
                var coordinate in AxialCoordinateInt
                    .TilesInRange(new AxialCoordinateInt(0, 0), order)
                    .Where(x => condition(x))
            )
            {
                storage[coordinate] = creator(coordinate);
            }
        }

        public (bool found, TileDataType tile) TryGetTile(AxialCoordinateInt coordinate)
        {
            TileDataType tile;

            if (storage.TryGetValue(coordinate, out tile))
            {
                return (true, tile);
            }

            return (false, default(TileDataType));
        }
    }
}
