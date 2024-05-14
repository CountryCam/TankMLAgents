using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class BulletCollision : NetworkBehaviour
{
    public float lifetime = 5.0f; // Lifetime of the bullet in seconds

    // ... existing Start method ...
    void Start()
    {
        // Schedule the bullet for destruction after 'lifetime' seconds
        Invoke(nameof(DestroyBullet), lifetime);
    }


    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[BulletCollision] Collision detected with: {collision.collider.name}");

        NetworkHealthState hitPlayerHealth = collision.collider.GetComponent<NetworkHealthState>();
        if (hitPlayerHealth != null)
        {
            Debug.Log($"[BulletCollision] Hit a player. IsServer: {IsServer}");
            // Always report collision to the server for processing
            ReportCollisionToServerServerRpc(hitPlayerHealth.GetComponent<NetworkObject>().NetworkObjectId, 10);
        }

        if (IsOwner)
        {
            var bulletHandler = FindObjectOfType<BulletHandler>();
            if (bulletHandler != null)
            {
                bulletHandler.RequestDestroyBulletServerRpc(GetComponent<NetworkObject>().NetworkObjectId);
                Debug.Log($"[BulletCollision] Request sent to destroy bullet owned by client.");
            }
        }
    }

    [ServerRpc]
    void ReportCollisionToServerServerRpc(ulong hitPlayerId, int damage)
    {
        if (!IsServer)
        {
            return;
        }

        // Find the NetworkObject with the given NetworkObjectId
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(hitPlayerId, out NetworkObject hitPlayerNetworkObject);

        if (hitPlayerNetworkObject != null)
        {
            // Get the NetworkHealthState component from the player NetworkObject
            NetworkHealthState hitPlayerHealth = hitPlayerNetworkObject.GetComponent<NetworkHealthState>();

            if (hitPlayerHealth != null)
            {
                // Apply damage to the player
                hitPlayerHealth.TakeDamageServerRpc(damage);
            }
            else
            {
                Debug.LogError("[BulletCollision] Hit player does not have a NetworkHealthState component.");
            }
        }
        else
        {
            Debug.LogError("[BulletCollision] No player found with the given NetworkObjectId.");
        }
    }



    void DestroyBullet()
    {
        // Only the server should execute this
        if (IsServer)
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Despawn();
                // Alternatively, use Destroy(gameObject) if you want to completely remove the object.
            }
        }
    }
}
