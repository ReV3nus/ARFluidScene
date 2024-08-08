using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RigidSphereScript : MonoBehaviour
{
    public float radius = 1f;
    public float density = 1f;
    private float mass;

    private Rigidbody rb;
    [HideInInspector] public Vector3 totalImpulse;

    public bool UsingScaleAsSize;

    [HideInInspector]
    public RigidSphere rigidSphere;

    public void InitBeforeUpdate()
    {
        totalImpulse = Vector3.zero;
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
        rb.AddForce(totalImpulse, ForceMode.Impulse);

        rigidSphere.centroid = rb.position;
        rigidSphere.velocity = rb.velocity;
        return rigidSphere;
    }
}