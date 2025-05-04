using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EnableDepthTexture : MonoBehaviour
{
    void Start()
    {
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;
    }
}