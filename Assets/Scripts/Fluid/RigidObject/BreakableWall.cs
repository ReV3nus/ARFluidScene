using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.TextCore.Text;
using System.Reflection.Emit;
using static UnityEditor.PlayerSettings;

[CustomEditor(typeof(BreakableWall))]
public class BreakableWallEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BreakableWall script = (BreakableWall)target;

        if (GUILayout.Button("Regenerate Mesh"))
        {
            script.GenerateFragmentedMesh();
            script.BuildMeshFromList();
        }

        if (GUILayout.Button("Debug Breaking"))
        {
            script.GenerateFragmentedMesh();
            script.BuildMeshFromList();
            script.InitTidFromList();
            script.InitCS();
            script.BreakWall(new Vector3(20f, 0f, 20f), 5f);
        }

        if (GUILayout.Button("Debug Rasterization"))
        {
            script.GenerateFragmentedMesh();
            script.BuildMeshFromList();
            script.InitTidFromList();
            script.CheckRasterization();
        }
        if (GUILayout.Button("Debug"))
        {
            Debug.Log(script.gameObject.transform.up);
            Debug.Log(script.gameObject.transform.right);
            Debug.Log(script.gameObject.transform.forward);
            script.DebugTemp();
        }
    }
}

public class BreakableWall : MonoBehaviour
{
    public Vector2 size = new Vector2(10f, 10f);

    public int ExpectCount = 100;
    public float RasterizeResolution = 100;

    private float ExpectSize = 1f;

    private int[] tid;
    private int[] exist;


    struct DebrisTriangle
    {
        public Vector3 a, b, c;
        public int id;
        public DebrisTriangle setId(int i) { this.id = i;return this; }

        public DebrisTriangle(Vector3 aa, Vector3 bb, Vector3 cc)
        {
            a = aa; b = bb; c = cc;
            id = 0;
        }
        public float SignedArea()
        {
            Vector3 area = Vector3.Cross(b - a, c - a);
            if (area.x != 0 || area.z != 0)
                Debug.LogError("Error In Calculating Signed Area In BreakableWall.cs!");
            return area.y * 0.5f;
        }
        public float DisToPoint(Vector3 point)
        {
            Vector3 v0 = c - a;
            Vector3 v1 = b - a;
            Vector3 v2 = point - a;

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            if ((u >= 0) && (v >= 0) && (u + v < 1))
                return 0f;

            float dis1 = DistancePointToSegment(point, a, b);
            float dis2 = DistancePointToSegment(point, b, c);
            float dis3 = DistancePointToSegment(point, c, a);
            return Mathf.Min(dis1, dis2, dis3);
        }
        private static float DistancePointToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 ap = p - a;

            float abLengthSquared = Vector3.Dot(ab, ab);
            float projection = Vector3.Dot(ap, ab) / abLengthSquared;

