using NaughtyWaterBuoyancy;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace NaughtyWaterBuoyancy
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(MeshFilter))]
    public class ModifiedWaterVolume : MonoBehaviour
    {
        public const string TAG = "Water Volume";

        [SerializeField]
        private float density = 1f;

        private int rows = 0;
        private int columns = 0;
        private float quadSegmentSize = 0f;

        private Mesh mesh;
        private Vector3[] meshLocalVertices;
        private Vector3[] meshWorldVertices;

        public float Density
        {
            get
            {
                return this.density;
            }
        }

        public int Rows
        {
            get
            {
                return this.rows;
            }
        }

        public int Columns
        {
            get
            {
                return this.columns;
            }
        }

        public float QuadSegmentSize
        {
            get
            {
                return this.quadSegmentSize;
            }
        }

        public Mesh Mesh
        {
            get
            {
                if (this.mesh == null)
                {
                    this.mesh = this.GetComponent<MeshFilter>().mesh;
                }

                return this.mesh;
            }
        }

        protected virtual void Awake()
        {
            this.CacheMeshVertices();
        }

        protected virtual void Update()
        {
            this.CacheMeshVertices();
        }


        public Vector3[] GetSurroundingTrianglePolygon(Vector3 worldPoint)
        {
            Vector3 localPoint = this.transform.InverseTransformPoint(worldPoint);
            int x = Mathf.CeilToInt(localPoint.x / this.QuadSegmentSize);
            int z = Mathf.CeilToInt(localPoint.z / this.QuadSegmentSize);
            if (x <= 0 || z <= 0 || x >= (this.Columns + 1) || z >= (this.Rows + 1))
            {
                return null;
            }

            Vector3[] trianglePolygon = new Vector3[3];
            if ((worldPoint - this.meshWorldVertices[this.GetIndex(z, x)]).sqrMagnitude <
                ((worldPoint - this.meshWorldVertices[this.GetIndex(z - 1, x - 1)]).sqrMagnitude))
            {
                trianglePolygon[0] = this.meshWorldVertices[this.GetIndex(z, x)];
            }
            else
            {
                trianglePolygon[0] = this.meshWorldVertices[this.GetIndex(z - 1, x - 1)];
            }

            trianglePolygon[1] = this.meshWorldVertices[this.GetIndex(z - 1, x)];
            trianglePolygon[2] = this.meshWorldVertices[this.GetIndex(z, x - 1)];

            return trianglePolygon;
        }

        public Vector3[] GetClosestPointsOnWaterSurface(Vector3 worldPoint, int pointsCount)
        {
            MinHeap<Vector3> allPoints = new MinHeap<Vector3>(new Vector3HorizontalDistanceComparer(worldPoint));
            for (int i = 0; i < this.meshWorldVertices.Length; i++)
            {
                allPoints.Add(this.meshWorldVertices[i]);
            }

            Vector3[] closestPoints = new Vector3[pointsCount];
            for (int i = 0; i < closestPoints.Length; i++)
            {
                closestPoints[i] = allPoints.Remove();
            }

            return closestPoints;
        }

        public Vector3 GetSurfaceNormal(Vector3 worldPoint)
        {
            Vector3[] meshPolygon = this.GetSurroundingTrianglePolygon(worldPoint);
            if (meshPolygon != null)
            {
                Vector3 planeV1 = meshPolygon[1] - meshPolygon[0];
                Vector3 planeV2 = meshPolygon[2] - meshPolygon[0];
                Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;
                if (planeNormal.y < 0f)
                {
                    planeNormal *= -1f;
                }

                return planeNormal;
            }

            return this.transform.up;
        }

        public float GetWaterLevel(Vector3 worldPoint)
        {
            Vector3[] meshPolygon = this.GetSurroundingTrianglePolygon(worldPoint);
            if (meshPolygon != null)
            {
                Vector3 planeV1 = meshPolygon[1] - meshPolygon[0];
                Vector3 planeV2 = meshPolygon[2] - meshPolygon[0];
                Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;
                if (planeNormal.y < 0f)
                {
                    planeNormal *= -1f;
                }

                // Plane equation
                float yOnWaterSurface = (-(worldPoint.x * planeNormal.x) - (worldPoint.z * planeNormal.z) + Vector3.Dot(meshPolygon[0], planeNormal)) / planeNormal.y;
                //Vector3 pointOnWaterSurface = new Vector3(point.x, yOnWaterSurface, point.z);
                //DebugUtils.DrawPoint(pointOnWaterSurface, Color.magenta);

                return yOnWaterSurface;
            }

            return this.transform.position.y;
        }

        public bool IsPointUnderWater(Vector3 worldPoint)
        {
            return this.GetWaterLevel(worldPoint) - worldPoint.y > 0f;
        }

        private int GetIndex(int row, int column)
        {
            return row * (this.Columns + 1) + column;
        }

        private void CacheMeshVertices()
        {
            this.meshLocalVertices = this.Mesh.vertices;
            this.meshWorldVertices = this.ConvertPointsToWorldSpace(meshLocalVertices);

            this.quadSegmentSize = this.GetComponent<LiquidSimulator>().geometryCellSize;
            this.rows = Mathf.RoundToInt(this.GetComponent<LiquidSimulator>().liquidWidth / this.quadSegmentSize);
            this.columns = Mathf.RoundToInt(this.GetComponent<LiquidSimulator>().liquidLength / this.quadSegmentSize);
        }

        private Vector3[] ConvertPointsToWorldSpace(Vector3[] localPoints)
        {
            Vector3[] worldPoints = new Vector3[localPoints.Length];
            for (int i = 0; i < localPoints.Length; i++)
            {
                worldPoints[i] = this.transform.TransformPoint(localPoints[i]);
            }

            return worldPoints;
        }

        private class Vector3HorizontalDistanceComparer : IComparer<Vector3>
        {
            private Vector3 distanceToVector;

            public Vector3HorizontalDistanceComparer(Vector3 distanceTo)
            {
                this.distanceToVector = distanceTo;
            }

            public int Compare(Vector3 v1, Vector3 v2)
            {
                v1.y = 0;
                v2.y = 0;
                float v1Distance = (v1 - distanceToVector).sqrMagnitude;
                float v2Distance = (v2 - distanceToVector).sqrMagnitude;

                if (v1Distance < v2Distance)
                {
                    return -1;
                }
                else if (v1Distance > v2Distance)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
