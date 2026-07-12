using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(RectTransform))]
public class StartMenuPlayButton : MonoBehaviour
{
    [SerializeField] private string levelSelectSceneName = "SelectNivel";
    [SerializeField, Range(0.4f, 0.95f)] private float widthRatio = 0.72f;
    [SerializeField, Range(0.05f, 0.35f)] private float maxHeightRatio = 0.16f;
    [SerializeField, Range(0.02f, 0.4f)] private float bottomAnchorRatio = 0.2f;
    [SerializeField] private Vector2 anchoredOffset;

    private RectTransform m_rectTransform;
    private RectTransform m_canvasRect;
    private Canvas m_canvas;

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_canvas = GetComponentInParent<Canvas>();
        m_canvasRect = m_canvas != null ? m_canvas.transform as RectTransform : null;
        FitToCanvas();
    }

    private void LateUpdate()
    {
        FitToCanvas();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandlePress(Mouse.current.position.ReadValue());
            return;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            HandlePress(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void HandlePress(Vector2 screenPosition)
    {
        Camera canvasCamera = m_canvas != null && m_canvas.renderMode != RenderMode.ScreenSpaceOverlay ? m_canvas.worldCamera : null;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectTransform, screenPosition, canvasCamera, out localPoint) &&
            m_rectTransform.rect.Contains(localPoint))
        {
            SceneManager.LoadScene(levelSelectSceneName);
        }
    }

    private void FitToCanvas()
    {
        if (m_rectTransform == null || m_canvasRect == null)
        {
            return;
        }

        Rect canvasRect = m_canvasRect.rect;
        float width = canvasRect.width * widthRatio;
        float height = width * 0.42f;
        float maxHeight = canvasRect.height * maxHeightRatio;
        if (height > maxHeight)
        {
            height = maxHeight;
            width = height / 0.42f;
        }

        m_rectTransform.anchorMin = new Vector2(0.5f, bottomAnchorRatio);
        m_rectTransform.anchorMax = new Vector2(0.5f, bottomAnchorRatio);
        m_rectTransform.pivot = new Vector2(0.5f, 0.5f);
        m_rectTransform.anchoredPosition = anchoredOffset;
        m_rectTransform.sizeDelta = new Vector2(width, height);
        m_rectTransform.localScale = Vector3.one;
    }
}
