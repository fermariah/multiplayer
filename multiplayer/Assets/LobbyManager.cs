using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{

    public TMP_InputField playerNameInput;
    public TMP_InputField lobbyCodeInput;

    public Lobby hostLobby, joinnedLobby;
    public GameObject lobbyIntro, lobbyPanel;
    public TMP_Text[] lobbyPlayersText;
    public TMP_Text lobbyCodeText;

    public GameObject startGameButton;

    bool startedGame;

    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    // Update is called once per frame
    void Update()
    {

    }

    async Task Authenticate()
    {

        if (AuthenticationService.Instance.IsSignedIn)
        {
            return;
        }

        AuthenticationService.Instance.ClearSessionToken();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Logado como " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    async public void CreateLobby()
    {
        try
        {

            await Authenticate();

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"StartGame", new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            Lobby lobby = await Unity.Services.Lobbies.LobbyService.Instance.CreateLobbyAsync("Lobby", 4, createLobbyOptions);

            Debug.Log("Criou o lobby " + lobby.LobbyCode);

            hostLobby = lobby;
            joinnedLobby = hostLobby;
            lobbyIntro.SetActive(false);
            lobbyPanel.SetActive(true);
            lobbyCodeText.text = lobby.LobbyCode;
            ShowPlayersOnLobby();
            InvokeRepeating("LobbyHeartBeat", 5, 5);
            startGameButton.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }


    }


    void CheckForLobbyUpdates()
    {
        if (joinnedLobby == null || startedGame)
        {
            return;
        }

        UpdateLobby();
        ShowPlayersOnLobby();
        if (joinnedLobby.Data["StartGame"].Value != "0")
        {
            if (hostLobby == null)
            {
                JoinRelay(joinnedLobby.Data["StartGame"].Value);
            }

            startedGame = true;
        }

    }

    async void LobbyHeartBeat()
    {
        if (hostLobby == null)
            return;

        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        Debug.Log("Atualizou lobby");
        UpdateLobby();
        ShowPlayersOnLobby();
    }

    async public void JoinLobby()
    {
        try
        {
            await Authenticate();

            JoinLobbyByCodeOptions createLobbyOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Unity.Services.Lobbies.LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCodeInput.text, createLobbyOptions);

            joinnedLobby = lobby;

            lobbyIntro.SetActive(false);
            lobbyPanel.SetActive(true);

            lobbyCodeText.text = lobby.LobbyCode;

            Debug.Log("Entrou no lobby " + lobby.LobbyCode);

            ShowPlayersOnLobby();
            InvokeRepeating("CheckForLobbyUpdates", 3, 3);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    Player GetPlayer()
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerNameInput.text) }
                    }
        };

        return player;
    }

    async void UpdateLobby()
    {
        if (joinnedLobby == null)
            return;

        joinnedLobby = await LobbyService.Instance.GetLobbyAsync(joinnedLobby.Id);

    }

    void ShowPlayersOnLobby()
    {
        for (int i = 0; i < joinnedLobby.Players.Count; i++)
        {
            lobbyPlayersText[i].text = joinnedLobby.Players[i].Data["name"].Value;
        }

    }


    async Task<string> CreateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();


        return joinCode;
    }

    async void JoinRelay(string joinCode)
    {
        Debug.Log("Tentando criar relay cliente com código: " + joinCode);
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();

        Debug.Log("Relay cliente iniciado");

        lobbyPanel.SetActive(false);
    }

    public void OnPlayerEnteredRoom()
    {
      lobbyIntro.SetActive(false);
      lobbyPanel.SetActive(true);
    }
    



    public async void StartGame()
    {
        string relayCode = await CreateRelay();

        Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinnedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                {"StartGame", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
            }
        });

        joinnedLobby = lobby;

        lobbyPanel.SetActive(false);

    }

}