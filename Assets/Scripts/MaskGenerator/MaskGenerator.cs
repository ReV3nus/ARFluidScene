using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskGenerator : MonoBehaviour
{
    public Material maskPass;
    // public Camera mainCamera; 
    public Vector2 eyeOffset;
    [Range(0.0f,1.0f)]
    public float thick = 1.0f;
    [Range(0.0f,1.0f)]
    public float depth = 0.0f;
    [Range(0.0f,20.0f)]
    public float bright = 0.4f;
    [Range(0.0f,1.0f)]
    public float alpha = 0.0f;
    [Range(0.0f,1.0f)]
    public float color = 0.0f;


    // public RenderTexture[] delayrenderTextures;
    // public int frameCount = 10;
    private int currentFrame = 0;
    
    void Start()
    {
        // mainCamera.depthTextureMode |= DepthTextureMode.Depth;
        //
        // delayrenderTextures = new RenderTexture[frameCount];
        // for (int i = 0; i < frameCount; i++)
        // {
        //     delayrenderTextures[i] = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 24);
        // }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        maskPass.SetFloat("_Thick", thick);
        maskPass.SetFloat("_Depth", depth);
        maskPass.SetFloat("_Bright", bright);
        maskPass.SetFloat("_Alpha", alpha);
        maskPass.SetFloat("_Color", color);
        
        maskPass.SetFloat("_EyeOffsetX", eyeOffset.x);
        maskPass.SetFloat("_EyeOffsetY", eyeOffset.y);
        
        Graphics.Blit(src, dest, maskPass, 0);

    }
}