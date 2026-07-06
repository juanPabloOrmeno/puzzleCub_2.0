# PuzzleCub - contexto de mecanica de niveles

Este documento resume la mecanica del proyecto viejo para usarlo como contexto al reconstruirlo desde cero en una version nueva de Unity. No cubre menus en detalle. Se enfoca en niveles, tablero, tiles, piezas, movimiento, matches, colapso, victoria y datos.

## Idea general del juego

PuzzleCub es un puzzle por niveles basado en un tablero 2D con coordenadas enteras `(x, y)`. Cada nivel define:

- El ancho y alto del tablero.
- El tiempo disponible.
- El tamano/correcion de borde para la camara.
- Las piezas iniciales colocadas en celdas concretas.
- Los tiles especiales u obstaculos colocados en celdas concretas.

La meta principal del nivel es eliminar todas las piezas del tablero. Cuando no queda ninguna pieza, el nivel se gana.

## Datos de nivel

El formato actual esta modelado por `LevelData.cs`.

```csharp
public class LevelCollection
{
    public List<LevelData> levels;
}

public class LevelData
{
    public int level;
    public int width;
    public int height;
    public float borderSize;
    public int time;
    public List<LevelPieceData> pieces;
    public List<LevelTileData> tiles;
}

public class LevelPieceData
{
    public string type;
    public int x;
    public int y;
}

public class LevelTileData
{
    public string type;
    public int x;
    public int y;
}
```

Los niveles se cargan desde `Resources/Levels/levels.json` con `Resources.Load<TextAsset>("Levels/levels")`.

El nivel seleccionado se guarda en `PlayerPrefs` con la key `SelectedLevel`.

Flujo actual:

1. Al elegir nivel, se llama `LevelDatabase.SelectLevel(level)`.
2. Se carga una escena jugable comun.
3. `Board.Start()` pide `LevelDatabase.GetSelectedLevelData()`.
4. `GameManager.Start()` tambien lee ese `LevelData` para configurar el tiempo y el siguiente nivel.

Recomendacion para el proyecto nuevo: mantener esta separacion. Un sistema solo selecciona nivel, y la escena de gameplay lee los datos del nivel seleccionado.

## Coordenadas y tablero

El tablero usa una matriz 2D:

```csharp
Tile[,] m_allTiles;
GamePiece[,] m_allGamePieces;
```

Las coordenadas son enteras:

- `x` va de `0` a `width - 1`.
- `y` va de `0` a `height - 1`.
- La posicion mundial de una celda es `(x, y, 0)`.
- El origen `(0, 0)` esta en una esquina inferior del tablero.

Cada `Tile` y cada `GamePiece` guarda sus indices internos:

```csharp
public int xIndex;
public int yIndex;
```

Regla basica de bounds:

```csharp
x >= 0 && x < width && y >= 0 && y < height
```

## Tiles

Los tiles actuales tienen dos tipos:

```csharp
public enum TileType
{
    Normal,
    Obstacle
}
```

Tipos usados en datos de nivel:

- `Normal`: tile jugable normal.
- `Obstacle`: obstaculo fijo.
- `Obtacle`: variante mal escrita legacy.
- `TileObtacle`: variante por nombre de prefab legacy.
- `ObstacleMovil`: obstaculo movil o variante especial.
- `ObtacleMovil`: variante mal escrita legacy.
- `MobileObstacle`: variante en ingles.
- `TileObtacleMovil`: variante por nombre de prefab legacy.

Comportamiento de tiles:

- Primero se instancian los tiles especiales definidos por el nivel.
- Despues se llena cualquier celda vacia con `tileNormalPrefab`.
- Todos los tiles quedan como hijos del objeto `Board`.
- Cada tile llama `Init(x, y, board)`.
- Cada tile detecta input con `OnMouseDown`, `OnMouseEnter` y `OnMouseUp`.

Input del tile:

- `OnMouseDown`: llama `Board.ClickTile(this)`.
- `OnMouseEnter`: llama `Board.DragToTile(this)`.
- `OnMouseUp`: llama `Board.ReleaseTile()`.

En el editor de niveles viejo habia dos tiles llamados `piso` y `pisoN`, usados para alternar entre tile normal y obstaculo. Esa parte era para creacion/edicion, no para gameplay principal.

## Piezas

Las piezas son `GameObject` con componente `GamePiece`.

Cada pieza tiene un `matchValue`:

```csharp
public enum MatchValue
{
    Yellow,
    Blue,
    Magenta,
    Indigo,
    Green,
    Teal,
    Red,
    Cyan,
    Wild,
    Blanc,
    Orange
}
```

Colores/piezas realmente usadas por niveles actuales:

- `Blanc`
- `Cyan`
- `Green`
- `Indigo`
- `Magenta`
- `Orange`
- `Red`
- `Teal`
- `Yellow`

