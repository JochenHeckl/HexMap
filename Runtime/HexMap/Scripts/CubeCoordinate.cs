/// Thanks Amit! https://www.redblobgames.com/grids/hexagons

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace de.JochenHeckl.Unity.HexMap
{
    public struct CubeCoordinateInt
    {
        public int x;
        public int y;
        public int z;

        public CubeCoordinateInt( int xIn, int yIn, int zIn )
        {
            x = xIn;
            y = yIn;
            z = zIn;
        }


        public static implicit operator CubeCoordinateInt( (int xIn, int yIn, int zIn) coordinateIn )
        {
            return new CubeCoordinateInt()
            {
                x = coordinateIn.xIn,
                y = coordinateIn.yIn,
                z = coordinateIn.zIn
            };
        }

        public static implicit operator CubeCoordinateInt( AxialCoordinateInt coordinateIn )
        {
            return new CubeCoordinateInt()
            {
                x = coordinateIn.q,
                y = coordinateIn.r,
                z = -coordinateIn.q - coordinateIn.r
            };
        }

        public static CubeCoordinateInt operator +( CubeCoordinateInt one, CubeCoordinateInt other )
        {
            return new CubeCoordinateInt()
            {
                x = one.x + other.x,
                y = one.y + other.y,
                z = one.z + other.z
            };
        }

        public static int Distance( CubeCoordinateInt one, CubeCoordinateInt other )
        {
            return (Math.Abs( one.x - other.x ) + Math.Abs( one.y - other.y ) + Math.Abs( one.z - other.z )) / 2;
        }

        public static IEnumerable<CubeCoordinateInt> TilesInRange( CubeCoordinateInt center, int distance )
        {
            return Enumerable.Range( -distance, 1 + (2 * distance) )
                .SelectMany( x =>
                {
                    var rangeStart = Math.Max( -distance, -x - distance );
                    var rangeEnd = Math.Min( distance, -x + distance );

                    return Enumerable.Range( rangeStart, 1 + (rangeEnd - rangeStart) )
                        .Select( y => new CubeCoordinateInt()
                        {
                            x = center.x + x,
                            y = center.y + y,
                            z = center.z + (-x - y)
                        } );
                } );
        }

        public static IEnumerable<CubeCoordinateInt> Neighbours( CubeCoordinateInt center )
        {
            return new CubeCoordinateInt[]
                {
                   new CubeCoordinateInt( 0, -1, +1 ),
                   new CubeCoordinateInt( 1, -1, 0 ),
                   new CubeCoordinateInt( 1, 0, -1 ),
                   new CubeCoordinateInt( 0, 1, -1 ),
                   new CubeCoordinateInt( -1, 1, 0 ),
                   new CubeCoordinateInt( -1, 0, +1 )
                };
        }
    }

    public struct CubeCoordinateFloat
    {
        public float x;
        public float y;
        public float z;

        static public implicit operator CubeCoordinateFloat( AxialCoordinateFloat coordinateIn )
        {
            return new CubeCoordinateFloat()
            {
                x = coordinateIn.q,
                y = coordinateIn.r,
                z = -coordinateIn.q - coordinateIn.r
            };
        }

        static public CubeCoordinateInt Round( CubeCoordinateFloat coordinateIn )
        {
            var rx = Mathf.Round( coordinateIn.x );
            var ry = Mathf.Round( coordinateIn.y );
            var rz = Mathf.Round( coordinateIn.z );

            var xDiff = Mathf.Abs( rx - coordinateIn.x );
            var yDiff = Mathf.Abs( ry - coordinateIn.y );
            var zDiff = Mathf.Abs( rz - coordinateIn.z );

            if ((xDiff > yDiff) && xDiff > zDiff)
            {
                rx = -ry - rz;
            }
            else if (yDiff > zDiff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new CubeCoordinateInt()
            {
                x = (int) rx,
                y = (int) ry,
                z = (int) rz
            };
        }
    }
}
