using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RigidSphereScript : MonoBehaviour
{
    public float radius = 1f;
    public float density = 1f;
    private float mass;

    private Vector3 velocity;
    public Vector3 gravity = new Vector3(0f, -9.8f, 0f);
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
        mass = density * 4f / 3f * Mathf.PI * radius * radius * radius;
    }

    public RigidSphere UpdateState(Vector3 minBounds, Vector3 maxBounds)
    {
        float dT = Time.deltaTime;
        Vector3 pos = this.transform.position;

        //Impulse
        velocity += totalImpulse / mass;
        Debug.Log(totalImpulse / mass + " vel: " + velocity);
        //Force
        velocity += gravity * dT;

        pos += velocity * dT;

        //Check Bounds
        if(pos.x < minBounds.x + radius)
        {
            pos.x = minBounds.x + radius;
            if(velocity.x < 0f)
                velocity.x = 0f;
        }
        else if(pos.x > maxBounds.x - radius)
        {
            pos.x = maxBounds.x - radius;
            if (velocity.x > 0f)
                velocity.x = 0f;
        }
        else if (pos.y < minBounds.y + radius)
        {
            pos.y = minBounds.y + radius;
            if (velocity.y < 0f)
                velocity.y = 0f;
        }
        else if (pos.y > maxBounds.y - radius)
        {
            pos.y = maxBounds.y - radius;
            if (velocity.y > 0f)
                velocity.y = 0f;
        }
        else if (pos.z < minBounds.z + radius)
        {
            pos.z = minBounds.z + radius;
            if (velocity.z < 0f)
                velocity.z = 0f;
        }
        else if (pos.z > maxBounds.z - radius)
        {
            pos.z = maxBounds.z - radius;
            if (velocity.z > 0f)
                velocity.z = 0f;
        }

        transform.position = pos;
        rigidSphere.centroid = pos;
        rigidSphere.velocity = velocity;
        return rigidSphere;
    }
}