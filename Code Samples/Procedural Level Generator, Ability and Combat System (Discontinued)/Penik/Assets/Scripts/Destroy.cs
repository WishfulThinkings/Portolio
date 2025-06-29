
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    Flute fluteScript;

    int damage;
    void Start()
    {
        Destroy(gameObject, 1f);
        fluteScript = GameObject.Find("Flute").GetComponent<Flute>();
    }

     void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.name);
        enemyHealth enemy = other.GetComponent<enemyHealth>();
        if(enemy != null)
        {
            enemy.takeDamage(fluteScript.damage);
        }
        
    }

}
