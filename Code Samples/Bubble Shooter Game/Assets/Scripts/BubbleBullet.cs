using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleBullet : MonoBehaviour
{
    public Transform handlePos;
    private float speed = 10;
    private Rigidbody2D _rb2d;

    public BubbleBehaviour bubbleBehaviour;
    public BubbleMoveDown bubbleDown;

    [SerializeField] private int bulletNumber;

    void Start()
    {
        bubbleBehaviour = GetComponent<BubbleBehaviour>();
        _rb2d = GetComponent<Rigidbody2D>();
        _rb2d.velocity = handlePos.up * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bubble"))
        {
            BubbleBehaviour bubble = collision.gameObject.GetComponent<BubbleBehaviour>();
            if (bubble != null && bulletNumber == bubble.number)
            {
                 _rb2d.constraints = RigidbodyConstraints2D.FreezePosition;
                bubble.GetAdjacentBubbleData();
                bubbleBehaviour.popMechanic.AddRange(bubble.popMechanic);
                bubbleBehaviour.popMechanic.Add(bubble.gameObject);
                bubbleBehaviour.popMechanic.Add(this.gameObject);
                bubbleBehaviour.PopBubble();
                BubbleShooter.instance.RegenerateBubbleBullet();            
            }
            else
            {
                _rb2d.constraints = RigidbodyConstraints2D.FreezePosition;
            }
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 incomingVelocity = _rb2d.velocity;
            Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal);
            _rb2d.velocity = reflectedVelocity.normalized * speed; 
            speed *= 0.9f;
        }
    }
}
