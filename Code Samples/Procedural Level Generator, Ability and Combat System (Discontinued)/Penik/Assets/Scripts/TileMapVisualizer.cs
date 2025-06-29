using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap floorTilemap, wallTilemap;
    [SerializeField]
    private TileBase floorTile, wallSprite, wallSideRight, wallSideLeft, wallBottom, wallFull,
       wallInnerCornerDownLeft, wallInnerCornerDownRight,
        wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;


    public void PaintFloorTile(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile); 
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tileMap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tileMap, tile, position);
        }
    }

    internal void PaintWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        if (WallPlacementBinaryValues.wallTop.Contains(typeAsInt))
        {
            tile = wallSprite;
        }
        else if (WallPlacementBinaryValues.wallSideRight.Contains(typeAsInt))
        {
            tile = wallSideRight;
        }
        else if (WallPlacementBinaryValues.wallSideLeft.Contains(typeAsInt))
        {
            tile = wallSideLeft;
        }
        else if (WallPlacementBinaryValues.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }
        else if (WallPlacementBinaryValues.wallBottm.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        if (tile != null)
        {
        PaintSingleTile(wallTilemap, tile, position);
        }
    }

   

    private void PaintSingleTile(Tilemap tileMap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tileMap.WorldToCell((Vector3Int)position);
        tileMap.SetTile(tilePosition, tile);
    }

    public void Reset()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeASInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        if (WallPlacementBinaryValues.wallInnerCornerDownLeft.Contains(typeASInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallPlacementBinaryValues.wallInnerCornerDownRight.Contains(typeASInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallPlacementBinaryValues.wallDiagonalCornerDownLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (WallPlacementBinaryValues.wallDiagonalCornerDownRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownRight;
        }
        else if (WallPlacementBinaryValues.wallDiagonalCornerUpRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (WallPlacementBinaryValues.wallDiagonalCornerUpLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (WallPlacementBinaryValues.wallFullEightDirections.Contains(typeASInt))
        {
            tile = wallFull;
        }
        else if (WallPlacementBinaryValues.wallBottmEightDirections.Contains(typeASInt))
        {
            tile = wallBottom;
        }

        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }
}
