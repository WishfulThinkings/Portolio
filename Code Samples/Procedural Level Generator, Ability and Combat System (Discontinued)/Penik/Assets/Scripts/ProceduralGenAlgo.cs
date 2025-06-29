using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGenAlgo
{
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPos, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        path.Add(startPos);
        var previousPos = startPos;

        for (int i = 0; i < walkLength; i++)
        {
            var newPos = previousPos + Direction2D.GetRandomCardnicalDirection();
            path.Add(newPos);
            previousPos = newPos;
        }
        return path;
    }

    public static List<Vector2Int> RandomWalkCorridor(Vector2Int corridorStartPos, int corridorLength)
    {
        List<Vector2Int> corridorList = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardnicalDirection();
        var currentCorridorPos = corridorStartPos;
        corridorList.Add(currentCorridorPos);
        for (int i = 0; i < corridorLength; i++)
        {
            currentCorridorPos += direction;
            corridorList.Add(currentCorridorPos);
        }
        return corridorList;
    }

    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);
        while(roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();
            if(room.size.y >= minHeight && room.size.x >= minWidth)
            {
                if(Random.value < 0.5)
                {
                    if(room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minWidth, roomsQueue, room);
                    }
                    else if(room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minHeight, roomsQueue, room);
                    }
                    else if(room.size.x >= minWidth && room.size.y > minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                   
                    if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minHeight, roomsQueue, room);
                    }
                    else if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minWidth, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y > minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }
        return roomsList;
    }

    private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        //algorithm that splits the room horizontally
        var xSplit = Random.Range(1, room.size.x);
        BoundsInt xRoom1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt xRoom2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
            new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
        roomsQueue.Enqueue(xRoom1);
        roomsQueue.Enqueue(xRoom2);
    }

    private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        //algorithm that splits the room vertically 
        var ySplit = Random.Range(1, room.size.y);
        BoundsInt yRoom1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt yRoom2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
            new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
        roomsQueue.Enqueue(yRoom1);
        roomsQueue.Enqueue(yRoom2);
    }
}

public static class Direction2D
{
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1), //Up
        new Vector2Int(1,0), //Right
        new Vector2Int(0,-1), //Down
        new Vector2Int(-1,0) //Left  
    };

    public static List<Vector2Int> diagonalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(1,1), //Up-right
        new Vector2Int(1,-1), //Right-down
        new Vector2Int(-1,-1), //Down-left
        new Vector2Int(-1,1) //Left-up
    };

    public static List<Vector2Int> eightDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1), //Up
        new Vector2Int(1,1), //Up-right
        new Vector2Int(1,0), //Right
        new Vector2Int(1,-1), //Right-down
        new Vector2Int(0,-1), //Down
        new Vector2Int(-1,-1), //Down-left
        new Vector2Int(-1,0), //Left  
        new Vector2Int(-1,1) //Left-up
    };

    public static Vector2Int GetRandomCardnicalDirection()
    { 
        return cardinalDirectionsList[Random.Range(0, cardinalDirectionsList.Count)]; 
    }

}
