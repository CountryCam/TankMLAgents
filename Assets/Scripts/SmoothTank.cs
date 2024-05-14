using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine.UI;

public class SmoothTank : Agent
{
    [SerializeField] private Transform env;
    [SerializeField] private Transform target;
    [SerializeField] private MeshRenderer groundRenderer;
    [SerializeField] private GameObject projectilePrefab; // Assign your projectile prefab here
    [SerializeField] private Transform shootingPoint; // Assign the shooting point here
    [SerializeField] private Transform obstacles;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private Slider slider;

    public Transform turret;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float shootingCooldown = 0.5f; // Time between shots
    private float timeSinceLastShot = 0.0f; // Time elapsed since the last shot
    private float episodeStartTime;
    private Vector3 lastPosition;
    private float stationaryTimeThreshold = 1.0f; // Time in seconds after which a penalty is applied
    private float moveThreshold = 3.0f; // Distance the agent must move to reset the timer
    private float timeStationary = 0.0f; // Timer to track movement
    private Rigidbody agentRb;
    private List<Projectile> projectiles = new List<Projectile>();
    public float CurrentHealth = 100f;
    public float Damage = 25f;

    private void Awake()
    {
        agentRb = GetComponent<Rigidbody>();
        slider.value = CurrentHealth / 100f;
    }
    private void Start()
    {

    }
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-13.5f, -10.5f), 0.3f, Random.Range(-13.5f, 13.5f));
        target.localPosition = new Vector3(Random.Range(-13f, 13f), 0.55f, Random.Range(-13f, 13f));
        obstacles.localPosition = new Vector3(Random.Range(-7f, 8f), 0.3f, Random.Range(-12f, 12f));

        env.localRotation = Quaternion.Euler(0, Random.Range(0f, 0f), 0);
        transform.rotation = Quaternion.identity;
        obstacles.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        timeSinceLastShot = shootingCooldown; // Reset the shooting cooldown
        episodeStartTime = Time.time;
        lastPosition = transform.localPosition;
        timeStationary = 0.0f; 
    }
    public void Update()
    {
        slider.gameObject.transform.LookAt(Camera.main.transform.position);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);
        if (Vector3.Distance(transform.localPosition, lastPosition) < moveThreshold)
        {
            timeStationary += Time.fixedDeltaTime; // Increase stationary time
        }
        else
        {
            timeStationary = 0.0f; // Reset if the agent has moved sufficiently
            lastPosition = transform.localPosition; // Update last known position
        }

        // Apply penalty if stationary time exceeds threshold
        if (timeStationary >= stationaryTimeThreshold)
        {
            AddReward(-1.1f); // Penalize the agent
            Debug.Log("Penalized for staying too long without significant movement.");
            timeStationary = 0.0f; // Reset to give the agent a chance to react
        }
        // For debugging: Draw a ray in the editor to show the shooting direction
        if (IsTargetVisible())
        {
            Debug.DrawRay(shootingPoint.localPosition, (target.localPosition - shootingPoint.localPosition).normalized * 200f, Color.green);
        }
        else
        {
            Debug.DrawRay(shootingPoint.localPosition, (target.localPosition - shootingPoint.localPosition).normalized * 200f, Color.red);
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 targetDirection = GetDirectionToTarget();
        sensor.AddObservation(Vector3.Dot(turret.position, targetDirection));
        sensor.AddObservation((Vector2)transform.localPosition);
        sensor.AddObservation((Vector2)target.localPosition);
        sensor.AddObservation((Vector2)obstacles.transform.position);
        sensor.AddObservation(agentRb.velocity.x);
        sensor.AddObservation(agentRb.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float rotateAmount = actions.ContinuousActions[0];
        targetSpeed = actions.ContinuousActions[1] * maxSpeed;
        float rotateTurrent = actions.ContinuousActions[2];
        float targetPosx = actions.ContinuousActions[3];
        float targetPosz = actions.ContinuousActions[4];

       //float movementSpeed = 7f;
        float rotationSpeed = 50f;
        target.localPosition += new Vector3(targetPosx,0, targetPosz) * Time.deltaTime * 5; 
        transform.Rotate(0, rotateAmount * Time.deltaTime * rotationSpeed, 0);
        //transform.position += transform.forward * moveAmount * Time.deltaTime * movementSpeed;
        turret.Rotate(0, rotateTurrent * Time.deltaTime * rotationSpeed, 0);

        RotateTurretTowardsTarget();
        // Shooting action
        Debug.Log("==> " + actions.DiscreteActions[0]);
        if (actions.DiscreteActions[0] == 4 && timeSinceLastShot >= shootingCooldown)
        {
            if (IsTargetVisible())
            {
                Shoot();
                timeSinceLastShot = 0f;
                float timeTaken = Time.time - episodeStartTime;
                float reward = Mathf.Clamp(5f - timeTaken, 1f, 5f);  // Encourage faster shooting
                AddReward(reward);
                //Debug.Log($"Shot taken in {timeTaken} seconds, reward: {reward}");
            }
            else
            {
                //AddReward(-0.5f);  // Penalize shooting when the target is not visible
            }
        }

        if (!IsWithinBounds(transform) || !IsWithinBounds(target))
        {
            Debug.Log("Object out of bounds, ending episode.");
            //EndEpisode();  // End the episode to reset positions
        }

        timeSinceLastShot += Time.deltaTime; // Ensure this line is in Update or Fixed Update if movement is continuous
        transform.position += transform.forward * currentSpeed * Time.deltaTime;

    }

    private void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, shootingPoint.position, turret.rotation, transform.parent);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectiles.Add(projectileScript);
        projectileScript.OnHit += OnMissileHit;
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
 
        if (rb != null )
        {
            rb.AddForce(turret.forward * 900);  // Adjust the force as necessary  
        }
        else
        {
            Debug.LogError("Projectile Rigidbody is missing!");
        } 
     
    }
    
    public void TakeDamage()
    {
        CurrentHealth -= Damage;
        slider.value = CurrentHealth / 100f;
        Debug.Log(CurrentHealth);
        if ( CurrentHealth <= 0 ) { Destroy(gameObject); }
    }

    private void OnMissileHit(bool isTarget, Projectile projectile, GameObject hitObj)
    {
        projectile.OnHit -= OnMissileHit;
        projectiles.Remove(projectile);
        //Debug.Log("==> " + projectile, this);
       
        if (isTarget)
        {
           //float distance = Vector3.Distance(transform.position, target.position);
           //float reward = Mathf.Clamp(distance / 10f, 1f, 5f); // Adjust the formula as needed
            AddReward(10f);
            Debug.Log("Plus 100 Target"); //$"Add Reward: {reward}"
            groundRenderer.material.color = Color.green;
            PlayerTank tank = hitObj.GetComponent<PlayerTank>();
            if(tank != null )
            {
                tank.TakeDamage();
            }
            //EndEpisode();
        }
        else
        {
            AddReward(-1f);
            Debug.Log("Minus 1 Reward");
            groundRenderer.material.color = Color.black;
        }
        //EndEpisode();
        return;
    }

    bool IsTargetVisible()
    {
        Vector3 directionToTarget = target.position - shootingPoint.position;
        float maxDetectionDistance = 40f;  // Set according to your game's scale
        RaycastHit hit;

        if (Physics.Raycast(shootingPoint.position, directionToTarget.normalized, out hit, maxDetectionDistance))
        {
            Debug.DrawRay(shootingPoint.position, directionToTarget.normalized * maxDetectionDistance, Color.blue);
            // Check if the ray hits an object tagged as 'Target'
            if (hit.collider.CompareTag("Target"))
            {
                return true;
            }
        }
        return false;
    }
    Vector3 GetDirectionToTarget()
    {
        return (target.position - shootingPoint.position).normalized;
    }

    void RotateTurretTowardsTarget()
    {
        float rotationSpeed = 300f;
        Vector3 targetDir = GetDirectionToTarget();
        Quaternion lookRotation = Quaternion.LookRotation(targetDir);
        
        lookRotation.x = 0;
        lookRotation.z = 0;

        turret.rotation = Quaternion.RotateTowards(turret.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
        continuousActionsOut[1] = Input.GetAxisRaw("Vertical");
        actionsOut.DiscreteActions.Array[0] = Input.GetKey(KeyCode.Space) ? 4 : 0; // Shoot with space bar
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent(out Target target))
        {
            Debug.Log("Tank: HIT TARGET");
            AddReward(-1f);
            groundRenderer.material.color = Color.red;
            //EndEpisode();

        }
        else if (collision.TryGetComponent(out Wall wall))
        {
            Debug.Log("Tank: HIT WALL");
            AddReward(-10f);
            groundRenderer.material.color = Color.yellow;
            //EndEpisode();
        }
    }

    bool IsWithinBounds(Transform objectTransform)
    {
        float minBound = -15f;
        float maxBound = 15f;

        // Check if the object's position is within the bounds
        return objectTransform.localPosition.x >= minBound && objectTransform.localPosition.x <= maxBound &&
               objectTransform.localPosition.z >= minBound && objectTransform.localPosition.z <= maxBound;
    }
}