using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Animates the timeout dialog using unscaled time and fits it inside the safe area.</summary>
public class TimeoutScreenView : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform hourglass;
    [SerializeField] private Image backdrop;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private CanvasGroup hourglassGroup;
    [SerializeField] private CanvasGroup retryGroup;
    [SerializeField] private CanvasGroup levelsGroup;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button levelsButton;

    private float m_entranceScale = .82f;
    private float m_verticalOffset = -90f;
    private bool m_leaving;

    public void Initialize(GameManager manager)
    {
        panelGroup.alpha = hourglassGroup.alpha = retryGroup.alpha = levelsGroup.alpha = 0f;
        backdrop.color = new Color(.015f, .025f, .045f, 0f);
        retryButton.interactable = levelsButton.interactable = false;
        retryButton.onClick.AddListener(() => Navigate(manager.RestartCurrentLevel));
        levelsButton.onClick.AddListener(() => Navigate(manager.GoToLevelSelect));
        ApplyLayout();
        StartCoroutine(AnimateEntrance());
    }

    private void Navigate(UnityEngine.Events.UnityAction action)
    {
        if (m_leaving) return;
        m_leaving = true;
        retryButton.interactable = levelsButton.interactable = false;
        action();
    }

    private void LateUpdate() => ApplyLayout();

    private void ApplyLayout()
    {
        if (panel == null || Screen.width <= 0 || Screen.height <= 0) return;
        Rect canvas = ((RectTransform)transform).rect;
        Rect safe = Screen.safeArea;
        float width = canvas.width * safe.width / Screen.width;
        float height = canvas.height * safe.height / Screen.height;
        float fit = Mathf.Min(width * .78f / 740f, height * .64f / 1100f);
        panel.anchorMin = panel.anchorMax = new Vector2(safe.center.x / Screen.width,
            (safe.y + safe.height * .49f) / Screen.height);
        panel.anchoredPosition = new Vector2(0f, m_verticalOffset * fit);
        panel.localScale = Vector3.one * (fit * m_entranceScale);
    }

    private IEnumerator AnimateEntrance()
    {
        const float duration = 1.05f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float fade = Mathf.Clamp01(elapsed / .35f);
            backdrop.color = new Color(.015f, .025f, .045f, .57f * Smooth(fade));
            float enter = Mathf.Clamp01((elapsed - .06f) / .58f);
            panelGroup.alpha = Smooth(Mathf.Clamp01(enter * 2f));
            m_entranceScale = Mathf.LerpUnclamped(.82f, 1f, EaseOutBack(enter));
            m_verticalOffset = Mathf.Lerp(-90f, 0f, 1f - Mathf.Pow(1f - enter, 3));
            panel.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-3f, 0f, Smooth(enter)));
            float medal = Mathf.Clamp01((elapsed - .22f) / .55f);
            hourglassGroup.alpha = Smooth(Mathf.Clamp01(medal * 2f));
            hourglass.localScale = Vector3.one * Mathf.LerpUnclamped(.65f, 1f, EaseOutBack(medal));
            hourglass.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-24f, 0f, Smooth(medal)));
            AnimateButton(retryGroup, Mathf.Clamp01((elapsed - .53f) / .3f));
            AnimateButton(levelsGroup, Mathf.Clamp01((elapsed - .69f) / .3f));
            yield return null;
        }
        panelGroup.alpha = hourglassGroup.alpha = retryGroup.alpha = levelsGroup.alpha = 1f;
        m_entranceScale = 1f;
        m_verticalOffset = 0f;
        panel.localRotation = hourglass.localRotation = Quaternion.identity;
        hourglass.localScale = retryGroup.transform.localScale = levelsGroup.transform.localScale = Vector3.one;
        retryButton.interactable = levelsButton.interactable = true;
    }

    private static void AnimateButton(CanvasGroup group, float t)
    {
        group.alpha = Smooth(t);
        group.transform.localScale = Vector3.one * Mathf.LerpUnclamped(.9f, 1f, EaseOutBack(t));
    }

    private static float Smooth(float t) => t * t * (3f - 2f * t);

    private static float EaseOutBack(float t)
    {
        float x = t - 1f;
        return 1f + 2.70158f * x * x * x + 1.70158f * x * x;
    }
}
