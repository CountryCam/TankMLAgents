using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Action<bool,Projectile,GameObject> OnHit;

    [SerializeField] private MeshRenderer groundRenderer;
    private float timer = 5f;
    //public int damage = 1;

    // This method is called when the projectile collides with another object.
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the projectile hits a target
        if (collision.gameObject.CompareTag("Target"))
        {
            Debug.Log("Projectile: HIT TARGET");
            OnHit?.Invoke(true, this, collision.gameObject);
            
        }
        // Check if the projectile hits a wall
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Projectile: HIT WALL");
            OnHit?.Invoke(false, this, collision.gameObject);
            
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Hit");
            OnHit?.Invoke(true, this, collision.gameObject);
        }

        // Destroy the projectile after it hits something
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        ;
        if ((timer -= 1 * Time.deltaTime) <= 0) 
        {
            Destroy(gameObject);
        }
    }

}
