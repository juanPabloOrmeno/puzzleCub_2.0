using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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


    public bool m_pause = false;
    private GameObject m_winScreen;
    private int m_nextLevel;

    void Start()
    {
      
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
        if (sonido == null)
        {
            sonido = SoundManager.Instance;
        }

        if (sonido != null)
        {
            sonido.PlaySoundWinner();
        }

        if (!winner)
        {
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
