using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed, moveX, defaultSpeed;
    public Rigidbody2D rb2d;
    private Vector2 moveDirection;
    
    public bool flipCheck;
    public Animator anim;
    public bool idleAnimCheck;
    private void Awake()
    {
        defaultSpeed = moveSpeed;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("flipCheck", moveX);
        anim.SetBool("BackToIdle", idleAnimCheck);
        Move();
        if(moveX > 0f && flipCheck == true)
        {
            Flip();
        }
        if(moveX < 0f && flipCheck == false)
        {
            Flip();
        }
        if (moveX == 0)
        {
            idleAnimCheck = true;
        }
        else
        {
            idleAnimCheck = false;
        }
    }

    void FixedUpdate()
    {
        processInputs();
    }

    void processInputs()
    {
        moveX = (Input.GetAxisRaw("Horizontal"));
        float moveY = (Input.GetAxisRaw("Vertical"));

        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    void Move()
    {
        rb2d.velocity = new Vector2(moveDirection.x * moveSpeed * Time.deltaTime, moveDirection.y * moveSpeed *Time.deltaTime);
    }

    void Flip()
    {      
            flipCheck = !flipCheck;
            Vector3 charScale = transform.localScale;
            charScale.x *= -1;
            transform.localScale = charScale;
    }
}
