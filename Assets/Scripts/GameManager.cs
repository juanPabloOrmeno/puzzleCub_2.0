using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class GameManager : Singleton<GameManager>
{

   
    private const string WinSoundResource = "WinSoundReference";
    private const string GambareSoundResource = "GambareSoundReference";
    private const string LoseSoundResource = "LoseSoundReference";


    Board m_board;

    public SoundManager sonido;

    public GameObject winScreenPrefab;
    public GameObject timeoutScreenPrefab;


    public bool m_pause = false;
    private GameObject m_winScreen;
    private int m_nextLevel;
    public bool IsFinished { get; private set; }

    void Start()
    {
        if (sonido == null)
        {
            sonido = SoundManager.Instance;
        }

        if (sonido != null)
        {
            sonido.PlaySoundGambare();
        }
    }
  

    public void ganar()
    {
        print("ganar");
        FinishGame(true);
    }


    public void perder()
    {
        FinishGame(false);
    }

    private void FinishGame(bool winner)
    {
        if (IsFinished) return;
        IsFinished = true;
        m_board = FindAnyObjectByType<Board>();
        if (m_board != null)
        {
            m_board.CancelSelection();
            m_board.m_playerInputEnabled = false;
            // A pending cascade must not award a victory after the clock expires.
            if (!winner) m_board.StopAllCoroutines();
        }
        if (sonido == null)
        {
            sonido = SoundManager.Instance;
        }

        if (sonido != null)
        {
            sonido.stopMusic();
        }

        if (sonido != null && winner)
        {
            sonido.PlaySoundWinner();
        }

        if (!winner)
        {
            if (sonido != null)
            {
                sonido.PlaySoundLoser();
            }

            ShowTimeoutScreen();
            return;
        }

        int completedLevel = LevelDatabase.SelectedLevel;
        LevelDatabase.CompleteLevel(completedLevel);
        m_nextLevel = LevelDatabase.GetNextLevel(completedLevel);
        ShowWinScreen(completedLevel, m_nextLevel);
    }

    private void ShowWinScreen(int completedLevel, int nextLevel)
    {
        EnsureEventSystem();

        if (m_winScreen != null)
        {
            Destroy(m_winScreen);
        }

        if (winScreenPrefab == null)
        {
            Debug.LogWarning("No hay prefab de pantalla de victoria asignado en GameManager.winScreenPrefab.");
            return;
        }

        m_winScreen = Instantiate(winScreenPrefab);
        ConfigureWinScreen(m_winScreen);
    }

    private void ShowTimeoutScreen()
    {
        EnsureEventSystem();
        if (timeoutScreenPrefab != null)
        {
            m_winScreen = Instantiate(timeoutScreenPrefab);
            m_winScreen.GetComponent<TimeoutScreenView>().Initialize(this);
            return;
        }
        m_winScreen = new GameObject("Timeout Screen", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = m_winScreen.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        CanvasScaler scaler = m_winScreen.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = .5f;
        RectTransform root = m_winScreen.GetComponent<RectTransform>();
        GameObject dim = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        RectTransform dimRect = dim.GetComponent<RectTransform>();
        dimRect.SetParent(root, false);
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = dimRect.offsetMax = Vector2.zero;
        dim.GetComponent<Image>().color = new Color(.025f, .035f, .08f, .94f);
        CreateTimeoutLabel(root, "Title", "TIEMPO AGOTADO", new Vector2(.06f, .55f), new Vector2(.94f, .65f), 64);
        CreateTimeoutLabel(root, "Message", "¡Inténtalo otra vez!", new Vector2(.1f, .47f), new Vector2(.9f, .54f), 38);
        CreateTimeoutButton(root, "Retry", "REINTENTAR", .33f, .42f, RestartCurrentLevel);
        CreateTimeoutButton(root, "Levels", "VOLVER A NIVELES", .2f, .29f, GoToLevelSelect);
    }

    private void CreateTimeoutLabel(RectTransform parent, string name, string value, Vector2 min, Vector2 max, int size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Text label = go.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = value;
        label.fontSize = size;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 16;
        label.resizeTextMaxSize = size;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(1f, .87f, .6f);
        label.raycastTarget = false;
    }

    private void CreateTimeoutButton(RectTransform root, string name, string label, float bottom, float top, UnityEngine.Events.UnityAction action)
    {
        CreateWinButtonZone(root, name, new Vector2(.17f, bottom), new Vector2(.83f, top), action);
        RectTransform button = (RectTransform)root.Find(name);
        button.GetComponent<Image>().color = new Color(.12f, .32f, .4f);
        CreateTimeoutLabel(button, "Label", label, new Vector2(.04f, .05f), new Vector2(.96f, .95f), 38);
    }

    private void ConfigureWinScreen(GameObject winScreen)
    {
        Canvas canvas = winScreen.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = short.MaxValue;
        }

        CanvasScaler scaler = winScreen.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform root = winScreen.GetComponent<RectTransform>();
        if (root == null)
        {
            return;
        }

        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.localScale = Vector3.one;

        CreateWinButtonZone(root, "MenuButtonZone", new Vector2(0.02f, 0.025f), new Vector2(0.255f, 0.095f), GoToLevelSelect);
        CreateWinButtonZone(root, "NextButtonZone", new Vector2(0.285f, 0.025f), new Vector2(0.705f, 0.095f), LoadNextLevel);
        CreateWinButtonZone(root, "RetryButtonZone", new Vector2(0.73f, 0.025f), new Vector2(0.98f, 0.095f), RestartCurrentLevel);
    }

    private void CreateWinButtonZone(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
    }

    public void LoadNextLevel()
    {
        if (m_nextLevel > 0)
        {
            LevelDatabase.SelectLevel(m_nextLevel);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        GoToLevelSelect();
    }

    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToLevelSelect()
    {
        SceneManager.LoadScene("SelectNivel");
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        DontDestroyOnLoad(eventSystem);
    }

 





    public void pause()
    {
           
    }

}
