using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LevelSelectChestGrid : MonoBehaviour
{
    private const int LayoutVersion = 15;
    private const int IntroEntry = 0;

    [SerializeField] private Sprite chestSprite;
    [SerializeField] private Sprite currentLevelSprite;
    [SerializeField] private Sprite completedLevelSprite;
    [SerializeField] private Sprite lockedLevelSprite;
    [SerializeField] private Sprite levelPanelSprite;
    [SerializeField] private Sprite arrowSprite;
    [SerializeField] private Texture2D chestTexture;
    [SerializeField] private Texture2D currentLevelChestTexture;
    [SerializeField] private Texture2D completedChestTexture;
    [SerializeField] private Texture2D lockedChestTexture;
    [SerializeField] private Texture2D levelPanelTexture;
    [SerializeField] private Texture2D arrowTexture;
    [SerializeField] private Texture2D actionPanelTexture;
    [SerializeField] private string gameplaySceneName = "Level";
    [SerializeField] private string tutorialSceneName = "Tutorial";
    [SerializeField] private Color numberColor = Color.white;
    [SerializeField] private Color arrowColor = Color.white;
    [SerializeField] private Color disabledArrowColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private int layoutVersion;

    private readonly List<int> m_levels = new List<int>();
    private RectTransform m_rectTransform;
    private Canvas m_canvas;
    private RectTransform m_chestRect;
    private RectTransform m_previousArrowRect;
    private RectTransform m_nextArrowRect;
    private RectTransform m_actionPanelRect;
    private RectTransform m_playActionRect;
    private RectTransform m_tutorialActionRect;
    private RectTransform m_exitActionRect;
    private RawImage m_chestImage;
    private RawImage m_levelPanelImage;
    private RawImage m_previousArrowImage;
    private RawImage m_nextArrowImage;
    private Text m_titleText;
    private Text m_levelLabelText;
    private Text m_instructionText;
    private int m_selectedIndex;

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        BuildSelector();
        layoutVersion = LayoutVersion;
    }

    private void Update()
    {
        if (layoutVersion != LayoutVersion)
        {
            BuildSelector();
            layoutVersion = LayoutVersion;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                ChangeSelectedLevel(-1);
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                ChangeSelectedLevel(1);
            }
            else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                PlaySelectedLevel();
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandlePress(Mouse.current.position.ReadValue());
            return;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            HandlePress(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void BuildSelector()
    {
        ClearGeneratedControls();
        LevelDatabase.ResetProgressIfOutOfRange();
        m_levels.Clear();
        m_levels.AddRange(GetLevelNumbers());
        if (m_levels.Count == 0)
        {
            return;
        }
        m_levels.Insert(0, IntroEntry);

        int highestCompletedLevel = LevelDatabase.HighestCompletedLevel;
        m_selectedIndex = m_levels.FindIndex(level => level > highestCompletedLevel);
        if (m_selectedIndex < 0)
        {
            m_selectedIndex = m_levels.Count - 1;
        }

        float chestSize = Mathf.Clamp(Mathf.Min(m_rectTransform.rect.width * 0.58f, m_rectTransform.rect.height * 0.29f), 200f, 320f);
        float selectorY = -m_rectTransform.rect.height * 0.13f;

        m_titleText = CreateTextControl("Title", transform, new Vector2(0f, m_rectTransform.rect.height * 0.45f), new Vector2(m_rectTransform.rect.width * 0.9f, chestSize * 0.24f), new Color(1f, 0.79f, 0.28f), Mathf.RoundToInt(chestSize * 0.12f), TextAnchor.MiddleCenter);

        Vector2 levelPanelPosition = new Vector2(0f, m_rectTransform.rect.height * 0.34f);
        Vector2 levelPanelSize = new Vector2(m_rectTransform.rect.width * 0.65f, m_rectTransform.rect.height * 0.25f);
        CreateImageControl("LevelPanel", levelPanelPosition, levelPanelSize, levelPanelTexture, out m_levelPanelImage);

        m_levelLabelText = CreateTextControl("LevelLabel", transform, levelPanelPosition + new Vector2(0f, -levelPanelSize.y * 0.12f), new Vector2(levelPanelSize.x * 0.65f, levelPanelSize.y * 0.32f), numberColor, Mathf.RoundToInt(chestSize * 0.13f), TextAnchor.MiddleCenter);
        m_levelLabelText.text = "";

        m_titleText.rectTransform.anchoredPosition = levelPanelPosition + new Vector2(0f, -levelPanelSize.y * 0.12f);
        m_titleText.rectTransform.sizeDelta = new Vector2(levelPanelSize.x * 0.65f, levelPanelSize.y * 0.32f);
        m_titleText.fontSize = Mathf.RoundToInt(chestSize * 0.13f);
        m_titleText.color = numberColor;
        m_titleText.gameObject.SetActive(false);
        m_levelLabelText.gameObject.SetActive(false);

        m_chestRect = CreateImageControl("SelectedLevelChest", new Vector2(0f, selectorY), new Vector2(chestSize, chestSize), chestTexture, out m_chestImage);

        float arrowSize = Mathf.Clamp(chestSize * 0.28f, 76f, 130f);
        float arrowOffset = chestSize * 0.62f;
        m_previousArrowRect = CreateArrow("PreviousLevel", -arrowOffset, selectorY, arrowSize, false, out m_previousArrowImage);
        m_nextArrowRect = CreateArrow("NextLevel", arrowOffset, selectorY, arrowSize, true, out m_nextArrowImage);

        m_instructionText = CreateTextControl("Instruction", transform, new Vector2(0f, -m_rectTransform.rect.height * 0.32f), new Vector2(m_rectTransform.rect.width * 0.86f, chestSize * 0.14f), new Color(0.78f, 0.59f, 1f), Mathf.RoundToInt(chestSize * 0.07f), TextAnchor.MiddleCenter);
        m_instructionText.rectTransform.anchoredPosition = levelPanelPosition + new Vector2(0f, -levelPanelSize.y * 0.12f);
        m_instructionText.rectTransform.sizeDelta = new Vector2(levelPanelSize.x * 0.65f, levelPanelSize.y * 0.32f);
        m_instructionText.fontSize = Mathf.RoundToInt(chestSize * 0.2f);
        m_instructionText.resizeTextForBestFit = true;
        m_instructionText.resizeTextMinSize = 12;
        m_instructionText.resizeTextMaxSize = m_instructionText.fontSize;
        m_instructionText.color = new Color(1f, 0.86f, 0.24f);
        m_instructionText.transform.SetAsLastSibling();

        m_actionPanelRect = CreateImageControl(
            "ActionPanel",
            new Vector2(0f, -m_rectTransform.rect.height * 0.38f),
            new Vector2(m_rectTransform.rect.width * 0.96f, m_rectTransform.rect.height * 0.42f),
            actionPanelTexture,
            out RawImage actionPanelImage);
        actionPanelImage.raycastTarget = false;

        m_tutorialActionRect = CreateActionZone("TutorialAction", m_actionPanelRect, new Vector2(-m_actionPanelRect.rect.width * 0.27f, 0f), new Vector2(m_actionPanelRect.rect.width * 0.26f, m_actionPanelRect.rect.height * 0.36f));
        m_playActionRect = CreateActionZone("PlayAction", m_actionPanelRect, new Vector2(0f, -m_actionPanelRect.rect.height * 0.12f), new Vector2(m_actionPanelRect.rect.width * 0.3f, m_actionPanelRect.rect.height * 0.34f));
        m_exitActionRect = CreateActionZone("ExitAction", m_actionPanelRect, new Vector2(m_actionPanelRect.rect.width * 0.27f, 0f), new Vector2(m_actionPanelRect.rect.width * 0.26f, m_actionPanelRect.rect.height * 0.36f));

        RefreshSelector();
    }

    private RectTransform CreateActionZone(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject zone = new GameObject(name, typeof(RectTransform));
        zone.transform.SetParent(parent, false);

        RectTransform rectTransform = zone.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        return rectTransform;
    }

    private RectTransform CreateImageControl(string name, Vector2 position, Vector2 size, Texture texture, out RawImage image)
    {
        GameObject control = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        control.transform.SetParent(transform, false);

        RectTransform rectTransform = control.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = FitSize(size, texture);

        image = control.GetComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;

        return rectTransform;
    }

    private RectTransform CreateArrow(string name, float xPosition, float yPosition, float size, bool flipHorizontally, out RawImage image)
    {
        GameObject control = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        control.transform.SetParent(transform, false);

        RectTransform rectTransform = control.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
        rectTransform.sizeDelta = FitSize(new Vector2(size, size), arrowTexture);

        image = control.GetComponent<RawImage>();
        image.texture = arrowTexture;
        image.raycastTarget = false;
        image.enabled = arrowTexture != null;

        if (flipHorizontally)
        {
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        }

        return rectTransform;
    }

    private Vector2 FitSize(Vector2 maximumSize, Texture texture)
    {
        if (texture == null || texture.height == 0)
        {
            return maximumSize;
        }

        float aspectRatio = (float)texture.width / texture.height;
        if (aspectRatio >= 1f)
        {
            float height = maximumSize.x / aspectRatio;
            return height <= maximumSize.y
                ? new Vector2(maximumSize.x, height)
                : new Vector2(maximumSize.y * aspectRatio, maximumSize.y);
        }

        float width = maximumSize.y * aspectRatio;
        return width <= maximumSize.x
            ? new Vector2(width, maximumSize.y)
            : new Vector2(maximumSize.x, maximumSize.x / aspectRatio);
    }

    private Text CreateTextControl(string name, Transform parent, Vector2 position, Vector2 size, Color color, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.alignment = alignment;
        text.color = color;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
        text.font = GetBuiltinFont("LegacyRuntime.ttf") ?? GetBuiltinFont("Arial.ttf");
        AddTextShadow(textObject);
        return text;
    }

    private void RefreshSelector()
    {
        int level = m_levels[m_selectedIndex];
        int highestCompletedLevel = LevelDatabase.HighestCompletedLevel;
        bool isIntro = level == IntroEntry;
        if (isIntro)
        {
            m_chestImage.texture = currentLevelChestTexture != null ? currentLevelChestTexture : chestTexture;
        }
        else if (level <= highestCompletedLevel && completedChestTexture != null)
        {
            m_chestImage.texture = completedChestTexture;
        }
        else if (level == highestCompletedLevel + 1 && currentLevelChestTexture != null)
        {
            m_chestImage.texture = currentLevelChestTexture;
        }
        else
        {
            m_chestImage.texture = lockedChestTexture != null ? lockedChestTexture : chestTexture;
        }
        bool isUnlocked = isIntro || level <= highestCompletedLevel + 1;
        m_chestImage.color = isUnlocked ? Color.white : new Color(0.55f, 0.55f, 0.68f, 0.85f);
        m_instructionText.text = isIntro ? "INTRO" : level.ToString("00");
        m_previousArrowImage.color = m_selectedIndex > 0 ? arrowColor : disabledArrowColor;
        m_nextArrowImage.color = m_selectedIndex < m_levels.Count - 1 ? arrowColor : disabledArrowColor;
    }

    private List<int> GetLevelNumbers()
    {
        List<int> levels = new List<int>();
        TextAsset json = Resources.Load<TextAsset>(LevelDatabase.ResourcePath);
        if (json == null)
        {
            Debug.LogWarning("No se encontró el archivo de niveles en Resources/" + LevelDatabase.ResourcePath);
            return levels;
        }

        LevelCollection collection = JsonUtility.FromJson<LevelCollection>(json.text);
        if (collection == null || collection.levels == null)
        {
            return levels;
        }

        foreach (LevelData levelData in collection.levels)
        {
            if (levelData != null)
            {
                levels.Add(levelData.level);
            }
        }

        levels.Sort();
        return levels;
    }

    private void HandlePress(Vector2 screenPosition)
    {
        if (ContainsScreenPoint(m_previousArrowRect, screenPosition))
        {
            ChangeSelectedLevel(-1);
        }
        else if (ContainsScreenPoint(m_nextArrowRect, screenPosition))
        {
            ChangeSelectedLevel(1);
        }
        else if (ContainsScreenPoint(m_chestRect, screenPosition))
        {
            PlaySelectedLevel();
        }
        else if (ContainsScreenPoint(m_playActionRect, screenPosition))
        {
            PlaySelectedLevel();
        }
        else if (ContainsScreenPoint(m_tutorialActionRect, screenPosition))
        {
            OpenTutorial();
        }
        else if (ContainsScreenPoint(m_exitActionRect, screenPosition))
        {
            ExitGame();
        }
    }

    private bool ContainsScreenPoint(RectTransform rectTransform, Vector2 screenPosition)
    {
        Camera canvasCamera = m_canvas != null && m_canvas.renderMode != RenderMode.ScreenSpaceOverlay ? m_canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, canvasCamera);
    }

    private void ChangeSelectedLevel(int direction)
    {
        int nextIndex = Mathf.Clamp(m_selectedIndex + direction, 0, m_levels.Count - 1);
        if (nextIndex == m_selectedIndex)
        {
            return;
        }

        m_selectedIndex = nextIndex;
        RefreshSelector();
    }

    private void PlaySelectedLevel()
    {
        int level = m_levels[m_selectedIndex];
        if (level == IntroEntry)
        {
            SceneManager.LoadScene("Historia");
            return;
        }
        if (level > LevelDatabase.HighestCompletedLevel + 1)
        {
            return;
        }

        LevelDatabase.SelectLevel(level);
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void OpenTutorial()
    {
        if (Application.CanStreamedLevelBeLoaded(tutorialSceneName))
        {
            SceneManager.LoadScene(tutorialSceneName);
        }
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    private Font GetBuiltinFont(string fontName)
    {
        try
        {
            return Resources.GetBuiltinResource<Font>(fontName);
        }
        catch (UnityException)
        {
            return null;
        }
    }

    private void AddTextShadow(GameObject textObject)
    {
        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.08f, 0.02f, 0.12f, 0.9f);
        shadow.effectDistance = new Vector2(3f, -3f);
    }

    private void ClearGeneratedControls()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
