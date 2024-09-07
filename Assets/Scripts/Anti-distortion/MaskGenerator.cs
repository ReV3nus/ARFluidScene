using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskGenerator : MonoBehaviour
{
    public Material maskPass;
    public Vector2 eyeOffset;

    void Start()
    {
        this.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        maskPass.SetFloat("_EyeOffsetX", eyeOffset.x);
        maskPass.SetFloat("_EyeOffsetY", eyeOffset.y);
        Graphics.Blit(src, dest, maskPass,0);

    }
}
