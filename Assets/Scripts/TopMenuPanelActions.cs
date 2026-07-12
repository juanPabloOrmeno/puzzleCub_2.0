using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(RectTransform))]
public class TopMenuPanelActions : MonoBehaviour
{
    [SerializeField] private string levelSelectSceneName = "SelectNivel";
    [SerializeField, Range(0.1f, 0.5f)] private float buttonZoneWidth = 0.42f;

    private RectTransform m_rectTransform;
    private Canvas m_canvas;

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleScreenPress(Mouse.current.position.ReadValue());
            return;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            HandleScreenPress(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void HandleScreenPress(Vector2 screenPosition)
    {
        if (m_rectTransform == null)
        {
            return;
        }

        Camera canvasCamera = m_canvas != null && m_canvas.renderMode != RenderMode.ScreenSpaceOverlay ? m_canvas.worldCamera : null;
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectTransform, screenPosition, canvasCamera, out localPoint))
        {
            return;
        }

        Rect rect = m_rectTransform.rect;
        if (!rect.Contains(localPoint))
        {
            return;
        }

        float leftLimit = rect.xMin + rect.width * buttonZoneWidth;
        float rightLimit = rect.xMax - rect.width * buttonZoneWidth;
        if (localPoint.x <= leftLimit)
        {
            RestartLevel();
        }
        else if (localPoint.x >= rightLimit)
        {
            GoToLevelSelect();
        }
    }

    private void RestartLevel()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private void GoToLevelSelect()
    {
        SceneManager.LoadScene(levelSelectSceneName);
    }
}
