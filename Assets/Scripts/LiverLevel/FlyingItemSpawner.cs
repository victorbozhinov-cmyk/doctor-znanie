using System.Collections;
using UnityEngine;

public class FlyingItemSpawner : MonoBehaviour
{
    [Header("Main References")]
    [SerializeField] private RectTransform flyingItemsRoot;
    [SerializeField] private GameObject flyingItemPrefab;

    [Header("Doctor Reference")]
    [SerializeField] private RectTransform doctorCharacter;

    [Header("Liver References")]
    [SerializeField] private RectTransform liverCharacter;
    [SerializeField] private LiverHealthController liverHealthController;

    [Tooltip("Разстояние вдясно от черния дроб, където балончето се пука.")]
    [SerializeField] private float liverBoundaryGap = 20f;

    [Header("Helpful Item Icons")]
    [SerializeField] private Sprite[] helpfulIcons;

    [Header("Harmful Item Icons")]
    [SerializeField] private Sprite[] harmfulIcons;

    [Header("Item Chances")]
    [Range(0f, 1f)]
    [SerializeField] private float helpfulChance = 0.5f;

    [Header("Spawn Interval By Difficulty")]
    [SerializeField] private float easySpawnInterval = 3.0f;
    [SerializeField] private float mediumSpawnInterval = 2.2f;
    [SerializeField] private float hardSpawnInterval = 1.5f;

    [Header("Spawn Area")]
    [SerializeField] private float topPadding = 230f;
    [SerializeField] private float bottomPadding = 20f;

