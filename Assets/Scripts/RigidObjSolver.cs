using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidObjSolver : MonoBehaviour
{
    private Solver solver;

    public float DensityCoefficient = 10f;

    private int numParticles;
    private Vector3 minBounds = new Vector3(-10, -10, -10);
    private Vector3 maxBounds = new Vector3(10, 10, 10);
    private ComputeShader solverShader;
    private int StepKernel;

    public RigidSphereScript[] rigidSphereScripts;
    private RigidSphere[] rigidSpheres;
    public RigidCubeScript[] rigidCubeScripts;
    private RigidCube[] rigidCubes;
    private int cntCube, cntSphere;

    private ComputeBuffer CubeBuffer, SphereBuffer;
    private ComputeBuffer ImpulseBuffer, IndexBuffer;
    private ComputeBuffer LastIndexBuffer; // 2N sphere, 2N+1 cube

    private Vector3[] impulses;
    private int[] indices;
    private int[] LastIndex;

    private void Start()
    {
        solver = GetComponent<Solver>();
        solverShader = solver.solverShader;
        numParticles = solver.numParticles;
        minBounds = solver.minBounds;
        maxBounds = solver.maxBounds;

        cntCube = rigidCubeScripts.Length;
        cntSphere = rigidSphereScripts.Length;

        solverShader.SetInt("cntCube", cntCube);
        solverShader.SetInt("cntSphere", cntSphere);

        StepKernel = solverShader.FindKernel("Step");

        //otherwise crashes
        CubeBuffer = new ComputeBuffer(Math.Max(1, cntCube), 42 * sizeof(float));
        if (cntCube > 0)
        {
            rigidCubes = new RigidCube[cntCube];
            for (int i = 0; i < cntCube; i++)
                rigidCubes[i] = rigidCubeScripts[i].rigidCube;
            CubeBuffer.SetData(rigidCubes);
        }

        SphereBuffer = new ComputeBuffer(Math.Max(1, cntSphere), 7 * sizeof(float));
        if (cntSphere > 0)
        {
            rigidSpheres = new RigidSphere[cntSphere];
            for (int i = 0; i < cntSphere; i++)
                rigidSpheres[i] = rigidSphereScripts[i].rigidSphere;
            SphereBuffer.SetData(rigidSpheres);
        }

        int ArrayLength = numParticles >> 1;
        impulses = new Vector3[ArrayLength];
        indices = new int[ArrayLength];
        LastIndex = new int[1];
        LastIndex[0] = 0;

        ImpulseBuffer = new ComputeBuffer(ArrayLength, 3 * sizeof(float));
        IndexBuffer = new ComputeBuffer(ArrayLength, sizeof(int));
        LastIndexBuffer = new ComputeBuffer(1, sizeof(int));

        ImpulseBuffer.SetData(impulses);
        IndexBuffer.SetData(indices);
        LastIndexBuffer.SetData(LastIndex);

        solverShader.SetBuffer(StepKernel, "rigidImpulses", ImpulseBuffer);
        solverShader.SetBuffer(StepKernel, "rigidIndices", IndexBuffer);
        solverShader.SetBuffer(StepKernel, "rigidLastIndex", LastIndexBuffer);
        solverShader.SetBuffer(StepKernel, "rigidCubes", CubeBuffer);
        solverShader.SetBuffer(StepKernel, "rigidSpheres", SphereBuffer);

    }

    public void UpdateRigidObjects()
    {
        ImpulseBuffer.GetData(impulses);
        IndexBuffer.GetData(indices);
        LastIndexBuffer.GetData(LastIndex);
        Debug.Log(LastIndex[0]);


        for (uint i = 0; i < cntCube; i++) rigidCubeScripts[i].InitBeforeUpdate();
        for (uint i = 0; i < cntSphere; i++) rigidSphereScripts[i].InitBeforeUpdate();

        for (uint i = 0; i < LastIndex[0]; i++)
        {
            if (indices[i] < 0)
                rigidCubeScripts[(-indices[i]) >> 1].totalAngularImpulse += impulses[i] / DensityCoefficient;
            else if ((indices[i] & 1) == 0)
                rigidSphereScripts[indices[i] >> 1].totalImpulse += impulses[i] / DensityCoefficient;
            else
                rigidCubeScripts[indices[i] >> 1].totalImpulse += impulses[i] / DensityCoefficient;
        }


        if (cntCube > 0)
        {
            for (uint i = 0; i < cntCube; i++)
                rigidCubes[i] = rigidCubeScripts[i].UpdateState(minBounds, maxBounds);
            CubeBuffer.SetData(rigidCubes);
            solverShader.SetBuffer(StepKernel, "rigidCubes", CubeBuffer);
        }
        if (cntSphere > 0)
        {
            for (uint i = 0; i < cntSphere; i++)
                rigidSpheres[i] = rigidSphereScripts[i].UpdateState(minBounds, maxBounds);
            SphereBuffer.SetData(rigidSpheres);
            solverShader.SetBuffer(StepKernel, "rigidSpheres", SphereBuffer);
        }

        LastIndex[0] = 0;
        LastIndexBuffer.SetData(LastIndex);
        Debug.Log(LastIndex[0]);

    }
}
