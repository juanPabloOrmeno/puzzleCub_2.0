using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Three playable lessons using the same board rules as normal levels.</summary>
[RequireComponent(typeof(Canvas))]
public class TutorialScreen : MonoBehaviour
{
    [SerializeField] private string returnSceneName = "SelectNivel";
    [SerializeField] private GameObject normalTilePrefab;
    [SerializeField] private GameObject obstacleTilePrefab;

    private readonly string[] m_titles = { "DESLIZA Y COMBINA", "LA GRAVEDAD AYUDA", "LOS OBSTÁCULOS" };
    private readonly string[] m_instructions =
    {
        "Arrastra la roja izquierda junto a la otra roja.",
        "Acerca la roja izquierda a la derecha.\nLa verde caerá y se unirá a su pareja.",
        "Desliza la roja de arriba dos casillas a la izquierda.\nCaerá junto a su pareja."
    };
    private Board m_board;
    private Camera m_camera;
    private Camera m_backgroundCamera;
    private RectTransform m_safeRoot;
    private RectTransform m_returnButton;
    private RectTransform m_retryButton;
    private RectTransform m_nextButton;
    private Text m_title;
    private Text m_description;
    private Text m_status;
    private Text m_nextLabel;
    private int m_lesson;
    private bool m_completed;

    private void Start()
    {
        // Clear the entire display before drawing the board's smaller viewport.
        // Otherwise pixels from the previous menu remain outside that viewport.
        GameObject backgroundObject = new GameObject("Tutorial Background", typeof(Camera));
        m_backgroundCamera = backgroundObject.GetComponent<Camera>();
        m_backgroundCamera.depth = -100;
        m_backgroundCamera.cullingMask = 0;
        m_backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        m_backgroundCamera.backgroundColor = new Color(0.035f, 0.045f, 0.09f);
        GameObject cameraObject = new GameObject("Tutorial Camera", typeof(Camera));
        cameraObject.tag = "MainCamera";
        m_camera = cameraObject.GetComponent<Camera>();
        m_camera.orthographic = true;
        m_camera.clearFlags = CameraClearFlags.SolidColor;
        m_camera.backgroundColor = m_backgroundCamera.backgroundColor;
        BuildScreen();
        BeginLesson();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ReturnToLevelSelect();
            return;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touch = Touchscreen.current.primaryTouch;
            Vector2 position = touch.position.ReadValue();
            if (touch.press.wasPressedThisFrame && HandleButton(position)) return;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            HandleButton(Mouse.current.position.ReadValue());
    }

    private void LateUpdate()
    {
        Rect safe = Screen.safeArea;
        m_safeRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        m_safeRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
        m_camera.rect = new Rect(safe.x / Screen.width, (safe.y + safe.height * 0.29f) / Screen.height,
            safe.width / Screen.width, safe.height * 0.43f / Screen.height);
        m_camera.orthographicSize = Mathf.Max(2f, 3f / m_camera.aspect);
        m_camera.transform.position = new Vector3(2f, 1f, -10f);
    }

    private void BeginLesson()
    {
        if (m_board != null)
        {
            m_board.MoveResolved -= OnMoveResolved;
            m_board.gameObject.SetActive(false);
            Destroy(m_board.gameObject);
        }
        m_completed = false;
        m_title.text = (m_lesson + 1) + " / 3 · " + m_titles[m_lesson];
        m_description.text = m_instructions[m_lesson];
        m_status.text = m_lesson == 0 ? "Une 2 del mismo color para eliminarlas." :
            m_lesson == 1 ? "Las piezas caen al quedar un hueco." : "Los bloques oscuros cortan el paso.";
        m_status.color = new Color(0.65f, 0.73f, 0.86f);
        m_nextButton.gameObject.SetActive(false);

        LevelData data = new LevelData { width = 5, height = 3, borderSize = 0.5f };
        AddPiece(data, "Red", m_lesson == 1 ? 1 : 0, 0);
        if (m_lesson == 0) AddPiece(data, "Red", 4, 0);
        if (m_lesson == 1)
        {
            AddPiece(data, "Red", 4, 0);
            AddPiece(data, "Green", 1, 1);
            AddPiece(data, "Green", 0, 0);
        }
        if (m_lesson == 2)
        {
            AddPiece(data, "Red", 3, 1);
            data.tiles.Add(new LevelTileData { type = "Obstacle", x = 3, y = 0 });
        }

        GameObject boardObject = new GameObject("Tutorial Board");
        boardObject.SetActive(false);
        m_board = boardObject.AddComponent<Board>();
        m_board.tileNormalPrefab = normalTilePrefab;
        m_board.tileObtaclePrefab = obstacleTilePrefab;
        m_board.PracticeLevel = data;
        m_board.MoveResolved += OnMoveResolved;
        boardObject.SetActive(true);
    }

