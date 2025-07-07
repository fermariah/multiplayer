using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class UIManager : NetworkBehaviour
{
    [SerializeField] private GameObject winnerUI;
    [SerializeField] private GameObject loserUI;

    public override void OnNetworkSpawn()
    {
        winnerUI.SetActive(false);
        loserUI.SetActive(false);
    }

    public void ShowWinnerUI()
    {
        winnerUI.SetActive(true);
        loserUI.SetActive(false);
        Debug.Log("Winner UI ativado");
    }

    public void ShowLoserUI()
    {
        winnerUI.SetActive(false);
        loserUI.SetActive(true);
        Debug.Log("Loser UI ativado");
    }
}
