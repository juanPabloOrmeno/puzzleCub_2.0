# PuzzleCub

PuzzleCub es un juego de puzzle 2D por niveles desarrollado con Unity 6000.7.0a1. El objetivo de cada nivel es eliminar todas las fichas de colores de un tablero con gravedad y obstaculos.

## Regla principal

El jugador selecciona una ficha y la arrastra horizontalmente a una celda normal vacia. Un destino solo es valido si tiene apoyo debajo: el limite inferior del tablero, un obstaculo o una ficha.

Al terminar el movimiento:

1. Se eliminan las lineas horizontales o verticales de 2 o mas fichas contiguas del mismo color.
2. Las fichas restantes caen por columna hacia los espacios vacios.
3. Los obstaculos dividen una columna: las fichas no pueden caer a traves de ellos.
4. Se resuelven automaticamente las nuevas coincidencias producidas por la caida.
5. El nivel termina al no quedar fichas en el tablero.

No es un match-3 clasico: no hay intercambio entre dos fichas ni se crean fichas nuevas. Es un puzzle cerrado de desplazamiento lateral, coincidencias y gravedad.

## Contenido actual

- 25 niveles numerados en orden.
- Fichas por color: `Blanc`, `Cyan`, `Green`, `Indigo`, `Magenta`, `Orange`, `Red`, `Teal` y `Yellow`.
- Tiles activos: `Normal` y `Obstacle`.
- Escenas: `Inicio`, `SelectNivel` y `Level`.
- Al completar un nivel se desbloquea el siguiente y se muestra la pantalla de victoria.

## Datos de niveles

La fuente de datos que usa el juego es `Assets/Resources/Levels/levels.json`. El archivo duplicado en `Assets/Levels/levels.json`, si existe, no se carga en tiempo de ejecucion.

Cada nivel tiene este esquema:

```json
{
  "level": 1,
  "width": 6,
  "height": 9,
  "borderSize": 0.5,
  "time": 100,
  "pieces": [
    { "type": "Red", "x": 0, "y": 1 }
  ],
  "tiles": [
    { "type": "Obstacle", "x": 0, "y": 0 }
  ]
}
```

- Las coordenadas usan enteros y el origen es la esquina inferior izquierda.
- `x` esta entre `0` y `width - 1`; `y` entre `0` y `height - 1`.
- Los obstaculos se declaran en `tiles`; el resto del tablero se rellena con tiles normales.
- El prefab de cada ficha se resuelve a partir de `type`.

## Estado de implementacion

- `time` esta presente en el JSON, pero actualmente no hay temporizador conectado al gameplay ni derrota automatica por tiempo.
- Hay nombres heredados para obstaculos moviles, pero la logica actual solo distingue entre tiles normales y obstaculos; los obstaculos no se mueven.
- La condicion de derrota existe como `GameManager.perder()`, pero no tiene un flujo de UI o reglas de juego conectado.

## Archivos clave

- `Assets/Scripts/Board.cs`: construye el tablero y resuelve movimiento, matches, gravedad y victoria.
- `Assets/Scripts/LevelData.cs`: modelo del JSON, carga de niveles y progreso en `PlayerPrefs`.
- `Assets/Scripts/GamePiece.cs`: datos y animacion de una ficha.
- `Assets/Scripts/Tile.cs`: tipo de celda e input del tablero.
- `Assets/Scripts/GameManager.cs`: victoria, progreso y navegacion al siguiente nivel.
- `Assets/Scripts/LevelSelectChestGrid.cs`: seleccion y bloqueo/desbloqueo de niveles.

## Progreso

- Nivel seleccionado: `PlayerPrefs["SelectedLevel"]`.
- Mayor nivel completado: `PlayerPrefs["HighestCompletedLevel"]`.

## Verificacion

Para comprobar la compilacion de los scripts de C#:

```sh
dotnet build Assembly-CSharp.csproj
```

Los cambios en escenas, prefabs o serializacion tambien deben probarse en el editor de Unity.
