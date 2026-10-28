using UnityEngine;
using UnityEngine.UI;

/// <summary>Draws non-interactive cell highlights and a drag arrow over the practice board.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public class TutorialMoveGuide : MaskableGraphic
{
    public Vector2 Source { get; set; }
    public Vector2 Target { get; set; }

    private void Update() => SetVerticesDirty();

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = rectTransform.rect;
        Vector2 cell = new Vector2(rect.width / 5f, rect.height / 3f);
        Vector2 start = rect.min + Vector2.Scale(Source + Vector2.one * .5f, cell);
        Vector2 end = rect.min + Vector2.Scale(Target + Vector2.one * .5f, cell);
        Color gold = new Color(1f, .82f, .2f, .8f + .2f * Mathf.Sin(Time.unscaledTime * 4f));
        Frame(mesh, start, cell * .93f, new Color(.2f, .9f, 1f, .9f));
        Frame(mesh, end, cell * .93f, gold);
        float radius = cell.x * .27f;
        for (int i = 0; i < 16; i++)
        {
            float a = i * Mathf.PI * 2f / 16f;
            float b = a + Mathf.PI * 2f / 24f;
            Line(mesh, end + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                end + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, cell.x * .014f, gold);
        }
        Vector2 direction = (end - start).normalized;
        Vector2 tip = end - direction * cell.x * .36f;
        Line(mesh, start + direction * cell.x * .3f, tip, cell.x * .025f, gold);
        Vector2 normal = new Vector2(-direction.y, direction.x) * cell.x * .11f;
        int index = mesh.currentVertCount;
        mesh.AddVert(tip + direction * cell.x * .13f, gold, Vector2.zero);
        mesh.AddVert(tip - direction * cell.x * .1f + normal, gold, Vector2.zero);
        mesh.AddVert(tip - direction * cell.x * .1f - normal, gold, Vector2.zero);
        mesh.AddTriangle(index, index + 1, index + 2);
    }

    private static void Frame(VertexHelper mesh, Vector2 center, Vector2 size, Color color)
    {
        Vector2 min = center - size * .5f;
        Vector2 max = center + size * .5f;
        float width = size.x * .02f;
        Line(mesh, min, new Vector2(max.x, min.y), width, color);
        Line(mesh, new Vector2(max.x, min.y), max, width, color);
        Line(mesh, max, new Vector2(min.x, max.y), width, color);
        Line(mesh, new Vector2(min.x, max.y), min, width, color);
    }

    private static void Line(VertexHelper mesh, Vector2 from, Vector2 to, float width, Color color)
    {
        Vector2 direction = (to - from).normalized;
        Vector2 offset = new Vector2(-direction.y, direction.x) * width * .5f;
        int index = mesh.currentVertCount;
        mesh.AddVert(from - offset, color, Vector2.zero);
        mesh.AddVert(from + offset, color, Vector2.zero);
        mesh.AddVert(to + offset, color, Vector2.zero);
        mesh.AddVert(to - offset, color, Vector2.zero);
        mesh.AddTriangle(index, index + 1, index + 2);
        mesh.AddTriangle(index, index + 2, index + 3);
    }
}
