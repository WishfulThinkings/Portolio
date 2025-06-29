using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BubbleBehaviour : MonoBehaviour
{
    private Animator _anim;
    private Rigidbody2D _rb2d;
    public PolygonCollider2D polygonCollider;
    public int number;
    public List<GameObject> popMechanic;

    public string clipName;

    private GameManager _gameManager;
    void Awake()
    {
        _anim = GetComponent<Animator>();
        _gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        _rb2d = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BubbleBehaviour bubble = collision.gameObject.GetComponent<BubbleBehaviour>();
        if (bubble != null)
        {
            if (number == bubble.number)
            {
                popMechanic.Add(bubble.gameObject);
                GetAdjacentBubbleData();
            }
        }
    }


    public void GetAdjacentBubbleData()
    {
        List<GameObject> newBubbles = new List<GameObject>();

        foreach (GameObject bubble in popMechanic)
        {
            newBubbles.AddRange(bubble.GetComponent<BubbleBehaviour>().popMechanic);
        }

        popMechanic.AddRange(newBubbles);
        popMechanic = popMechanic.Distinct().ToList();
    }

    public void PopBubble()
    {
        if (popMechanic.Count > 2)
        {
            StartCoroutine(DestroyAnim());
        }
    }
    private IEnumerator DestroyAnim()
    {
        foreach (GameObject go in popMechanic)
        {
            Animator tempAnim = go.GetComponent<Animator>();
            _gameManager.AddScore(100);
            tempAnim.Play(clipName);
        }
        yield return new WaitForSeconds(1);
        foreach (GameObject go in popMechanic)
        {
            Destroy(go);
        }
    }
}
