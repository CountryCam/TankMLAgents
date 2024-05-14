using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static UnityEngine.GraphicsBuffer;
using Unity.PlasticSCM.Editor.WebApi;

public class PlayerTank :MonoBehaviour
{
    [SerializeField] private float speed = 70f;
    [SerializeField] private GameObject projectilePrefab; // Assign your projectile prefab here
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private MeshRenderer groundRenderer;
    public int playerHealth;
    private int maxPlayerHealth = 3;
    private List<Projectile> projectiles = new(); //List<Projectile>(); Same thing
    public Transform turret;

    public void Start()
    {
        playerHealth = maxPlayerHealth;
    }
    public void Update()
    {
        Debug.Log(Input.GetButton("Jump"));
        if (Input.GetKeyDown(KeyCode.Space))  // Check if the space bar is pressed
        {
            Shoot();
        }
    }
    private void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, shootingPoint.position, turret.rotation, transform.parent);
        Projectile projectilePlayer = projectile.GetComponent<Projectile>();
        projectiles.Add(projectilePlayer);
        projectilePlayer.OnHit += OnMissileHit;


        Rigidbody rb = projectile.GetComponent<Rigidbody>();


        if (rb != null)
        {
            rb.AddForce(turret.forward * 900);  // Adjust the force as necessary

        }
        else
        {
            Debug.LogError("Projectile Rigidbody is missing!");
            
        }


    }
    private void OnMissileHit(bool isTarget, Projectile projectile, GameObject hitObj)
    {
        projectile.OnHit -= OnMissileHit;
        projectiles.Remove(projectile);
        

        if (isTarget)
        {
            Debug.Log("Player: Hit target");
            groundRenderer.material.color = Color.green;
            SmoothTank tankAgent = hitObj.GetComponent<SmoothTank>();
            if (tankAgent != null)
            {
                tankAgent.TakeDamage();
            }

        }
        else
        {
            Debug.Log("Player: Missed");
            groundRenderer.material.color = Color.red;
        }
        return;
    }
    public void FirePlayerShoot()
    {

        GetComponent<Rigidbody>().velocity = this.transform.forward * speed;
    }

    public float CurrentHealth = 100f;
    public float Damage = 25f;
    public void TakeDamage()
    {
        CurrentHealth -= Damage;
        
        if (CurrentHealth <= 0){Destroy(gameObject);}
    }
    
}
