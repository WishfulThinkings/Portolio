using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleRandomWalkParameters_", menuName = "ProcContentGen/SimpleRandomWalkData")]

public class SimpleRandomWalkData : ScriptableObject
{
    // Start is called before the first frame update
    public int iterations = 10, walkLength = 10;
    public bool startRandomlyEachIteration = true;

}
