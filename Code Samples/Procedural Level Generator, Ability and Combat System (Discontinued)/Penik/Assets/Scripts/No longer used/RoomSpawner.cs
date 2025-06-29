using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public int openingDirection;
    private roomTemplates templates;
    private int rng;
    public bool spawned = false;
    //1 = upper room
    //2 = left room
    //3 = down room
    //4 = right room
   
    void Start()
    {
        templates = GameObject.FindGameObjectWithTag("Rooms").GetComponent<roomTemplates>();
        Invoke("Spawn", 0.1f);
    }

    
    void Spawn()
    {
        if(spawned == false)
        {
            if (openingDirection == 1)
            {
                //need bottom door to be open
                rng = Random.Range(0, templates.bottomRooms.Length);
                Instantiate(templates.bottomRooms[rng], transform.position, templates.bottomRooms[rng].transform.rotation);
            }
            else if (openingDirection == 2)
            {
                //need right door to be open
                rng = Random.Range(0, templates.rightRooms.Length);
                Instantiate(templates.rightRooms[rng], transform.position, templates.rightRooms[rng].transform.rotation);
            }
            else if (openingDirection == 3)
            {
                //need top door to be open
                rng = Random.Range(0, templates.topRooms.Length);
                Instantiate(templates.topRooms[rng], transform.position, templates.topRooms[rng].transform.rotation);
            }
            else if (openingDirection == 4)
            {
                //need left door to be open
                rng = Random.Range(0, templates.leftRooms.Length);
                Instantiate(templates.leftRooms[rng], transform.position, templates.leftRooms[rng].transform.rotation);
            }
            spawned = true;
        }
       
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerSpawn"))
        {
            if (collision.GetComponent<RoomSpawner>().spawned == false && this.spawned == false)
            {
                // spawn walls blocking off any opening
                Instantiate(templates.closedRoom, transform.position, Quaternion.identity);   
            }
            Destroy(gameObject);

        }
       
    }
}
