using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine.UI;

public class SmoothTank : Agent
{
    public Transform env;
    public Transform target;
    public MeshRenderer groundRenderer;
    public GameObject projectilePrefab;
    public Transform shootingPoint;
    public Transform obstacles;
    public float maxSpeed = 10f;
    public Slider slider;

    public Transform turret;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float shootingCooldown = 0.5f;
    private float timeSinceLastShot = 0.0f;
    private float episodeStartTime;
    private Vector3 lastPosition;
    private float stationaryTimeThreshold = 1.0f;
    private float moveThreshold = 3.0f;
    private float timeStationary = 0.0f;
    private Rigidbody agentRb;
    private List<Projectile> projectiles = new List<Projectile>();
    public float CurrentHealth = 500f; // Increased health
    public float Damage = 25f;

    private void Awake()
    {
        agentRb = GetComponent<Rigidbody>();
        ResetHealth();
    }

    private void ResetHealth()
    {
        CurrentHealth = 500f; // Increased health
        if (slider != null)
        {
            slider.value = CurrentHealth / 500f;
        }
        Debug.Log($"Health Reset: CurrentHealth: {CurrentHealth}, Slider Value: {slider?.value}");
    }

    public override void OnEpisodeBegin()
    {
        ResetHealth();

        // Set fixed positions for target and obstacles if needed
        // Remove random position setting
        // if (target != null)
        // {
        //     target.localPosition = new Vector3(Random.Range(-13f, 13f), 0.55f, Random.Range(-13f, 13f));
        // }
        // if (obstacles != null)
        // {
        //     obstacles.localPosition = new Vector3(Random.Range(-7f, 8f), 0.3f, Random.Range(-12f, 12f));
        //     obstacles.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        // }
        // if (env != null)
        // {
        //     env.localRotation = Quaternion.Euler(0, Random.Range(0f, 0f), 0);
        // }

        // Remove tank position setting
        // transform.localPosition = new Vector3(Random.Range(-13.5f, -10.5f), 0.3f, Random.Range(-13.5f, 13.5f));
        transform.rotation = Quaternion.identity;

        timeSinceLastShot = shootingCooldown;
        episodeStartTime = Time.time;
        lastPosition = transform.localPosition;
        timeStationary = 0.0f;
    }

