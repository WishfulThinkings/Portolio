using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemySpawner : MonoBehaviour
{
    //public Vector2 spawnParameters;
   
    public GameObject[] enemies;
    public GameObject[] rareEnemies;
    [SerializeField]
    GameObject boss;
    [SerializeField]
    bool bossSpawn;
    public static int bossProgress = 0;
    void Start()
    {
        bossProgress = 0;
        StartCoroutine(Spawner(2f));
    }

    // Update is called once per frame
    void Update()
    {
        // if(bossProgress == 0)
        // {
        //     bossSpawn = true;
        //     spawnBoss();
        // }
        
    }

    IEnumerator Spawner(float spawnRate)
    {
        while(bossSpawn != true)
        {
            yield return new WaitForSeconds(spawnRate);
            roll();
            Instantiate(enemies[Random.Range(0, enemies.Length - 1)], new Vector2(Random.Range(-16f, 16f), Random.Range(-9f, 9f)), Quaternion.identity);
            yield return new WaitForSeconds(spawnRate);
        }
    }

    public void waveProgress(int mobValue)
    {
        bossProgress += mobValue;
        if(bossProgress == 10)
        {
            bossSpawn = true;
            spawnBoss();
        }
    }
    void spawnBoss()
    {
        Instantiate(boss, transform.position, Quaternion.identity);
    }

    void roll()
    {
        int numberRoll = Random.Range(1, 100);
        Debug.Log(numberRoll);
        
        
        if(numberRoll == 1f)
        {
            Instantiate(rareEnemies[Random.Range(0, rareEnemies.Length - 1)], new Vector2(Random.Range(-16f, 16f), Random.Range(-9f, 9f)), Quaternion.identity);
        }
    }
}
