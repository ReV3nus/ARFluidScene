using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InheritAndFlipCamera : MonoBehaviour
{
    void Start()
    {
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Material mat = new Material(Shader.Find("Custom/FlipShader"));
        Graphics.Blit(src, dest, mat);
    }
}