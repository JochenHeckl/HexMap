using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace de.JochenHeckl.Unity.HexMap.Samples
{
    public class HexWithInsetBorderMeshMapCell : ITileData
    {
        public AxialCoordinateInt Coordinate { get; private set; }
        public float SurfaceHeight { get; private set; }

        public HexWithInsetBorderMeshMapCell(AxialCoordinateInt coordinate, float surfaceHeight)
        {
            Coordinate = coordinate;
            SurfaceHeight = surfaceHeight;
        }
    }
}
