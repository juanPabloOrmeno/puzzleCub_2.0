using UnityEngine;

/// <summary>Lightweight board framing and completion feedback shared by levels and practice.</summary>
[RequireComponent(typeof(Board))]
public class BoardVisualEffects : MonoBehaviour
{
    private Board m_board;
    private SpriteRenderer m_frame;
    private Texture2D m_texture;
    private Sprite m_sprite;
    private float m_flash;

    private void Start()
    {
        m_board = GetComponent<Board>();
        const int size = 128;
        m_texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        m_texture.name = "Board Glow";
        m_texture.wrapMode = TextureWrapMode.Clamp;
        m_texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Vector2 q = new Vector2(Mathf.Abs(x - 63.5f), Mathf.Abs(y - 63.5f)) - new Vector2(49, 49);
            float distance = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude
                + Mathf.Min(Mathf.Max(q.x, q.y), 0) - 8;
            float edge = Mathf.Abs(distance);
            float alpha = .65f * Mathf.Exp(-edge * edge / 2f) + .22f * Mathf.Exp(-edge * edge / 24f);
            pixels[y * size + x] = new Color(1, 1, 1, alpha);
        }
        m_texture.SetPixels(pixels);
        m_texture.Apply(false, true);
        m_sprite = Sprite.Create(m_texture, new Rect(0, 0, size, size), Vector2.one * .5f, size,
            0, SpriteMeshType.FullRect, new Vector4(20, 20, 20, 20));
        GameObject frame = new GameObject("Board Glow Frame", typeof(SpriteRenderer));
        frame.transform.SetParent(transform, false);
        m_frame = frame.GetComponent<SpriteRenderer>();
        m_frame.sprite = m_sprite;
        m_frame.drawMode = SpriteDrawMode.Sliced;
        m_frame.sortingOrder = -1;
        m_board.MoveResolved += OnMoveResolved;
    }

    private void Update()
    {
        if (m_frame == null) return;
        m_frame.transform.position = new Vector3((m_board.width - 1) * .5f, (m_board.height - 1) * .5f, .1f);
        m_frame.size = new Vector2(m_board.width + .38f, m_board.height + .38f);
        m_flash = Mathf.MoveTowards(m_flash, 0, Time.deltaTime * 1.5f);
        Color idle = new Color(.2f, .65f, .85f, .55f + Mathf.Sin(Time.time * 1.6f) * .08f);
        m_frame.color = Color.Lerp(idle, new Color(.45f, 1f, .7f, 1f), m_flash);
    }

    private void OnMoveResolved()
    {
        m_flash = 1;
    }

    private void OnDestroy()
    {
        if (m_board != null) m_board.MoveResolved -= OnMoveResolved;
        if (m_sprite != null) Destroy(m_sprite);
        if (m_texture != null) Destroy(m_texture);
    }
}
