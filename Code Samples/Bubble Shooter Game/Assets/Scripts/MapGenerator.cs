using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public BubbleMoveDown bubbleDown;

    public GameObject[] bubblePrefabs;

    [SerializeField]private int mapWidth = 25;
    [SerializeField]private int mapHeight = 20;

    public float tileXOffset = 1.8f;
    public float tileYOffset = 1.8f;
    void Start()
    {
        GenerateMap();
    }
    private void GenerateMap()
    {
        for (int x = 0; x <= mapWidth; x++) 
        { 
            for(int y = 0; y <= mapHeight; y++)
            {
                GameObject tempGO = Instantiate(bubblePrefabs[Random.Range(0, bubblePrefabs.Length -1)]);
                bubbleDown.bubbleList.Add(tempGO);
                
                if( y % 2 == 0)
                {
                    tempGO.transform.position = new Vector2(x  * tileXOffset, y * tileYOffset);
                }
                else { tempGO.transform.position = new Vector2(x * tileXOffset + tileXOffset/2, y * tileYOffset); }

            }
 
        }
    }

}
