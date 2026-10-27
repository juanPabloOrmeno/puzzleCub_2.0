using System.Collections;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public int xIndexF;
    public int yIndexF;

    public InterpType interpolation = InterpType.SmootherStep;
    public MatchValue matchValue;
    public int scoreValue = 20;
    public AudioClip clearSound;

    private Board m_board;
    private bool m_isMoving;
    private Vector3 m_visualScale;
    private float m_appearanceTime;
    private float m_landingTime = -10f;
    private bool m_selected;
    private bool m_clearing;
    private float m_clearTime;
    private float m_movePhase;
    private Vector3 m_moveDirection;
    private SpriteRenderer m_renderer;
    private Color m_baseColor;
    private readonly SpriteRenderer[] m_trail = new SpriteRenderer[3];
    private readonly float[] m_trailTimes = new float[3];
    private int m_trailIndex;
    private float m_nextTrailTime;
    private const float ClearDuration = .28f;

    private void Awake()
    {
        m_visualScale = transform.localScale;
        m_renderer = GetComponent<SpriteRenderer>();
        if (m_renderer != null) m_baseColor = m_renderer.color;
    }

    private void Update()
    {
        float enter = Mathf.Clamp01((Time.time - m_appearanceTime) / .35f);
        float scale = Mathf.SmoothStep(.65f, 1f, enter) + Mathf.Sin(enter * Mathf.PI) * .12f;
        float landing = (Time.time - m_landingTime) / .32f;
        float bounce = landing >= 0f && landing <= 1f ? Mathf.Sin(landing * Mathf.PI * 2f) * (1f - landing) * .18f : 0f;
        if (m_selected && !m_isMoving) scale *= 1.06f + Mathf.Sin(Time.time * 7f) * .025f;
        float motion = m_isMoving ? Mathf.Sin(m_movePhase * Mathf.PI) : 0f;
        float stretch = motion * (Mathf.Abs(m_moveDirection.x) > .5f ? .12f : -.15f);
        float angle = -m_moveDirection.x * motion * 9f;
        if (m_clearing)
        {
            float t = Mathf.Clamp01((Time.time - m_clearTime) / ClearDuration);
            if (t < .25f)
                scale = Mathf.Lerp(1f, .82f, Mathf.SmoothStep(0, 1, t / .25f));
            else if (t < .5f)
                scale = Mathf.Lerp(.82f, 1.22f, Mathf.SmoothStep(0, 1, (t - .25f) / .25f));
            else
                scale = Mathf.Lerp(1.22f, 0f, Mathf.SmoothStep(0, 1, (t - .5f) / .5f));
            bounce = stretch = 0f;
            angle = Mathf.SmoothStep(0, 35f, t) * ((xIndex + yIndex) % 2 == 0 ? 1f : -1f);
            if (m_renderer != null)
            {
                Color color = Color.Lerp(m_baseColor, Color.white, Mathf.Sin(t * Mathf.PI));
                color.a = m_baseColor.a * (1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.45f, 1f, t)));
                m_renderer.color = color;
            }
        }
        transform.localScale = Vector3.Scale(m_visualScale, new Vector3(scale * (1f + bounce + stretch), scale * (1f - bounce - stretch), 1f));
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        UpdateTrail();
    }

    public void SetSelected(bool selected)
    {
        m_selected = selected;
    }

    public void ClearAnimated()
    {
        if (m_clearing) return;
        StopAllCoroutines();
        m_isMoving = m_selected = false;
        m_clearing = true;
        m_clearTime = Time.time;
        foreach (Collider2D hitbox in GetComponents<Collider2D>()) hitbox.enabled = false;
        foreach (SpriteRenderer ghost in m_trail) if (ghost != null) ghost.enabled = false;
        if (m_board != null && m_renderer != null)
        {
            BoardVisualEffects effects = m_board.GetComponent<BoardVisualEffects>();
            if (effects != null)
                effects.PlayClearBurst(transform.position, matchValue, m_renderer.sortingLayerID);
        }
        Destroy(gameObject, ClearDuration);
    }

    private void UpdateTrail()
    {
        if (m_renderer == null || m_clearing) return;
        if (m_isMoving && Time.time >= m_nextTrailTime)
        {
            int index = m_trailIndex++ % m_trail.Length;
            if (m_trail[index] == null)
            {
                GameObject ghost = new GameObject("Piece Afterimage", typeof(SpriteRenderer));
                ghost.transform.SetParent(m_board.transform, false);
                m_trail[index] = ghost.GetComponent<SpriteRenderer>();
                m_trail[index].sprite = m_renderer.sprite;
                m_trail[index].sharedMaterial = m_renderer.sharedMaterial;
                m_trail[index].sortingLayerID = m_renderer.sortingLayerID;
                m_trail[index].sortingOrder = m_renderer.sortingOrder - 1;
            }
            Transform trailTransform = m_trail[index].transform;
            trailTransform.SetPositionAndRotation(transform.position, transform.rotation);
            trailTransform.localScale = transform.localScale;
            m_trailTimes[index] = Time.time;
            m_trail[index].enabled = true;
            m_nextTrailTime = Time.time + .04f;
        }
        for (int i = 0; i < m_trail.Length; i++)
        {
            if (m_trail[i] == null || !m_trail[i].enabled) continue;
            float fade = Mathf.Clamp01((Time.time - m_trailTimes[i]) / .14f);
            Color color = m_baseColor;
            color.a = .2f * (1f - fade);
            m_trail[i].color = color;
            if (fade >= 1f) m_trail[i].enabled = false;
        }
    }

    private void OnDestroy()
    {
        foreach (SpriteRenderer ghost in m_trail)
            if (ghost != null) Destroy(ghost.gameObject);
    }

    public bool IsMoving
    {
        get { return m_isMoving; }
    }

    public enum InterpType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    }

    public enum MatchValue
    {
        Yellow,
        Blue,
        Magenta,
        Indigo,
        Green,
        Teal,
        Red,
        Cyan,
        Wild,
        Blanc,
        Orange
    }

    public void Init(Board board)
    {
        m_board = board;
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void SnapTo(int x, int y)
    {
        StopAllCoroutines();
        m_isMoving = false;
        m_selected = m_clearing = false;
        m_landingTime = -10f;
        if (m_renderer != null) m_renderer.color = m_baseColor;
        foreach (SpriteRenderer ghost in m_trail) if (ghost != null) ghost.enabled = false;
        transform.position = new Vector3(x, y, 0f);
        SetCoord(x, y);
        m_appearanceTime = Time.time + Mathf.Min((x + y) * .025f, .25f);
    }

    public void ScorePoints()
    {
        // Punto de extensión para sumar score, sonido o efectos al limpiar una pieza.
    }

    public void Move(int destX, int destY, float timeToMove)
    {
        if (m_isMoving)
        {
            StopAllCoroutines();
            m_isMoving = false;
        }

        StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0f), timeToMove));
    }

    private IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        if (timeToMove <= 0f)
        {
            transform.position = destination;
            yield break;
        }

        Vector3 startPosition = transform.position;
        m_moveDirection = (destination - startPosition).normalized;
        m_movePhase = 0;
        m_selected = false;
        float elapsedTime = 0f;
        m_isMoving = true;

        while (Vector3.Distance(transform.position, destination) >= 0.01f)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / timeToMove);
            m_movePhase = t;

            // Permite cambiar la curva de movimiento desde el Inspector sin tocar la lógica del tablero.
            transform.position = Vector3.Lerp(startPosition, destination, ApplyInterpolation(t));
            yield return null;
        }

        transform.position = destination;
        m_isMoving = false;
        m_landingTime = Time.time;
    }

    private float ApplyInterpolation(float t)
    {
        switch (interpolation)
        {
            case InterpType.EaseOut:
                return Mathf.Sin(t * Mathf.PI * 0.5f);
            case InterpType.EaseIn:
                return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            case InterpType.SmoothStep:
                return t * t * (3f - 2f * t);
            case InterpType.SmootherStep:
                return t * t * t * (t * (t * 6f - 15f) + 10f);
            default:
                return t;
        }
    }
}