            if (projection < 0)
            {
                return Vector3.Distance(p, a);
            }
            else if (projection > 1)
            {
                return Vector3.Distance(p, b);
            }
            else
            {
                Vector3 closestPoint = a + projection * ab;
                return Vector3.Distance(p, closestPoint);
            }
        }
    };
    struct Quad
    {
        // b d
        // a c
        public Vector3 a, b, c, d;
        public Quad(Vector3 aa, Vector3 bb, Vector3 cc, Vector3 dd)
        {
            a = aa; b = bb; c = cc; d = dd;
        }
        public float CalcArea()
        {
            Vector3 ad = d - a;
            Vector3 cb = b - c;
            return 0.5f * Vector3.Cross(ad, cb).magnitude;
        }
        public void Log()
        {
            Debug.Log(b + " " + d + "\n" + a + " " + c);
        }
    };
    private List<DebrisTriangle> debrisTriangles;
    private Queue<Quad> quads;

    private void Start()
    {
        GenerateFragmentedMesh();
        BuildMeshFromList();
        InitTidFromList();
        InitCS();
    }
    protected float NormalRandom()
    {
        System.Random random = new System.Random();
        float u1 = (float)random.NextDouble();
        float u2 = (float)random.NextDouble();
        float z0 = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
        return (0.5f + z0 / 6f);
    }
    
    public void GenerateFragmentedMesh()
    {
        ExpectSize = size.x * size.y / ExpectCount;

        debrisTriangles = new List<DebrisTriangle>();
        quads = new Queue<Quad>();
        quads.Enqueue(new Quad(new Vector3(0f, 0f, 0f),
                                new Vector3(0f, 0f, size.y),
                                new Vector3(size.x, 0f, 0f),
                                new Vector3(size.x, 0f, size.y)));


        while (quads.Count > 0)
        {
            
            Quad u = quads.Dequeue();
            Vector3 ac, ab, bd, cd, o;
            ac = Vector3.Lerp(u.a, u.c, NormalRandom());
            ab = Vector3.Lerp(u.a, u.b, NormalRandom());
            bd = Vector3.Lerp(u.b, u.d, NormalRandom());
            cd = Vector3.Lerp(u.c, u.d, NormalRandom());
            o = Vector3.Lerp(
                    Vector3.Lerp(u.a, u.c, NormalRandom()),
                    Vector3.Lerp(u.b, u.d, NormalRandom()),
                    NormalRandom()
                );

            //Debug.Log(u.b + " " + bd + " " + u.d + "\n" + ab + " " + o + " " + cd + "\n" + u.a + " " + ac + " " + u.c);

            Quad[] v = new Quad[4];
            v[0] = new Quad(u.a, ab, ac, o);
            v[1] = new Quad(ab, u.b, o, bd);
            v[2] = new Quad(o, bd, cd, u.d);
            v[3] = new Quad(ac, o, u.c, cd);

            foreach (Quad vv in v)
            {
                float area = vv.CalcArea();
                float rate = 1f;
                rate = 2f - area / ExpectSize;
                if (Random.value < rate)
                {
                    DebrisTriangle a1 = new DebrisTriangle(vv.a, vv.b, vv.c);
                    DebrisTriangle a2 = new DebrisTriangle(vv.c, vv.b, vv.d);
                    DebrisTriangle b1 = new DebrisTriangle(vv.a, vv.d, vv.c);
                    DebrisTriangle b2 = new DebrisTriangle(vv.a, vv.b, vv.d);

                    float adiff = Mathf.Abs(a1.SignedArea() - a2.SignedArea());
                    float bdiff = Mathf.Abs(b1.SignedArea() - b2.SignedArea());
                    int N = debrisTriangles.Count;
                    if(adiff > bdiff)
                    {
                        debrisTriangles.Add(b1.setId(N));
                        debrisTriangles.Add(b2.setId(N + 1));
                    }
                    else
                    {
                        debrisTriangles.Add(a1.setId(N));
                        debrisTriangles.Add(a2.setId(N + 1));
                    }
                }
                else
                {
                    quads.Enqueue(vv);
                }
            }
        }
    }

    void Swap(ref Vector3 v1, ref Vector3 v2)
    {
        Vector3 temp = v1;
        v1 = v2;
        v2 = temp;
    }
    public void InitTidFromList()
    {
        int N = debrisTriangles.Count;

        tid = new int[(int)(size.x * RasterizeResolution * size.y * RasterizeResolution)];
        exist = new int[N];

        for (int i = 0; i < N; i++)
        {
            exist[i] = 1;

            Vector3 a = debrisTriangles[i].a;
            Vector3 b = debrisTriangles[i].b;
            Vector3 c = debrisTriangles[i].c;
            if (a.x > b.x) Swap(ref a, ref b);
            if (a.x > c.x) Swap(ref a, ref c);
            if (b.z < c.z) Swap(ref b, ref c);
            float midX = Mathf.Min(b.x, c.x);
            float maxX = Mathf.Max(b.x, c.x);

            if (midX != a.x)
            {
                for (int xid = (int)(a.x * RasterizeResolution); (xid / RasterizeResolution <= midX) && xid < size.x * RasterizeResolution; xid++)
                {
                    float t = (xid / RasterizeResolution - a.x) / (b.x - a.x);
                    float y = Vector3.Lerp(a, b, t).z;
                    float ymax = y;
                    float ymin = y;
                    t = ((xid + 1f) / RasterizeResolution - a.x) / (b.x - a.x);
                    y = Vector3.Lerp(a, b, t).z;
                    ymax = Mathf.Max(ymax, y);ymin = Mathf.Min(ymin, y);

                    t = ((xid) / RasterizeResolution - a.x) / (c.x - a.x);
                    y = Vector3.Lerp(a, c, t).z;
                    ymax = Mathf.Max(ymax, y); ymin = Mathf.Min(ymin, y);
                    t = ((xid + 1f) / RasterizeResolution - a.x) / (c.x - a.x);
                    y = Vector3.Lerp(a, c, t).z;
                    ymax = Mathf.Max(ymax, y); ymin = Mathf.Min(ymin, y);

                    for (int yid = (int)(ymin * RasterizeResolution); (yid / RasterizeResolution <= ymax) && yid < size.y * RasterizeResolution; yid++)
                    {
                        tid[(int)(yid * RasterizeResolution * size.x + xid)] = i;
                    }
                }
            }

            if(midX == c.x) Swap(ref b, ref c);

            if (b.x != c.x)
            {
                for (int xid = (int)(midX * RasterizeResolution); (xid / RasterizeResolution <= maxX) && xid < size.x * RasterizeResolution; xid++)
                {
                    float t = (xid / RasterizeResolution - a.x) / (c.x - a.x);
                    float y = Vector3.Lerp(a, c, t).z;
                    float ymax = y;
                    float ymin = y;
                    t = ((xid + 1f) / RasterizeResolution - a.x) / (c.x - a.x);
                    y = Vector3.Lerp(a, c, t).z;
                    ymax = Mathf.Max(ymax, y); ymin = Mathf.Min(ymin, y);

                    t = ((xid) / RasterizeResolution - b.x) / (c.x - b.x);
                    y = Vector3.Lerp(b, c, t).z;
                    ymax = Mathf.Max(ymax, y); ymin = Mathf.Min(ymin, y);
                    t = ((xid + 1f) / RasterizeResolution - b.x) / (c.x - b.x);
                    y = Vector3.Lerp(b, c, t).z;
                    ymax = Mathf.Max(ymax, y); ymin = Mathf.Min(ymin, y);

                    for (int yid = (int)(ymin * RasterizeResolution); (yid / RasterizeResolution <= ymax) && yid < size.y * RasterizeResolution; yid++)
                    {
                        tid[(int)(yid * RasterizeResolution * size.x + xid)] = i;
                    }
                }
            }
        }

    }

    public void CheckRasterization()
    {
        Texture2D texture = new Texture2D((int)(size.x * RasterizeResolution), (int)(size.y * RasterizeResolution));
        int N = debrisTriangles.Count;

        for (int y = 0; y < (int)(size.y * RasterizeResolution); y++)
        {
            for (int x = 0; x < (int)(size.x * RasterizeResolution); x++)
            {
                int index = y * (int)(size.x * RasterizeResolution) + x;
                float value = (float)tid[index];
                Color color = new Color(value / N, value / N, value / N);
                texture.SetPixel(x, y, color);
            }
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        Renderer renderer = GetComponent<Renderer>();
        renderer.sharedMaterial.mainTexture = texture;
    }
    public void BuildMeshFromList()
    {
        Mesh mesh = new Mesh();
        mesh.name = "TempMesh";
        GetComponent<MeshFilter>().mesh = mesh;

        int N = debrisTriangles.Count;
        Vector3[] vertices = new Vector3[3 * N];
        for (int i = 0; i < N; i++)
        {
            vertices[3 * i + 0] = debrisTriangles[i].a;
            vertices[3 * i + 1] = debrisTriangles[i].b;
            vertices[3 * i + 2] = debrisTriangles[i].c;
        }

        int[] triangles = new int[3 * N];
        for (int i = 0; i < N; i++)
        {
            triangles[3 * i + 0] = 3 * i + 0;
            triangles[3 * i + 1] = 3 * i + 1;
            triangles[3 * i + 2] = 3 * i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        this.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void BreakWall(Vector3 point, float strength)
    {
        point = transform.InverseTransformPoint(point);
        List<DebrisTriangle> removeList = new List<DebrisTriangle>();
        foreach (DebrisTriangle triangle in debrisTriangles)
        {
            if (triangle.DisToPoint(point) <= strength)
            {
                removeList.Add(triangle);
                exist[triangle.id] = 0;
            }
        }
        foreach(DebrisTriangle triangle in removeList)
            debrisTriangles.Remove(triangle);
        if (removeList.Count > 0) BuildMeshFromList();

        existBuffer.SetData(exist);
        solverShader.SetBuffer(kernel, "breakableWallExist", existBuffer);

    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            BreakWall(new Vector3(0f, 5f, 0f), 2f);
        }
    }










    public Vector3 offset;

    private int kernel;
    public Solver solver;
    private ComputeShader solverShader;
    private int numParticles;

    private ComputeBuffer tidBuffer, existBuffer;
    public void InitCS()
    {
        this.solverShader = solver.solverShader;

        kernel = solverShader.FindKernel("CheckBreakableWall");
        solverShader.SetFloats("breakableWallPos", transform.position.x + offset.x, transform.position.y + offset.y, transform.position.z + offset.z);
        solverShader.SetFloats("breakableWallNormal", transform.up.x, transform.up.y, transform.up.z);
        solverShader.SetFloats("breakableWallX", transform.right.x, transform.right.y, transform.right.z);
        solverShader.SetFloats("breakableWallY", transform.forward.x, transform.forward.y, transform.forward.z);
        solverShader.SetFloats("breakableWallSize", size.x, size.y);
        solverShader.SetFloat("breakableWallResolution", RasterizeResolution);
        solverShader.SetInt("breakableTotalIndex", tid.Length);
        tidBuffer = new ComputeBuffer(tid.Length, sizeof(int));
        existBuffer = new ComputeBuffer(exist.Length, sizeof(int));
        tidBuffer.SetData(tid);
        existBuffer.SetData(exist);

        solverShader.SetBuffer(kernel, "breakableWallTid", tidBuffer);
        solverShader.SetBuffer(kernel, "breakableWallExist", existBuffer);
    }
    public void Solve(int x, int y, int z)
    {
        if (!enabled) return;
        solverShader.Dispatch(kernel, x, y, z);
    }
    public void DebugTemp()
    {
        Vector3 pos = new Vector3(1f, 2f, 19f);
        Vector3 plane2pos = pos - transform.position;

        int planeX = (int)(Vector3.Dot(plane2pos, transform.right) * RasterizeResolution);
        int planeY = (int)(Vector3.Dot(plane2pos, transform.forward) * RasterizeResolution);
        int tindex = ((int)(size.x * RasterizeResolution)) * planeY + planeX;

        int exists = exist[tid[tindex]];

        Debug.Log("orig:" + pos + " planePos:" + planeX + " " + planeY + "\ntindex is " + tindex + " exist: " + exists);

    }
}
