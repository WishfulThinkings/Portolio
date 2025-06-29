using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleRandomWalkGen : AbstractDungenGenerator
{
   
    [SerializeField]
    protected SimpleRandomWalkData randomWalkParameters;
    //[SerializeField]
    //public int walkLength = 10;
    //[SerializeField]
    //public bool startRandomlyEachIteration = true; 

    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPos = RunRandomWalk(randomWalkParameters, startPos);
        tilemapVisualizer.PaintFloorTile(floorPos);
        wallGenerator.CreateWalls(floorPos, tilemapVisualizer);
    }

    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkData parameters, Vector2Int position)
    {
        var currentPos = position;
        HashSet<Vector2Int> floorPos = new HashSet<Vector2Int>();
        for (int i = 0; i < parameters.iterations; i++)
        {
            var path = ProceduralGenAlgo.SimpleRandomWalk(currentPos, parameters.walkLength);
            floorPos.UnionWith(path);
            if (parameters.startRandomlyEachIteration)
            {
                currentPos = floorPos.ElementAt(Random.Range(0, floorPos.Count));
            }
        }
        return floorPos;
    }
}
