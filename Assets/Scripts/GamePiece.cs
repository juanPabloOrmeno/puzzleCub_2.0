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
        transform.position = new Vector3(x, y, 0f);
        SetCoord(x, y);
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

    void OnMouseDown()
    {
        if (m_board != null)
        {
            m_board.ClickGamePiece(this);
        }
    }

    void OnMouseUp()
    {
        if (m_board != null)
        {
            m_board.ReleaseTile();
        }
    }

    private IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        if (timeToMove <= 0f)
        {
            transform.position = destination;
            yield break;
        }

        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        m_isMoving = true;

        while (Vector3.Distance(transform.position, destination) >= 0.01f)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / timeToMove);

            // Permite cambiar la curva de movimiento desde el Inspector sin tocar la lógica del tablero.
            transform.position = Vector3.Lerp(startPosition, destination, ApplyInterpolation(t));
            yield return null;
        }

        transform.position = destination;
        m_isMoving = false;
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
