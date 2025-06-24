using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Threading.Tasks;
using UnityEditor;
using TMPro;


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
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Usuário logado como: " + AuthenticationService.Instance.PlayerId);

    }

    async public void CreateLobby()
    {
        await Authenticate();
        hostLobby = await Lobbies.Instance.CreateLobbyAsync("lobby", 4);
        Debug.Log("Criou o lobby " + hostLobby.LobbyCode);
    }

    async public void JoinLobbyByCode()
    {
        await Authenticate();
        joinnedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(CODEinput.text);
        Debug.Log("Entrou no lobby " + joinnedLobby.LobbyCode);
    }

}
