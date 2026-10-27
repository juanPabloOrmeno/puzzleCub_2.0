using UnityEngine;

public enum TileType
{
    Normal,
    Obstacle
}

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public TileType tileType = TileType.Normal;
    public InterpType interpolation = InterpType.SmootherStep;

    private Board m_board;
    private Color m_defaultColor = Color.white;
    private Vector3 m_defaultScale = Vector3.one;
    private SpriteRenderer m_spriteRenderer;
    private bool m_highlighted;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        if (m_spriteRenderer != null)
        {
            m_defaultColor = m_spriteRenderer.color;
        }

        m_defaultScale = transform.localScale;
    }

    public enum InterpType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    }

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
    }

    public void tileColor()
    {
        m_highlighted = true;
        ApplyVisualStyle(new Color32(255, 211, 64, 255), m_defaultScale);
    }

    public void tileColorN()
    {
        m_highlighted = false;
        ApplyVisualStyle(m_defaultColor, m_defaultScale);
    }

    private void Update()
    {
        if (!m_highlighted) return;
        float pulse = .5f + .5f * Mathf.Sin(Time.time * 5f - xIndex * .4f);
        Color color = Color.Lerp(new Color(1f, .62f, .14f), new Color(1f, .9f, .42f), pulse);
        ApplyVisualStyle(color, m_defaultScale * (1f + .025f * pulse));
    }

    private void ApplyVisualStyle(Color color, Vector3 scale)
    {
        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.color = color;
        }

        transform.localScale = scale;
    }
}
