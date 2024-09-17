using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class MaskGenerator : MonoBehaviour
{
    public Material maskPass;
    // public Camera mainCamera; 
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
    [Range(0.0f,1.0f)]
    public float transparency = 0.0f;
    [Range(0.0f,1.0f)]
    public float shadow = 0.0f;

    private TransparencyCapturer transparencyCapturer;

    // public RenderTexture[] delayrenderTextures;
    // public int frameCount = 10;
    private int currentFrame = 0;
    
    void Start()
    {
        transparencyCapturer = GetComponent<TransparencyCapturer>();
#if UNITY_EDITOR
        // 编辑器模式下的代码
        Debug.Log("Running in Unity Editor");
#else
        // 非编辑器模式下的代码（即在构建的游戏中）
        int display1Width = 1920;
        int display1Height = 1080;
        
        Debug.Log("Number of displays: " + Display.displays.Length);
        
        if (Display.displays.Length > 0)
        {
            Display.displays[0].Activate();
            Display.displays[0].SetRenderingResolution(display1Width, display1Width);
        }
        
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Display.displays[1].SetRenderingResolution(display1Width, display1Height);
        }
#endif
        // mainCamera.depthTextureMode |= DepthTextureMode.Depth;
        
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        transparencyCapturer?.CaptureTransparency();
        maskPass.SetFloat("_Thick", thick);
        maskPass.SetFloat("_Depth", depth);
        maskPass.SetFloat("_Bright", bright);
        maskPass.SetFloat("_Alpha", alpha);
        maskPass.SetFloat("_Color", color);
        maskPass.SetFloat("_Shadow", shadow);
        maskPass.SetFloat("_Transparency", transparency);
        
        Graphics.Blit(src, dest, maskPass, 0);
    }

}