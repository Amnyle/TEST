using System.Collections;
using System.Collections.Generic;
using Unity.Netcode; 
using UnityEngine;


public class FirstPersonCamera : NetworkBehaviour
{
  
    public Transform orientation;
    public float sensX;
    public float sensY;

    float xRotation;
    float yRotation;
  
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        if (!IsOwner) return;
        if (IsLocalPlayer)
            gameObject.SetActive(true);


        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        // On recupere l'input de la souris
        float mouseX = Input.GetAxis("Mouse Y") * sensX;
        float mouseY = Input.GetAxis("Mouse X") * sensY;
        xRotation -= mouseX;
        yRotation += mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //bouger la camera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);

        

    }

}

