using Unity.Netcode;
using UnityEngine;

public class NetworkHealthState : NetworkBehaviour
{
    public NetworkVariable<int> HealthPoint = new NetworkVariable<int>(100);

    private void Start()
    {
        HealthPoint.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (newValue <= 0 && IsServer)
        {
            GameManager.Instance.ReportTankDestructionServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (IsServer)
        {
            HealthPoint.Value = Mathf.Max(HealthPoint.Value - damage, 0);
        }
    }

    public override void OnNetworkDespawn()
    {
        HealthPoint.OnValueChanged -= OnHealthChanged;
        base.OnNetworkDespawn();
    }
}
