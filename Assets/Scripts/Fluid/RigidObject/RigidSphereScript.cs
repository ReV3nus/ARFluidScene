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



    private Vector3 velocity;
    public Vector3 gravity = new Vector3(0f, -9.8f, 0f);
    [HideInInspector] public Vector3 totalImpulse;

    public bool UsingScaleAsSize;
    public bool Kinetic = true;


    [HideInInspector]
    public RigidSphere rigidSphere;
    
    
    public bool Shoot = false;
    public float vel;
    public float boundary ;
    public Vector3 initPos = new Vector3(0,15,0);
    private bool isShooting = false;

    public void InitBeforeUpdate()
    {
        totalImpulse = Vector3.zero;
    }

    private void Update()
    {
        // Debug.Log(transform.localPosition);
        // Debug.Log("---");
        // Debug.Log(transform.position);

        if (Shoot)
        {
            if (!isShooting)
            {
                StartCoroutine(Shooting());
            }

        }
    }

    IEnumerator Shooting()
    {
        isShooting = true;
        float rotationTime = 1.0f;

        float startTime = Time.time;

        float angle = 0f; 

        Vector3 rotationAxis = Vector3.up;

        while (Time.time < startTime + rotationTime)
        {
            float t = (Time.time - startTime) / rotationTime;

            angle = Mathf.Lerp(0f, 720f, t);

            transform.Rotate(rotationAxis * angle * Time.deltaTime);

            yield return null;
        }
        
        transform.Rotate(rotationAxis * (720f - angle));

        
        while (transform.localPosition.x <= boundary)
        {
            transform.localPosition += new Vector3(vel * Time.deltaTime, 0,0);
            yield return null;
        }
        
        transform.localPosition = initPos;
        isShooting = false;
    }


    private void OnEnable()
    {
        if (UsingScaleAsSize) radius = transform.localScale.x / 2f;
        rigidSphere = new RigidSphere(radius, transform.position);
        mass = density * 4f / 3f * Mathf.PI * radius * radius * radius;
        
        
    }

    public RigidSphere UpdateState(Vector3 minBounds, Vector3 maxBounds)
    {
        
        if (!Kinetic)
        {
            rigidSphere.centroid = transform.position;
            return rigidSphere;
        }
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
            // pos.x = maxBounds.x - radius;
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