using UnityEngine;

[ExecuteInEditMode]
public class DepthAndTransparencyCapture : MonoBehaviour
{
    public Shader postProcessingShader;
    private Material postProcessingMaterial;

    void Start()
    {
        if (postProcessingShader == null)
        {
            Debug.LogError("Post-Processing Shader not assigned.");
            return;
        }
        postProcessingMaterial = new Material(postProcessingShader);
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, postProcessingMaterial);
    }
}
