using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ECGLineGraphic : Graphic
{
    [SerializeField] private float lineThickness = 4f;
    [SerializeField] private float bpm = 80f;
    [SerializeField] private float scrollSpeed = 120f;
    [SerializeField] private float sampleSpacing = 2f;

    private readonly List<Vector2> points = new();

    private float sampleTimer;
    private float currentTime;

    protected override void Start()
    {
        base.Start();

        Rect rect = rectTransform.rect;

        for (float x = rect.xMin; x <= rect.xMax; x += sampleSpacing)
        {
            points.Add(new Vector2(x, 0f));
        }
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        // Линията се движи с постоянна скорост.
        float sampleInterval = sampleSpacing / scrollSpeed;

        sampleTimer += Time.deltaTime;

        if (sampleTimer >= sampleInterval)
        {
            sampleTimer = 0f;

            for (int i = 0; i < points.Count; i++)
            {
                points[i] += Vector2.left * sampleSpacing;
            }

            Rect rect = rectTransform.rect;

            while (points.Count > 0 && points[0].x < rect.xMin)
            {
                points.RemoveAt(0);
            }

            float y = GetECGY(currentTime);

            points.Add(new Vector2(rect.xMax, y));

            SetVerticesDirty();
        }
    }

    public void SetBPM(float newBPM)
    {
        bpm = Mathf.Max(1f, newBPM);
    }

    private float GetECGY(float time)
    {
        /*
         * Визуалната честота е малко по-ниска от реалния BPM,
         * за да има повече пространство между ударите.
         *
         * Например:
         * 50 BPM  -> ~43 визуално
         * 80 BPM  -> ~66 визуално
         * 100 BPM -> ~82 визуално
         * 120 BPM -> ~98 визуално
         */
        float visualBPM = bpm * 0.82f;

        visualBPM = Mathf.Max(30f, visualBPM);

        float beatInterval = 60f / visualBPM;

        float phase =
            Mathf.Repeat(time, beatInterval) / beatInterval;

        /*
         * Амплитудата се променя доста по-видимо.
         *
         * Нисък пулс  -> малка вертикална вълна
         * Нормален    -> средна
         * Висок       -> голям пик нагоре И голям спад надолу
         */
        float amplitudeMultiplier = Mathf.Lerp(
            0.45f,
            1.60f,
            Mathf.InverseLerp(50f, 120f, bpm)
        );

        // P wave
        if (phase < 0.12f)
        {
            return Mathf.Sin(
                (phase / 0.12f) * Mathf.PI
            ) * 8f * amplitudeMultiplier;
        }

        // Кратка права линия
        if (phase < 0.18f)
        {
            return 0f;
        }

        // Q - спад
        if (phase < 0.22f)
        {
            float t = Mathf.InverseLerp(
                0.18f,
                0.22f,
                phase
            );

            return Mathf.Lerp(
                0f,
                -14f * amplitudeMultiplier,
                t
            );
        }

        // R - главен пик нагоре
        if (phase < 0.28f)
        {
            float t = Mathf.InverseLerp(
                0.22f,
                0.28f,
                phase
            );

            return Mathf.Lerp(
                -14f * amplitudeMultiplier,
                82f * amplitudeMultiplier,
                t
            );
        }

        // S - по-силен спад надолу
        if (phase < 0.34f)
        {
            float t = Mathf.InverseLerp(
                0.28f,
                0.34f,
                phase
            );

            return Mathf.Lerp(
                82f * amplitudeMultiplier,
                -32f * amplitudeMultiplier,
                t
            );
        }

        // Връщане към основната линия
        if (phase < 0.40f)
        {
            float t = Mathf.InverseLerp(
                0.34f,
                0.40f,
                phase
            );

            return Mathf.Lerp(
                -32f * amplitudeMultiplier,
                0f,
                t
            );
        }

        // T wave
        if (phase < 0.58f)
        {
            float t = Mathf.InverseLerp(
                0.40f,
                0.58f,
                phase
            );

            return Mathf.Sin(
                t * Mathf.PI
            ) * 13f * amplitudeMultiplier;
        }

        // Права линия до следващия удар
        return 0f;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points.Count < 2)
            return;

        for (int i = 0; i < points.Count - 1; i++)
        {
            AddLine(
                vh,
                points[i],
                points[i + 1]
            );
        }
    }

    private void AddLine(
        VertexHelper vh,
        Vector2 start,
        Vector2 end
    )
    {
        Vector2 direction =
            (end - start).normalized;

        Vector2 normal =
            new Vector2(
                -direction.y,
                direction.x
            )
            * lineThickness
            * 0.5f;

        int index = vh.currentVertCount;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = start - normal;
        vh.AddVert(vertex);

        vertex.position = start + normal;
        vh.AddVert(vertex);

        vertex.position = end + normal;
        vh.AddVert(vertex);

        vertex.position = end - normal;
        vh.AddVert(vertex);

        vh.AddTriangle(
            index,
            index + 1,
            index + 2
        );

        vh.AddTriangle(
            index,
            index + 2,
            index + 3
        );
    }
}