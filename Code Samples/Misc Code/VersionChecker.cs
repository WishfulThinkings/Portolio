using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionChecker : MonoBehaviour
{
    public TextMeshProUGUI versionText;


    private void OnEnable()
    {
        versionText.text = "v"+Application.version;
    }
}
