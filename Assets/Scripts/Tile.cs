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

    void OnMouseDown()
    {
        if (m_board != null)
        {
            m_board.ClickTile(this);
        }
    }

    void OnMouseEnter()
    {
        if (m_board != null)
        {
            m_board.DragToTile(this);
        }
    }

    void OnMouseUp()
    {
        if (m_board != null)
        {
            m_board.ReleaseTile();
        }
    }

    public void tileColor()
    {
        ApplyVisualStyle(new Color32(219, 197, 86, 162), new Vector3(0.9f, 0.9f, 1f));
    }

    public void tileColorN()
    {
        ApplyVisualStyle(m_defaultColor, m_defaultScale);
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
