using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InheritAndFlipCamera : MonoBehaviour
{
    public Camera mainCamera; 
    public RenderTexture renderTexture;
    void Start()
    {
        mainCamera.targetTexture = renderTexture;
    }
    

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Material mat = new Material(Shader.Find("Custom/FlipShader"));
        Graphics.Blit(renderTexture, dest, mat);
    }
}