using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskGenerator : MonoBehaviour
{
    public Material maskPass;
    public Camera mainCamera; 
    public RenderTexture renderTexture;
    public Vector2 eyeOffset;
    public GetThickness thickness;
    public float thick = 1.0f;
    
    public RenderTexture[] delayrenderTextures;
    public int frameCount = 10;
    private int currentFrame = 0;
    
    void Start()
    {
        mainCamera.targetTexture = renderTexture;

        mainCamera.depthTextureMode |= DepthTextureMode.Depth;
        
        delayrenderTextures = new RenderTexture[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            delayrenderTextures[i] = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 24);
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        maskPass.SetFloat("_Thick", thick);

        Graphics.Blit(thickness.thicknessTexture, delayrenderTextures[currentFrame] ,maskPass);
        Debug.Log(currentFrame);
        Debug.Log((currentFrame+500)%frameCount);
        Graphics.Blit(delayrenderTextures[(currentFrame+500)%frameCount], dest ,maskPass);
        currentFrame = (currentFrame + 1) % frameCount; 
        // Graphics.Blit(thickness.thicknessTexture, dest, maskPass, 0);

    }
}
