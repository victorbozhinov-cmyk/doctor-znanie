using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInfoPageFlipController : MonoBehaviour
{
    [System.Serializable]
    private class PageData
    {
        public GameObject pageObject;

        [HideInInspector] public RectTransform rectTransform;
        [HideInInspector] public CanvasGroup canvasGroup;

        [HideInInspector] public Vector2 originalAnchoredPosition;
        [HideInInspector] public Vector3 originalScale;
        [HideInInspector] public Quaternion originalRotation;
        [HideInInspector] public Vector2 originalPivot;
    }

    [Header("Pages (подреди ги: Page1, Page2, Page3...)")]
    [SerializeField] private List<GameObject> pages = new List<GameObject>();

    [Header("Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Animation")]
    [SerializeField] private float fallDuration = 0.40f;
    [SerializeField] private float revealDuration = 0.22f;
    [SerializeField] private float fallDistanceY = 220f;
    [SerializeField] private float fallDistanceX = 70f;
    [SerializeField] private float fallAngle = 14f;
    [SerializeField] private float nextPageStartOffsetY = 18f;
    [SerializeField] private float nextPageStartOffsetX = 22f;
    [SerializeField] private float nextPageStartScale = 0.98f;

    [Header("Start Page")]
    [SerializeField] private int startPageIndex = 0;

    private readonly List<PageData> pageDataList = new List<PageData>();

    private int currentPageIndex = 0;
    private bool isAnimating = false;

    private void Awake()
    {
        BuildPageData();
        ClampStartIndex();
        PrepareInitialState();
        UpdateButtonsState();
    }

    private void BuildPageData()
    {
        pageDataList.Clear();

        for (int i = 0; i < pages.Count; i++)
        {
            GameObject page = pages[i];

            if (page == null)
            {
                continue;
            }

            RectTransform rect = page.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError($"Page object '{page.name}' няма RectTransform.");
                continue;
            }

            CanvasGroup canvasGroup = page.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = page.AddComponent<CanvasGroup>();
            }

            PageData data = new PageData
            {
                pageObject = page,
                rectTransform = rect,
                canvasGroup = canvasGroup,
                originalAnchoredPosition = rect.anchoredPosition,
                originalScale = rect.localScale,
                originalRotation = rect.localRotation,
                originalPivot = rect.pivot
            };

            pageDataList.Add(data);
        }
    }

    private void ClampStartIndex()
    {
        if (pageDataList.Count == 0)
        {
            currentPageIndex = 0;
            return;
        }

        currentPageIndex = Mathf.Clamp(startPageIndex, 0, pageDataList.Count - 1);
    }

    private void PrepareInitialState()
    {
        for (int i = 0; i < pageDataList.Count; i++)
        {
            bool shouldBeActive = i == currentPageIndex;

            pageDataList[i].pageObject.SetActive(shouldBeActive);
            ResetPageVisual(pageDataList[i]);

            if (shouldBeActive)
            {
                pageDataList[i].canvasGroup.alpha = 1f;
            }
        }
    }

    public void NextPage()
    {
        if (isAnimating)
        {
            return;
        }

        if (currentPageIndex >= pageDataList.Count - 1)
        {
            return;
        }

        StartCoroutine(AnimatePageChange(currentPageIndex + 1, 1));
    }

    public void PreviousPage()
    {
        if (isAnimating)
        {
            return;
        }

        if (currentPageIndex <= 0)
        {
            return;
        }

        StartCoroutine(AnimatePageChange(currentPageIndex - 1, -1));
    }

    private IEnumerator AnimatePageChange(int targetPageIndex, int direction)
    {
        isAnimating = true;
        SetButtonsInteractable(false);

        PageData currentPage = pageDataList[currentPageIndex];
        PageData targetPage = pageDataList[targetPageIndex];

        ResetPageVisual(currentPage);
        ResetPageVisual(targetPage);

        currentPage.pageObject.SetActive(true);
        targetPage.pageObject.SetActive(true);

        // Текущата страница остава отгоре и после "пада"
        currentPage.pageObject.transform.SetAsLastSibling();

        // Подготвяме следващата / предишната страница отдолу
        targetPage.canvasGroup.alpha = 0f;
        targetPage.rectTransform.anchoredPosition =
            targetPage.originalAnchoredPosition +
            new Vector2(-direction * nextPageStartOffsetX, nextPageStartOffsetY);

        targetPage.rectTransform.localScale =
            targetPage.originalScale * nextPageStartScale;

        targetPage.rectTransform.localRotation =
            Quaternion.Euler(0f, 0f, -direction * 3f);

        // За да изглежда като закачена за клипборда,
        // временно сменяме pivot-а горе в центъра
        SetPivotWithoutMoving(currentPage.rectTransform, new Vector2(0.5f, 1f));

        Vector2 currentStartPos = currentPage.rectTransform.anchoredPosition;
        Vector2 currentEndPos = currentPage.originalAnchoredPosition +
                                new Vector2(direction * fallDistanceX, -fallDistanceY);

        float currentEndAngle = direction > 0 ? -fallAngle : fallAngle;

        float elapsed = 0f;
        float revealDelay = 0.05f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / fallDuration);
            float easedOut = EaseOutCubic(t);

            // Падаща текуща страница
            currentPage.rectTransform.anchoredPosition =
                Vector2.Lerp(currentStartPos, currentEndPos, easedOut);

            currentPage.rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, currentEndAngle, easedOut));

            currentPage.rectTransform.localScale =
                Vector3.Lerp(currentPage.originalScale, currentPage.originalScale * 0.97f, easedOut);

            currentPage.canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            // Поява на следващата / предишната страница
            float revealT = Mathf.Clamp01((elapsed - revealDelay) / revealDuration);
            float revealEase = EaseOutCubic(revealT);

            targetPage.canvasGroup.alpha = Mathf.Lerp(0f, 1f, revealEase);
            targetPage.rectTransform.anchoredPosition = Vector2.Lerp(
                targetPage.originalAnchoredPosition + new Vector2(-direction * nextPageStartOffsetX, nextPageStartOffsetY),
                targetPage.originalAnchoredPosition,
                revealEase
            );

            targetPage.rectTransform.localScale = Vector3.Lerp(
                targetPage.originalScale * nextPageStartScale,
                targetPage.originalScale,
                revealEase
            );

            targetPage.rectTransform.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Lerp(-direction * 3f, 0f, revealEase)
            );

            yield return null;
        }

        // Финално състояние
        ResetPageVisual(currentPage);
        currentPage.canvasGroup.alpha = 1f;
        currentPage.pageObject.SetActive(false);

        ResetPageVisual(targetPage);
        targetPage.canvasGroup.alpha = 1f;
        targetPage.pageObject.SetActive(true);

        currentPageIndex = targetPageIndex;

        // Скриваме всички останали
        for (int i = 0; i < pageDataList.Count; i++)
        {
            bool shouldBeActive = i == currentPageIndex;
            pageDataList[i].pageObject.SetActive(shouldBeActive);

            if (shouldBeActive)
            {
                ResetPageVisual(pageDataList[i]);
                pageDataList[i].canvasGroup.alpha = 1f;
            }
        }

        isAnimating = false;
        UpdateButtonsState();
    }

    private void ResetPageVisual(PageData page)
    {
        if (page == null || page.rectTransform == null)
        {
            return;
        }

        page.rectTransform.pivot = page.originalPivot;
        page.rectTransform.anchoredPosition = page.originalAnchoredPosition;
        page.rectTransform.localScale = page.originalScale;
        page.rectTransform.localRotation = page.originalRotation;

        if (page.canvasGroup != null)
        {
            page.canvasGroup.alpha = 1f;
        }
    }

    private void SetButtonsInteractable(bool isInteractable)
    {
        if (previousButton != null)
        {
            previousButton.interactable = isInteractable;
        }

        if (nextButton != null)
        {
            nextButton.interactable = isInteractable;
        }
    }

    private void UpdateButtonsState()
    {
        if (previousButton != null)
        {
            previousButton.interactable = currentPageIndex > 0;
        }

        if (nextButton != null)
        {
            nextButton.interactable = currentPageIndex < pageDataList.Count - 1;
        }
    }

    private void SetPivotWithoutMoving(RectTransform rect, Vector2 newPivot)
    {
        if (rect == null)
        {
            return;
        }

        Vector2 size = rect.rect.size;
        Vector2 deltaPivot = rect.pivot - newPivot;

        Vector3 deltaPosition = new Vector3(
            deltaPivot.x * size.x * rect.localScale.x,
            deltaPivot.y * size.y * rect.localScale.y,
            0f
        );

        rect.pivot = newPivot;
        rect.localPosition -= deltaPosition;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}