using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMoveDown : MonoBehaviour
{
    private bool _moveDown = true;
    public float delay;
    public List<GameObject> bubbleList = new List<GameObject>();
    public GameObject victoryGO;
    void Start()
    {
        StartCoroutine(MoveDownwards());

    }

    private void Update()
    {

        if(bubbleList.Count < 21)
        {
            
        }
    }
    private IEnumerator MoveDownwards()
    {
        while(_moveDown == true)
        {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i < bubbleList.Count; i++) 
            {
                if (bubbleList[i] == null)
                {
                    bubbleList.RemoveAt(i);
                }
                else { bubbleList[i].gameObject.transform.position = new Vector3(bubbleList[i].transform.position.x, bubbleList[i].transform.position.y - 0.3f, 0); }
            }
        }
    }
}
