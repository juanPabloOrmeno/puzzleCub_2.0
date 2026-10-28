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
    [Header("Tutorial artwork")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Sprite titlePanelSprite;
    [SerializeField] private Sprite instructionPanelSprite;
    [SerializeField] private Sprite activeStepSprite;
    [SerializeField] private Sprite inactiveStepSprite;
    [SerializeField] private Sprite handSprite;
    [SerializeField] private Sprite boardFrameSprite;
    [SerializeField] private Sprite topPanelSprite;
    [SerializeField] private Sprite bottomPanelSprite;
    [SerializeField] private Sprite girlSprite;
    [SerializeField] private Sprite boySprite;
    [SerializeField] private Font tutorialFont;

    private readonly string[] m_titles = { "DESLIZA Y COMBINA", "LA GRAVEDAD AYUDA", "LOS OBSTÁCULOS" };
    private readonly string[] m_instructions =
    {
        "<b>Arrastra la gema roja</b>\nhasta la casilla iluminada.",
        "<b>Junta las gemas rojas.</b>\nLa verde caerá junto a su pareja.",
        "<b>Desliza la roja hacia la izquierda.</b>\nCaerá junto a su pareja."
    };
    private Board m_board;
    private Camera m_camera;
    private Camera m_backgroundCamera;
    private RectTransform m_safeRoot;
    private RectTransform m_returnButton;
    private RectTransform m_retryButton;
    private RectTransform m_nextButton;
    private RectTransform m_skipButton;
    private RectTransform m_boardFrame;
    private RectTransform m_hand;
    private TutorialMoveGuide m_guide;
    private Text m_pieces;
    private Text m_moves;
    private Text m_step;
    private SpriteRenderer m_backdrop;
    private Text m_title;
    private Text m_description;
    private Text m_status;
    private Text m_nextLabel;
    private int m_lesson;
    private bool m_completed;
    private SoundManager m_soundManager;
    private readonly Image[] m_progress = new Image[3];

    private void Start()
    {
        // Clear the entire display before drawing the board's smaller viewport.
        // Otherwise pixels from the previous menu remain outside that viewport.
        GameObject backgroundObject = new GameObject("Tutorial Background", typeof(Camera));
        m_backgroundCamera = backgroundObject.GetComponent<Camera>();
        m_backgroundCamera.depth = -100;
        m_backgroundCamera.cullingMask = 1 << 30;
        m_backgroundCamera.orthographic = true;
        m_backgroundCamera.orthographicSize = 5;
        m_backgroundCamera.transform.position = new Vector3(0, 0, -20);
        m_backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        m_backgroundCamera.backgroundColor = new Color(0.035f, 0.045f, 0.09f);
        GameObject backdrop = new GameObject("Sunset Ruins", typeof(SpriteRenderer));
        backdrop.layer = 30;
        backdrop.transform.SetParent(backgroundObject.transform, false);
        backdrop.transform.localPosition = new Vector3(0, 0, 10);
        m_backdrop = backdrop.GetComponent<SpriteRenderer>();
        m_backdrop.sprite = backgroundSprite;
        GameObject cameraObject = new GameObject("Tutorial Camera", typeof(Camera));
        cameraObject.tag = "MainCamera";
        m_camera = cameraObject.GetComponent<Camera>();
        m_camera.orthographic = true;
        m_camera.clearFlags = CameraClearFlags.Depth;
        m_camera.cullingMask &= ~(1 << 30);
        m_camera.backgroundColor = m_backgroundCamera.backgroundColor;
        if (FindAnyObjectByType<AudioListener>() == null)
            cameraObject.AddComponent<AudioListener>();
        m_soundManager = SoundManager.Instance;
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
        float boardWidth = Mathf.Min(safe.width * .9f, safe.height * .285f * 5f / 3f);
        float boardHeight = boardWidth * 3f / 5f;
        Rect boardRect = new Rect(safe.center.x - boardWidth * .5f,
            safe.y + safe.height * .46f - boardHeight * .5f, boardWidth, boardHeight);
        m_camera.rect = new Rect(boardRect.x / Screen.width, boardRect.y / Screen.height,
            boardRect.width / Screen.width, boardRect.height / Screen.height);
        m_camera.orthographicSize = 1.5f;
        m_camera.transform.position = new Vector3(2f, 1f, -10f);
        Vector2 min = new Vector2((boardRect.xMin - safe.x) / safe.width, (boardRect.yMin - safe.y) / safe.height);
        Vector2 max = new Vector2((boardRect.xMax - safe.x) / safe.width, (boardRect.yMax - safe.y) / safe.height);
        m_guide.rectTransform.anchorMin = min;
        m_guide.rectTransform.anchorMax = max;
        Vector2 padding = new Vector2(boardWidth * .018f / safe.width, boardHeight * .03f / safe.height);
        m_boardFrame.anchorMin = min - padding;
        m_boardFrame.anchorMax = max + padding;
        if (m_backdrop.sprite != null)
        {
            Vector2 size = m_backdrop.sprite.bounds.size;
            float cover = Mathf.Max(10f * m_backgroundCamera.aspect / size.x, 10f / size.y);
            m_backdrop.transform.localScale = Vector3.one * cover;
        }
        bool showGuide = m_board != null && !m_completed && m_board.MovesMade == 0;
        m_guide.gameObject.SetActive(showGuide);
        if (showGuide)
        {
            float t = Mathf.SmoothStep(0, 1, Mathf.Repeat(Time.unscaledTime * .45f, 1f));
            Vector2 point = Vector2.Lerp(m_guide.Source, m_guide.Target, t);
            m_hand.anchorMin = m_hand.anchorMax = new Vector2((point.x + .5f) / 5f, (point.y + .5f) / 3f);
            float handSize = m_guide.rectTransform.rect.width * .16f;
            m_hand.sizeDelta = new Vector2(handSize, handSize);
        }
    }

    private void BeginLesson()
    {
        if (m_board != null)
        {
            m_board.MoveResolved -= OnMoveResolved;
            m_board.StatsChanged -= RefreshStats;
            m_board.gameObject.SetActive(false);
            Destroy(m_board.gameObject);
        }
        m_completed = false;
        m_title.text = m_titles[m_lesson];
        m_description.text = m_instructions[m_lesson];
        m_status.text = m_lesson == 0 ? "<b>Junta 2 gemas del mismo color</b>\npara eliminarlas." :
            m_lesson == 1 ? "<b>La gravedad también combina.</b>\nLas gemas caen cuando queda un hueco." : "<b>Busca un camino alrededor del bloque.</b>\nLos obstáculos cortan el paso.";
        m_status.color = new Color(1f, .94f, .76f);
        m_step.text = "<size=14>PASO</size>\n" + (m_lesson + 1) + "/3";
        m_guide.Source = m_lesson == 0 ? new Vector2(0, 0) : m_lesson == 1 ? new Vector2(1, 0) : new Vector2(3, 1);
        m_guide.Target = m_lesson == 2 ? new Vector2(2, 1) : new Vector2(3, 0);
        m_nextButton.gameObject.SetActive(false);
        UpdateProgress();

        LevelData data = new LevelData { width = 5, height = 3, borderSize = 0.5f };
        AddPiece(data, "Red", m_lesson == 0 ? 0 : 1, 0);
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
        m_board.StatsChanged += RefreshStats;
        boardObject.SetActive(true);
        RefreshStats();
    }

    private void RefreshStats()
    {
        m_pieces.text = "PIEZAS: " + m_board.RemainingPieces;
        m_moves.text = "MOVIMIENTOS: " + m_board.MovesMade;
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
        UpdateProgress();
        m_soundManager.PlaySoundWinner();
    }

    private void UpdateProgress()
    {
        for (int i = 0; i < m_progress.Length; i++)
        {
            m_progress[i].sprite = i <= m_lesson ? activeStepSprite : inactiveStepSprite;
            m_progress[i].GetComponentInChildren<Text>().color = i <= m_lesson
                ? new Color(1f, .94f, .65f) : new Color(.5f, .5f, .5f);
        }
    }

    private void PlayButtonSound()
    {
        if (m_soundManager.pieceSound != null && m_soundManager.pieceSound.Length > 0)
            m_soundManager.PlayClipAtPoint(m_soundManager.pieceSound[0], Vector3.zero, m_soundManager.fxVolume * .2f);
    }

    private void BuildScreen()
    {
        m_safeRoot = new GameObject("Safe Area", typeof(RectTransform)).GetComponent<RectTransform>();
        m_safeRoot.SetParent(transform, false);
        m_safeRoot.offsetMin = m_safeRoot.offsetMax = Vector2.zero;
        CreateImage("Top Panel", topPanelSprite, new Vector2(.15f, .88f), new Vector2(.85f, .995f));
        CreateImage("Title Frame", titlePanelSprite, new Vector2(.1f, .765f), new Vector2(.9f, .89f));
        Text eyebrow = CreateText("Section", "— APRENDE JUGANDO —", new Vector2(.16f, .848f), new Vector2(.84f, .873f), 16);
        eyebrow.color = new Color(.45f, .72f, .85f);
        m_title = CreateText("Lesson", "", new Vector2(.14f, .795f), new Vector2(.86f, .846f), 30);
        m_title.fontStyle = FontStyle.Bold;
        CreateImage("Instruction Frame", instructionPanelSprite, new Vector2(.075f, .615f), new Vector2(.925f, .715f));
        m_description = CreateText("Instructions", "", new Vector2(.12f, .635f), new Vector2(.88f, .695f), 25);
        m_boardFrame = CreateImage("Board Frame", boardFrameSprite, Vector2.zero, Vector2.one).rectTransform;
        GameObject guide = new GameObject("Move Guide", typeof(RectTransform), typeof(TutorialMoveGuide));
        m_guide = guide.GetComponent<TutorialMoveGuide>();
        m_guide.rectTransform.SetParent(m_safeRoot, false);
        m_guide.rectTransform.offsetMin = m_guide.rectTransform.offsetMax = Vector2.zero;
        m_guide.raycastTarget = false;
        m_hand = CreateImage("Drag Hand", handSprite, Vector2.zero, Vector2.zero).rectTransform;
        m_hand.SetParent(m_guide.transform, false);
        m_hand.pivot = new Vector2(.18f, .88f);
        m_hand.GetComponent<Image>().preserveAspect = true;
        CreateImage("Hint Frame", instructionPanelSprite, new Vector2(.1f, .205f), new Vector2(.9f, .29f));
        m_status = CreateText("Status", "", new Vector2(.14f, .225f), new Vector2(.86f, .275f), 23);
        CreateImage("Bottom Panel", bottomPanelSprite, new Vector2(.035f, .045f), new Vector2(.965f, .177f));
        CreateImage("Girl", girlSprite, new Vector2(.22f, .075f), new Vector2(.37f, .165f)).preserveAspect = true;
        CreateImage("Boy", boySprite, new Vector2(.63f, .075f), new Vector2(.78f, .165f)).preserveAspect = true;
        m_step = CreateText("Step", "", new Vector2(.445f, .093f), new Vector2(.555f, .148f), 27);
        m_step.fontStyle = FontStyle.Bold;
        m_pieces = CreateText("Pieces", "", new Vector2(.15f, .055f), new Vector2(.405f, .076f), 17);
        m_moves = CreateText("Moves", "", new Vector2(.595f, .055f), new Vector2(.85f, .076f), 17);
        m_retryButton = CreateHitArea("Retry", new Vector2(.2f, .9f), new Vector2(.4f, .98f));
        m_returnButton = CreateHitArea("Home", new Vector2(.6f, .9f), new Vector2(.8f, .98f));
        m_skipButton = CreateText("Skip Tutorial", "Omitir tutorial", new Vector2(.25f, .007f), new Vector2(.75f, .04f), 17).rectTransform;
        m_nextButton = CreateButton("SIGUIENTE", .18f, .82f, out m_nextLabel);
        m_nextButton.anchorMin = new Vector2(.3f, .175f);
        m_nextButton.anchorMax = new Vector2(.7f, .21f);
        for (int i = 0; i < m_progress.Length; i++)
        {
            GameObject segment = new GameObject("Lesson Progress " + (i + 1), typeof(RectTransform), typeof(Image));
            RectTransform rect = segment.GetComponent<RectTransform>();
            rect.SetParent(m_safeRoot, false);
            rect.anchorMin = new Vector2(.29f + i * .16f, .745f);
            rect.anchorMax = new Vector2(.39f + i * .16f, .805f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            m_progress[i] = segment.GetComponent<Image>();
            m_progress[i].raycastTarget = false;
            m_progress[i].preserveAspect = true;
            Text number = CreateText("Step " + (i + 1), (i + 1).ToString(), new Vector2(.2f, .15f), new Vector2(.8f, .85f), 28);
            number.transform.SetParent(rect, false);
            number.fontStyle = FontStyle.Bold;
        }
    }

    private RectTransform CreateHitArea(string name, Vector2 min, Vector2 max)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(m_safeRoot, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    private Image CreateImage(string name, Sprite sprite, Vector2 min, Vector2 max)
    {
        RectTransform rect = CreateHitArea(name, min, max);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
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
        text.font = tutorialFont != null ? tutorialFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 6;
        text.resizeTextMaxSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, .94f, .76f);
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, .85f);
        outline.effectDistance = new Vector2(.5f, -.5f);
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
        go.GetComponent<Image>().sprite = instructionPanelSprite;
        go.GetComponent<Image>().raycastTarget = false;
        text = CreateText(label + " Label", label, Vector2.zero, Vector2.one, 18);
        text.transform.SetParent(rect, false);
        return rect;
    }

    private bool HandleButton(Vector2 position)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(m_returnButton, position) ||
            RectTransformUtility.RectangleContainsScreenPoint(m_skipButton, position))
        {
            ReturnToLevelSelect();
            return true;
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(m_retryButton, position))
        {
            PlayButtonSound();
            BeginLesson();
            return true;
        }
        if (m_completed && RectTransformUtility.RectangleContainsScreenPoint(m_nextButton, position))
        {
            PlayButtonSound();
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
        if (m_board != null)
        {
            m_board.MoveResolved -= OnMoveResolved;
            m_board.StatsChanged -= RefreshStats;
        }
        if (m_camera != null) Destroy(m_camera.gameObject);
        if (m_backgroundCamera != null) Destroy(m_backgroundCamera.gameObject);
    }
}
