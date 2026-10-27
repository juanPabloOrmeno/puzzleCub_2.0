using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>Guía creada en runtime para todas las escenas que usan Board.</summary>
[RequireComponent(typeof(Board))]
public class PuzzleTutorial : MonoBehaviour
{
    public const string SeenKey = "PuzzleCub.Tutorial.Seen.v1";
    public const string CompletedKey = "PuzzleCub.Tutorial.Completed.v1";

    private static readonly string[] Titles =
    {
        "Deja el tablero vacío", "Desliza, no intercambies", "Dos del mismo color", "Deja actuar a la gravedad", "¡Tu primer movimiento!"
    };

    private static readonly string[] Descriptions =
    {
        "El objetivo es eliminar todas las piezas.\n\nCuando el tablero queda vacío, ganas y desbloqueas el siguiente nivel.",
        "Mantén pulsada una pieza. Las casillas amarillas muestran sus destinos válidos.\n\nArrastra a una de ellas en la misma fila y suelta. El destino debe estar vacío.",
        "Junta DOS o más piezas del mismo color en una fila o columna para eliminarlas.\n\nLas diagonales no cuentan. No necesitas juntar tres.",
        "Después de mover, las piezas caen en los huecos y pueden formar nuevas combinaciones.\n\nLos obstáculos bloquean el paso y la caída. Espera a que termine la reacción.",
        "Busca dos piezas iguales y acerca una usando las casillas iluminadas.\n\nSi no hay destinos, prueba otra pieza. Puedes reiniciar desde el menú superior y consultar esta guía con «Ayuda»."
    };

    private Board m_board;
    private GameObject m_root;
    private RectTransform m_safeArea;
    private GameObject m_modal;
    private RectTransform m_diagram;
    private Text m_progress;
    private Text m_title;
    private Text m_description;
    private Text m_nextLabel;
    private Button m_back;
    private Button m_help;
    private Text m_hint;
    private GameObject m_hintPanel;
    private Font m_font;
    private int m_page;
    private int m_closedFrame = -1;
    private bool m_waitingForMove;
    private Coroutine m_hideHint;
    private Rect m_lastSafeArea;
    private Vector2Int m_screenSize;

    public bool IsOpen { get { return m_modal != null && m_modal.activeSelf; } }

    private void Start()
    {
        m_board = GetComponent<Board>();
        m_board.MoveResolved += OnMoveResolved;
        BuildUI();
        if (PlayerPrefs.GetInt(SeenKey, 0) == 0)
        {
            Open();
        }
    }

