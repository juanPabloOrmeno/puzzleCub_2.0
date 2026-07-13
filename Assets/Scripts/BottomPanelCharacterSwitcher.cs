using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class BottomPanelCharacterSwitcher : MonoBehaviour
{
    public Image leftCharacterImage;
    public Image rightCharacterImage;
    public Sprite[] leftCharacters;
    public Sprite[] rightCharacters;

    [Header("Layout responsive")]
    [Range(0f, 1f)] public float leftAnchorX = 0.29f;
    [Range(0f, 1f)] public float rightAnchorX = 0.71f;
    [Range(0f, 1f)] public float baselineY = 0.24f;
    [Range(0.1f, 1.5f)] public float heightRatio = 0.62f;
    [Range(0.1f, 0.8f)] public float maxWidthRatio = 0.28f;
    [Range(0.5f, 2.5f)] public float characterScale = 1.18f;

    [SerializeField] private int leftIndex;
    [SerializeField] private int rightIndex;

    private RectTransform m_panelRect;

    private void Start()
    {
        m_panelRect = transform as RectTransform;
        ApplyCurrentCharacters();
        ApplyLayout();
    }

    private void LateUpdate()
    {
        ApplyLayout();
    }

    private void OnValidate()
    {
        m_panelRect = transform as RectTransform;
        ApplyCurrentCharacters();
        ApplyLayout();
    }

    public void ApplyCurrentCharacters()
    {
        SetCharacter(leftCharacterImage, leftCharacters, leftIndex);
        SetCharacter(rightCharacterImage, rightCharacters, rightIndex);
    }

    public void SetByScore(int score)
    {
        // Listo para conectar cuando exista puntaje real.
        ApplyCurrentCharacters();
    }

    public void SetLeftCharacter(int index)
    {
        leftIndex = index;
        SetCharacter(leftCharacterImage, leftCharacters, leftIndex);
    }

    public void SetRightCharacter(int index)
    {
        rightIndex = index;
        SetCharacter(rightCharacterImage, rightCharacters, rightIndex);
    }

    private void SetCharacter(Image image, Sprite[] characters, int index)
    {
        if (image == null || characters == null || characters.Length == 0)
        {
            return;
        }

        int safeIndex = Mathf.Clamp(index, 0, characters.Length - 1);
        if (characters[safeIndex] != null)
        {
            image.sprite = characters[safeIndex];
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.enabled = true;
        }
    }

    private void ApplyLayout()
    {
        if (m_panelRect == null)
        {
            m_panelRect = transform as RectTransform;
        }

        if (m_panelRect == null)
        {
            return;
        }

        Rect panelRect = m_panelRect.rect;
        if (panelRect.width <= 0f || panelRect.height <= 0f)
        {
            return;
        }

        LayoutCharacter(leftCharacterImage, leftAnchorX, panelRect);
        LayoutCharacter(rightCharacterImage, rightAnchorX, panelRect);
    }

    private void LayoutCharacter(Image image, float anchorX, Rect panelRect)
    {
        if (image == null)
        {
            return;
        }

        RectTransform rect = image.rectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(anchorX, baselineY);
        rect.anchorMax = new Vector2(anchorX, baselineY);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = Vector2.zero;

        float targetHeight = panelRect.height * heightRatio;
        float targetWidth = targetHeight;
        if (image.sprite != null && image.sprite.rect.height > 0f)
        {
            targetWidth = targetHeight * (image.sprite.rect.width / image.sprite.rect.height);
        }

        float maxWidth = panelRect.width * maxWidthRatio;
        if (targetWidth > maxWidth)
        {
            float scale = maxWidth / targetWidth;
            targetWidth *= scale;
            targetHeight *= scale;
        }

        rect.sizeDelta = new Vector2(targetWidth, targetHeight) * characterScale;
    }
}
