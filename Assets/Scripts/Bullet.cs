using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float launchSpeed = 75.0f; // You can also make this a public property if you want to set it in the editor

    void Start()
    {
        if (IsOwner)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                Vector3 velocity = transform.forward * launchSpeed;
                rb.velocity = velocity;
            }
        }
    }
}