El viejo enum tambien tiene `Blue` y `Wild`, pero no parecen estar integrados con los prefabs actuales de puntos.

Carga de prefabs en el proyecto viejo:

- Campos directos en `Board`: `Blanc`, `Cyan`, `Green`, `Indigo`, `Magenta`, `Orange`, `Red`, `Teal`, `Yellow`.
- Array `gamePiecePrefabs`.
- `Resources/PiecePrefabLibrary.asset`.

Recomendacion para el proyecto nuevo: evitar referencias fragiles en Inspector. Usar una base de datos explicita tipo `ScriptableObject` con pares `{PieceType, Prefab}` o un diccionario generado al iniciar.

## Inicializacion de un nivel

Flujo actual de `Board.Start()` para niveles no-web:

1. Obtiene `LevelData` seleccionado.
2. Aplica datos del nivel:
   - `width = levelData.width`
   - `height = levelData.height`
   - `borderSize = levelData.borderSize`
   - Convierte `levelData.pieces` en `StartingObject[] piezasComienzo`.
   - Convierte `levelData.tiles` en `StartingTile[] startingTiles`.
3. Crea matrices:
   - `m_allTiles = new Tile[width, height]`
   - `m_allGamePieces = new GamePiece[width, height]`
4. Llama `SetupBoard()`.

`SetupBoard()` hace:

1. `SetupTiles()`.
2. `SetupCamera()`.
3. `setupGamePiezas()`.
4. Busca `ParticleManager` por tag `ParticleManager`.
5. Busca `GameManager` por tag `GameManager`.

Orden importante:

- Crear tiles antes que piezas.
- Inicializar matrices antes de instanciar tiles/piezas.
- No permitir movimiento hasta que el tablero este listo.

## Creacion de tiles

`SetupTiles()` funciona asi:

1. Recorre `startingTiles`.
2. Instancia los tiles especificos del nivel en sus coordenadas.
3. Recorre todas las coordenadas del tablero.
4. Donde no exista tile, instancia tile normal.

Cada tile se instancia con:

```csharp
Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity)
```

Luego:

- Nombre: `Tile (x,y)`.
- Se guarda en `m_allTiles[x, y]`.
- Se setea parent al `Board`.
- Se llama `tile.Init(x, y, this)`.

## Creacion de piezas

`setupGamePiezas()` recorre `piezasComienzo`.

Por cada pieza:

1. Instancia el prefab en `(x, y, 0)`.
2. Llama `MakeGamePiece(piece, x, y)`.
3. `MakeGamePiece` inicializa el `GamePiece` con el board.
4. Se guarda en `m_allGamePieces[x, y]`.
5. Se llama `SetCoord(x, y)`.
6. Se mueve visualmente desde arriba usando `fillYOffset = 20`.
7. Se hace parent del `Board`.

El efecto visual actual es que la pieza aparece inicialmente en `y + fillYOffset` y se anima hacia su celda.

## Movimiento principal

El juego no es un match-3 clasico con swap entre dos piezas. La mecanica real parece ser mover una pieza horizontalmente hacia un espacio vacio.

Reglas observadas:

- El jugador hace click sobre un tile que contiene pieza.
- Se calcula la distancia disponible hacia derecha e izquierda.
- Al pasar/arrastrar sobre un tile valido, se marca como target.
- Al soltar, si el target es tile normal y esta en la misma fila, la pieza se mueve horizontalmente.
- Solo se mueve si la celda objetivo esta vacia.
- El movimiento usa `GamePiece.Move(destX, destY, time)`.

`SwitchTilesRoutine`:

```csharp
GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

if (targetPiece == null && clickedPiece != null)
{
    clickedPiece.Move(targetTile.xIndex, clickedTile.yIndex, swapTime);
    m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex] = null;
    ...
}
```

Nota: el codigo viejo mueve a `targetTile.xIndex` manteniendo `clickedTile.yIndex`, por eso el movimiento efectivo es horizontal.

## Calculo de distancias validas

Al clickear una pieza, se llama `Distancia(tile)`, que calcula derecha e izquierda.

Variables:

- `dere`: maximo alcance hacia la derecha.
- `izqu`: maximo alcance hacia la izquierda.

Condiciones principales para marcar tiles disponibles:

- La celda de destino debe estar vacia.
- El tile de destino debe ser `TileType.Normal`.
- La logica revisa relacion con la celda inferior y obstaculos, especialmente cuando `y != 0`.
- Se colorean visualmente los tiles alcanzables con `tileColor()`.

Regla visual:

- `tileColor()` cambia color a amarillo/transparente y escala a `0.9`.
- `tileColorN()` restaura color normal y escala `1`.

`ReleaseTile()` llama siempre `colorNormal()` para limpiar highlights.

