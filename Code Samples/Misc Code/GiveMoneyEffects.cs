using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Microsoft.MixedReality.GraphicsTools.MeshInstancer;

public class GiveMoneyEffects : MonoBehaviour
{
    public static       GiveMoneyEffects Instance;

    public GameObject[] playerPortraits;
    public GameObject bills;
    public Transform parent;
    public Vector3 middle;

    public GameObject[] chipGO;

    public UInt64 amount;

    [SerializeField]
    private List<GameObject> _totalBills;


    private void Awake()
    {
        middle = parent.transform.position;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }
    public IEnumerator GiveMoney(GameObject giver, GameObject receiver, int chipsAmount)
    {
        Vector3 start = giver.transform.position;
        //Vector3 middle = (giver.transform.position + receiver.transform.position) / 2 + Vector3.up * 2; // Add upward bounce
        Vector3 end = receiver.transform.position;

        float toMiddleDuration = 0.12f;
        float toReceiverDuration = 0.12f;
        float pauseDuration = 0.0005f; 
        //int totalBills = chipsAmount / 100;

        List<GameObject> _totalBills = new List<GameObject>();

        AudioManager.Instance.PlaySFX(UnityEngine.Random.Range(1, 3));
        yield return new WaitForSeconds(0.0001f);
        
        if(chipsAmount > 0)
        {
            for (int i = 0; i < 7; i++)
            {
                AudioManager.Instance.PlaySFX(UnityEngine.Random.Range(1,3));
                yield return new WaitForSeconds(0.0001f);
                GameObject animatedBills = Instantiate(bills, start, Quaternion.identity);
                animatedBills.transform.parent = parent;
                animatedBills.transform.SetAsFirstSibling();
                animatedBills.GetComponent<Animator>().Play("bills_fade_in");
                _totalBills.Add(animatedBills);
                chipGO[i].SetActive(true);
                float elapsedTime = 0f;


                while (elapsedTime < toMiddleDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / toMiddleDuration;


                    Vector3 currentPos = Vector3.Lerp(
                        Vector3.Lerp(start, middle, t),
                        Vector3.Lerp(middle, middle, t),
                        t
                    );

                    animatedBills.transform.position = currentPos;

                    yield return null;
                }


                animatedBills.transform.position = middle;


                yield return new WaitForSeconds(pauseDuration);
            }

            yield return new WaitForSeconds(0.5f);

            //yield return new WaitForSeconds(0.15f);
            int index = 6;
            foreach (GameObject go in _totalBills)
            {
                AudioManager.Instance.PlaySFX(UnityEngine.Random.Range(1, 3));
                chipGO[index].transform.GetChild(0).GetComponent<Animator>().Play("Chip_Out_Anim");
                yield return new WaitForSeconds(pauseDuration);
                float elapsedTime = 0f;
                go.GetComponent<Animator>().Play("bills_fade_out");


                while (elapsedTime < toReceiverDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / toReceiverDuration;

                    Vector3 currentPos = Vector3.Lerp(middle, end, t);
                    
                    go.transform.position = currentPos;

                    yield return null;
                }


                go.transform.position = end;
                chipGO[index].SetActive(false);
                index--;
                Destroy(go);
            }
        }
    }

}
