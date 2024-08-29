using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Attributes;

public class HandFluidInteractor : MonoBehaviour
{
    public Solver solver;
    private int kernel;
    private ComputeShader solverShader;
    private int numParticles;
    private ComputeBuffer jointsBuffer;

    public CapsuleHand leftHand;
    public CapsuleHand rightHand;
    private int totalJointsCount;
    private float leftHandScale, rightHandScale;


    public struct HandJoints
    {
        public Vector3 centroid;
        public float radius;

        public HandJoints(Vector3 c, float r)
        {
            centroid = c;
            radius = r;
        }

        public void setCentroid(Vector3 pos)
        {
            centroid = pos;
        }
    }

    private HandJoints[] joints;

    private void Start()
    {
        // 在Start或Awake中初始化 jonts 数组
        int initJointsCount = leftHand._spherePositions.Length + rightHand._spherePositions.Length;
        joints = new HandJoints[initJointsCount];
        leftHandScale = leftHand.gameObject.transform.localScale.x;
        rightHandScale = rightHand.gameObject.transform.localScale.x;
    }
    private void UpdateHandJoints()
    {
        totalJointsCount = leftHand._spherePositions.Length + rightHand._spherePositions.Length;
        if (totalJointsCount > 0 && (joints == null || joints.Length < totalJointsCount))
        {
            joints = new HandJoints[totalJointsCount];
        }        
        
        int index = 0;
        foreach (Vector3 pos in leftHand._spherePositions)
            joints[index++] = (new HandJoints(pos, Mathf.Min(2,leftHand._jointRadius * 2f * rightHand.stretchFactor)));
        foreach (Vector3 pos in rightHand._spherePositions)
            joints[index++] = (new HandJoints(pos, Mathf.Min(2,rightHand._jointRadius * 2f * rightHand.stretchFactor)));

        if (totalJointsCount > 0)
        {
            if (jointsBuffer == null || jointsBuffer.count < totalJointsCount)
            {
                jointsBuffer?.Release();
                jointsBuffer = new ComputeBuffer(totalJointsCount, sizeof(float) * 4);
            }
            if(joints != null)
                jointsBuffer?.SetData(joints);
        
            solverShader.SetBuffer(kernel, "handJoints", jointsBuffer);
            // Debug.Log("Joints buffer size :" + joints.Length + " with first element:\ncentroid: " + joints[0].centroid + " radius:" + joints[0].radius);
        }
        solverShader.SetInt("jointsCount", totalJointsCount);
    }

    private void OnEnable()
    {
        solverShader = solver.solverShader;
        kernel = solverShader.FindKernel("CheckHandJoints");
        UpdateHandJoints();
    }

    private void Update()
    {
        UpdateHandJoints();
    }
    
    public void Solve(int x, int y, int z)
    {
        if (!enabled) return;
        solverShader.Dispatch(kernel, x, y, z);
    }
}
