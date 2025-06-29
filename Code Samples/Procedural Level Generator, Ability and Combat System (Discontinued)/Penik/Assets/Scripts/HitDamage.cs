using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDamage : MonoBehaviour
{
    public int weaponDamage = 50;
    public float knockback;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enemyHealth enemy = collision.transform.GetComponent<enemyHealth>();
        if(enemy != null)
        {
            enemy.takeDamage(weaponDamage);
         
            Debug.Log(enemy.health);
        }

    }
}
