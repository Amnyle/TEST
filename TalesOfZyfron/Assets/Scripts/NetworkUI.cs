using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : NetworkBehaviour
{
    public Button hostButton;
    public Button clientButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }


    // Update is called once per frame
    void Update()
    {
    
       
    }
}