    private static void AddPiece(LevelData data, string type, int x, int y)
    {
        data.pieces.Add(new LevelPieceData { type = type, x = x, y = y });
    }

    private void OnMoveResolved()
    {
        m_completed = !m_board.m_playerInputEnabled;
        if (!m_completed)
        {
            m_status.text = "¡Bien! Sigue combinando colores.";
            return;
        }
        m_status.text = m_lesson == 2 ? "¡Listo! Ya sabes jugar." : "¡Perfecto! Tablero completado.";
        m_status.color = new Color(0.4f, 0.9f, 0.7f);
        m_nextLabel.text = m_lesson == 2 ? "JUGAR" : "SIGUIENTE";
        m_nextButton.gameObject.SetActive(true);
    }

    private void BuildScreen()
    {
        m_safeRoot = new GameObject("Safe Area", typeof(RectTransform)).GetComponent<RectTransform>();
        m_safeRoot.SetParent(transform, false);
        m_safeRoot.offsetMin = m_safeRoot.offsetMax = Vector2.zero;
        Text eyebrow = CreateText("Section", "APRENDE JUGANDO", new Vector2(.08f, .92f), new Vector2(.92f, .96f), 15);
        eyebrow.color = new Color(.45f, .72f, .85f);
        m_title = CreateText("Lesson", "", new Vector2(.06f, .83f), new Vector2(.94f, .91f), 27);
        m_title.fontStyle = FontStyle.Bold;
        m_description = CreateText("Instructions", "", new Vector2(.1f, .73f), new Vector2(.9f, .82f), 21);
        m_description.color = new Color(.75f, .81f, .9f);
        m_status = CreateText("Status", "", new Vector2(.08f, .21f), new Vector2(.92f, .28f), 18);
        m_returnButton = CreateButton("Salir", .08f, .46f, out _);
        m_retryButton = CreateButton("Reintentar", .54f, .92f, out _);
        m_nextButton = CreateButton("SIGUIENTE", .18f, .82f, out m_nextLabel);
        m_nextButton.anchorMin = new Vector2(.18f, .12f);
        m_nextButton.anchorMax = new Vector2(.82f, .19f);
        m_nextButton.GetComponent<Image>().color = new Color(.12f, .48f, .48f);
    }

    private Text CreateText(string name, string value, Vector2 min, Vector2 max, int size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(m_safeRoot, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private RectTransform CreateButton(string label, float left, float right, out Text text)
    {
        GameObject go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(m_safeRoot, false);
        rect.anchorMin = new Vector2(left, 0.025f);
        rect.anchorMax = new Vector2(right, 0.1f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(.09f, .12f, .2f);
        text = CreateText(label + " Label", label, Vector2.zero, Vector2.one, 18);
        text.transform.SetParent(rect, false);
        return rect;
    }

    private bool HandleButton(Vector2 position)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(m_returnButton, position))
        {
            ReturnToLevelSelect();
            return true;
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(m_retryButton, position))
        {
            BeginLesson();
            return true;
        }
        if (m_completed && RectTransformUtility.RectangleContainsScreenPoint(m_nextButton, position))
        {
            if (m_lesson == 2) ReturnToLevelSelect();
            else { m_lesson++; BeginLesson(); }
            return true;
        }
        return false;
    }

    private void ReturnToLevelSelect()
    {
        SceneManager.LoadScene(returnSceneName);
    }

    private void OnDestroy()
    {
        if (m_board != null) m_board.MoveResolved -= OnMoveResolved;
        if (m_camera != null) Destroy(m_camera.gameObject);
        if (m_backgroundCamera != null) Destroy(m_backgroundCamera.gameObject);
    }
}