    [Header("Right Side Spawn")]
    [SerializeField] private float outsideOffset = 10f;

    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        SetSpawnIntervalFromDifficulty();
    }

    private void OnEnable()
    {
        SetSpawnIntervalFromDifficulty();

        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }
    }

    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    // =========================================================
    // DIFFICULTY
    // =========================================================

    private void SetSpawnIntervalFromDifficulty()
    {
        string difficultyString =
            PlayerPrefs.GetString("Difficulty", "").ToLower();

        if (difficultyString == "easy")
        {
            currentSpawnInterval = easySpawnInterval;
            return;
        }

        if (difficultyString == "medium")
        {
            currentSpawnInterval = mediumSpawnInterval;
            return;
        }

        if (difficultyString == "hard")
        {
            currentSpawnInterval = hardSpawnInterval;
            return;
        }

        // Ако Difficulty е записана като int:
        // 0 = Easy
        // 1 = Medium
        // 2 = Hard

        int difficultyInt =
            PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficultyInt)
        {
            case 0:
                currentSpawnInterval = easySpawnInterval;
                break;

            case 2:
                currentSpawnInterval = hardSpawnInterval;
                break;

            default:
                currentSpawnInterval = mediumSpawnInterval;
                break;
        }
    }

    // =========================================================
    // SPAWN LOOP
    // =========================================================

    private IEnumerator SpawnLoop()
    {
        // Кратка пауза преди първото балонче.
        yield return new WaitForSeconds(1f);

        while (true)
        {
            SpawnFlyingItem();

            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    // =========================================================
    // SPAWN
    // =========================================================

    private void SpawnFlyingItem()
    {
        if (flyingItemsRoot == null)
        {
            Debug.LogWarning(
                "FlyingItemSpawner: FlyingItemsRoot не е зададен!"
            );

            return;
        }

        if (flyingItemPrefab == null)
        {
            Debug.LogWarning(
                "FlyingItemSpawner: FlyingItem Prefab не е зададен!"
            );

            return;
        }

        if (doctorCharacter == null)
        {
            Debug.LogWarning(
                "FlyingItemSpawner: Doctor Character не е зададен!"
            );

            return;
        }

        if (liverCharacter == null)
        {
            Debug.LogWarning(
                "FlyingItemSpawner: Liver Character не е зададен!"
            );

            return;
        }

        if (liverHealthController == null)
        {
            Debug.LogWarning(
                "FlyingItemSpawner: Liver Health Controller не е зададен!"
            );

            return;
        }

        GameObject newItem =
            Instantiate(flyingItemPrefab, flyingItemsRoot);

        RectTransform itemRect =
            newItem.GetComponent<RectTransform>();

        FlyingItem flyingItem =
            newItem.GetComponent<FlyingItem>();

        if (itemRect == null)
        {
            Debug.LogWarning(
                "FlyingItemSpawner: FlyingItem няма RectTransform!"
            );

            Destroy(newItem);
            return;
        }

        if (flyingItem == null)
        {
            Debug.LogWarning(
                "FlyingItemSpawner: Prefab-ът няма FlyingItem script!"
            );

            Destroy(newItem);
            return;
        }

        SetupItemTransform(itemRect);

        SetupItemPosition(itemRect);

        SetupRandomItem(flyingItem);
    }

    // =========================================================
    // TRANSFORM
    // =========================================================

    private void SetupItemTransform(RectTransform itemRect)
    {
        itemRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        itemRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        itemRect.pivot =
            new Vector2(0.5f, 0.5f);

        itemRect.localScale =
            Vector3.one;

        itemRect.localRotation =
            Quaternion.identity;
    }

    // =========================================================
    // POSITION
    // =========================================================

    private void SetupItemPosition(RectTransform itemRect)
    {
        Rect area =
            flyingItemsRoot.rect;

        float halfItemWidth =
            itemRect.rect.width * 0.5f;

        float halfItemHeight =
            itemRect.rect.height * 0.5f;

        // Винаги се появява от най-дясната страна.
        float spawnX =
            area.xMax +
            halfItemWidth +
            outsideOffset;

        // Долна граница.
        float minY =
            area.yMin +
            halfItemHeight +
            bottomPadding;

        // Горна граница.
        float maxY =
            area.yMax -
            halfItemHeight -
            topPadding;

        if (maxY < minY)
        {
            Debug.LogWarning(
                "FlyingItemSpawner: Невалидна Y spawn зона!"
            );

            maxY = minY;
        }

        float randomY =
            Random.Range(minY, maxY);

        itemRect.anchoredPosition =
            new Vector2(spawnX, randomY);
    }

    // =========================================================
    // RANDOM ITEM
    // =========================================================

    private void SetupRandomItem(FlyingItem flyingItem)
    {
        bool hasHelpful =
            helpfulIcons != null &&
            helpfulIcons.Length > 0;

        bool hasHarmful =
            harmfulIcons != null &&
            harmfulIcons.Length > 0;

        if (!hasHelpful && !hasHarmful)
        {
            Debug.LogWarning(
                "FlyingItemSpawner: Няма зададени Item иконки!"
            );

            return;
        }

        bool shouldBeHelpful;

        if (!hasHarmful)
        {
            shouldBeHelpful = true;
        }
        else if (!hasHelpful)
        {
            shouldBeHelpful = false;
        }
        else
        {
            shouldBeHelpful =
                Random.value < helpfulChance;
        }

        float liverBoundaryX =
            CalculateLiverBoundaryX();

        if (shouldBeHelpful)
        {
            Sprite randomIcon =
                helpfulIcons[
                    Random.Range(
                        0,
                        helpfulIcons.Length
                    )
                ];

            flyingItem.Setup(
                randomIcon,
                FlyingItem.ItemType.Helpful,
                liverHealthController,
                liverBoundaryX,
                doctorCharacter
            );
        }
        else
        {
            Sprite randomIcon =
                harmfulIcons[
                    Random.Range(
                        0,
                        harmfulIcons.Length
                    )
                ];

            flyingItem.Setup(
                randomIcon,
                FlyingItem.ItemType.Harmful,
                liverHealthController,
                liverBoundaryX,
                doctorCharacter
            );
        }
    }

    // =========================================================
    // LIVER BOUNDARY
    // =========================================================

    private float CalculateLiverBoundaryX()
    {
        Vector3[] corners =
            new Vector3[4];

        liverCharacter.GetWorldCorners(corners);

        // Средата на десния край на черния дроб.
        Vector3 rightWorldPoint =
            (corners[2] + corners[3]) * 0.5f;

        Vector3 localPoint =
            flyingItemsRoot.InverseTransformPoint(
                rightWorldPoint
            );

        return localPoint.x + liverBoundaryGap;
    }
}