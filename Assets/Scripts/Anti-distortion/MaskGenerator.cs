using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskGenerator : MonoBehaviour
{
    public Material maskPass;
    public Camera mainCamera; 
    public RenderTexture renderTexture;

    public bool mask = true;

    void Start()
    {
        mainCamera.targetTexture = renderTexture;

        this.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        float m;
        if (mask) {
            m = 1;
        }
        else {
            m = 0;
        }
        maskPass.SetFloat("_Mask", m);
        Graphics.Blit(renderTexture, dest, maskPass,0);

    }
}
