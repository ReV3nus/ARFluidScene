using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetThickness : MonoBehaviour
{
    public RenderTexture thicknessTexture;

    void Start()
    {
        thicknessTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);
    }

    void OnDestroy()
    {
        if (thicknessTexture != null) {
            thicknessTexture.Release();
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest);
        Graphics.Blit(src, thicknessTexture, new Material(Shader.Find("Hidden/Thickness")));
    }
}
