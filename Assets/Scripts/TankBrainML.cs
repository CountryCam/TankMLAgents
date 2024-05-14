using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Barracuda.Editor;
using static UnityEngine.GraphicsBuffer;

public class TankBrainML : Agent
{

    [SerializeField] private Transform env;
    [SerializeField] private Transform target;
    [SerializeField] private MeshRenderer groundRenderer;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-3.5f, -1.5f), 1.2f, Random.Range(-3.5f, 3.5f));
        target.localPosition = new Vector3(Random.Range(1.5f, 3.5f), 0.55f, Random.Range(-3.5f, 3.5f));

        env.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        transform.rotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)transform.localPosition);
        sensor.AddObservation((Vector2)target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Extract actions for movement and rotation
        float rotateAmount = actions.ContinuousActions[0]; // Use this for rotation
        float moveAmount = actions.ContinuousActions[1]; // Use this for forward/backward movement

        float movementSpeed = 5f;
        float rotationSpeed = 100f; // Adjust this value as needed for desired rotation speed

        // Rotate the tank around its y-axis
        transform.Rotate(0, rotateAmount * Time.deltaTime * rotationSpeed, 0);

        // Move the tank forward/backward
        transform.position += transform.forward * moveAmount * Time.deltaTime * movementSpeed;
    }



    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal"); // Now controls rotation
        continuousActions[1] = Input.GetAxisRaw("Vertical"); // Still controls forward/backward movement
    }



    private void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent(out Target target))
        {
            Debug.Log("HIT TARGET");
            AddReward(10f);
            groundRenderer.material.color = Color.green;
            
        }
        else if (collision.TryGetComponent(out Wall wall))
        {
            Debug.Log("HIT WALL");
            AddReward(-2f);
            groundRenderer.material.color = Color.red;
            
        }
    }

   


}
