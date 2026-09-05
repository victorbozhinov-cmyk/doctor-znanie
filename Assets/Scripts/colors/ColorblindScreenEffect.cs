using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class ColorblindScreenEffect : MonoBehaviour
{
    private Material filterMaterial;
    private float strength = 1f;

    public void Configure(Material material, float newStrength)
    {
        filterMaterial = material;
        strength = Mathf.Clamp01(newStrength);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (filterMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        filterMaterial.SetFloat("_Strength", strength);

        Graphics.Blit(
            source,
            destination,
            filterMaterial
        );
    }
}