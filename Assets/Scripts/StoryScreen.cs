using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Shows the story one image per tap, then starts level one.</summary>
public class StoryScreen : MonoBehaviour
{
    [SerializeField] private Texture2D[] pages;
    [SerializeField] private string gameplaySceneName = "Level";
    private RawImage m_page;
    private RectTransform m_canvasRect;
    private int m_pageIndex;
    private int m_firstInputFrame;
    private bool m_loading;

    private void Start()
    {
        m_firstInputFrame = Time.frameCount + 1;
        var cameraObject = new GameObject("Story Camera", typeof(Camera));
        cameraObject.transform.SetParent(transform, false);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.cullingMask = 0;
        if (FindAnyObjectByType<AudioListener>() == null)
            cameraObject.AddComponent<AudioListener>();

        var canvasObject = new GameObject("Story Canvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        m_canvasRect = (RectTransform)canvasObject.transform;
        var pageObject = new GameObject("Story Image", typeof(RectTransform), typeof(RawImage));
        pageObject.transform.SetParent(canvasObject.transform, false);
        m_page = pageObject.GetComponent<RawImage>();
        m_page.raycastTarget = false;
        if (pages == null || pages.Length == 0)
        {
            StartFirstLevel();
            return;
        }
        m_page.texture = pages[0];
        FitPage();
    }

    private void Update()
    {
        // Ignore the click that opened this scene, and consume only one input source per frame.
        if (m_loading || Time.frameCount <= m_firstInputFrame) return;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            AdvancePage();
            return;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            AdvancePage();
    }

    private void LateUpdate() => FitPage();

    /// <summary>Advances one page, or enters the first level after the final page.</summary>
    public void AdvancePage()
    {
        if (m_loading || m_page == null) return;
        m_pageIndex++;
        if (m_pageIndex >= pages.Length)
        {
            StartFirstLevel();
            return;
        }
        m_page.texture = pages[m_pageIndex];
        FitPage();
    }

    private void FitPage()
    {
        if (m_page == null || m_page.texture == null || Screen.width <= 0 || Screen.height <= 0) return;
        Rect safe = Screen.safeArea;
        Rect canvas = m_canvasRect.rect;
        Vector2 available = new Vector2(canvas.width * safe.width / Screen.width,
            canvas.height * safe.height / Screen.height);
        Vector2 imageSize = new Vector2(m_page.texture.width, m_page.texture.height);
        float scale = Mathf.Min(available.x / imageSize.x, available.y / imageSize.y);
        RectTransform rect = m_page.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(safe.center.x / Screen.width, safe.center.y / Screen.height);
        rect.pivot = Vector2.one * .5f;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = imageSize * scale;
    }

    private void StartFirstLevel()
    {
        if (m_loading) return;
        m_loading = true;
        LevelDatabase.SelectLevel(1);
        SceneManager.LoadScene(gameplaySceneName);
    }
}
