using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhibitionCamera : MonoBehaviour
{
    private void Start()
    {
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;
    }
}
