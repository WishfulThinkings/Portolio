using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyHealth : MonoBehaviour
{
    public int health, mobValue = 1;
    enemySpawner enemSpawn;

    void Start()
    {
        
        enemSpawn = GameObject.FindGameObjectWithTag("Spawner").GetComponent<enemySpawner>();
    }
    public void takeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)

        {          
            Die();
        }
    }

    void Die()
    {
        enemSpawn.waveProgress(mobValue);
        Destroy(gameObject);
    }
}
