using System.Collections;
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
    private Texture2D m_burstTexture;
    private Sprite m_ringSprite;
    private Sprite m_sparkSprite;

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

    public void PlayClearBurst(Vector3 position, GamePiece.MatchValue match, int sortingLayer)
    {
        if (m_ringSprite == null) CreateBurstSprites();
        Color color;
        switch (match)
        {
            case GamePiece.MatchValue.Red: color = new Color(1f, .3f, .35f); break;
            case GamePiece.MatchValue.Green: color = new Color(.4f, 1f, .55f); break;
            case GamePiece.MatchValue.Yellow: color = new Color(1f, .85f, .25f); break;
            case GamePiece.MatchValue.Orange: color = new Color(1f, .55f, .2f); break;
            case GamePiece.MatchValue.Magenta: color = new Color(1f, .4f, .85f); break;
            case GamePiece.MatchValue.Indigo: color = new Color(.65f, .5f, 1f); break;
            case GamePiece.MatchValue.Blue: color = new Color(.35f, .6f, 1f); break;
            case GamePiece.MatchValue.Cyan:
            case GamePiece.MatchValue.Teal: color = new Color(.3f, 1f, 1f); break;
            default: color = new Color(.9f, .95f, 1f); break;
        }
        StartCoroutine(ClearBurstRoutine(position, color, sortingLayer));
    }

    private void CreateBurstSprites()
    {
        // One small atlas per board, shared by every match and cascade.
        const int size = 64;
        m_burstTexture = new Texture2D(size * 2, size, TextureFormat.RGBA32, false);
        m_burstTexture.name = "Match Ring and Spark";
        m_burstTexture.wrapMode = TextureWrapMode.Clamp;
        m_burstTexture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size * 2];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Vector2 p = new Vector2(x - 31.5f, y - 31.5f) / 32f;
            float ringDistance = (p.magnitude - .72f) / .055f;
            float ring = Mathf.Exp(-ringDistance * ringDistance);
            float diamond = Mathf.Clamp01(1f - (Mathf.Abs(p.x) + Mathf.Abs(p.y)) / .85f);
            pixels[y * size * 2 + x] = new Color(1, 1, 1, ring);
            pixels[y * size * 2 + x + size] = new Color(1, 1, 1, Mathf.SmoothStep(0, 1, diamond));
        }
        m_burstTexture.SetPixels(pixels);
        m_burstTexture.Apply(false, true);
        m_ringSprite = Sprite.Create(m_burstTexture, new Rect(0, 0, size, size), Vector2.one * .5f, size);
        m_sparkSprite = Sprite.Create(m_burstTexture, new Rect(size, 0, size, size), Vector2.one * .5f, size);
    }

    private IEnumerator ClearBurstRoutine(Vector3 position, Color color, int sortingLayer)
    {
        yield return new WaitForSeconds(.07f);
        GameObject burst = new GameObject("Match Burst");
        burst.transform.SetParent(transform, false);
        burst.transform.position = position;
        SpriteRenderer[] sprites = new SpriteRenderer[6];
        for (int i = 0; i < sprites.Length; i++)
        {
            GameObject part = new GameObject(i == 0 ? "Ring" : "Spark", typeof(SpriteRenderer));
            part.transform.SetParent(burst.transform, false);
            sprites[i] = part.GetComponent<SpriteRenderer>();
            sprites[i].sprite = i == 0 ? m_ringSprite : m_sparkSprite;
            sprites[i].sortingLayerID = sortingLayer;
            sprites[i].sortingOrder = 20;
        }
        float elapsed = 0;
        const float duration = .42f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float ease = 1f - (1f - t) * (1f - t);
            sprites[0].transform.localScale = Vector3.one * Mathf.Lerp(.4f, 1.7f, ease);
            Color ringColor = Color.Lerp(Color.white, color, Mathf.Clamp01(t * 4f));
            ringColor.a = .65f * (1f - t) * (1f - t);
            sprites[0].color = ringColor;
            for (int i = 1; i < sprites.Length; i++)
            {
                float angle = (i - 1) * Mathf.PI * 2f / 5f + position.x * .7f;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                sprites[i].transform.localPosition = direction * (.12f + .55f * ease) + Vector3.down * (.16f * t * t);
                sprites[i].transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg + t * 100f);
                sprites[i].transform.localScale = Vector3.one * (.25f * (1f - t));
                Color sparkColor = Color.Lerp(Color.white, color, .65f);
                sparkColor.a = 1f - t;
                sprites[i].color = sparkColor;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(burst);
    }

    private void OnDestroy()
    {
        if (m_board != null) m_board.MoveResolved -= OnMoveResolved;
        if (m_sprite != null) Destroy(m_sprite);
        if (m_texture != null) Destroy(m_texture);
        if (m_ringSprite != null) Destroy(m_ringSprite);
        if (m_sparkSprite != null) Destroy(m_sparkSprite);
        if (m_burstTexture != null) Destroy(m_burstTexture);
    }
}
