using UnityEngine;
using System.Collections;

public class ModifiedLiquidSampleObject : MonoBehaviour
{
    private Renderer m_Renderer;

    private Matrix4x4 m_LocalMatrix;

    private Vector3 prePos;
    private Quaternion preAngle;

    private float TotalTransition;

    private float lastRenderTime;

    private bool onSurface = false;

    void Start()
    {
        m_Renderer = gameObject.GetComponent<Renderer>();
        m_LocalMatrix = transform.localToWorldMatrix;
        prePos = transform.localPosition;
        preAngle = transform.localRotation;
        TotalTransition = 0f;
        lastRenderTime = Time.time;
    }

    void OnRenderObject()
    {
        if (!m_Renderer) return;

        //TotalTransition += (transform.position - prePos).magnitude * 1.0f;
        //TotalTransition += Quaternion.Angle(preAngle, transform.rotation) * 0.01f;

        //if(TotalTransition >= 0f)
        //{
            LiquidSimulator.DrawObject(m_Renderer);
        //    TotalTransition = 0f;
        //}

        //prePos = transform.position;
        //preAngle = transform.rotation;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.CompareTag("Water Surface"))
    //        onSurface = true;
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Water Surface"))
    //        onSurface = false;
    //}
}
