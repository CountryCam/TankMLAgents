using UnityEngine;
using Unity.Netcode;

public class BulletHandler : NetworkBehaviour
{
    public float launchSpeed = 75.0f;
    public GameObject objectPrefab; // Assign your Bullet prefab here

    void Update()
    {
        if (IsOwner && Input.GetKeyDown("space"))
        {
            RequestSpawnBulletServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc]
    void RequestSpawnBulletServerRpc(ulong clientId)
    {
        SpawnBullet(clientId);
    }

    void SpawnBullet(ulong clientId)
    {
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation; // Use the rotation of the spawner

        GameObject newObject = Instantiate(objectPrefab, spawnPosition, spawnRotation);
        NetworkObject networkObject = newObject.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.SpawnWithOwnership(clientId);
        }
        else
        {
            Debug.LogError("Bullet prefab does not have a NetworkObject component.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDestroyBulletServerRpc(ulong networkObjectId)
    {
        NetworkObject networkObject;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out networkObject))
        {
            networkObject.Despawn();
        }
    }

    //void OnTriggerEnter(Collider collider)
    //{
    //    if (!IsServer) return;
    //    if (collider.GetComponent<BulletCollision>() && GetComponent<NetworkObject>().OwnerClientId != collider.GetComponent<NetworkObject>().OwnerClientId)
    //    {
    //        GetComponent<NetworkHealthState>().HealthPoints.Value -= 10;
    //    }
    //}
}
