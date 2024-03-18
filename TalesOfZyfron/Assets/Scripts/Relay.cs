using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;

public class Relay : MonoBehaviour
{
    public GameObject codeGameObject;
    public GameObject canvasGameObject;
    public new GameObject camera;
    [SerializeField] TMP_InputField iF;
    [SerializeField] TMP_Text text;
    private async void Start()
    {
        codeGameObject.SetActive(false);
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    public async void CreateRelay()
    {
        try
        {

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            text.text ="Room number: " + joinCode;
            Debug.Log(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );
            NetworkManager.Singleton.StartHost();
            canvasGameObject.SetActive(false);
            codeGameObject.SetActive(true);
            camera.SetActive(false);

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void JoinRelay()
    {
        try
        {
            string joinCode = iF.text;
            Debug.Log("Joining Relay with code " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );
            NetworkManager.Singleton.StartClient();
            canvasGameObject.SetActive(false);
            camera.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    
}
