using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiDistortion : MonoBehaviour
{
    public Material mapPass;
    public Vector2 eyeOffset;
    public Vector2 screenOffset;
    public Vector3 K_R;
    public Vector3 K_G;
    public Vector3 K_B;
    public float pixelsize;

    // Start is called before the first frame update
    void Start()
    {
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
        mapPass.SetFloat("pixelsize", pixelsize);
        mapPass.SetFloat("_EyeOffsetX", eyeOffset.x);
        mapPass.SetFloat("_EyeOffsetY", eyeOffset.y);
        mapPass.SetFloat("_ScreenOffsetX", screenOffset.x);
        mapPass.SetFloat("_ScreenOffsetY", screenOffset.y);
        mapPass.SetVector("_K_R", K_R);
        mapPass.SetVector("_K_G", K_G);
        mapPass.SetVector("_K_B", K_B);
        Graphics.Blit(src, dest, mapPass, 0);
    }
}
