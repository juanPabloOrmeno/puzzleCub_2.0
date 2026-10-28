using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows level, remaining pieces and valid moves in the bottom panel.</summary>
[RequireComponent(typeof(RectTransform))]
public class LevelStatsDisplay : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private Font labelFont;
    [SerializeField] private bool showLevelLabel = true;
    [SerializeField] private Vector2 counterVerticalRange = new Vector2(.075f, .19f);
    private Text m_level;
    private Text m_pieces;
    private Text m_moves;
    private RectTransform m_textRoot;

    private void Start()
    {
        if (board == null) board = FindAnyObjectByType<Board>();
        if (board == null || board.PracticeLevel != null) return;
        m_textRoot = BottomPanelTextLayout.Create(transform, "Level Statistics");
        if (showLevelLabel)
            m_level = CreateLabel("Level Number", new Vector2(.34f, 1.02f), new Vector2(.66f, 1.17f), 24);
        m_pieces = CreateLabel("Pieces Remaining", new Vector2(.12f, counterVerticalRange.x), new Vector2(.405f, counterVerticalRange.y), 23);
        m_moves = CreateLabel("Moves Made", new Vector2(.595f, counterVerticalRange.x), new Vector2(.88f, counterVerticalRange.y), 23);
        board.StatsChanged += Refresh;
        Refresh();
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
        text.font = labelFont != null ? labelFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 8;
        text.resizeTextMaxSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, .93f, .75f);
        text.raycastTarget = false;
        Outline outline = go.GetComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, .85f);
        outline.effectDistance = new Vector2(1, -1);
        return text;
    }

    private void Refresh()
    {
        if (m_level != null) m_level.text = "NIVEL " + board.Level;
        m_pieces.text = "PIEZAS: " + board.RemainingPieces;
        m_moves.text = "MOVIMIENTOS: " + board.MovesMade;
    }

    private void OnDestroy()
    {
        if (board != null) board.StatsChanged -= Refresh;
    }
}
