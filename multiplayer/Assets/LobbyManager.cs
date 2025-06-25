using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Threading.Tasks;
using UnityEditor;
using TMPro;
using System.Collections.Generic;


public class LobbyManager : MonoBehaviour
{
    public TMP_InputField namePLAYERinput, CODEinput;
    Lobby hostLobby, joinnedLobby;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

   async Task Authenticate()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            return;

        AuthenticationService.Instance.ClearSessionToken();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Usuário logado como: " + AuthenticationService.Instance.PlayerId);

    }

    async public void CreateLobby()
    {
        await Authenticate();

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = GetPlayer()
        };

        hostLobby = await Lobbies.Instance.CreateLobbyAsync("lobby", 4, options);
        joinnedLobby = hostLobby;
        Debug.Log("Criou o lobby " + hostLobby.LobbyCode);
        InvokeRepeating("SendLobbyHeartBeat", 10, 10);
        ShowPlayers();
    }

    async public void JoinLobbyByCode()
    {
        await Authenticate();

        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = GetPlayer()
        };


        joinnedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(CODEinput.text);
        Debug.Log("Entrou no lobby " + joinnedLobby.LobbyCode);
    }

    Player GetPlayer()
    {
        Player player = new Player()
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, namePLAYERinput.text) }
            }
        };

        return player;
    }
    async void SendLobbyHeartBeat()
    {
        if (hostLobby == null)
            return;

        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        Debug.Log("Atualizou o lobby");
        UptadeLobby();
        ShowPlayers();
    }

    void ShowPlayers()
    {
        for (int i = 0; i < joinnedLobby.Players.Count; i++)
        {
            Debug.Log(joinnedLobby.Players[i].Data["name"].Value);
        }
    }

    async void UptadeLobby()
    {
        if (joinnedLobby == null)
            return;

        joinnedLobby = await LobbyService.Instance.GetLobbyAsync(joinnedLobby.Id);
    }
}
