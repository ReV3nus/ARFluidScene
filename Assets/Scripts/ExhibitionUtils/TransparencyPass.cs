using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class TransparencyPass : MonoBehaviour
{
    public RenderTexture transparencyTexture;
    public GameObject[] renderObjects;

    private Camera _camera;

    void OnEnable()
    {
        _camera = GetComponent<Camera>();
        if (transparencyTexture == null)
        {
            transparencyTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture.active = transparencyTexture;
        GL.Clear(true, true, Color.clear);

        // Render objects with transparency
        var cmd = new CommandBuffer { name = "Transparency Pass" };
        cmd.SetRenderTarget(transparencyTexture);
        cmd.ClearRenderTarget(true, true, Color.clear);

        foreach (var renderObject in renderObjects)
        {
            cmd.DrawRenderer(renderObject.GetComponent<Renderer>(), renderObject.GetComponent<Renderer>().sharedMaterial);
        }

        // Execute the command buffer
        Graphics.ExecuteCommandBuffer(cmd);
        RenderTexture.active = null;

        // Optionally copy to the destination texture
        Graphics.Blit(transparencyTexture, destination);
    }
}
