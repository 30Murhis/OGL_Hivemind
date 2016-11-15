using UnityEngine;

[ExecuteInEditMode]
public class WorldBender : MonoBehaviour {

    public bool enableWorldBending = true;
    public Material material;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (enableWorldBending)
            Graphics.Blit(src, dest, material);
    }
}
