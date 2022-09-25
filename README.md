# HexMap
This package contains some utilities to deal with hex meshes.

It is inspired by redblobgames excellent information on the [Implementation of Hex Grids](https://www.redblobgames.com/grids/hexagons/implementation.html).

Tiles are considered to be [flat top](https://www.redblobgames.com/grids/hexagons/#coordinates) with vertex order.

Tile indices try to  follow an order starting to the right running counter clockwise:
```
Vertices:                        Tileneighbours:
                                                   1 
     3-------2                                 *-------*
    /         \                               /         \
   /           \                          2  /           \  0
  /             \                           /             \
 4       0       1                         *               *
  \             /                           \             /
   \           /                          3  \           /  5
    \         /                               \         /
     5-------6                                 *-------*
                                                   4
```