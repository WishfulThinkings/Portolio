using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class wallGenerator
{
    public static void CreateWalls(HashSet<Vector2Int> floorPos, TileMapVisualizer tilemapVisualizer)
    {
        var basicWallPos = wallFinder(floorPos, Direction2D.cardinalDirectionsList);
        var cornerWallPos = wallFinder(floorPos, Direction2D.diagonalDirectionsList);
        CreateBasicWall(tilemapVisualizer, basicWallPos, floorPos);
        CreateCornerWall(tilemapVisualizer, cornerWallPos, floorPos);
    }

    private static void CreateCornerWall(TileMapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerWallPos, HashSet<Vector2Int> floorPos)
    {
        foreach (var position in cornerWallPos)
        {
            string neighborsBinaryType = "";
            foreach (var direction in Direction2D.eightDirectionsList)
            {
                var neighborPos = position + direction;
                if (floorPos.Contains(neighborPos))
                {
                    neighborsBinaryType += "1";
                }
                else
                {
                    neighborsBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleCornerWall(position, neighborsBinaryType);
        }
    }

    private static void CreateBasicWall(TileMapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPos, HashSet<Vector2Int> floorPos)
    {
        foreach (var position in basicWallPos)
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.cardinalDirectionsList)
            {
                var neighbourPos = position + direction;
                if (floorPos.Contains(neighbourPos))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintWall(position, neighboursBinaryType);
        }
    }

    private static HashSet<Vector2Int> wallFinder(HashSet<Vector2Int> floorPos, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPos)
        {
            foreach(var direction in directionList)
            {
                var neighborPos = position + direction;
                if(floorPos.Contains(neighborPos) == false)
                {
                    wallPositions.Add(neighborPos);
                }
                    
            }
        }
        return wallPositions;
    }
}
