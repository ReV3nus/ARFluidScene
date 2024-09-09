using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskGenerator : MonoBehaviour
{
    public Material maskPass;

    int display1Width = 1920;
    int display1Height = 1080;
    int display2Width = 5120;
    int display2Height = 2560;

    void Start()
    {
        this.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

        Display.displays[0].Activate();
        Display.displays[0].SetRenderingResolution(display2Width, display2Height);
        
        Display.displays[1].Activate();
        Display.displays[1].SetRenderingResolution(display1Width, display1Height);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, maskPass, 0);
    }
}
