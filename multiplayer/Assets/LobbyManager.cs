using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;


public class LobbyManager : MonoBehaviour
{
    Lobby hostLobby, joinnedLobby;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

   async void Authenticate()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    }

    async public void CreateLobby()
    {
        Authenticate();
        hostLobby = await Lobbies.Instance.CreateLobbyAsync("lobby", 4);
    }


}
