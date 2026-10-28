using UnityEngine;
using UnityEngine.UI;

/// <summary>Fits the start background and independent logo to the screen and safe area.</summary>
[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class StartMenuLayout : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image logo;
    [SerializeField, Range(.3f, .95f)] private float logoWidthRatio = .8f;
    [SerializeField, Range(.05f, .4f)] private float logoMaxHeightRatio = .23f;
    [SerializeField, Range(0f, 1f)] private float logoHeightPosition = .74f;

    private void LateUpdate()
    {
        Rect canvas = ((RectTransform)transform).rect;
        if (canvas.width <= 0f || canvas.height <= 0f || Screen.width <= 0 || Screen.height <= 0) return;
        if (background != null && background.sprite != null)
        {
            Vector2 size = background.sprite.rect.size;
            float cover = Mathf.Max(canvas.width / size.x, canvas.height / size.y);
            RectTransform rect = background.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * .5f;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size * cover;
        }
        if (logo == null || logo.sprite == null) return;
        Rect safe = Screen.safeArea;
        float safeWidth = canvas.width * safe.width / Screen.width;
        float safeHeight = canvas.height * safe.height / Screen.height;
        float aspect = logo.sprite.rect.height / logo.sprite.rect.width;
        float width = Mathf.Min(safeWidth * logoWidthRatio, safeHeight * logoMaxHeightRatio / aspect);
        RectTransform logoRect = logo.rectTransform;
        logoRect.anchorMin = logoRect.anchorMax = new Vector2(safe.center.x / Screen.width,
            (safe.y + safe.height * logoHeightPosition) / Screen.height);
        logoRect.pivot = Vector2.one * .5f;
        logoRect.anchoredPosition = Vector2.zero;
        logoRect.sizeDelta = new Vector2(width, width * aspect);
    }
}
