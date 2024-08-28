using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;


public class RigidSphereScript : MonoBehaviour
{
    public float radius = 1f;
    public float density = 1f;
    private float mass;



    private Rigidbody rb;
    [HideInInspector] public Vector3 totalImpulse;

    public bool UsingScaleAsSize;
    public bool Kinetic = true;


    [HideInInspector]
    public RigidSphere rigidSphere;
    
    

    public void InitBeforeUpdate()
    {
        totalImpulse = Vector3.zero;
    }

    private void Update()
    {
        // Debug.Log(transform.localPosition);
        // Debug.Log("---");
        // Debug.Log(transform.position);
    }


    private void OnEnable()
    {
        if (UsingScaleAsSize) radius = transform.localScale.x / 2f;
        rigidSphere = new RigidSphere(radius, transform.position);
        rb = GetComponent<Rigidbody>();
        mass = density * 4f / 3f * Mathf.PI * radius * radius * radius;
        rb.mass = mass;
    }

    public RigidSphere UpdateState()
    {
        //Impulse
        if(Kinetic)
            rb.AddForce(totalImpulse, ForceMode.Impulse);

        rigidSphere.centroid = rb.position;
        rigidSphere.velocity = rb.velocity;
        return rigidSphere;
    }
}