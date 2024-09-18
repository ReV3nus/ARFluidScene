using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RigidCube
{
    public float halfSize;
    
    public Vector4 upPlane;

    public Vector3 lengthWidthHeight;

    public Vector3 centroid;

    public Vector3 velocity;
    public Vector3 angularVelocity;

    public Matrix4x4 cubeRotationMatrix;
    public Matrix4x4 inverseRotationMatrix;

    // public RigidCube(float size, Vector3 centroid, Quaternion rotation)
    // {
    //     this.halfSize = size;
    //     this.centroid = centroid;
    //     cubeRotationMatrix = Matrix4x4.Rotate(rotation);
    //     inverseRotationMatrix = cubeRotationMatrix.inverse;
    //     velocity = Vector3.zero;
    //     angularVelocity = Vector3.zero;
    //     lengthWidthHeight = Vector3.zero;
    // }
    public RigidCube(float size, Vector3 centroid, Quaternion rotation,Vector4 upPlane,Vector3 lengthWidthHeight)
    {
        this.halfSize = size;
        this.upPlane = upPlane;
        // Debug.Log(size);
        // Debug.Log(_lengthWidthHeight);
        // Debug.Log(centroid);
        this.centroid = centroid;
        this.lengthWidthHeight = lengthWidthHeight;
        cubeRotationMatrix = Matrix4x4.Rotate(rotation);
        inverseRotationMatrix = cubeRotationMatrix.inverse;
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
    }

    public void UpdateRotationMatrix(Quaternion rotation)
    {
        cubeRotationMatrix = Matrix4x4.Rotate(rotation);
        inverseRotationMatrix = cubeRotationMatrix.inverse;
    }
}

public struct RigidSphere
{
    public float radius;
    public Vector3 centroid;
    public Vector3 velocity;
    public RigidSphere(float radius, Vector3 centroid)
    {
        this.radius = radius;
        this.centroid = centroid;
        velocity = Vector3.zero;
    }
}