using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays the current level's time limit inside the bottom-panel shield.</summary>
[RequireComponent(typeof(RectTransform))]
public class LevelCountdown : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private GameManager gameManager;
    private Text m_label;
    private RectTransform m_textRoot;
    private PuzzleTutorial m_tutorial;
    private bool m_ready;
    private bool m_applicationPaused;
    private int m_displayedSeconds = -1;

    public float RemainingSeconds { get; private set; }

    private IEnumerator Start()
    {
        // Board.Start selects/falls back to the actual level before its time is read.
        yield return null;
        if (board == null) board = FindAnyObjectByType<Board>();
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (board == null || gameManager == null || board.PracticeLevel != null) yield break;
        LevelData level = LevelDatabase.GetLevelData(board.Level);
        if (level == null) yield break;
        m_tutorial = board.GetComponent<PuzzleTutorial>();
        RemainingSeconds = Mathf.Max(0, level.time);
        m_textRoot = BottomPanelTextLayout.Create(transform, "Level Timer");
        m_label = CreateLabel("Countdown", new Vector2(.452f, .47f), new Vector2(.548f, .62f), 36);
        m_label.fontStyle = FontStyle.Bold;
        Text caption = CreateLabel("Timer Caption", new Vector2(.46f, .64f), new Vector2(.54f, .7f), 12);
        caption.text = "TIEMPO";
        caption.color = new Color(.85f, .71f, .44f);
        m_ready = true;
        RefreshDisplay();
    }

    private Text CreateLabel(string name, Vector2 min, Vector2 max, int size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
        go.layer = gameObject.layer;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(m_textRoot, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 6;
        text.resizeTextMaxSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        Outline outline = go.GetComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, .85f);
        outline.effectDistance = new Vector2(1, -1);
        return text;
    }

    private void Update()
    {
        if (!m_ready || gameManager.IsFinished || gameManager.m_pause || m_applicationPaused ||
            (m_tutorial != null && m_tutorial.IsOpen)) return;
        RemainingSeconds = Mathf.Max(0, RemainingSeconds - Time.deltaTime);
        RefreshDisplay();
        if (RemainingSeconds <= 0) gameManager.perder();
    }

    private void RefreshDisplay()
    {
        int seconds = Mathf.CeilToInt(RemainingSeconds);
        if (seconds != m_displayedSeconds)
        {
            m_label.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
            m_displayedSeconds = seconds;
        }
        m_label.color = seconds <= 10 ? new Color(1f, .35f, .25f) : new Color(1f, .91f, .65f);
        float pulse = seconds > 0 && seconds <= 10 ? 1f + .045f * Mathf.Sin(RemainingSeconds * Mathf.PI * 2f) : 1f;
        m_label.rectTransform.localScale = Vector3.one * pulse;
    }

    private void OnApplicationPause(bool paused)
    {
        m_applicationPaused = paused;
    }
}
