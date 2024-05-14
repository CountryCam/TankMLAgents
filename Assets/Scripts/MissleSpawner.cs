using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MissleSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject misslePrefab;
    [SerializeField]
    private Transform missleTransform;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsOwner) 
        {
            SpawnMissleServerRpc(missleTransform.position, missleTransform.rotation);
        }
    }

    [ServerRpc]
    private void SpawnMissleServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject InstantiateMissle = Instantiate(misslePrefab, position, rotation);
        InstantiateMissle.GetComponent<NetworkObject>().Spawn();
    }
}
