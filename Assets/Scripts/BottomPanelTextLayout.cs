using UnityEngine;

/// <summary>Keeps panel text in a stable 600-unit layout as the canvas changes size.</summary>
[RequireComponent(typeof(RectTransform))]
public class BottomPanelTextLayout : MonoBehaviour
{
    private RectTransform m_rect;
    private RectTransform m_panel;

    public static RectTransform Create(Transform panel, string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform));
        root.layer = panel.gameObject.layer;
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.SetParent(panel, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        root.AddComponent<BottomPanelTextLayout>();
        return rect;
    }

    private void OnEnable()
    {
        m_rect = (RectTransform)transform;
        m_panel = transform.parent as RectTransform;
        ApplyLayout();
        Canvas.willRenderCanvases += ApplyLayout;
    }

    private void OnDisable()
    {
        Canvas.willRenderCanvases -= ApplyLayout;
    }

    private void ApplyLayout()
    {
        if (m_panel == null || m_panel.rect.width <= 0f) return;
        float scale = m_panel.rect.width / 600f;
        Vector2 size = m_panel.rect.size / scale;
        if (m_rect.sizeDelta != size) m_rect.sizeDelta = size;
        Vector3 localScale = Vector3.one * scale;
        if (m_rect.localScale != localScale) m_rect.localScale = localScale;
    }
}
