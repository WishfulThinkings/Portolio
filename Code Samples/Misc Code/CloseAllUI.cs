using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Microsoft.MixedReality.GraphicsTools.MeshInstancer;

public class CloseAllUI : MonoBehaviour
{
    public static CloseAllUI Instance;

    public List<GameObject> uiList;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }
    public void ClearUI()
    {
        AnimationController.Instance.ResetTilePosition();
        foreach (GameObject go in uiList)
        {
            if (go != null) 
            {
                go.SetActive(false);
            }

        }
    }
}
