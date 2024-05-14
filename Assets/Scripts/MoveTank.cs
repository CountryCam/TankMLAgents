using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveTank : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 50.0f;
    public GameObject[] leftWheels;
    public GameObject[] rightWheels;

    public float wheelRotationSpeed = 200.0f;

    private Rigidbody rb;
    private float moveInput;
    private float rotationInput;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
       
    }
    // Update is called once per frame
    void Update()
    {
        //if (!IsOwner) return;
        moveInput = Input.GetAxisRaw("Vertical");
        rotationInput = Input.GetAxisRaw("Horizontal");

        RotateWheels(moveInput, rotationInput);
        //Debug.Log("Move Input: " + moveInput);
        //Debug.Log("Rigidbody Position: " + rb.position);
        //Debug.Log("Rigidbody Velocity: " + rb.velocity);
        

    }
    private void FixedUpdate()
    {
        MoveTankObj(moveInput);
        RotateTankObj(rotationInput);
    }

    void MoveTankObj(float input)
    {
        Vector3 moveDirection = transform.forward * input * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    void RotateTankObj(float input) 
    { 
        float rotation = input * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
    void RotateWheels(float moveInput, float rotationInput)
    {
        float wheelRotation = moveInput * wheelRotationSpeed * Time.deltaTime;

        //Left Wheels moving
        foreach (GameObject wheel in leftWheels) 
        { 
            if (wheel != null) 
            {
                wheel.transform.Rotate(wheelRotation - rotationInput * wheelRotationSpeed * Time.deltaTime, 0.0f, 0.0f);
            }
        }
        //Right Wheels moving
        foreach (GameObject wheel in rightWheels)
        {
            if (wheel != null)
            {
                wheel.transform.Rotate(wheelRotation + rotationInput * wheelRotationSpeed * Time.deltaTime, 0.0f, 0.0f);
            }
        }
    }

}
