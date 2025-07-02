using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SimpleLobbyManager : NetworkBehaviour
{
    [Header("Configurações")]
    public string gameSceneName = "GameScene"; // Nome da sua cena de jogo

    private Lobby hostedLobby;
    private bool isHost;

    // Chamado quando o Host clica em "Start Game"
    public async void StartGame()
    {
        if (!isHost) return;

        // 1. Criar conexão Relay
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // 2. Configurar NetworkManager
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData);

        // 3. Iniciar o Host
        NetworkManager.Singleton.StartHost();

        // 4. Atualizar lobby com código de conexão
        await Lobbies.Instance.UpdateLobbyAsync(hostedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> {
                {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode)}
            }
        });

        // 5. Carregar cena do jogo para todos
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    // Chamado quando Player entra como Host
    public async void CreateLobby()
    {
        hostedLobby = await Lobbies.Instance.CreateLobbyAsync("MeuLobby", 4);
        isHost = true;
        // Verifica quando novos players entram
        InvokeRepeating(nameof(PollLobby), 1.1f, 1.1f);
    }

    // Chamado quando Player entra como Cliente
    public async void JoinLobby(string code)
    {
        hostedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code);
        isHost = false;
        // Verifica se o host iniciou o jogo
        InvokeRepeating(nameof(PollLobby), 1.1f, 1.1f);
    }

    // Verifica atualizações do lobby
    private async void PollLobby()
    {
        hostedLobby = await Lobbies.Instance.GetLobbyAsync(hostedLobby.Id);

        // Se for cliente e houver código Relay, conecta
        if (!isHost && hostedLobby.Data.ContainsKey("RelayCode"))
        {
            await JoinRelay(hostedLobby.Data["RelayCode"].Value);
        }
    }

    // Conexão do Cliente ao Relay
    private async Task JoinRelay(string joinCode)
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData);

        NetworkManager.Singleton.StartClient();
    }
}