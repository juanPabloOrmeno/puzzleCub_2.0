using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    // Punto central para agregar estado global del juego cuando haga falta.

    public void ganar()
    {
        int completedLevel = LevelDatabase.SelectedLevel;
        LevelDatabase.CompleteLevel(completedLevel);
        Debug.Log("Nivel completado");

        int nextLevel = LevelDatabase.GetNextLevel(completedLevel);
        if (nextLevel > 0)
        {
            LevelDatabase.SelectLevel(nextLevel);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        SceneManager.LoadScene("SelectNivel");
    }
}
