using System.Collections.Generic;
using UnityEngine;

public class DraggableCamera : MonoBehaviour
{
    public Camera targetCamera;
    public float dragSpeed = 0.1f;

    public float minX = -55.8f;
    public float maxX = -39.9f;
    public float minZ = 37.7f;
    public float maxZ = 55.2f;

    private Vector3 lastMousePosition;
    public List<GameObject> ui;

    public static bool isDragging { get; private set; }

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        isDragging = false;
    }

    void Update()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;

            bool isAvailable = CheckAvailability();
            if (isAvailable)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;


            float moveX = mouseDelta.x * dragSpeed * Time.deltaTime;  
            float moveZ = mouseDelta.y * dragSpeed * Time.deltaTime;  

            Vector3 newPosition = targetCamera.transform.position + new Vector3(moveX, 0, moveZ);

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

            targetCamera.transform.position = newPosition;

            lastMousePosition = Input.mousePosition;
        }
        else
        {

            targetCamera.transform.position = new Vector3(-49.5168304f, 48.3181686f, 46.0010605f);
            isDragging = false;
        }
    }

    private bool CheckAvailability()
    {
        // Check if dragging is allowed based on various conditions
        if (AnimationController.Instance.freePropertyUpgrade == false &&
            AnimationController.Instance.remoteUpgrade == false &&
            AnimationController.Instance.mortgageProperty == false &&
            AnimationController.Instance.targetEntityTile == false &&
            isDragging == false)
        {
            for (int i = 0; i < ui.Count; i++)
            {
                if (ui[i].activeSelf)
                {
                    return false; 
                }
            }
            return true;  
        }
        return false;
    }
}
