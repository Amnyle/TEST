using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using TMPro;


public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] TMP_InputField iF;
    private Lobby hostLobby;
    private float heartbeatTimer;
    // Start is called before the first frame update
    public async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    private void Update()
    {
        heartBeatHandler();


    }
    private async void heartBeatHandler()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f) 
            {
                float hearBeatTimerMax = 15f;
                heartbeatTimer = hearBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);

            }

        }

    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "My Lobby";
            int maxPlayers = 4;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            hostLobby = lobby;

            Debug.Log("Created Lobby:" + lobby.Name + " " + maxPlayers + " code:" + lobby.LobbyCode);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void ListLobbies()
    {
        try
        {
            QueryResponse qr = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log($"Number of lobbies found{qr.Results.Count}");
            foreach (Lobby lobby in qr.Results)
            {
                Debug.Log(lobby.Name + " " +lobby.LobbyCode);
            }
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void JoinLobbyByCode()
    {
        try
        {
            string lobbyCode = iF.text;
            Debug.Log(lobbyCode);
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log($"Joined Lobby witg code {lobbyCode}");


        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }
  


}
