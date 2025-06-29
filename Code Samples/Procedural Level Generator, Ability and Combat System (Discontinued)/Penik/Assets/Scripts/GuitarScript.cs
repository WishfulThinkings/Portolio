using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuitarScript : MonoBehaviour
{
    public Animator anim;
    public int resetCheck;
    public float attackSpeed;
    PlayerMovement playerM;
    private GameObject player;
    [SerializeField]
    private float attackCooldown, activeTime;
    public bool hitMarkFlip;
    
    //Controlls the movespeed when attacking can be upgraded later
    public float movementHalve;

    public float hitMarkSize;
    
   
    public GameObject hitMarkObject;

    enum CanAttack
    {
        ready,
        active,
        cooldown
    }

    CanAttack state = CanAttack.ready;
    void Start()
    {
      
        player = GameObject.FindGameObjectWithTag("Player");
        activeTime = attackCooldown;
    }

    // Put a cooldown timer before slasing to prevent a broken mechanic
    void Update()
    {
        
        switch (state)
        {
            case CanAttack.ready:
                if (Input.GetButton("Fire1"))
                {
                    
                    if (player.gameObject.transform.localScale.x < -1)
                    {
                        var hitMark = Instantiate(hitMarkObject, transform.position, transform.rotation);
                        hitMark.transform.parent = gameObject.transform;
                        hitMark.transform.localScale = new Vector3(hitMarkSize, hitMarkSize, transform.position.z);


                    }
                    else
                    {
                        var hitMark = Instantiate(hitMarkObject, transform.position, transform.rotation);
                        hitMark.transform.parent = gameObject.transform;
                        hitMark.transform.localScale = new Vector3(hitMarkSize, hitMarkSize, transform.position.z);
                    }
                   
                    
                    anim.Play("guitar_attack_anim");
                    state = CanAttack.active;
                    activeTime = attackSpeed;

                    //anim.Play("guitar_attack_anim");
                }
                break;
            case CanAttack.active:
                if (activeTime > 0)
                {
                    
                    activeTime -= Time.deltaTime;
                }
                else
                {
                    state = CanAttack.cooldown;
                    attackCooldown = attackSpeed;

                }
                break;
            case CanAttack.cooldown:
                if (attackCooldown > 0)
                {
                    attackCooldown -= Time.deltaTime;
                    anim.Play("guitar_attack_idle");
                }
                else { state = CanAttack.ready; }
                break;



         


        }
    }

   

   
}
