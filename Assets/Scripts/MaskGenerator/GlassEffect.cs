using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlassEffect : MonoBehaviour
{
    public Camera reflectionCamera;
    public Cubemap reflectionCubemap;
    public int cubemapSize = 128;

    void Start()
    {
        if (reflectionCamera == null || reflectionCubemap == null)
        {
            Debug.LogError("Reflection camera or cubemap not assigned!");
            return;
        }

        reflectionCubemap = new Cubemap(cubemapSize, TextureFormat.RGB24, false);
        reflectionCamera.RenderToCubemap(reflectionCubemap);
    }
}