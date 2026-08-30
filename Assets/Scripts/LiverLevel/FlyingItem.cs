using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlyingItem : MonoBehaviour
{
    public enum ItemType
    {
        Helpful,
        Harmful
    }

    [Header("Item")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private Image bubbleImage;
    [SerializeField] private Image itemIcon;

    [Header("Speed by Difficulty")]
    [SerializeField] private float easySpeed = 140f;
    [SerializeField] private float mediumSpeed = 180f;
    [SerializeField] private float hardSpeed = 230f;

    [Header("Doctor Collision")]
    [SerializeField] private float doctorHitboxShrink = 15f;
    [SerializeField] private float bubbleHitboxShrink = 5f;

    [Header("Harmful - Correct POP")]
    [SerializeField] private float harmfulSquashDuration = 0.06f;
    [SerializeField] private float harmfulBurstDuration = 0.16f;
    [SerializeField] private float harmfulSquashScale = 0.78f;
    [SerializeField] private float harmfulBurstScale = 1.7f;
    [SerializeField] private float harmfulRotation = 35f;

    [Header("Helpful - Mistake Effect")]
    [SerializeField] private float helpfulMistakeDuration = 0.32f;
    [SerializeField] private float helpfulShakeAmount = 18f;
    [SerializeField] private float helpfulWobbleRotation = 12f;
    [SerializeField] private float helpfulPulseAmount = 0.08f;

    [Header("Liver POP")]
    [SerializeField] private float liverPopDuration = 0.12f;
    [SerializeField] private float liverPopScale = 1.35f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip harmfulPopSfx;
    [SerializeField] private AudioClip helpfulMistakeSfx;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.8f;

    private RectTransform rectTransform;
    private RectTransform doctorCharacter;

    private LiverHealthController liverHealth;
    private CanvasGroup canvasGroup;

    private float moveSpeed;
    private float liverBoundaryX;

    private Vector3 normalScale;
    private Quaternion normalRotation;

    private Color normalBubbleColor = Color.white;
    private Color normalIconColor = Color.white;

    private bool hasFinished = false;

    public ItemType Type => itemType;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        normalScale = rectTransform.localScale;
        normalRotation = rectTransform.localRotation;

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Ако не са свързани ръчно в Inspector,
        // опитваме да ги намерим автоматично.
        if (bubbleImage == null)
        {
            Transform bubbleTransform = transform.Find("BubbleImage");

            if (bubbleTransform != null)
                bubbleImage = bubbleTransform.GetComponent<Image>();
        }

        if (itemIcon == null)
        {
            Transform iconTransform = transform.Find("ItemIcon");

            if (iconTransform != null)
                itemIcon = iconTransform.GetComponent<Image>();
        }

        if (bubbleImage != null)
            normalBubbleColor = bubbleImage.color;

        if (itemIcon != null)
            normalIconColor = itemIcon.color;

        SetSpeedFromDifficulty();
    }

    private void Update()
    {
        if (hasFinished)
            return;

        MoveLeft();

        // Докторът има приоритет.
        CheckDoctorCollision();

        if (hasFinished)
            return;

        CheckLiverBoundary();
    }

    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        Sprite iconSprite,
        ItemType newType,
        LiverHealthController healthController,
        float boundaryX,
        RectTransform doctor)
    {
        itemType = newType;

        liverHealth = healthController;
        liverBoundaryX = boundaryX;
        doctorCharacter = doctor;

        if (itemIcon != null)
        {
            itemIcon.sprite = iconSprite;
            itemIcon.enabled = iconSprite != null;
        }
    }

    // =========================================================
    // SPEED
    // =========================================================

    private void SetSpeedFromDifficulty()
    {
        string difficultyString =
            PlayerPrefs.GetString("Difficulty", "").ToLower();

        if (difficultyString == "easy")
        {
            moveSpeed = easySpeed;
            return;
        }

        if (difficultyString == "medium")
        {
            moveSpeed = mediumSpeed;
            return;
        }

        if (difficultyString == "hard")
        {
            moveSpeed = hardSpeed;
            return;
        }

        int difficultyInt =
            PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficultyInt)
        {
            case 0:
                moveSpeed = easySpeed;
                break;

            case 2:
                moveSpeed = hardSpeed;
                break;

            default:
                moveSpeed = mediumSpeed;
                break;
        }
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void MoveLeft()
    {
        rectTransform.anchoredPosition +=
            Vector2.left * moveSpeed * Time.deltaTime;
    }

    // =========================================================
    // DOCTOR COLLISION
    // =========================================================

    private void CheckDoctorCollision()
    {
        if (doctorCharacter == null)
            return;

        Rect bubbleRect =
            GetWorldRect(
                rectTransform,
                bubbleHitboxShrink
            );

        Rect doctorRect =
            GetWorldRect(
                doctorCharacter,
                doctorHitboxShrink
            );

        if (bubbleRect.Overlaps(doctorRect))
        {
            HitByDoctor();
        }
    }

    private void HitByDoctor()
    {
        if (hasFinished)
            return;

        hasFinished = true;

        if (itemType == ItemType.Harmful)
        {
            PlaySfx(harmfulPopSfx);

            StartCoroutine(
                HarmfulDoctorPopAnimation()
            );
        }
        else
        {
            PlaySfx(helpfulMistakeSfx);

            StartCoroutine(
                HelpfulDoctorMistakeAnimation()
            );
        }
    }

    // =========================================================
    // HARMFUL = ПРАВИЛНО
    // =========================================================

    private IEnumerator HarmfulDoctorPopAnimation()
    {
        // -----------------------------------------
        // ФАЗА 1:
        // Балончето първо се свива рязко.
        // -----------------------------------------

        float timer = 0f;

        Vector3 startScale =
            rectTransform.localScale;

        Quaternion startRotation =
            rectTransform.localRotation;

        Vector3 squashScale =
            normalScale * harmfulSquashScale;

        while (timer < harmfulSquashDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / harmfulSquashDuration
                );

            // Ease Out
            float smoothT =
                1f - Mathf.Pow(1f - t, 3f);

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    squashScale,
                    smoothT
                );

            rectTransform.localRotation =
                Quaternion.Lerp(
                    startRotation,
                    Quaternion.Euler(0f, 0f, -8f),
                    smoothT
                );

            yield return null;
        }

        // -----------------------------------------
        // ФАЗА 2:
        // Силен burst / POP.
        // -----------------------------------------

        timer = 0f;

        Vector3 burstScale =
            normalScale * harmfulBurstScale;

        Color targetBubbleColor =
            new Color(
                0.55f,
                1f,
                0.55f,
                normalBubbleColor.a
            );

        while (timer < harmfulBurstDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / harmfulBurstDuration
                );

            float smoothT =
                1f - Mathf.Pow(1f - t, 3f);

            rectTransform.localScale =
                Vector3.Lerp(
                    squashScale,
                    burstScale,
                    smoothT
                );

            rectTransform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        -8f,
                        harmfulRotation,
                        smoothT
                    )
                );

            // Зелен flash при правилно действие.
            if (bubbleImage != null)
            {
                bubbleImage.color =
                    Color.Lerp(
                        normalBubbleColor,
                        targetBubbleColor,
                        Mathf.Sin(t * Mathf.PI)
                    );
            }

            // Иконката също светва леко.
            if (itemIcon != null)
            {
                itemIcon.color =
                    Color.Lerp(
                        normalIconColor,
                        Color.white,
                        Mathf.Sin(t * Mathf.PI)
                    );
            }

            // Изчезване към края.
            canvasGroup.alpha =
                1f - Mathf.Pow(t, 2f);

            yield return null;
        }

        Destroy(gameObject);
    }

    // =========================================================
    // HELPFUL = ГРЕШКА
    // =========================================================

    private IEnumerator HelpfulDoctorMistakeAnimation()
    {
        float timer = 0f;

        Vector2 startPosition =
            rectTransform.anchoredPosition;

        Color mistakeBubbleColor =
            new Color(
                1f,
                0.45f,
                0.45f,
                normalBubbleColor.a
            );

        while (timer < helpfulMistakeDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / helpfulMistakeDuration
                );

            float remaining =
                1f - t;

            // -----------------------------------------
            // Силно разклащане ляво / дясно
            // -----------------------------------------

            float shake =
                Mathf.Sin(t * Mathf.PI * 10f) *
                helpfulShakeAmount *
                remaining;

            rectTransform.anchoredPosition =
                startPosition +
                new Vector2(shake, 0f);

            // -----------------------------------------
            // Wobble rotation
            // -----------------------------------------

            float wobble =
                Mathf.Sin(t * Mathf.PI * 8f) *
                helpfulWobbleRotation *
                remaining;

            rectTransform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    wobble
                );

            // -----------------------------------------
            // Лек pulsating ефект
            // -----------------------------------------

            float pulse =
                1f +
                Mathf.Sin(t * Mathf.PI * 6f) *
                helpfulPulseAmount;

            // Последната част се свива.
            float shrink = 1f;

            if (t > 0.55f)
            {
                float shrinkT =
                    Mathf.InverseLerp(
                        0.55f,
                        1f,
                        t
                    );

                shrink =
                    Mathf.Lerp(
                        1f,
                        0f,
                        shrinkT
                    );
            }

            rectTransform.localScale =
                normalScale *
                pulse *
                shrink;

            // -----------------------------------------
            // Червен flash
            // -----------------------------------------

            if (bubbleImage != null)
            {
                bubbleImage.color =
                    Color.Lerp(
                        normalBubbleColor,
                        mistakeBubbleColor,
                        Mathf.Sin(t * Mathf.PI)
                    );
            }

            if (itemIcon != null)
            {
                itemIcon.color =
                    Color.Lerp(
                        normalIconColor,
                        new Color(1f, 0.6f, 0.6f, 1f),
                        Mathf.Sin(t * Mathf.PI)
                    );
            }

            // Fade в последната половина.
            if (t > 0.5f)
            {
                canvasGroup.alpha =
                    Mathf.Lerp(
                        1f,
                        0f,
                        (t - 0.5f) / 0.5f
                    );
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    // =========================================================
    // LIVER
    // =========================================================

    private void CheckLiverBoundary()
    {
        float halfWidth =
            rectTransform.rect.width * 0.5f;

        float bubbleLeftEdge =
            rectTransform.anchoredPosition.x -
            halfWidth;

        if (bubbleLeftEdge <= liverBoundaryX)
        {
            ReachLiver();
        }
    }

    private void ReachLiver()
    {
        if (hasFinished)
            return;

        hasFinished = true;

        if (liverHealth != null)
        {
            if (itemType == ItemType.Helpful)
            {
                liverHealth.IncreaseHealth();
            }
            else
            {
                liverHealth.DecreaseHealth();
            }
        }

        StartCoroutine(
            LiverPopAnimation()
        );
    }

    private IEnumerator LiverPopAnimation()
    {
        float timer = 0f;

        Vector3 startScale =
            rectTransform.localScale;

        while (timer < liverPopDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / liverPopDuration
                );

            rectTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    normalScale * liverPopScale,
                    t
                );

            canvasGroup.alpha =
                1f - t;

            yield return null;
        }

        Destroy(gameObject);
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null)
            return;

        /*
         * Правим временен AudioSource.
         *
         * Така звукът няма да бъде прекъснат,
         * когато FlyingItem бъде Destroy-нат.
         */

        GameObject audioObject =
            new GameObject("FlyingItem_SFX");

        AudioSource source =
            audioObject.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = sfxVolume;

        // 2D звук
        source.spatialBlend = 0f;

        source.playOnAwake = false;

        source.Play();

        Destroy(
            audioObject,
            clip.length + 0.1f
        );
    }

    // =========================================================
    // RECT COLLISION
    // =========================================================

    private Rect GetWorldRect(
        RectTransform target,
        float shrink)
    {
        Vector3[] corners =
            new Vector3[4];

        target.GetWorldCorners(corners);

        float minX = corners[0].x;
        float maxX = corners[0].x;

        float minY = corners[0].y;
        float maxY = corners[0].y;

        for (int i = 1; i < 4; i++)
        {
            if (corners[i].x < minX)
                minX = corners[i].x;

            if (corners[i].x > maxX)
                maxX = corners[i].x;

            if (corners[i].y < minY)
                minY = corners[i].y;

            if (corners[i].y > maxY)
                maxY = corners[i].y;
        }

        Rect result =
            Rect.MinMaxRect(
                minX,
                minY,
                maxX,
                maxY
            );

        result.xMin += shrink;
        result.xMax -= shrink;

        result.yMin += shrink;
        result.yMax -= shrink;

        return result;
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public bool IsHelpful()
    {
        return itemType == ItemType.Helpful;
    }

    public bool IsHarmful()
    {
        return itemType == ItemType.Harmful;
    }
}