using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LevelSelectChestGrid : MonoBehaviour
{
    [SerializeField] private Sprite chestSprite;
    [SerializeField] private Sprite currentLevelSprite;
    [SerializeField] private Sprite completedLevelSprite;
    [SerializeField] private Sprite lockedLevelSprite;
    [SerializeField] private string gameplaySceneName = "Level";
    [SerializeField] private int columns = 4;
    [SerializeField, Range(0.02f, 0.25f)] private float horizontalMargin = 0.06f;
    [SerializeField, Range(0.02f, 0.25f)] private float verticalMargin = 0.14f;
    [SerializeField, Range(0.25f, 1f)] private float chestScale = 0.88f;
    [SerializeField, Range(0f, 0.3f)] private float horizontalSpacingRatio = 0.08f;
    [SerializeField, Range(-0.45f, 0.3f)] private float verticalSpacingRatio = 0.16f;
    [SerializeField] private Vector2 gridOffset;
    [SerializeField] private Color numberColor = Color.white;

    private readonly List<ChestButton> m_chests = new List<ChestButton>();
    private RectTransform m_rectTransform;
    private Canvas m_canvas;
    private Vector2 m_cellSize;

    private class ChestButton
    {
        public int level;
        public bool unlocked;
        public RectTransform rectTransform;
    }

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_canvas = GetComponentInParent<Canvas>();
        BuildGrid();
    }

    private void Update()
    {
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

    private void BuildGrid()
    {
        ClearGeneratedChests();

        List<int> levels = GetLevelNumbers();
        if (levels.Count == 0)
        {
            return;
        }

        int highestCompletedLevel = LevelDatabase.HighestCompletedLevel;
        int rowCount = Mathf.CeilToInt(levels.Count / (float)columns);
        CalculateGridMetrics(rowCount, out Vector2 spacing, out Vector2 firstCellPosition);

        for (int i = 0; i < levels.Count; i++)
        {
            int row = i / columns;
            int column = i % columns;
            float x = firstCellPosition.x + column * (m_cellSize.x + spacing.x);
            float y = firstCellPosition.y - row * (m_cellSize.y + spacing.y);
            bool unlocked = levels[i] <= highestCompletedLevel + 1;
            CreateChest(levels[i], unlocked, new Vector2(x, y), GetSpriteForLevel(levels[i], highestCompletedLevel));
        }
    }

    private void CalculateGridMetrics(int rowCount, out Vector2 spacing, out Vector2 firstCellPosition)
    {
        Rect canvasRect = m_rectTransform.rect;
        float availableWidth = canvasRect.width * (1f - horizontalMargin * 2f);
        float availableHeight = canvasRect.height * (1f - verticalMargin * 2f);
        float widthDenominator = columns + (columns - 1) * horizontalSpacingRatio;
        float heightDenominator = rowCount + (rowCount - 1) * verticalSpacingRatio;
        float cell = Mathf.Min(availableWidth / widthDenominator, availableHeight / heightDenominator);

       
        m_cellSize = new Vector2(cell, cell);
        spacing = new Vector2(cell * horizontalSpacingRatio, cell * verticalSpacingRatio);

        float totalWidth = columns * m_cellSize.x + (columns - 1) * spacing.x;
        float totalHeight = rowCount * m_cellSize.y + (rowCount - 1) * spacing.y;
        firstCellPosition = new Vector2(
            -totalWidth * 0.5f + m_cellSize.x * 0.5f + gridOffset.x,
            totalHeight * 0.5f - m_cellSize.y * 0.5f + gridOffset.y
        );
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

    private Sprite GetSpriteForLevel(int level, int highestCompletedLevel)
    {
        if (level <= highestCompletedLevel && completedLevelSprite != null)
        {
            return completedLevelSprite;
        }

        if (level == highestCompletedLevel + 1 && currentLevelSprite != null)
        {
            return currentLevelSprite;
        }

        return lockedLevelSprite != null ? lockedLevelSprite : chestSprite;
    }

    private void CreateChest(int level, bool unlocked, Vector2 anchoredPosition, Sprite sprite)
    {
        GameObject chest = new GameObject("LevelChest_" + level, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        chest.transform.SetParent(transform, false);

        RectTransform chestRect = chest.GetComponent<RectTransform>();
        chestRect.anchorMin = new Vector2(0.5f, 0.5f);
        chestRect.anchorMax = new Vector2(0.5f, 0.5f);
        chestRect.pivot = new Vector2(0.5f, 0.5f);
        chestRect.anchoredPosition = anchoredPosition;
        chestRect.sizeDelta = m_cellSize * chestScale;

        Image image = chest.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        CreateLevelText(level, chest.transform);
        m_chests.Add(new ChestButton { level = level, unlocked = unlocked, rectTransform = chestRect });
    }

    private void CreateLevelText(int level, Transform parent)
    {
        GameObject textObject = new GameObject("LevelText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        Vector2 visualSize = m_cellSize * chestScale;
        textRect.anchoredPosition = new Vector2(0f, -visualSize.y * 0.34f);
        textRect.sizeDelta = new Vector2(visualSize.x, visualSize.y * 0.25f);

        Text text = textObject.GetComponent<Text>();
        text.text = level.ToString();
        text.alignment = TextAnchor.MiddleCenter;
        text.color = numberColor;
        text.fontSize = Mathf.RoundToInt(visualSize.y * 0.2f);
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
        Font font = GetBuiltinFont("LegacyRuntime.ttf") ?? GetBuiltinFont("Arial.ttf");
        if (font != null)
        {
            text.font = font;
        }
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

    private void HandlePress(Vector2 screenPosition)
    {
        Camera canvasCamera = m_canvas != null && m_canvas.renderMode != RenderMode.ScreenSpaceOverlay ? m_canvas.worldCamera : null;
        foreach (ChestButton chest in m_chests)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(chest.rectTransform, screenPosition, canvasCamera, out localPoint) &&
                chest.rectTransform.rect.Contains(localPoint))
            {
                if (chest.unlocked)
                {
                    SelectLevel(chest.level);
                }

                return;
            }
        }
    }

    private void SelectLevel(int level)
    {
        LevelDatabase.SelectLevel(level);
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void ClearGeneratedChests()
    {
        m_chests.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
