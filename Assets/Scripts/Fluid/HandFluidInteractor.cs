using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Leap.Unity;

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
    }
    private void UpdateHandJoints()
    {
        totalJointsCount = leftHand._spherePositions.Length + rightHand._spherePositions.Length;
        if (joints.Length < totalJointsCount)
        {
            joints = new HandJoints[totalJointsCount];
        }        
        
        int index = 0;
        foreach (Vector3 pos in leftHand._spherePositions)
            joints[index++] = (new HandJoints(pos, leftHand._jointRadius));
        foreach (Vector3 pos in rightHand._spherePositions)
        {
            Debug.Log(pos);
            joints[index++] = (new HandJoints(pos*30, rightHand._jointRadius*100));
            Debug.Log(pos*30);
        }

        if (totalJointsCount > 0)
        {
            if (jointsBuffer == null || jointsBuffer.count < totalJointsCount)
            {
                if (jointsBuffer != null)
                {
                    jointsBuffer.Release();
                }
                jointsBuffer = new ComputeBuffer(totalJointsCount, sizeof(float) * 4);
            }
            else
            {
                jointsBuffer.SetData(joints);
            }
        
            solverShader.SetBuffer(kernel, "handJoints", jointsBuffer);
        }
    }

    private void OnEnable()
    {
        solverShader = solver.solverShader;
        kernel = solverShader.FindKernel("CheckHandJoints");
        solverShader.SetInt("jointsCount", totalJointsCount);
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
