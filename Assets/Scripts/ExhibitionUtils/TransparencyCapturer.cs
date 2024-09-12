using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class TransparencyCapturer : MonoBehaviour
{
    public RenderTexture globalTransparencyTexture;
    public GameObject[] opaqueObjects, transparentObjects;
    private Shader opaqueShader;
    private Material opaqueMaterial;


    //void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    CaptureTransparency();
    //    Graphics.Blit(globalTransparencyTexture, destination);
    //}

    public void InitCapturer()
    {
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;
        opaqueShader = Shader.Find("ReV3nus/OpaqueTransparency");
        if (opaqueShader == null)
            Debug.LogError("[ReV]Cannot find opaque shader");
        opaqueMaterial = new Material(opaqueShader);

        globalTransparencyTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        Shader.SetGlobalTexture("_GlobalTransparencyTexture", globalTransparencyTexture);
    }

    public void CaptureTransparency()
    {
        RenderTexture.active = globalTransparencyTexture;

        var cmd = new CommandBuffer { name = "Transparency Pass" };
        cmd.SetRenderTarget(globalTransparencyTexture);
        cmd.ClearRenderTarget(true, true, Color.white);

        foreach (var renderObject in opaqueObjects)
        {
            cmd.DrawRenderer(renderObject.GetComponent<Renderer>(), opaqueMaterial);
        }

        foreach (var renderObject in transparentObjects)
        {
            Renderer renderer = renderObject.GetComponent<Renderer>();
            if (renderer == null) continue;
            Material mat = renderer.sharedMaterial;
            int passIndex = mat.FindPass("TransparencyCapturePass");
            if (passIndex != -1)
            {
                cmd.DrawRenderer(renderer, mat, 0, passIndex);
            }
        }

        Graphics.ExecuteCommandBuffer(cmd);
        RenderTexture.active = null;
    }
}
