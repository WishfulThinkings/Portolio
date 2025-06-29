using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkGen
{
    [SerializeField]
    private int corridorLength = 14, corridorCount = 5;
    [SerializeField]
    [Range(0.1f, 1)]
    public float roomPercent = 0.8f;

    protected override void RunProceduralGeneration()
    {
        CorridorFirstGeneration();  
    }

    private void CorridorFirstGeneration()
    {
        //position generator
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRooms = new HashSet<Vector2Int>();
        
        //generates the corridor and chooses which to use as a room for the dungeon
        CreateCorridors(floorPositions, potentialRooms);

        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRooms);

        floorPositions.UnionWith(roomPositions);
        
        //floor generator
        tilemapVisualizer.PaintFloorTile(floorPositions);
        wallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
        
        //creates rooms at dead ends
        List<Vector2Int> deadEnds = FindDeadEnds(floorPositions);
        CreateRoomsAtDeadEnd(deadEnds, roomPositions);
    }

    private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        foreach(var position in deadEnds)
        {
            if(roomFloors.Contains(position) == false)
            {
                var deadEndRoom = RunRandomWalk(randomWalkParameters, position);
                roomFloors.UnionWith(deadEndRoom);
            }
        }
    }

    private List<Vector2Int> FindDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();



        foreach (var positions in floorPositions)
        {
            int neighborCount = 0;
            foreach (var direction in Direction2D.cardinalDirectionsList)
            {
                if (floorPositions.Contains(positions + direction))
                {
                    neighborCount++;
                }
                if (neighborCount == 1)
                {
                    deadEnds.Add(positions);
                }
            }
        }
            return deadEnds;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRooms)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomToCreateCount = Mathf.RoundToInt(potentialRooms.Count * roomPercent);

        List<Vector2Int> roomToCreate = potentialRooms.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();
        foreach (var roomPosition in roomToCreate)
        {
            var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);
            roomPositions.UnionWith(roomFloor);
        }
        return roomPositions;
    }

    private void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRooms)
    {
        var currentPosition = startPos;
        potentialRooms.Add(currentPosition);

        for (int i = 0; i < corridorCount; i++)
        {
            var corridorPath = ProceduralGenAlgo.RandomWalkCorridor(currentPosition, corridorLength);
            currentPosition = corridorPath[corridorPath.Count - 1];
            potentialRooms.Add(currentPosition);
            floorPositions.UnionWith(corridorPath);
        }
    }
}
