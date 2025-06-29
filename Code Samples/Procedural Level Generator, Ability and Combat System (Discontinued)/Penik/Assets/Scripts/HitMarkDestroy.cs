using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitMarkDestroy : MonoBehaviour
{
    public Animator anim;
    PlayerMovement player;
    public float speedHalve;
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerMovement>();
        anim = anim.GetComponent<Animator>();
        anim.Play("guitar_attack_hitImpact");
        StartCoroutine(ThisDestroy(.5f));

    }
    IEnumerator ThisDestroy(float delay)
    {
        player.moveSpeed = speedHalve;
        yield return new WaitForSeconds(delay);
        player.moveSpeed = player.defaultSpeed;
       
        Destroy(gameObject);
    }
}


