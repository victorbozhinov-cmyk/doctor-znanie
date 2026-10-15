using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class BrainTutorialTextSorting : MonoBehaviour
{
    [Header("Sorting Settings")]
    [SerializeField]
    private string sortingLayerName = "Default";

    [SerializeField]
    private int orderInLayer = 41;

    private void OnEnable()
    {
        ApplySorting();
    }

    private void OnValidate()
    {
        ApplySorting();
    }

    private void ApplySorting()
    {
        MeshRenderer meshRenderer =
            GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            return;

        meshRenderer.sortingLayerName =
            sortingLayerName;

        meshRenderer.sortingOrder =
            orderInLayer;
    }
}
