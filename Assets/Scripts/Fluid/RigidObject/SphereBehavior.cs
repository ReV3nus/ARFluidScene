using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereBehavior : MonoBehaviour
{
    private RigidSphereScript sphere;
    private Rigidbody rb;

    private void OnEnable()
    {
        sphere = GetComponent<RigidSphereScript>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        sphere.Kinetic = false;
    }

    public void StartShooting()
    {
        sphere.Kinetic = false;
        rb.useGravity = false;
        StartCoroutine(Shooting());
    }

    private float rotationTime = 1.0f;
    private Vector3 rotationSpeed = new Vector3(0f, 0f, 5f);
    private Vector3 initPos = new Vector3(0f, 15f, 0f);
    private Vector3 shotVel = new Vector3(0f, 0f, 10f);
    
    IEnumerator Shooting()
    {
        rb.angularVelocity = rotationSpeed;
        yield return new WaitForSeconds(rotationTime);

        rb.velocity = shotVel;
        while (rb.position.x <= -80f)
            yield return new WaitForSeconds(0.5f);

        rb.velocity = Vector3.zero;
        rb.position = initPos;
        yield return null;
    }
}
