using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class ColorblindScreenEffect : MonoBehaviour
{
    private Material sourceMaterial;
    private Material runtimeMaterial;

    private float strength = 1f;

    // =========================================================
    // CONFIGURE
    // =========================================================

    public void Configure(
        Material material,
        float newStrength
    )
    {
        strength = Mathf.Clamp01(newStrength);

        // Ако няма материал, изчистваме ефекта.
        if (material == null)
        {
            sourceMaterial = null;
            DestroyRuntimeMaterial();
            return;
        }

        /*
         * ВАЖНО:
         * В старата версия, ако sourceMaterial беше същият,
         * функцията приключваше веднага.
         *
         * Ако runtimeMaterial по някаква причина вече е бил
         * унищожен, ефектът никога повече не се създаваше.
         */
        if (
            sourceMaterial == material &&
            runtimeMaterial != null
        )
        {
            UpdateStrength();
            return;
        }

        sourceMaterial = material;

        CreateRuntimeMaterial();
    }

    // =========================================================
    // CREATE MATERIAL
    // =========================================================

    private void CreateRuntimeMaterial()
    {
        DestroyRuntimeMaterial();

        if (sourceMaterial == null)
        {
            return;
        }

        if (
            sourceMaterial.shader == null ||
            !sourceMaterial.shader.isSupported
        )
        {
            Debug.LogWarning(
                "ColorblindScreenEffect: Shader-ът на материала не се поддържа."
            );

            return;
        }

        runtimeMaterial = new Material(sourceMaterial);

        runtimeMaterial.name =
            sourceMaterial.name + " (Runtime Color Filter)";

        runtimeMaterial.hideFlags =
            HideFlags.HideAndDontSave;

        UpdateStrength();
    }

    // =========================================================
    // UPDATE STRENGTH
    // =========================================================

    private void UpdateStrength()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        if (runtimeMaterial.HasProperty("_Strength"))
        {
            runtimeMaterial.SetFloat(
                "_Strength",
                strength
            );
        }
    }

    // =========================================================
    // RENDER
    // =========================================================

    private void OnRenderImage(
        RenderTexture source,
        RenderTexture destination
    )
    {
        // При липса на валиден материал просто показваме
        // оригиналната картина без никакви промени.
        if (
            runtimeMaterial == null ||
            runtimeMaterial.shader == null ||
            !runtimeMaterial.shader.isSupported
        )
        {
            Graphics.Blit(
                source,
                destination
            );

            return;
        }

        UpdateStrength();

        Graphics.Blit(
            source,
            destination,
            runtimeMaterial,
            0
        );
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        DestroyRuntimeMaterial();
    }

    private void DestroyRuntimeMaterial()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(runtimeMaterial);
        }
        else
        {
            DestroyImmediate(runtimeMaterial);
        }

        runtimeMaterial = null;
    }
}