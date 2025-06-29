using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laserScript : MonoBehaviour
{
    public Transform firePoint;
    public LineRenderer laser;
    [SerializeField]
    int laserDamage = 25;
    void Start()
    {
             
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            StartCoroutine(Shoot());
        }   
    }

    IEnumerator Shoot()
    {
        //var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D isHitting = Physics2D.Raycast(firePoint.position, firePoint.right);
        
        if (isHitting)
        {
            Debug.Log(isHitting.transform.name);
            enemyHealth enemy = isHitting.transform.GetComponent<enemyHealth>();
            if(enemy != null)
            {
                enemy.takeDamage(laserDamage);
                Debug.Log(enemy.health);
            }
            laser.SetPosition(0, firePoint.position);
            laser.SetPosition(1, isHitting.point);
        }

        else
        {
            laser.SetPosition(0, firePoint.position);
            laser.SetPosition(1, firePoint.position + firePoint.right * 100);
        }

        laser.enabled = true;
        yield return new WaitForSeconds(0.2f);
        laser.enabled = false;
        StopCoroutine(Shoot());
    }
}
