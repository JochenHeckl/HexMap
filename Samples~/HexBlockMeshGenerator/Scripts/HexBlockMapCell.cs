using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace de.JochenHeckl.Unity.HexMap.Samples
{
    public class HexBlockMapCell : ITileData
    {
        public AxialCoordinateInt Coordinate { get; private set; }
        public float VerticalOffset { get; private set; }
        public float Height { get; private set; }

        public HexBlockMapCell(AxialCoordinateInt coordinate, float verticalOffset, float height)
        {
            Coordinate = coordinate;
            VerticalOffset = verticalOffset;
            Height = height;
        }
    }
}
