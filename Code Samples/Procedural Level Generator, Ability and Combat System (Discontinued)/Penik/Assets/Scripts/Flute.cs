using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Flute : MonoBehaviour
{
    [SerializeField]
    float holdDuration, maxCap;
    public GameObject attack;
    [SerializeField]   
    public Slider chargeSlider;
    public int damage;
    float reloadTime = 3f, speedHalve = 750f, normalSpeed;
    [SerializeField]
    bool isReloading;
    public TextMeshProUGUI reloadText;
    PlayerMovement player;
    
    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<PlayerMovement>();
        chargeSlider = GameObject.Find("UI_RechargeMeter").GetComponent<Slider>();
        reloadText = GameObject.Find("UI_reloadText").GetComponent<TextMeshProUGUI>();
        chargeSlider.gameObject.SetActive(true);
    }
    void Start()
    {     
        chargeSlider.maxValue = maxCap;
        normalSpeed = player.moveSpeed;
    }
    void Update()
    {
        reloadText.text = reloadTime.ToString();
        if (Input.GetButton("Fire2") && isReloading == false)
        {
            //halves the move speed of the player
            player.moveSpeed = speedHalve;
            chargeSlider.gameObject.SetActive(true);
            chargeSlider.value = holdDuration;
            holdDuration += Time.deltaTime * 2;
            attack.transform.localScale = new Vector3(1 + holdDuration, 1 + holdDuration, 0);
            if (holdDuration >= maxCap)
            {
                damage = 100;
                attack.transform.localScale = new Vector3(maxCap, maxCap, 0);
            }
            else { damage = 25; }
        }
        else { holdDuration = 0f;}

        if (Input.GetButtonUp("Fire2") && isReloading == false)
        {     
            Instantiate(attack, transform.position, Quaternion.identity);
            isReloading = true;
            player.moveSpeed = normalSpeed;
        }
        
        if(isReloading == true)
        {
            reloadTime -= 1 * Time.deltaTime;
            Reload();
            chargeSlider.gameObject.SetActive(false);

        }

        Vector2 flutePos = transform.position;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - flutePos;
        transform.right = direction;
    }
    void Reload()
    {
        //reloadTime -= 1 * Time.deltaTime;
        if (reloadTime <= 0f)
        {
            
            isReloading = false;
            reloadTime = 3f;
        }

    }
}
