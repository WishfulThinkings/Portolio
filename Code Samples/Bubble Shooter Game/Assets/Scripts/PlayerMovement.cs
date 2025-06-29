using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rb2d;
    [SerializeField] private float _moveSpeed;
    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        float moveInput = 0f;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            moveInput = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveInput = -1f;
        }

        Vector2 moveDirection = new Vector2(moveInput, 0);
        _rb2d.velocity = moveDirection * _moveSpeed;
    }
}
