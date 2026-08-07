using UnityEngine;
using UnityEngine.UI;

public class ECGLineGraphic : Graphic
{
    [SerializeField] private float lineThickness = 4f;
    [SerializeField] private float animationSpeed = 0.5f;

    private float progress = 0f;

    private void Update()
    {
    progress += Time.deltaTime * animationSpeed;

    if (progress > 1f)
        progress = 0f;

    SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;

        Vector2[] points =
        {
            new Vector2(rect.xMin, 0),

            new Vector2(rect.xMin + rect.width * 0.10f, 0),
            new Vector2(rect.xMin + rect.width * 0.14f, 8),
            new Vector2(rect.xMin + rect.width * 0.17f, -4),
            new Vector2(rect.xMin + rect.width * 0.20f, 0),

            new Vector2(rect.xMin + rect.width * 0.24f, 0),
            new Vector2(rect.xMin + rect.width * 0.27f, 18),
            new Vector2(rect.xMin + rect.width * 0.29f, -28),
            new Vector2(rect.xMin + rect.width * 0.31f, 65),
            new Vector2(rect.xMin + rect.width * 0.34f, -18),
            new Vector2(rect.xMin + rect.width * 0.37f, 0),

            new Vector2(rect.xMin + rect.width * 0.50f, 0),

            new Vector2(rect.xMin + rect.width * 0.54f, 8),
            new Vector2(rect.xMin + rect.width * 0.57f, -4),
            new Vector2(rect.xMin + rect.width * 0.60f, 0),

            new Vector2(rect.xMin + rect.width * 0.70f, 0),
            new Vector2(rect.xMin + rect.width * 0.73f, 18),
            new Vector2(rect.xMin + rect.width * 0.75f, -28),
            new Vector2(rect.xMin + rect.width * 0.77f, 65),
            new Vector2(rect.xMin + rect.width * 0.80f, -18),
            new Vector2(rect.xMin + rect.width * 0.83f, 0),

            new Vector2(rect.xMax, 0)
        };

        float maxX = Mathf.Lerp(rect.xMin, rect.xMax, progress);

        for (int i = 0; i < points.Length - 1; i++)
        {
            if (points[i].x > maxX)
                break;

            Vector2 start = points[i];
            Vector2 end = points[i + 1];

            if (end.x > maxX)
            {
                float t = Mathf.InverseLerp(start.x, end.x, maxX);
                end = Vector2.Lerp(start, end, t);
            }

            AddLine(vh, start, end);
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