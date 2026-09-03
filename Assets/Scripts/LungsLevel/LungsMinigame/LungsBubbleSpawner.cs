using System.Collections;
using UnityEngine;

public class LungsBubbleSpawner : MonoBehaviour
{
    [Header("Bubble Prefabs")]
    [SerializeField] private GameObject o2BubblePrefab;
    [SerializeField] private GameObject co2BubblePrefab;

    [Header("Left Lung Spawn Areas")]
    [SerializeField] private RectTransform[] leftSpawnAreas;

    [Header("Right Lung Spawn Areas")]
    [SerializeField] private RectTransform[] rightSpawnAreas;

    [Header("Bubble Container")]
    [SerializeField] private RectTransform bubblesContainer;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int maxBubblesOnScreen = 12;

    private Coroutine spawnCoroutine;

    private void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (bubblesContainer != null &&
                bubblesContainer.childCount < maxBubblesOnScreen)
            {
                SpawnBubble();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBubble()
    {
        if (o2BubblePrefab == null ||
            co2BubblePrefab == null ||
            bubblesContainer == null)
        {
            Debug.LogWarning("LungsBubbleSpawner: липсват свързани обекти.");
            return;
        }

        bool spawnO2 = Random.value < 0.5f;
        bool spawnLeft = Random.value < 0.5f;

        GameObject prefabToSpawn =
            spawnO2 ? o2BubblePrefab : co2BubblePrefab;

        RectTransform[] selectedAreas =
            spawnLeft ? leftSpawnAreas : rightSpawnAreas;

        if (selectedAreas == null || selectedAreas.Length == 0)
        {
            Debug.LogWarning("LungsBubbleSpawner: няма spawn areas.");
            return;
        }

        RectTransform selectedArea =
            selectedAreas[Random.Range(0, selectedAreas.Length)];

        if (selectedArea == null)
        {
            return;
        }

        GameObject bubble =
            Instantiate(prefabToSpawn, bubblesContainer);

        RectTransform bubbleRect =
            bubble.GetComponent<RectTransform>();

        Vector3 randomWorldPosition =
            GetRandomWorldPositionInside(selectedArea);

        bubbleRect.position = randomWorldPosition;
    }

    private Vector3 GetRandomWorldPositionInside(RectTransform area)
    {
        Rect rect = area.rect;

        float randomX =
            Random.Range(rect.xMin, rect.xMax);

        float randomY =
            Random.Range(rect.yMin, rect.yMax);

        Vector3 localPoint =
            new Vector3(randomX, randomY, 0f);

        return area.TransformPoint(localPoint);
    }
}
