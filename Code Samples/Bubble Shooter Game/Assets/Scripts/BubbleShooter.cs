using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class BubbleShooter : MonoBehaviour
{
    public static BubbleShooter instance;

    private int poolAmount = 20;
    [SerializeField]private List<GameObject> pooledGO = new List<GameObject>();

    [SerializeField] private GameObject[] bubble;

    public Transform bubbleHandle;
    public Image[] nextBubbleImages;
    public BubbleMoveDown bubbleMoveDown;
    [SerializeField]private bool _canFire = true;
    void Start()
    {
        
        for (int i = 0; i < poolAmount; i++)
        {
            GameObject tempGO = Instantiate(bubble[Random.Range(0, bubble.Length - 1)]);
            bubbleMoveDown.bubbleList.Add(tempGO);
            BubbleBullet bulletscript = tempGO.GetComponent<BubbleBullet>();
            bulletscript.handlePos = bubbleHandle;
            tempGO.SetActive(false);
            pooledGO.Add(tempGO);
        }
        for (int i = 0; i < nextBubbleImages.Length - 1; i++)
        {
            nextBubbleImages[i].sprite = pooledGO[i].GetComponent<SpriteRenderer>().sprite;
        }
      

    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && _canFire == true || Input.GetKeyDown(KeyCode.Space) && _canFire == true)
        {
            Fire();
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        _canFire = false;
        yield return new WaitForSeconds(1f);
        _canFire = true;
    }
    private void Fire()
    {
        RegenerateBubbleBullet();
        if (GetPooledObject() != null)
        {
            GetPooledObject().transform.position = bubbleHandle.transform.position;
            GetPooledObject().SetActive(true);
        }
    }
    public GameObject GetPooledObject()
    {
        RegenerateBubbleBullet();
        for (int i = 0; i < pooledGO.Count; i++)
        {
            if (!pooledGO[i].activeInHierarchy)
            {
                nextBubbleImages[0].sprite = pooledGO[i + 1].GetComponent<SpriteRenderer>().sprite;
                nextBubbleImages[1].sprite = pooledGO[i + 2].GetComponent<SpriteRenderer>().sprite;
                nextBubbleImages[2].sprite = pooledGO[i + 3].GetComponent<SpriteRenderer>().sprite;
                return pooledGO[i];
            }
        }
        return null;
    }

    public void RegenerateBubbleBullet() 
    {
        for (int i = 0; i < pooledGO.Count; i++)
        {
            if (pooledGO[i] == null)
            {
                pooledGO.RemoveAt(i);
                GameObject tempGO = Instantiate(bubble[Random.Range(0, bubble.Length - 1)]);
                bubbleMoveDown.bubbleList.Add(tempGO);
                BubbleBullet bulletscript = tempGO.GetComponent<BubbleBullet>();
                bulletscript.handlePos = bubbleHandle;
                tempGO.SetActive(false);
                pooledGO.Add(tempGO);
            }
        }
    }

}
