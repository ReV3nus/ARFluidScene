using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Solver : MonoBehaviour
{
    
    public float timeToWait = 10.0f; // 设置时间值
    private float elapsedTime = 0.0f;
    const int numHashes = 1<<20;
    const int numThreads = 1<<10; // Compute shader dependent value.
    public int numParticles = 1024;
    public float radius = 1;
    public float gasConstant = 2000;
    public float restDensity = 10;
    public float mass = 1;
    // public float density = 1;
    public float viscosity = 0.01f;
    public float gravity = 9.8f;
    public float deltaTime = 0.001f;

    public Vector3 maxSpawns = new Vector3(-10, -10, -10);
    public Vector3 minSpawns = new Vector3(10, 10, 10);
    public Vector3 minBounds = new Vector3(-10, -10, -10);
    public Vector3 maxBounds = new Vector3(10, 10, 10);

    public ComputeShader solverShader;

    public Shader renderShader;
    public Material renderMat;

    public Mesh particleMesh;
    public float particleRenderSize = 0.5f;

    public Mesh sphereMesh;

    public Color primaryColor;
    public Color secondaryColor;

    private ComputeBuffer hashesBuffer;
    private ComputeBuffer globalHashCounterBuffer;
    private ComputeBuffer localIndicesBuffer;
    private ComputeBuffer inverseIndicesBuffer;
    private ComputeBuffer particlesBuffer;
    private ComputeBuffer sortedBuffer;
    private ComputeBuffer forcesBuffer;
    private ComputeBuffer groupArrBuffer;
    private ComputeBuffer hashDebugBuffer;
    private ComputeBuffer hashValueDebugBuffer;
    private ComputeBuffer meanBuffer;
    private ComputeBuffer covBuffer;
    private ComputeBuffer principleBuffer;
    private ComputeBuffer hashRangeBuffer;

    private ComputeBuffer quadInstancedArgsBuffer;
    private ComputeBuffer sphereInstancedArgsBuffer;

    private int solverFrame = 0;

    private int moveParticleBeginIndex = 0;
    public int moveParticles = 10;

    private double lastFrameTimestamp = 0;
    private double totalFrameTime = 0;

    private Vector4[] boxPlanes = new Vector4[7];

    private RigidObjSolver rigidObjSolver;

    public BreakableWall breakableWall;
    public HandFluidInteractor handInteractor;
    RenderTextureFormat _TextureFormat = RenderTextureFormat.ARGB32;

    struct Particle {
        public Vector4 pos; // with pressure.
        public Vector4 vel;
    }

    public bool paused = false;
    private bool usePositionSmoothing = true;

    private CommandBuffer commandBuffer;
    private Mesh screenQuadMesh;

    public Camera LeapCamera;
    public Camera[] renderCameras;

    Vector4 GetPlaneEq(Vector3 p, Vector3 n) {
        return new Vector4(n.x, n.y, n.z, -Vector3.Dot(p, n));
    }

    void UpdateParams() {

        boxPlanes[0] = GetPlaneEq(new Vector3(0, minBounds.y, 0), Vector3.up);
        boxPlanes[1] = GetPlaneEq(new Vector3(0, maxBounds.y, 0), Vector3.down);
        boxPlanes[2] = GetPlaneEq(new Vector3(minBounds.x, 0, 0), Vector3.right);
        boxPlanes[3] = GetPlaneEq(new Vector3(maxBounds.x, 0, 0), Vector3.left);
        boxPlanes[4] = GetPlaneEq(new Vector3(0, 0, minBounds.z), Vector3.forward);
        boxPlanes[5] = GetPlaneEq(new Vector3(0, 0, maxBounds.z), Vector3.back);

        solverShader.SetVectorArray("planes", boxPlanes);
        
    }
     
    void Start() {

        GameObject LeapProviderEsky =  GameObject.Find("LeapMotion");
        if(LeapProviderEsky != null){
            Debug.Log("Setting the hand models");
            LeapCamera = LeapProviderEsky.GetComponent<Camera>();
        }
        
        
        rigidObjSolver = GetComponent<RigidObjSolver>();

        Particle[] particles = new Particle[numParticles];

        for (int i = 0; i < numParticles; i++) {
            Vector3 pos = new Vector3(
                Random.Range(0f, 1f) * (maxSpawns.x - minSpawns.x) + minSpawns.x,
                Random.Range(0f, 1f) * (maxSpawns.y - minSpawns.y) + minSpawns.y,
                Random.Range(0f, 1f) * (maxSpawns.z - minSpawns.z) + minSpawns.z
            ); ;
            particles[i].pos = pos;
        }

        solverShader.SetInt("numHash", numHashes);
        solverShader.SetInt("numParticles", numParticles);

        solverShader.SetFloat("radiusSqr", radius * radius);
        solverShader.SetFloat("radius", radius);
        solverShader.SetFloat("gasConst", gasConstant);
        solverShader.SetFloat("restDensity", restDensity);
        solverShader.SetFloat("mass", mass);
        solverShader.SetFloat("viscosity", viscosity);
        solverShader.SetFloat("gravity", gravity);
        solverShader.SetFloat("deltaTime", deltaTime);


        float poly6 = 315f / (64f * Mathf.PI * Mathf.Pow(radius, 9f));
        float spiky = 45f / (Mathf.PI * Mathf.Pow(radius, 6f));
        float visco = 45f / (Mathf.PI * Mathf.Pow(radius, 6f));

        solverShader.SetFloat("poly6Coeff", poly6);
        solverShader.SetFloat("spikyCoeff", spiky);
        solverShader.SetFloat("viscoCoeff", visco * viscosity);

        UpdateParams();

        hashesBuffer = new ComputeBuffer(numParticles, 4);

        globalHashCounterBuffer = new ComputeBuffer(numHashes, 4);

        localIndicesBuffer = new ComputeBuffer(numParticles, 4);

        inverseIndicesBuffer = new ComputeBuffer(numParticles, 4);

        particlesBuffer = new ComputeBuffer(numParticles, 4 * 8);
        particlesBuffer.SetData(particles);

        sortedBuffer = new ComputeBuffer(numParticles, 4 * 8);

        forcesBuffer = new ComputeBuffer(numParticles * 2, 4 * 4);

        int groupArrLen = Mathf.CeilToInt(numHashes / 1024f);
        groupArrBuffer = new ComputeBuffer(groupArrLen, 4);

        hashDebugBuffer = new ComputeBuffer(4, 4);
        hashValueDebugBuffer = new ComputeBuffer(numParticles, 4 * 3);

        meanBuffer = new ComputeBuffer(numParticles, 4 * 4);
        covBuffer = new ComputeBuffer(numParticles * 2, 4 * 3);
        principleBuffer = new ComputeBuffer(numParticles * 4, 4 * 3);
        hashRangeBuffer = new ComputeBuffer(numHashes, 4 * 2);

        for (int i = 0; i < 17; i++) {
            solverShader.SetBuffer(i, "hashes", hashesBuffer);
            solverShader.SetBuffer(i, "globalHashCounter", globalHashCounterBuffer);
            solverShader.SetBuffer(i, "localIndices", localIndicesBuffer);
            solverShader.SetBuffer(i, "inverseIndices", inverseIndicesBuffer);
            solverShader.SetBuffer(i, "particles", particlesBuffer);
            solverShader.SetBuffer(i, "sorted", sortedBuffer);
            solverShader.SetBuffer(i, "forces", forcesBuffer);
            solverShader.SetBuffer(i, "groupArr", groupArrBuffer);
            solverShader.SetBuffer(i, "hashDebug", hashDebugBuffer);
            solverShader.SetBuffer(i, "mean", meanBuffer);
            solverShader.SetBuffer(i, "cov", covBuffer);
            solverShader.SetBuffer(i, "principle", principleBuffer);
            solverShader.SetBuffer(i, "hashRange", hashRangeBuffer);
            solverShader.SetBuffer(i, "hashValueDebug", hashValueDebugBuffer);
        }

        renderMat.SetBuffer("particles", particlesBuffer);
        renderMat.SetBuffer("principle", principleBuffer);
        renderMat.SetFloat("radius", particleRenderSize * 0.5f);

        quadInstancedArgsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);

        uint[] args = new uint[5];
        args[0] = particleMesh.GetIndexCount(0);
        args[1] = (uint) numParticles;
        args[2] = particleMesh.GetIndexStart(0);
        args[3] = particleMesh.GetBaseVertex(0);
        args[4] = 0;

        quadInstancedArgsBuffer.SetData(args);

        sphereInstancedArgsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);

        uint[] args2 = new uint[5];
        args2[0] = sphereMesh.GetIndexCount(0);
        args2[1] = (uint) numParticles;
        args2[2] = sphereMesh.GetIndexStart(0);
        args2[3] = sphereMesh.GetBaseVertex(0);
        args2[4] = 0;

        sphereInstancedArgsBuffer.SetData(args2);

        screenQuadMesh = new Mesh();
        screenQuadMesh.vertices = new Vector3[4] {
            new Vector3( 1.0f , 1.0f,  0.0f),
            new Vector3(-1.0f , 1.0f,  0.0f),
            new Vector3(-1.0f ,-1.0f,  0.0f),
            new Vector3( 1.0f ,-1.0f,  0.0f),
        };
        screenQuadMesh.uv = new Vector2[4] {
            new Vector2(1, 0),
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        screenQuadMesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };

        commandBuffer = new CommandBuffer();
        commandBuffer.name = "Fluid Render";

        foreach (var camera in renderCameras) {
            UpdateCommandBuffer(camera);
            camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
        }
    }

    void Update() {
        
        // elapsedTime += Time.deltaTime;
        //
        // if (elapsedTime >= timeToWait)
        // {
        //     return;
        // }
        
        
        // Update solver.
        {
            UpdateParams();

            if (Input.GetKeyDown(KeyCode.Space)) {
                paused = !paused;
            }

            if (Input.GetKeyDown(KeyCode.Z)) {
                usePositionSmoothing = !usePositionSmoothing;
                Debug.Log("usePositionSmoothing: " + usePositionSmoothing);
            }

            renderMat.SetColor("primaryColor", primaryColor.linear);
            renderMat.SetColor("secondaryColor", secondaryColor.linear);
            renderMat.SetInt("usePositionSmoothing", usePositionSmoothing ? 1 : 0);

            solverShader.Dispatch(solverShader.FindKernel("ResetCounter"), Mathf.CeilToInt((float)numHashes / numThreads), 1, 1);
            solverShader.Dispatch(solverShader.FindKernel("InsertToBucket"), Mathf.CeilToInt((float)numParticles / numThreads), 1, 1);

            solverShader.Dispatch(solverShader.FindKernel("PrefixSum1"), Mathf.CeilToInt((float)numHashes / numThreads), 1, 1);

            // @Important: Because of the way prefix sum algorithm implemented,
            // Currently maximum numHashes value is numThreads^2.
            Debug.Assert(numHashes <= numThreads*numThreads);
            solverShader.Dispatch(solverShader.FindKernel("PrefixSum2"), 1, 1, 1);

            solverShader.Dispatch(solverShader.FindKernel("PrefixSum3"), Mathf.CeilToInt((float)numHashes / numThreads), 1, 1);
            solverShader.Dispatch(solverShader.FindKernel("Sort"), Mathf.CeilToInt((float)numParticles / numThreads), 1, 1);
            solverShader.Dispatch(solverShader.FindKernel("CalcHashRange"), Mathf.CeilToInt((float)numHashes / numThreads), 1, 1);

            if (!paused) {
                for (int iter = 0; iter < 1; iter++) {
                    solverShader.Dispatch(solverShader.FindKernel("CalcPressure"), Mathf.CeilToInt((float)numParticles / 128), 1, 1);
                    solverShader.Dispatch(solverShader.FindKernel("CalcForces"), Mathf.CeilToInt((float)numParticles / 128), 1, 1);
                    solverShader.Dispatch(solverShader.FindKernel("CalcPCA"), Mathf.CeilToInt((float)numParticles / numThreads), 1, 1);
                    
                    handInteractor?.Solve(Mathf.CeilToInt((float)numParticles / numThreads), 1, 1);
                    solverShader.Dispatch(solverShader.FindKernel("CheckPlane"), Mathf.CeilToInt((float)numParticles / numThreads), 1, 1);
                    breakableWall?.Solve(Mathf.CeilToInt((float)numParticles / numThreads), 1, 1);
                    rigidObjSolver?.Solve(Mathf.CeilToInt((float)numParticles / numThreads), 1, 1);
                    solverShader.Dispatch(solverShader.FindKernel("Step"), Mathf.CeilToInt((float)numParticles / numThreads), 1, 1);
                }

                rigidObjSolver?.UpdateRigidObjects();

                solverFrame++;

                if (solverFrame > 1) {
                    totalFrameTime += Time.realtimeSinceStartupAsDouble - lastFrameTimestamp;
                }

                if (solverFrame == 400 || solverFrame == 1200) {
                    Debug.Log($"Avg frame time at #{solverFrame}: {totalFrameTime / (solverFrame-1) * 1000}ms.");
                }
            }

            lastFrameTimestamp = Time.realtimeSinceStartupAsDouble;
        }
    }

    void UpdateCommandBuffer(Camera camera) {
        commandBuffer.Clear();

        int lastColorId = Shader.PropertyToID("lastColor");
        commandBuffer.GetTemporaryRT(lastColorId, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        int lastDepthId = Shader.PropertyToID("lastDepth");
        commandBuffer.GetTemporaryRT(lastDepthId, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.RHalf);
        commandBuffer.Blit(Shader.GetGlobalTexture("_CameraDepthTexture"), lastDepthId);

        int[] worldPosBufferIds = new int[] {
            Shader.PropertyToID("worldPosBuffer1"),
            Shader.PropertyToID("worldPosBuffer2")
        };

        commandBuffer.GetTemporaryRT(worldPosBufferIds[0], Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        commandBuffer.GetTemporaryRT(worldPosBufferIds[1], Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);

        int depthId = Shader.PropertyToID("depthBuffer");
        commandBuffer.GetTemporaryRT(depthId, Screen.width, Screen.height, 32, FilterMode.Point, RenderTextureFormat.Depth);

        commandBuffer.SetRenderTarget((RenderTargetIdentifier)worldPosBufferIds[0], (RenderTargetIdentifier)depthId);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);

        commandBuffer.DrawMeshInstancedIndirect(
            sphereMesh,
            0,  // submeshIndex
            renderMat,
            0,  // shaderPass
            sphereInstancedArgsBuffer
        );

        int depth2Id = Shader.PropertyToID("depth2Buffer");
        commandBuffer.GetTemporaryRT(depth2Id, Screen.width, Screen.height, 32, FilterMode.Point, RenderTextureFormat.Depth);

        commandBuffer.SetRenderTarget((RenderTargetIdentifier)worldPosBufferIds[0], (RenderTargetIdentifier)depth2Id);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);

        commandBuffer.SetGlobalTexture("depthBuffer", depthId);

        commandBuffer.DrawMesh(
            screenQuadMesh,
            Matrix4x4.identity,
            renderMat,
            0, // submeshIndex
            1  // shaderPass
        );

        int normalBufferId = Shader.PropertyToID("normalBuffer");
        commandBuffer.GetTemporaryRT(normalBufferId, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);

        int colorBufferId = Shader.PropertyToID("colorBuffer");
        commandBuffer.GetTemporaryRT(colorBufferId, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.RGHalf);

        commandBuffer.SetRenderTarget(new RenderTargetIdentifier[] { normalBufferId, colorBufferId }, (RenderTargetIdentifier)depth2Id);
        commandBuffer.ClearRenderTarget(false, true, Color.clear);

        commandBuffer.SetGlobalTexture("worldPosBuffer", worldPosBufferIds[0]);

        commandBuffer.DrawMeshInstancedIndirect(
            particleMesh,
            0,  // submeshIndex
            renderMat,
            2,  // shaderPass
            quadInstancedArgsBuffer
        );

        // draw thickness
        int thicknessBufferId = Shader.PropertyToID("thicknessBuffer");
        commandBuffer.GetTemporaryRT(thicknessBufferId, Screen.width, Screen.height, 0, FilterMode.Point, RenderTextureFormat.RHalf);
        commandBuffer.SetRenderTarget((RenderTargetIdentifier)thicknessBufferId, (RenderTargetIdentifier)depth2Id);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);
        commandBuffer.DrawMeshInstancedIndirect(
            particleMesh,
            0,  // submeshIndex
            renderMat,
            4,  // shaderPass
            quadInstancedArgsBuffer
        );
        //
        //
        //
        // var _Material = new Material(Shader.Find("Hidden/SeparableGlassBlur"));
        // _Material.hideFlags = HideFlags.HideAndDontSave;
        //
        // if (camera.allowHDR && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR))
        //     _TextureFormat = RenderTextureFormat.DefaultHDR;
        //
        // int numIterations = 4;
        //
        // Vector2[] sizes = {
        //     new Vector2(Screen.width, Screen.height),
        //     new Vector2(Screen.width / 2, Screen.height / 2),
        //     new Vector2(Screen.width / 4, Screen.height / 4),
        //     new Vector2(Screen.width / 8, Screen.height / 8),
        // };
        //
        // for (int i = 0; i < numIterations; ++i)
        // {
        //     int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
        //     commandBuffer.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Bilinear, _TextureFormat);
        //     commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);
        //
        //     int blurredID = Shader.PropertyToID("_Grab" + i + "_Temp1");
        //     int blurredID2 = Shader.PropertyToID("_Grab" + i + "_Temp2");
        //     commandBuffer.GetTemporaryRT(blurredID, (int)sizes[i].x, (int)sizes[i].y, 0, FilterMode.Bilinear, _TextureFormat);
        //     commandBuffer.GetTemporaryRT(blurredID2, (int)sizes[i].x, (int)sizes[i].y, 0, FilterMode.Bilinear, _TextureFormat);
        //
        //     commandBuffer.Blit(screenCopyID, blurredID);
        //     commandBuffer.ReleaseTemporaryRT(screenCopyID);
        //
        //     commandBuffer.SetGlobalVector("offsets", new Vector4(2.0f / sizes[i].x, 0, 0, 0));
        //     commandBuffer.Blit(blurredID, blurredID2, _Material);
        //     commandBuffer.SetGlobalVector("offsets", new Vector4(0, 2.0f / sizes[i].y, 0, 0));
        //     commandBuffer.Blit(blurredID2, blurredID, _Material);
        //
        //     commandBuffer.SetGlobalTexture("_GrabBlurTexture_" + i, blurredID);
        // }

        // camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer);
        
        
        
        
        
        commandBuffer.SetGlobalTexture("thicknessBuffer", thicknessBufferId);

        // final
        commandBuffer.SetGlobalTexture("normalBuffer", normalBufferId);
        commandBuffer.SetGlobalTexture("colorBuffer", colorBufferId);

        commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

        commandBuffer.DrawMesh(
            screenQuadMesh,
            Matrix4x4.identity,
            renderMat,
            0, // submeshIndex
            3  // shaderPass
        );
    }

    void OnDisable() {
        hashesBuffer.Dispose();
        globalHashCounterBuffer.Dispose();
        localIndicesBuffer.Dispose();
        inverseIndicesBuffer.Dispose();
        particlesBuffer.Dispose();
        sortedBuffer.Dispose();
        forcesBuffer.Dispose();
        groupArrBuffer.Dispose();
        hashDebugBuffer.Dispose();
        meanBuffer.Dispose();
        covBuffer.Dispose();
        principleBuffer.Dispose();
        hashRangeBuffer.Dispose();

        quadInstancedArgsBuffer.Dispose();
    }
}
