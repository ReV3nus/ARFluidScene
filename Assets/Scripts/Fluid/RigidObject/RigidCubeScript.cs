using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class RigidCubeScript : MonoBehaviour
{
    public float halfSize;
    public float density = 1f;
    private float mass;
    // public float3 upY;

    private Rigidbody rb;

    [HideInInspector]
    public Vector3 totalImpulse, totalAngularImpulse;

    [HideInInspector]
    public RigidCube rigidCube;

    public bool UsingScaleAsSize;
    //public bool Kinetic = true;

    public void InitBeforeUpdate()
    {
        totalImpulse = Vector3.zero;
        totalAngularImpulse = Vector3.zero;
    }
    
    Vector4 GetPlaneEq(Vector3 p, Vector3 n) {
        return new Vector4(n.x, n.y, n.z, -Vector3.Dot(p, n));
    }
    private void OnEnable()
    {
        
        Vector4 upPlane = GetPlaneEq(transform.position + new Vector3(0, transform.localScale.y, 0), Vector3.up);   

        if (UsingScaleAsSize) halfSize = transform.localScale.x / 2f;
        var lengthWidthHeight = new Vector3(transform.localScale.x / 2f,transform.localScale.y / 2f,transform.localScale.z / 2f);
        rb = GetComponent<Rigidbody>();
        rigidCube = new RigidCube(halfSize, transform.position, transform.rotation,upPlane,lengthWidthHeight);
        mass = density * 8 * halfSize * halfSize * halfSize;
        rb.mass = mass;
    }

    public RigidCube UpdateState()
    {
        //if (!Kinetic)
        //{
        //    rb.velocity = Vector3.zero;
        //    rb.angularVelocity = Vector3.zero;
        //    rigidCube.centroid = rb.position;
        //    return rigidCube;
        //}
        //float dT = Time.deltaTime;
        //Vector3 pos = this.transform.position;

        ////Impulse
        //velocity += totalImpulse / mass;

        ////Angular
        //float inertia = 4f / 6f * mass * halfSize * halfSize;
        //Matrix4x4 InertiaTensor = Matrix4x4.identity;
        //InertiaTensor[0, 0] = inertia;
        //InertiaTensor[1, 1] = inertia;
        //InertiaTensor[2, 2] = inertia;
        //InertiaTensor = rigidCube.cubeRotationMatrix * InertiaTensor * rigidCube.inverseRotationMatrix;
        //Vector3 deltaAngularVelocity = InertiaTensor.inverse.MultiplyVector(totalAngularImpulse);

        //rigidCube.angularVelocity += deltaAngularVelocity;
        //Quaternion deltaRotation = Quaternion.Euler(rigidCube.angularVelocity * dT * Mathf.Rad2Deg);
        //transform.rotation = deltaRotation * transform.rotation;

        //rigidCube.cubeRotationMatrix = Matrix4x4.Rotate(transform.rotation);
        //rigidCube.inverseRotationMatrix = rigidCube.cubeRotationMatrix.inverse;

        ////Force
        //velocity += gravity * dT;
        //pos += velocity * dT;

        ////Check Bounds
        //if (pos.x < minBounds.x + halfSize)
        //{
        //    pos.x = minBounds.x + halfSize;
        //    if (velocity.x < 0f)
        //        velocity.x = 0f;
        //}
        //else if (pos.x > maxBounds.x - halfSize)
        //{
        //    pos.x = maxBounds.x - halfSize;
        //    if (velocity.x > 0f)
        //        velocity.x = 0f;
        //}
        //else if (pos.y < minBounds.y + halfSize)
        //{
        //    pos.y = minBounds.y + halfSize;
        //    if (velocity.y < 0f)
        //        velocity.y = 0f;
        //}
        //else if (pos.y > maxBounds.y - halfSize)
        //{
        //    pos.y = maxBounds.y - halfSize;
        //    if (velocity.y > 0f)
        //        velocity.y = 0f;
        //}
        //else if (pos.z < minBounds.z + halfSize)
        //{
        //    pos.z = minBounds.z + halfSize;
        //    if (velocity.z < 0f)
        //        velocity.z = 0f;
        //}
        //else if (pos.z > maxBounds.z - halfSize)
        //{
        //    pos.z = maxBounds.z - halfSize;
        //    if (velocity.z > 0f)
        //        velocity.z = 0f;
        //}

        rb.AddForce(totalImpulse, ForceMode.Impulse);
        rb.AddTorque(totalAngularImpulse, ForceMode.Impulse);

        rigidCube.upPlane =  GetPlaneEq(transform.position + new Vector3(0, transform.localScale.y-20, 0), Vector3.up);   
        rigidCube.velocity = rb.velocity;
        rigidCube.centroid = rb.position;
        rigidCube.angularVelocity = rb.angularVelocity;
        rigidCube.UpdateRotationMatrix(rb.rotation);

        return rigidCube;
    }
}