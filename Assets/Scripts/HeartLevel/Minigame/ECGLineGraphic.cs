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

    private float GetECGY(float time)
    {
        float beatInterval = 60f / bpm;
        float phase = Mathf.Repeat(time, beatInterval) / beatInterval;

        // P wave - малка плавна вълна
        if (phase < 0.12f)
        {
            return Mathf.Sin((phase / 0.12f) * Mathf.PI) * 8f;
        }

        // Кратка права линия
        if (phase < 0.18f)
        {
            return 0f;
        }

        // Q - лек спад надолу
        if (phase < 0.22f)
        {
            float t = Mathf.InverseLerp(0.18f, 0.22f, phase);
            return Mathf.Lerp(0f, -12f, t);
        }

        // R - голям основен пик нагоре
        if (phase < 0.28f)
        {
            float t = Mathf.InverseLerp(0.22f, 0.28f, phase);
            return Mathf.Lerp(-12f, 85f, t);
        }

        // S - рязък спад след големия пик
        if (phase < 0.34f)
        {
            float t = Mathf.InverseLerp(0.28f, 0.34f, phase);
            return Mathf.Lerp(85f, -22f, t);
        }

        // Връщане към средната линия
        if (phase < 0.40f)
        {
            float t = Mathf.InverseLerp(0.34f, 0.40f, phase);
            return Mathf.Lerp(-22f, 0f, t);
        }

        // T wave - по-мека вълна
        if (phase < 0.58f)
        {
            float t = Mathf.InverseLerp(0.40f, 0.58f, phase);
            return Mathf.Sin(t * Mathf.PI) * 12f;
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
            AddLine(vh, points[i], points[i + 1]);
        }
    }

    private void AddLine(VertexHelper vh, Vector2 start, Vector2 end)
    {
        Vector2 direction = (end - start).normalized;

        Vector2 normal =
            new Vector2(-direction.y, direction.x) *
            lineThickness * 0.5f;

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

        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }
}