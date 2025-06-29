using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueLookAt : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform.position);
    }
}
