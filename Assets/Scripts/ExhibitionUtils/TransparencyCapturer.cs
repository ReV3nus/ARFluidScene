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

    private void Awake()
    {
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;
        if (globalTransparencyTexture == null || globalTransparencyTexture.width != Screen.width || globalTransparencyTexture.height != Screen.height)
        {
            globalTransparencyTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        }
        opaqueShader = Shader.Find("ReV3nus/OpaqueTransparency");
        if (opaqueShader == null)
            Debug.LogError("[ReV]Cannot find opaque shader");
        opaqueMaterial = new Material(opaqueShader);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture.active = globalTransparencyTexture;
        GL.Clear(true, true, Color.clear);

        // Render objects with transparency
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

        Graphics.Blit(globalTransparencyTexture, destination);
    }
}