    public void Update()
    {
        if (slider != null)
        {
            slider.gameObject.transform.LookAt(Camera.main.transform.position);
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);

        if (transform != null && lastPosition != null && Vector3.Distance(transform.localPosition, lastPosition) < moveThreshold)
        {
            timeStationary += Time.fixedDeltaTime;
        }
        else
        {
            timeStationary = 0.0f;
            if (transform != null)
            {
                lastPosition = transform.localPosition;
            }
        }

        if (timeStationary >= stationaryTimeThreshold)
        {
            AddReward(-1.1f);
            Debug.Log("Penalized for staying too long without significant movement.");
            timeStationary = 0.0f;
        }

        if (IsTargetVisible())
        {
            if (shootingPoint != null && target != null)
            {
                Debug.DrawRay(shootingPoint.position, (target.position - shootingPoint.position).normalized * 200f, Color.green);
            }
        }
        else
        {
            if (shootingPoint != null && target != null)
            {
                Debug.DrawRay(shootingPoint.position, (target.position - shootingPoint.position).normalized * 200f, Color.red);
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 targetDirection = GetDirectionToTarget();
        sensor.AddObservation(Vector3.Dot(turret.position, targetDirection));
        sensor.AddObservation((Vector2)transform.localPosition);

        if (target != null)
        {
            sensor.AddObservation((Vector2)target.localPosition);
        }
        else
        {
            sensor.AddObservation(Vector2.zero);
        }

        if (obstacles != null)
        {
            sensor.AddObservation((Vector2)obstacles.transform.position);
        }
        else
        {
            sensor.AddObservation(Vector2.zero);
        }

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

        float rotationSpeed = 50f;
        if (target != null)
        {
            target.localPosition += new Vector3(targetPosx, 0, targetPosz) * Time.deltaTime * 5;
        }
        transform.Rotate(0, rotateAmount * Time.deltaTime * rotationSpeed, 0);
        turret.Rotate(0, rotateTurrent * Time.deltaTime * rotationSpeed, 0);

        RotateTurretTowardsTarget();

        if (actions.DiscreteActions[0] == 4 && timeSinceLastShot >= shootingCooldown)
        {
            if (IsTargetVisible())
            {
                Shoot();
                timeSinceLastShot = 0f;
                float timeTaken = Time.time - episodeStartTime;
                float reward = Mathf.Clamp(5f - timeTaken, 1f, 5f);
                AddReward(reward);
            }
        }

        if (!IsWithinBounds(transform) || (target != null && !IsWithinBounds(target)))
        {
            Debug.Log("Object out of bounds, ending episode.");
        }

        timeSinceLastShot += Time.deltaTime;
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    private void Shoot()
    {
        if (shootingPoint == null)
        {
            Debug.LogError("Shooting point is not assigned.");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, shootingPoint.position, turret.rotation, transform.parent);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectiles.Add(projectileScript);
        projectileScript.OnHit += OnMissileHit;
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(turret.forward * 900);
        }
        else
        {
            Debug.LogError("Projectile Rigidbody is missing!");
        }
    }

    public void TakeDamage(float damage)
    {
        if (CurrentHealth <= 0)
        {
            Debug.LogWarning("TakeDamage called on an already dead tank.");
            return;
        }

        CurrentHealth -= damage;
        if (slider != null)
        {
            slider.value = CurrentHealth / 500f; // Adjusted for increased health
        }
        Debug.Log($"CurrentHealth: {CurrentHealth}, Slider Value: {slider?.value}");
        if (CurrentHealth <= 0)
        {
            Debug.Log("Tank destroyed");
            Die();
        }
    }

    private void OnMissileHit(bool isTarget, Projectile projectile, GameObject hitObj)
    {
        projectile.OnHit -= OnMissileHit;
        projectiles.Remove(projectile);

        if (isTarget)
        {
            AddReward(10f);
            Debug.Log("Plus 100 Target");
            groundRenderer.material.color = Color.green;
            PlayerTank tank = hitObj.GetComponent<PlayerTank>();
            if (tank != null)
            {
                tank.TakeDamage();
            }
        }
        else
        {
            AddReward(-1f);
            Debug.Log("Minus 1 Reward");
            groundRenderer.material.color = Color.black;
        }
    }

    bool IsTargetVisible()
    {
        if (target == null || shootingPoint == null) return false;

        Vector3 directionToTarget = target.position - shootingPoint.position;
        float maxDetectionDistance = 40f;
        RaycastHit hit;

        if (Physics.Raycast(shootingPoint.position, directionToTarget.normalized, out hit, maxDetectionDistance))
        {
            Debug.DrawRay(shootingPoint.position, directionToTarget.normalized * maxDetectionDistance, Color.blue);
            if (hit.collider.CompareTag("Target"))
            {
                return true;
            }
        }
        return false;
    }

    Vector3 GetDirectionToTarget()
    {
        if (target != null && shootingPoint != null)
        {
            return (target.position - shootingPoint.position).normalized;
        }
        return Vector3.zero;
    }

    void RotateTurretTowardsTarget()
    {
        if (target == null) return;

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
        actionsOut.DiscreteActions.Array[0] = Input.GetKey(KeyCode.Space) ? 4 : 0;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent(out Target target))
        {
            Debug.Log("Tank: HIT TARGET");
            AddReward(-1f);
            groundRenderer.material.color = Color.red;
        }
        else if (collision.TryGetComponent(out Wall wall))
        {
            Debug.Log("Tank: HIT WALL");
            AddReward(-10f);
            groundRenderer.material.color = Color.yellow;
        }
    }

    bool IsWithinBounds(Transform objectTransform)
    {
        float minBound = -15f;
        float maxBound = 15f;

        return objectTransform.localPosition.x >= minBound && objectTransform.localPosition.x <= maxBound &&
               objectTransform.localPosition.z >= minBound && objectTransform.localPosition.z <= maxBound;
    }

    void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TankDestroyed(this);
        }
        Destroy(gameObject);
    }
}
