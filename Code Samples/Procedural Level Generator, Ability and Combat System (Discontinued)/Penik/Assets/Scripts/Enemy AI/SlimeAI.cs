using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Rigidbody2D))]

public class SlimeAI : MonoBehaviour
{
    public float slimeSpeed;
    private Rigidbody2D rb2d;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Animator anim;
    private float knockbackRes = 15f;
   
    

    void Start()
    {

        anim = this.GetComponent<Animator>();
        rb2d = this.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(slimeBehaviour(1f));
    }
    void Update()
    {
        anim.SetFloat("Moving", slimeSpeed);
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, slimeSpeed * Time.deltaTime);
    }
    IEnumerator slimeBehaviour(float delay)
    {
        slimeSpeed =1f;
        yield return new WaitForSeconds(delay);
        slimeSpeed = 0f;
        yield return new WaitForSeconds(delay);
        StartCoroutine(slimeBehaviour(1f));      
    }

    private void OnTrigSgerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Weapon")
        {
            Vector2 direction = (transform.position - player.transform.position).normalized;
            rb2d.AddForce(direction * knockbackRes, ForceMode2D.Impulse);

        }
    }




}
