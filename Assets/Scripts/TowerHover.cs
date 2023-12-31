using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerHover : MonoBehaviour
{

    float opacity = 0.2f;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        foreach (Material material in renderer.materials)
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            Color color = material.color;
            color.a = opacity;
            material.color = color;
        }
    }

}
