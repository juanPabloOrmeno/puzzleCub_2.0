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
        ApplyVisualStyle(new Color32(149, 154, 180, 119), Vector3.one);
    }

    private void ApplyVisualStyle(Color32 color, Vector3 scale)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        transform.localScale = scale;
    }
}
