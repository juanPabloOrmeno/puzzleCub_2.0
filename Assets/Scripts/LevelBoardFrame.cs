using UnityEngine;
using UnityEngine.UI;

/// <summary>Fits a decorative frame and level plaque to the loaded board dimensions.</summary>
[RequireComponent(typeof(Board))]
public class LevelBoardFrame : MonoBehaviour
{
    [SerializeField] private Sprite frameSprite;
    [SerializeField] private Sprite plaqueSprite;
    [SerializeField] private Font labelFont;
    private Board m_board;
    private SpriteRenderer m_frame;
    private RectTransform m_plaque;
    private Text m_label;
    private Vector3Int m_lastLayout;

    private void Start()
    {
        m_board = GetComponent<Board>();
        if (m_board.PracticeLevel != null) return;
        GameObject frame = new GameObject("Golden Board Frame", typeof(SpriteRenderer));
        frame.transform.SetParent(transform, false);
        m_frame = frame.GetComponent<SpriteRenderer>();
        m_frame.sprite = frameSprite;
        m_frame.drawMode = SpriteDrawMode.Sliced;
        m_frame.sortingOrder = 8;

        GameObject plaque = new GameObject("Level Plaque", typeof(RectTransform), typeof(Canvas), typeof(Image));
        m_plaque = plaque.GetComponent<RectTransform>();
        m_plaque.SetParent(transform, false);
        m_plaque.sizeDelta = new Vector2(600, 120);
        Canvas canvas = plaque.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 9;
        Image image = plaque.GetComponent<Image>();
        image.sprite = plaqueSprite;
        image.raycastTarget = false;

        GameObject label = new GameObject("Level Number", typeof(RectTransform), typeof(Text), typeof(Outline));
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.SetParent(m_plaque, false);
        rect.anchorMin = new Vector2(.1f, .15f);
        rect.anchorMax = new Vector2(.9f, .85f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        m_label = label.GetComponent<Text>();
        m_label.font = labelFont != null ? labelFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        m_label.fontSize = 64;
        m_label.fontStyle = FontStyle.Bold;
        m_label.resizeTextForBestFit = true;
        m_label.resizeTextMinSize = 20;
        m_label.resizeTextMaxSize = 64;
        m_label.alignment = TextAnchor.MiddleCenter;
        m_label.color = new Color(1f, .93f, .7f);
        m_label.raycastTarget = false;
        label.GetComponent<Outline>().effectColor = new Color(0, 0, 0, .8f);
        ApplyLayout();
    }

    private void LateUpdate()
    {
        if (m_frame != null && m_lastLayout != new Vector3Int(m_board.width, m_board.height, m_board.Level))
            ApplyLayout();
    }

    private void ApplyLayout()
    {
        m_lastLayout = new Vector3Int(m_board.width, m_board.height, m_board.Level);
        m_frame.transform.localPosition = new Vector3((m_board.width - 1) * .5f, (m_board.height - 1) * .5f, -.1f);
        m_frame.size = new Vector2(m_board.width + .38f, m_board.height + .46f);
        float plaqueWidth = Mathf.Clamp(m_board.width * .42f, 1.8f, 3f);
        m_plaque.localScale = Vector3.one * (plaqueWidth / 600f);
        m_plaque.localPosition = new Vector3((m_board.width - 1) * .5f, m_board.height - .5f + .18f + plaqueWidth * .1f, -.2f);
        m_label.text = "NIVEL " + m_board.Level;
    }
}
