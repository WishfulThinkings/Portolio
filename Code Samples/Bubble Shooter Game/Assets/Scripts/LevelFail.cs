using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFail : MonoBehaviour
{
    public GameObject retryButton;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bubble"))
        {
            retryButton.SetActive(true);
        }

    }
}