Para reconstruirlo mejor: convertir esta logica en una funcion pura tipo `GetValidMoveTargets(piecePosition)` que devuelva una lista de coordenadas validas. Despues la UI/highlight solo pinta esa lista.

## Restriccion de movimiento

`IsNextTo(Tile start, Tile end)` valida si el target esta dentro de la distancia calculada:

- Si `start.xIndex > end.xIndex`, usa `dere`.
- Si no, usa `izqu`.
- Requiere misma fila `start.yIndex == end.yIndex`.

Ojo: los nombres pueden estar invertidos porque `start` es el tile sobre el que se esta arrastrando y `end` es el tile clickeado. En una reimplementacion conviene nombrar claramente:

- `origin`
- `candidateTarget`
- `maxLeftDistance`
- `maxRightDistance`

## Matches

Los matches actuales son grupos lineales de piezas del mismo `matchValue`.

La longitud minima actual es `2`, no `3`.

Funciones:

- `FindMatches(startX, startY, direction, minLength = 2)` busca desde una pieza inicial en una direccion.
- `FindVerticalMatches` combina busqueda hacia arriba y abajo.
- `FindHorizontalMatches` combina busqueda hacia derecha e izquierda.
- `FindMatchesAt` une matches horizontales y verticales.
- `FindAllMatches` escanea todo el tablero.

Reglas de match:

- Si no hay pieza inicial, no hay match.
- Se agrega la pieza inicial.
- Se avanza en direccion hasta salirse del tablero, encontrar vacio o encontrar un color distinto.
- Si las piezas tienen el mismo `matchValue`, se agregan.
- Si el total llega a `minLength`, se considera match.
- Por defecto `minLength = 2`, asi que dos piezas iguales contiguas se eliminan.

Recomendacion para nuevo proyecto: definir explicitamente si el match debe ser de 2 o 3. El codigo viejo usa 2, y eso cambia mucho el diseno de niveles.

## Limpieza de piezas

Cuando hay matches:

1. Se desactiva input del jugador.
2. Se espera un pequeno delay.
3. Se eliminan piezas coincidentes.
4. Se suma puntaje.
5. Se disparan particulas si existe `ParticleManager`.
6. Se comprueba victoria despues de cada pieza eliminada.

`ClearPieceAt(x, y)`:

- Obtiene `m_allGamePieces[x, y]`.
- Si existe, borra la referencia de la matriz.
- Si `play == true`, llama `Ganar()`.
- Destruye el `GameObject`.

`ClearPieceAt(List<GamePiece>)`:

- Recorre piezas.
- Llama `ClearPieceAt(piece.xIndex, piece.yIndex)`.
- Llama `piece.ScorePoints()`.
- Llama `m_particleManager.ClearPieceFXAt(...)` si existe.

## Colapso / gravedad

La gravedad es por columnas.

`CollapseColumn(column)` busca espacios vacios desde abajo hacia arriba.

Reglas:

- Solo colapsa si la celda vacia no es obstaculo.
- Busca una pieza arriba para bajarla.
- No atraviesa obstaculos: usa `siguenteObtacule(column, pos)` para encontrar el siguiente obstaculo sobre la posicion actual.
- Si encuentra una pieza antes del obstaculo, la mueve hacia la celda vacia.
- Actualiza la matriz y las coordenadas internas.
- La animacion tarda `collapseTime * distancia`.

Pseudocodigo:

```text
for y desde 0 hasta height - 2:
    si no hay pieza en (x,y) y tile no es obstaculo:
        limite = siguienteObstaculo(x, y+1) o height
        buscar pieza en y+1 hasta limite-1
        si existe:
            mover pieza hacia (x,y)
            actualizar matriz
            cortar busqueda
```

Importante: los obstaculos dividen una columna en segmentos independientes. Las piezas no caen a traves de obstaculos.

## Resolucion de cascadas

Despues de mover una pieza:

1. Se buscan matches cerca de origen/destino.
2. Se limpian esos matches.
3. Se colapsan columnas.
4. Se buscan matches en todo el tablero.
5. Se repite mientras sigan apareciendo matches.

La intencion es permitir cascadas automaticas.

Nota tecnica del codigo viejo: `ClearAndCollapseRoutine` crea `movingPieces` pero no siempre lo llena con el resultado de `CollapseColumn`, por lo que la implementacion vieja puede tener bugs. En el proyecto nuevo conviene escribir esta parte de forma mas clara:

```text
do:
    matches = FindAllMatches()
    if matches vacio: break
    Clear(matches)
    movingPieces = CollapseAllColumns()
    esperar animaciones
while true
```

## Condicion de victoria

La victoria se evalua contando piezas restantes.

`Ganar()`:

1. Recorre todo `m_allGamePieces`.
2. Cuenta cuantas celdas tienen pieza.
3. Si `cont == 0`, llama `m_gameManager.ganar()`.
4. Desactiva input.

