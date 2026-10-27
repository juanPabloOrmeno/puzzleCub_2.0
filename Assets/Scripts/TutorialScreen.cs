using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class TutorialScreen : MonoBehaviour
{
    [SerializeField] private Texture2D backgroundTexture;
    [SerializeField] private string returnSceneName = "SelectNivel";

    private Canvas m_canvas;
    private RectTransform m_returnButton;

    private void Awake()
    {
        m_canvas = GetComponent<Canvas>();
        BuildScreen();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ReturnToLevelSelect();
            return;
        }

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

    private void BuildScreen()
    {
        CreateBackground();
        CreateText("Title", "TUTORIAL", new Vector2(0f, 230f), new Vector2(540f, 90f), 62, new Color(1f, 0.81f, 0.3f));
        CreateText("ComingSoon", "PROXIMAMENTE", new Vector2(0f, 80f), new Vector2(540f, 80f), 48, Color.white);

        GameObject button = new GameObject("ReturnButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        button.transform.SetParent(transform, false);
        m_returnButton = button.GetComponent<RectTransform>();
        m_returnButton.anchorMin = new Vector2(0.5f, 0.5f);
        m_returnButton.anchorMax = new Vector2(0.5f, 0.5f);
        m_returnButton.pivot = new Vector2(0.5f, 0.5f);
        m_returnButton.anchoredPosition = new Vector2(0f, -310f);
        m_returnButton.sizeDelta = new Vector2(330f, 96f);

        Image buttonImage = button.GetComponent<Image>();
        buttonImage.color = new Color(0.19f, 0.08f, 0.32f, 0.94f);
        buttonImage.raycastTarget = false;

        CreateText("ReturnLabel", "REGRESAR", Vector2.zero, new Vector2(300f, 72f), 38, new Color(1f, 0.82f, 0.28f), button.transform);
    }

    private void CreateBackground()
    {
        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        background.transform.SetParent(transform, false);

        RectTransform rectTransform = background.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        RawImage image = background.GetComponent<RawImage>();
        image.texture = backgroundTexture;
        image.raycastTarget = false;
    }

    private void CreateText(string name, string value, Vector2 position, Vector2 size, int fontSize, Color color, Transform parent = null)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow));
        textObject.transform.SetParent(parent == null ? transform : parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.raycastTarget = false;

        Shadow shadow = textObject.GetComponent<Shadow>();
        shadow.effectColor = new Color(0.05f, 0.01f, 0.09f, 0.9f);
        shadow.effectDistance = new Vector2(3f, -3f);
    }

    private void HandlePress(Vector2 screenPosition)
    {
        Camera canvasCamera = m_canvas != null && m_canvas.renderMode != RenderMode.ScreenSpaceOverlay ? m_canvas.worldCamera : null;
        if (RectTransformUtility.RectangleContainsScreenPoint(m_returnButton, screenPosition, canvasCamera))
        {
            ReturnToLevelSelect();
        }
    }

    private void ReturnToLevelSelect()
    {
        SceneManager.LoadScene(returnSceneName);
    }
}
