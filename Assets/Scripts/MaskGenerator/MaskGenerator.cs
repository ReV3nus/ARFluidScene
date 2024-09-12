using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
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

    private TransparencyCapturer transparencyCapturer;
    public AnimationCurve transCurve = AnimationCurve.Linear(0, 0, 1, 1);
    private Texture2D curveTexture;


    // public RenderTexture[] delayrenderTextures;
    // public int frameCount = 10;
    private int currentFrame = 0;
    
    void Start()
    {
        transparencyCapturer = GetComponent<TransparencyCapturer>();
        transparencyCapturer.InitCapturer();

        GenerateCurveTexture();

        // mainCamera.depthTextureMode |= DepthTextureMode.Depth;
        //
        // delayrenderTextures = new RenderTexture[frameCount];
        // for (int i = 0; i < frameCount; i++)
        // {
        //     delayrenderTextures[i] = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 24);
        // }


        maskPass.SetFloat("_Thick", thick);
        maskPass.SetFloat("_Depth", depth);
        maskPass.SetFloat("_Bright", bright);
        maskPass.SetFloat("_Alpha", alpha);
        maskPass.SetFloat("_Color", color);

        maskPass.SetFloat("_EyeOffsetX", eyeOffset.x);
        maskPass.SetFloat("_EyeOffsetY", eyeOffset.y);

    }

    private void OnValidate()
    {
        GenerateCurveTexture();
    }
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        transparencyCapturer.CaptureTransparency();
        Graphics.Blit(src, dest, maskPass, 0);

    }
    void GenerateCurveTexture()
    {
        curveTexture = new Texture2D(256, 1, TextureFormat.RFloat, false);
        curveTexture.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < 256; i++)
        {
            float t = (float)i / (256 - 1);
            float curveValue = transCurve.Evaluate(t);
            curveTexture.SetPixel(i, 0, new Color(curveValue, 0, 0, 1));
        }

        curveTexture.Apply();
        maskPass.SetTexture("_CurveTex", curveTexture);
    }
}