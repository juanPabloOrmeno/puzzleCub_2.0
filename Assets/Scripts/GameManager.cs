using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // Punto central para agregar estado global del juego cuando haga falta.

    public void ganar()
    {
        LevelDatabase.CompleteLevel(LevelDatabase.SelectedLevel);
        Debug.Log("Nivel completado");
    }
}
