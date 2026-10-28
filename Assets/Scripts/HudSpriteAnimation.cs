using UnityEngine;
using UnityEngine.UI;

/// <summary>Animates a HUD character's emotion according to the fraction of level time remaining.</summary>
[RequireComponent(typeof(Image))]
public class HudSpriteAnimation : MonoBehaviour
{
    public enum Emotion { Confident, Scared, Terrified }

    [SerializeField] private Sprite[] confidentFrames;
    [SerializeField] private Sprite[] scaredFrames;
    [SerializeField] private Sprite[] terrifiedFrames;
    [SerializeField, Min(.1f)] private float framesPerSecond = 1.6f;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LevelCountdown countdown;
    [SerializeField, Range(0f, 1f)] private float scaredThreshold = .5f;
    [SerializeField, Range(0f, 1f)] private float terrifiedThreshold = .2f;

    public Emotion CurrentEmotion { get; private set; }

    private Image m_image;
    private float m_elapsed;
    private PuzzleTutorial m_tutorial;

    private void OnEnable()
    {
        m_image = GetComponent<Image>();
        m_elapsed = 0f;
        CurrentEmotion = Emotion.Confident;
        ApplyFrame();
    }

    private void Start()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (countdown == null) countdown = FindAnyObjectByType<LevelCountdown>();
        m_tutorial = FindAnyObjectByType<PuzzleTutorial>();
    }

    private void Update()
    {
        Emotion emotion = Emotion.Confident;
        if (countdown != null && countdown.IsReady)
        {
            float fraction = countdown.TotalSeconds > 0f
                ? Mathf.Clamp01(countdown.RemainingSeconds / countdown.TotalSeconds) : 0f;
            emotion = fraction <= terrifiedThreshold ? Emotion.Terrified :
                fraction <= scaredThreshold ? Emotion.Scared : Emotion.Confident;
        }
        if (emotion != CurrentEmotion)
        {
            CurrentEmotion = emotion;
            m_elapsed = 0f;
        }
        Sprite[] frames = GetFrames();
        if (frames == null || frames.Length == 0) return;
        bool paused = (gameManager != null && (gameManager.IsFinished || gameManager.m_pause)) ||
            (m_tutorial != null && m_tutorial.IsOpen);
        if (!paused)
            m_elapsed = (m_elapsed + Time.deltaTime) % (frames.Length / Mathf.Max(.1f, framesPerSecond));
        ApplyFrame();
    }

    private void ApplyFrame()
    {
        Sprite[] frames = GetFrames();
        if (m_image == null || frames == null || frames.Length == 0) return;
        int index = Mathf.FloorToInt(m_elapsed * Mathf.Max(.1f, framesPerSecond)) % frames.Length;
        if (frames[index] != null && m_image.sprite != frames[index])
            m_image.sprite = frames[index];
    }

    private Sprite[] GetFrames()
    {
        Sprite[] selected = CurrentEmotion == Emotion.Terrified ? terrifiedFrames :
            CurrentEmotion == Emotion.Scared ? scaredFrames : confidentFrames;
        return selected != null && selected.Length > 0 ? selected : confidentFrames;
    }

    private void OnValidate()
    {
        framesPerSecond = Mathf.Max(.1f, framesPerSecond);
        terrifiedThreshold = Mathf.Clamp(terrifiedThreshold, 0f, scaredThreshold);
    }
}
