using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class BottomPanelCameraFit : MonoBehaviour
{
    private enum PanelEdge
    {
        Bottom,
        Top
    }

    [SerializeField] private Camera targetCamera;
    [SerializeField] private Board board;
    [SerializeField] private PanelEdge panelEdge = PanelEdge.Bottom;
    [SerializeField, Range(0.5f, 1.5f)] private float cameraWidth = 1.05f;
    [SerializeField, Range(0.05f, 0.4f)] private float maxCameraHeight = 0.18f;
    [SerializeField] private float boardGap = 0.15f;
    [SerializeField, Range(0f, 0.2f)] private float bottomMargin = 0.03f;
    [SerializeField] private Vector2 anchoredOffset;
    [SerializeField] private float zPosition = -1f;

    private RectTransform m_rectTransform;
    private RectTransform m_canvasRect;
    private Canvas m_canvas;
    private Image m_image;

    private void Awake()
    {
        CacheReferences();
        FitPanel();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        FitPanel();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        if (m_rectTransform == null)
        {
            m_rectTransform = GetComponent<RectTransform>();
        }

        if (m_canvasRect == null)
        {
            m_canvas = GetComponentInParent<Canvas>();
            if (m_canvas != null)
            {
                m_canvasRect = m_canvas.transform as RectTransform;
            }
        }

        if (m_image == null)
        {
            m_image = GetComponent<Image>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (board == null && panelEdge == PanelEdge.Bottom)
        {
            board = FindAnyObjectByType<Board>();
        }
    }

    private void FitPanel()
    {
        CacheReferences();
        if (targetCamera == null || m_rectTransform == null || m_canvasRect == null || m_image == null || m_image.sprite == null || !targetCamera.orthographic)
        {
            return;
        }

        float viewHeight = targetCamera.orthographicSize * 2f;
        float canvasWidth = m_canvasRect.rect.width;
        float canvasHeight = m_canvasRect.rect.height;
        float targetWidth = canvasWidth * cameraWidth;

        Vector2 spriteSize = m_image.sprite.rect.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        float targetHeight = targetWidth * spriteSize.y / spriteSize.x;
        float maxHeight = canvasHeight * maxCameraHeight;
        if (targetHeight > maxHeight)
        {
            targetHeight = maxHeight;
            targetWidth = targetHeight * spriteSize.x / spriteSize.y;
        }

        float minY = -canvasHeight * 0.5f + canvasHeight * bottomMargin + targetHeight * 0.5f;
        float maxY = canvasHeight * 0.5f - canvasHeight * bottomMargin - targetHeight * 0.5f;
        float targetY = minY;

        if (panelEdge == PanelEdge.Bottom && board != null && board.height > 0)
        {
            Vector3 boardEdgeWorld = board.transform.position + new Vector3((board.width - 1) * 0.5f, -0.5f, 0f);
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvasRect, targetCamera.WorldToScreenPoint(boardEdgeWorld), targetCamera, out localPoint))
            {
                float canvasGap = boardGap * canvasHeight / viewHeight;
                targetY = localPoint.y - canvasGap - targetHeight * 0.5f;
            }
        }

        targetY = Mathf.Clamp(targetY, minY, maxY);
        if (panelEdge == PanelEdge.Top)
        {
            m_rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            m_rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            m_rectTransform.pivot = new Vector2(0.5f, 1f);

            Camera canvasCamera = m_canvas != null && m_canvas.renderMode != RenderMode.ScreenSpaceOverlay ? m_canvas.worldCamera : null;
            Vector2 topScreenPoint = new Vector2(Screen.width * 0.5f, Screen.height);
            Vector2 topLocalPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvasRect, topScreenPoint, canvasCamera, out topLocalPoint))
            {
                m_rectTransform.anchoredPosition = topLocalPoint + new Vector2(0f, -canvasHeight * bottomMargin) + anchoredOffset;
            }
        }
        else
        {
            m_rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            m_rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            m_rectTransform.pivot = new Vector2(0.5f, 0f);

            Camera canvasCamera = m_canvas != null && m_canvas.renderMode != RenderMode.ScreenSpaceOverlay ? m_canvas.worldCamera : null;
            Vector2 bottomScreenPoint = new Vector2(Screen.width * 0.5f, 0f);
            Vector2 bottomLocalPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvasRect, bottomScreenPoint, canvasCamera, out bottomLocalPoint))
            {
                m_rectTransform.anchoredPosition = bottomLocalPoint + new Vector2(0f, canvasHeight * bottomMargin) + anchoredOffset;
            }
        }

        m_rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
        m_rectTransform.localScale = Vector3.one;
        m_rectTransform.localPosition = new Vector3(m_rectTransform.localPosition.x, m_rectTransform.localPosition.y, zPosition);
        m_image.raycastTarget = false;

    }
}
