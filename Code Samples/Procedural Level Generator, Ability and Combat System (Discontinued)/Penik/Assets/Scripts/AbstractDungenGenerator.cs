using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class AbstractDungenGenerator : MonoBehaviour
{
    [SerializeField]
    protected TileMapVisualizer tilemapVisualizer = null;
    [SerializeField]
    protected Vector2Int startPos = Vector2Int.zero;

    public void DungenGenerator()
    {
        //You have to learn a way to clear all the tiles unless it doesnt make a big difference lol
        // here's hoping this shit will still works lmao
        tilemapVisualizer.Reset();
        RunProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();
}
