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
        
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // mapPass.SetFloat("_Mask", 0);
        if (useAntiDistortion)
        {
            mapPass.SetFloat("_EyeOffsetX", eyeOffset.x);
            mapPass.SetFloat("_EyeOffsetY", eyeOffset.y);
            Graphics.Blit(renderTexture, dest, mapPass, 0);
        }
        else
        {
            Graphics.Blit(renderTexture, dest, flipPass,0);
        }
        

    }
}
