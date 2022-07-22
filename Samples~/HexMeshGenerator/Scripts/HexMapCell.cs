using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace de.JochenHeckl.Unity.HexMap.Samples
{
    internal class HexMapCell : ITileData
    {
        public AxialCoordinateInt Coordinate { get; private set; }

        public HexMapCell(AxialCoordinateInt coordinate)
        {
            Coordinate = coordinate;
        }
    }
}
