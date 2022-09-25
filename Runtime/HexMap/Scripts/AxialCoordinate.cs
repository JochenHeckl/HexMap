/// Thanks Amit! https://www.redblobgames.com/grids/hexagons
///
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace de.JochenHeckl.Unity.HexMap
{
    [DebuggerDisplay("({q}, {r})")]
    public struct AxialCoordinateInt
    {
        public static AxialCoordinateInt Origin = new AxialCoordinateInt(0, 0);

        private const float threeOverTwo = 3f / 2f;
        private const float sqrtThree = 1.732050807568877f; // sqrt( 3f )
        private const float sqrtThreeOverTwo = 0.8660254037844386f; // sqrt( 3f ) / 2f

        public int q;
        public int r;

        static public implicit operator AxialCoordinateInt((int q, int r) coordinateIn)
        {
            return new AxialCoordinateInt() { q = coordinateIn.q, r = coordinateIn.r };
        }

        static public implicit operator AxialCoordinateInt(CubeCoordinateInt coordinateIn)
        {
            return new AxialCoordinateInt() { q = coordinateIn.x, r = coordinateIn.y };
        }

        static public AxialCoordinateInt operator +(
            AxialCoordinateInt one,
            AxialCoordinateInt other
        )
        {
            return new AxialCoordinateInt(one.q + other.q, one.r + other.r);
        }

        static public AxialCoordinateInt operator -(
            AxialCoordinateInt one,
            AxialCoordinateInt other
        )
        {
            return new AxialCoordinateInt(one.q - other.q, one.r - other.r);
        }

        public static AxialCoordinateInt FromCartesianUnit(Vector2 cartesianCoordinate)
        {
            return FromCartesian(cartesianCoordinate, 0.5f);
        }

        public static AxialCoordinateInt FromCartesian(Vector2 cartesianCoordinate, float hexRadius)
        {
            return CubeCoordinateFloat.Round(
                AxialCoordinateFloat.FromCartesian(cartesianCoordinate, hexRadius)
            );
        }

        public static int Distance(AxialCoordinateInt one, AxialCoordinateInt other)
        {
            return CubeCoordinateInt.Distance(one, other);
        }

        public static bool operator ==(AxialCoordinateInt one, AxialCoordinateInt other)
        {
            return one.q == other.q && one.r == other.r;
        }

        public static bool operator !=(AxialCoordinateInt one, AxialCoordinateInt other)
        {
            return one.q != other.q || one.r != other.r;
        }

        public AxialCoordinateInt(int qIn, int rIn)
        {
            q = qIn;
            r = rIn;
        }

        public Vector2 ToCartesianUnit()
        {
            return ToCartesian(0.5f);
        }

        public Vector2 ToCartesian(float hexRadius)
        {
            return new Vector2()
            {
                x = hexRadius * threeOverTwo * q,
                y = -(hexRadius * ((sqrtThreeOverTwo * q) + (sqrtThree * r)))
            };
        }

        public static IEnumerable<AxialCoordinateInt> TilesInRange(
            AxialCoordinateInt center,
            int distance
        )
        {
            return CubeCoordinateInt
                .TilesInRange(center, distance)
                .Select(x => (AxialCoordinateInt)x);
        }

        /// <summary>
        /// Enumerates the coordinates of the neighbour tiles for the given tile
        /// </summary>
        /// <param name="center">Center tile to find neighbours for</param>
        /// <returns>
        /// The coordinates of the neighbour tiles for the given tile.
        /// Assuming flat top topology. Starting at the top running clock wise.
        /// </returns>
        public static AxialCoordinateInt[] Neighbours(AxialCoordinateInt center)
        {
            return new AxialCoordinateInt[]
            {
                new AxialCoordinateInt(center.q + 1, center.r - 1),
                new AxialCoordinateInt(center.q + 0, center.r - 1),
                new AxialCoordinateInt(center.q - 1, center.r + 0),
                new AxialCoordinateInt(center.q - 1, center.r + 1),
                new AxialCoordinateInt(center.q + 0, center.r + 1),
                new AxialCoordinateInt(center.q + 1, center.r + 0),
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AxialCoordinateInt other))
            {
                return false;
            }

            return (other.q == q) && (other.r == r);
        }

        public override int GetHashCode()
        {
            return q << 16 | r;
        }
    }

    public struct AxialCoordinateFloat
    {
        private const float threeOverTwo = 3f / 2f;
        private const float twoOverThree = 2f / 3f;
        private const float oneOverThree = 1f / 3f;
        private const float sqrtThree = 1.732050807568877f; // sqrt( 3f )
        private const float sqrtThreeOverTwo = 0.8660254037844386f; // sqrt( 3f ) / 2f
        private const float sqrtThreeOverThree = 0.5773502691896258f; // sqrt( 3f ) / 3f

        public float q;
        public float r;

        static public implicit operator AxialCoordinateFloat((float q, float r) coordinateIn)
        {
            return new AxialCoordinateFloat() { q = coordinateIn.q, r = coordinateIn.r };
        }

        static public implicit operator AxialCoordinateFloat(CubeCoordinateFloat coordinateIn)
        {
            return new AxialCoordinateFloat() { q = coordinateIn.x, r = coordinateIn.y };
        }

        public Vector2 ToCartesianUnit()
        {
            return ToCartesian(0.5f);
        }

        public Vector2 ToCartesian(float tileSize)
        {
            return new Vector2()
            {
                x = tileSize * threeOverTwo * q,
                y = tileSize * ((sqrtThreeOverTwo * q) + (sqrtThree * r))
            };
        }

        public static AxialCoordinateFloat FromCartesianUnit(Vector2 cartesianCoordinate)
        {
            return FromCartesian(cartesianCoordinate, 0.5f);
        }

        public static AxialCoordinateFloat FromCartesian(
            Vector2 cartesianCoordinate,
            float tileSize
        )
        {
            return new AxialCoordinateFloat()
            {
                q = (twoOverThree * cartesianCoordinate.x) / tileSize,
                r =
                    (
                        (-oneOverThree * cartesianCoordinate.x)
                        + (sqrtThreeOverThree * cartesianCoordinate.y)
                    ) / tileSize
            };
        }
    }
}