    private void Update()
    {
        if (m_root == null)
        {
            return;
        }

        UpdateSafeArea();
        m_help.gameObject.SetActive(!IsOpen && m_board.m_playerInputEnabled);
        if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close(false);
        }
    }

    public void Open()
    {
        // No interrumpir una caída o la pantalla de victoria.
        if (m_modal == null || !m_board.m_playerInputEnabled)
        {
            return;
        }

        m_board.CancelSelection();
        StopHint();
        m_waitingForMove = false;
        m_help.gameObject.SetActive(false);
        m_page = 0;
        m_modal.SetActive(true);
        ShowPage();
    }

    private void Close(bool completed)
    {
        m_board.CancelSelection();
        m_closedFrame = Time.frameCount;
        m_modal.SetActive(false);
        PlayerPrefs.SetInt(SeenKey, 1);
        if (completed)
        {
            PlayerPrefs.SetInt(CompletedKey, 1);
        }
        PlayerPrefs.Save();

        m_help.gameObject.SetActive(m_board.m_playerInputEnabled);
        m_waitingForMove = completed;
        if (completed)
        {
            m_hint.text = "Mantén pulsada una pieza, arrastra a una casilla amarilla y suelta.";
            m_hintPanel.SetActive(true);
        }
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public bool BlocksCurrentPointer()
    {
        if (Touchscreen.current != null &&
            (Touchscreen.current.primaryTouch.press.isPressed || Touchscreen.current.primaryTouch.press.wasReleasedThisFrame))
        {
            return BlocksPointer(Touchscreen.current.primaryTouch.position.ReadValue());
        }
        return BlocksPointer(Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(-1, -1));
    }

    public bool BlocksPointer(Vector2 position)
    {
        if (!isActiveAndEnabled)
        {
            return false;
        }
        return IsOpen || Time.frameCount == m_closedFrame ||
            (m_help != null && m_help.gameObject.activeInHierarchy &&
             RectTransformUtility.RectangleContainsScreenPoint((RectTransform)m_help.transform, position));
    }

    private void OnMoveResolved()
    {
        if (!m_waitingForMove)
        {
            return;
        }
        m_waitingForMove = false;
        if (!m_board.m_playerInputEnabled)
        {
            StopHint();
            return;
        }
        m_hint.text = "¡Bien! Ya sabes mover. Sigue combinando hasta vaciar el tablero.";
        m_hideHint = StartCoroutine(HideHintRoutine());
    }

    private IEnumerator HideHintRoutine()
    {
        yield return new WaitForSecondsRealtime(4f);
        m_hintPanel.SetActive(false);
        m_hideHint = null;
    }

    private void StopHint()
    {
        if (m_hideHint != null)
        {
            StopCoroutine(m_hideHint);
            m_hideHint = null;
        }
        m_hintPanel.SetActive(false);
    }

    private void ShowPage()
    {
        m_progress.text = "CÓMO JUGAR  ·  " + (m_page + 1) + " / " + Titles.Length;
        m_title.text = Titles[m_page];
        m_description.text = Descriptions[m_page];
        m_nextLabel.text = m_page == Titles.Length - 1 ? "¡A jugar!" : "Siguiente";
        m_back.interactable = m_page > 0;
        DrawDiagram();
    }

    private void Next()
    {
        if (m_page == Titles.Length - 1)
        {
            Close(true);
            return;
        }
        m_page++;
        ShowPage();
    }

    private void BuildUI()
    {
        if (EventSystem.current == null)
        {
            new GameObject("TutorialEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
        m_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        m_root = new GameObject("PuzzleTutorialCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        m_root.transform.SetParent(transform, false);
        Canvas canvas = m_root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        CanvasScaler scaler = m_root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        m_safeArea = Rect("SafeArea", m_root.transform, Vector2.zero, Vector2.one);
        m_help = MakeButton("Ayuda", m_safeArea, new Vector2(.78f, .025f), new Vector2(.98f, .075f), Open);

        m_hintPanel = Image("PracticeHint", m_safeArea, new Vector2(.04f, .09f), new Vector2(.96f, .18f), new Color(.07f, .10f, .20f, .96f)).gameObject;
        m_hintPanel.GetComponent<Image>().raycastTarget = false;
        m_hint = Label("Hint", m_hintPanel.transform, new Vector2(.04f, .08f), new Vector2(.96f, .92f), 30, TextAnchor.MiddleCenter);
        m_hintPanel.SetActive(false);

        m_modal = Rect("TutorialModal", m_root.transform, Vector2.zero, Vector2.one).gameObject;
        Image("Dim", m_modal.transform, Vector2.zero, Vector2.one, new Color(0f, .01f, .05f, .83f));
        // El panel comparte la misma área segura que el botón de ayuda.
        RectTransform modalSafe = Rect("SafeArea", m_modal.transform, Vector2.zero, Vector2.one);
        RectTransform panel = Image("Card", modalSafe, new Vector2(.04f, .12f), new Vector2(.96f, .88f), new Color(.08f, .11f, .22f, 1f)).rectTransform;
        Image("Accent", panel, new Vector2(0f, .985f), Vector2.one, new Color(1f, .78f, .22f));
        m_progress = Label("Progress", panel, new Vector2(.06f, .89f), new Vector2(.94f, .95f), 28, TextAnchor.MiddleLeft);
        m_progress.color = new Color(1f, .82f, .32f);
        m_title = Label("Title", panel, new Vector2(.06f, .77f), new Vector2(.94f, .88f), 54, TextAnchor.MiddleLeft);
        m_title.fontStyle = FontStyle.Bold;
        m_diagram = Rect("Diagram", panel, new Vector2(.08f, .51f), new Vector2(.92f, .75f));
        m_description = Label("Description", panel, new Vector2(.07f, .23f), new Vector2(.93f, .49f), 36, TextAnchor.MiddleLeft);
        m_back = MakeButton("Atrás", panel, new Vector2(.06f, .115f), new Vector2(.40f, .20f), () => { m_page--; ShowPage(); });
        Button next = MakeButton("Siguiente", panel, new Vector2(.44f, .115f), new Vector2(.94f, .20f), Next);
        next.GetComponent<Image>().color = new Color(.12f, .48f, .52f);
        m_nextLabel = next.GetComponentInChildren<Text>();
        MakeButton("Omitir / cerrar", panel, new Vector2(.28f, .025f), new Vector2(.72f, .085f), () => Close(false));
        m_modal.SetActive(false);
        UpdateSafeArea();
    }

    private void UpdateSafeArea()
    {
        Rect safe = Screen.safeArea;
        Vector2Int size = new Vector2Int(Screen.width, Screen.height);
        if (size.x <= 0 || size.y <= 0 || (safe == m_lastSafeArea && size == m_screenSize))
        {
            return;
        }
        m_lastSafeArea = safe;
        m_screenSize = size;
        Vector2 min = new Vector2(safe.xMin / size.x, safe.yMin / size.y);
        Vector2 max = new Vector2(safe.xMax / size.x, safe.yMax / size.y);
        m_safeArea.anchorMin = min;
        m_safeArea.anchorMax = max;
        RectTransform modalSafe = (RectTransform)m_modal.transform.Find("SafeArea");
        modalSafe.anchorMin = min;
        modalSafe.anchorMax = max;
    }

    private void DrawDiagram()
    {
        foreach (Transform child in m_diagram)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        string[] rows;
        switch (m_page)
        {
            case 1: rows = new[] { ".....", "R>+..", "#####" }; break;
            case 2: rows = new[] { "..Y..", "RRY..", "#####" }; break;
            case 3: rows = new[] { "R.#..", "v.#..", "+####" }; break;
            case 4: rows = new[] { ".....", "Y>+Y.", "#####" }; break;
            default: rows = new[] { ".....", ".....", "#####" }; break;
        }
        for (int y = 0; y < rows.Length; y++)
        {
            for (int x = 0; x < rows[y].Length; x++)
            {
                char cell = rows[y][x];
                Color color = cell == '#' ? new Color(.35f, .40f, .50f) :
                    cell == 'R' ? new Color(.86f, .22f, .32f) :
                    cell == 'Y' || cell == '+' ? new Color(.97f, .76f, .22f) : new Color(.15f, .20f, .33f);
                Vector2 min = new Vector2(x / 5f + .012f, (2 - y) / 3f + .025f);
                Vector2 max = new Vector2((x + 1) / 5f - .012f, (3 - y) / 3f - .025f);
                RectTransform tile = Image("ExampleCell", m_diagram, min, max, color).rectTransform;
                Text symbol = Label("Symbol", tile, Vector2.zero, Vector2.one, 36, TextAnchor.MiddleCenter);
                symbol.text = cell == 'R' ? "R" : cell == 'Y' ? "A" : cell == '#' ? "X" : cell == '+' ? "+" : cell == '>' ? "→" : cell == 'v' ? "↓" : "";
                if (cell == 'Y' || cell == '+') symbol.color = new Color(.12f, .16f, .25f);
            }
        }
    }

    private RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private Image Image(string name, Transform parent, Vector2 min, Vector2 max, Color color)
    {
        Image image = Rect(name, parent, min, max).gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private Text Label(string name, Transform parent, Vector2 min, Vector2 max, int size, TextAnchor alignment)
    {
        Text text = Rect(name, parent, min, max).gameObject.AddComponent<Text>();
        text.font = m_font;
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 14;
        text.resizeTextMaxSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private Button MakeButton(string caption, Transform parent, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        Image background = Image(caption, parent, min, max, new Color(.23f, .29f, .43f));
        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(action);
        Label("Label", background.transform, new Vector2(.04f, .04f), new Vector2(.96f, .96f), 32, TextAnchor.MiddleCenter).text = caption;
        return button;
    }

    private void OnDisable()
    {
        if (m_root != null)
        {
            m_root.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (m_root != null)
        {
            m_root.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (m_board != null)
        {
            m_board.MoveResolved -= OnMoveResolved;
        }
        if (m_root != null)
        {
            Destroy(m_root);
        }
    }
}