Por lo tanto, la meta del nivel es dejar el tablero sin piezas.

No hay condicion de objetivo por color en el codigo activo. Habia codigo comentado para contar colores, pero no esta usado.

## Condicion de derrota

La derrota principal parece venir del tiempo.

`GameManager` configura `tiempoLevel` desde el nivel:

```csharp
tiempoLevel = levelData.time;
```

Si no hay tiempo valido, usa default `200`.

Al acabarse el flujo de juego o por llamada manual, `GameManager.perder()` termina el nivel con derrota.

El detalle exacto del contador esta repartido en `GameManager.PlayGameRoutine`, pero para reconstruir basta con:

- Cada nivel tiene tiempo.
- Si llega a cero antes de limpiar todas las piezas, se pierde.
- En pausa se desactiva input y se detiene/reanuda el flujo.

## Camara

La camara ortografica se ajusta al tablero.

Parametros actuales:

- `borderSize`
- `topScreenPadding`
- `bottomScreenPadding`
- `horizontalScreenPadding`

La camara se centra en el tablero y calcula `orthographicSize` para que quepa dentro del area segura de pantalla.

Recomendacion: conservar esta idea en el proyecto nuevo, pero aislarla en un componente `BoardCameraFitter`.

## Editor/creador de niveles viejo

Habia una escena `LevelCreador` con modo de edicion:

- Si `scene.name == "LevelCreador" && play == false`, los clicks editan el nivel.
- Si la celda no tiene pieza y el color seleccionado no es `piso`, crea una pieza.
- Si el color seleccionado es `piso`, alterna tile normal/obstaculo.
- Si la celda ya tiene pieza, la borra.
- Guardar nivel subia datos a un backend viejo por HTTP.

Backend viejo:

- Base URL: `http://178.128.179.253/api/`
- Endpoints usados:
  - `nivel/verPiezasNivel`
  - `nivel/iPieza`
  - `nivel/borrarPiezas`

Para el proyecto nuevo, mejor no depender de ese backend. Conviene crear niveles localmente como JSON, ScriptableObjects o un editor tool moderno.

## Recomendaciones para rehacer en Unity nuevo

Arquitectura sugerida:

- `LevelDefinition`: ScriptableObject o JSON con `width`, `height`, `time`, `pieces`, `tiles`.
- `BoardController`: construye tablero, maneja matrices y reglas.
- `TileView`: input y visual de celda, sin reglas de negocio.
- `PieceView`: animacion y datos visuales de pieza.
- `MoveResolver`: calcula destinos validos.
- `MatchResolver`: encuentra matches.
- `GravityResolver`: aplica colapso por columnas y obstaculos.
- `LevelState`: controla jugando, pausado, ganado, perdido.
- `BoardCameraFitter`: ajusta camara al tablero.

Decisiones importantes a confirmar antes de reimplementar:

- Si los matches deben ser de 2 piezas como el juego viejo o de 3 piezas como match-3 tradicional.
- Si el movimiento debe ser solo horizontal o tambien vertical.
- Si una pieza puede moverse cualquier distancia dentro del rango o solo una celda.
- Si los obstaculos siempre bloquean la gravedad.
- Si hay que mantener `ObstacleMovil` como tipo real o simplificarlo.
- Si el objetivo sigue siendo eliminar todas las piezas o si habra objetivos por color/cantidad.

## Bugs o deuda tecnica del proyecto viejo que conviene evitar

- Referencias a prefabs por Inspector y `Resources` fragiles.
- Mezcla de gameplay, editor de niveles, red, UI y camara dentro de `Board`.
- Uso de strings para tipos de piezas y tiles sin validacion fuerte.
- Variantes mal escritas como `Obtacle`.
- Coroutines encadenadas dificiles de razonar.
- `ClearAndCollapseRoutine` no usa claramente las piezas que se movieron tras colapsar.
- Uso de `OnMouseDown/Enter/Up`, que en mobile moderno conviene reemplazar por Input System o raycasts controlados.
- Dependencia del backend viejo para crear/guardar niveles.

## Resumen compacto de gameplay

1. Cargar nivel seleccionado.
2. Crear tablero `width x height`.
3. Colocar obstaculos/tiles especiales.
4. Rellenar resto con tiles normales.
5. Colocar piezas iniciales.
6. El jugador selecciona una pieza.
7. Se muestran destinos horizontales validos.
8. El jugador arrastra/suelta sobre un destino normal y vacio.
9. La pieza se mueve horizontalmente.
10. Se eliminan grupos de piezas iguales contiguas de longitud minima 2.
11. Las piezas sobre espacios vacios caen por columna, sin atravesar obstaculos.
12. Se repiten cascadas hasta que no haya matches.
13. Si no quedan piezas, gana.
14. Si se acaba el tiempo antes de ganar, pierde.
