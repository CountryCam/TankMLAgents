using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI winLoseText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CheckGameConditions()
    {
        if (!IsServer) return;

        NetworkHealthState[] tanks = FindObjectsOfType<NetworkHealthState>();
        int activeTanks = 0;
        NetworkHealthState lastActiveTank = null;

        foreach (var tank in tanks)
        {
            if (tank.HealthPoint.Value > 0)
            {
                activeTanks++;
                lastActiveTank = tank;
            }
        }

        if (activeTanks == 1 && lastActiveTank != null)
        {
            NotifyWinClientRpc(lastActiveTank.OwnerClientId);
        }
        else if (activeTanks == 0)
        {
            // Still working on this one! sorry Hiren
        }
    }

    [ServerRpc]
    public void ReportTankDestructionServerRpc()
    {
        CheckGameConditions();
    }

    [ClientRpc]
    private void NotifyWinClientRpc(ulong winnerClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == winnerClientId)
        {
            Debug.Log("You Win!");
            winLoseText.text = "You Win!";

        }
        else
        {
            Debug.Log("You Lose!");
            winLoseText.text = "You Lose!";
        }
    }
}
