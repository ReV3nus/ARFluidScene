using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiDistortion : MonoBehaviour
{
    public Material mapPass, flipPass;
    public Vector2 eyeOffset;
    public Camera mainCamera; 
    public RenderTexture renderTexture;
    public bool useAntiDistortion = true;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera.targetTexture = renderTexture;

    }

    // Update is called once per frame
    void Update()
    {
        float fps = 1.0f / Time.deltaTime;
        // 将帧数转换为字符串
        string fpsText = "FPS: " + ((int)fps).ToString();
        // 在屏幕上显示帧数
        // GUI.Label(new Rect(Screen.width - 100, 0, 100, 20), fpsText);
        Debug.LogWarning(fpsText);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // mapPass.SetFloat("_Mask", 0);
        if (useAntiDistortion)
        {
            mapPass.SetFloat("_EyeOffsetX", eyeOffset.x);
            mapPass.SetFloat("_EyeOffsetY", eyeOffset.y);
            Graphics.Blit(src, dest, mapPass, 0);
        }
        else
        {
            Graphics.Blit(src, dest, flipPass,0);
        }
        

    }
}
