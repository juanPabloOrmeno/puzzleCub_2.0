# PuzzleCub: tutorial integrado

## Cómo verlo

1. Abre el proyecto con Unity **6000.7.0a1**.
2. Entra a un nivel desde el menú, o ejecuta `Assets/Scenes/Level.unity`.
3. La primera vez aparece la guía automáticamente.
4. Usa **Siguiente**, **Atrás** y **¡A jugar!**, o **Omitir / cerrar**.
5. Durante la partida puedes volver a abrirla desde **Ayuda**, abajo a la derecha.

`Board.Start()` agrega `PuzzleTutorial` automáticamente. La interfaz se crea en
runtime: no hay que arrastrar componentes ni conectar botones en el Inspector.

## Tutorial para el jugador

### 1. Deja el tablero vacío

Elimina todas las piezas para ganar. Completar un nivel permite avanzar al siguiente.
Los obstáculos forman parte del tablero; no tienes que eliminarlos.

### 2. Desliza una pieza

Mantén pulsada una pieza con el dedo o el botón izquierdo del ratón. Las casillas
amarillas indican sus destinos válidos. Arrastra hacia una de ellas en la misma
fila y suelta. El movimiento es horizontal y el destino debe estar vacío.
Si no aparecen destinos, prueba otra pieza.

### 3. Combina colores

Junta **dos o más** piezas del mismo color en horizontal o vertical para
eliminarlas. Las diagonales no cuentan y no es necesario juntar tres piezas.

### 4. Aprovecha las caídas

Después de un movimiento se resuelven las combinaciones y las piezas caen a
los huecos disponibles. Una caída puede producir otra combinación. Los
obstáculos bloquean el paso y la caída. Espera a que termine la reacción.

### 5. Practica

Busca dos piezas del mismo color y acerca una usando las casillas iluminadas.
La guía deja un recordatorio al volver al tablero y confirma el primer movimiento
cuando termina su resolución. Continúa hasta que no queden piezas.
Si necesitas empezar de nuevo, usa el reinicio del menú superior.

Las ilustraciones son esquemas explicativos, no un tablero interactivo adicional.
La práctica se realiza en el nivel seleccionado; la guía no lo reemplaza ni mueve
piezas por el jugador.

## Integración

- `Assets/Scripts/PuzzleTutorial.cs`: contenido, diagramas, área segura, navegación,
  persistencia y recordatorio del primer movimiento.
- `Assets/Scripts/Board.cs`: creación de la guía, cancelación de selecciones al abrirla,
  bloqueo del input detrás de la guía, lectura explícita del toque principal y
  evento `MoveResolved` después de resolver el movimiento.
- `Assets/Scripts/TopMenuPanelActions.cs`: respeta el bloqueo de la guía al procesar
  los accesos de reinicio y selección de nivel.
- La ayuda no se puede abrir durante una resolución ni después de ganar.
- Se bloquea el input desde el inicio del desplazamiento para evitar otra acción
  durante los primeros 0,15 segundos de animación.
- No se modifica `Time.timeScale`; la guía se abre con el tablero disponible.
- El canvas se adapta al área segura y a cambios de resolución/orientación.

### Persistencia

| PlayerPrefs | Significado |
| --- | --- |
| `PuzzleCub.Tutorial.Seen.v1` | El jugador cerró u omitió la guía; evita repetir la apertura automática. |
| `PuzzleCub.Tutorial.Completed.v1` | Llegó a la última página y pulsó «¡A jugar!». No acredita haber superado el nivel. |

La guía puede repetirse con **Ayuda** aunque estas claves estén guardadas.
Para probar otra vez la primera apertura, elimina exclusivamente estas dos claves
en un entorno de prueba; no es necesario borrar el progreso de niveles.

## Análisis con MCP REA

Fecha: 2026-10-18. Evidencia exportada en `PuzzleCub_REA.evidence.json`.

### APK

- Archivo: `PuzzleCub.apk`.
- SHA-256: `55bedfe69fc59c4534e87a5854d4f945f0245043865c74b983747fcb76fa2a0f`.
- Inventario REA: **403 entradas**, sin contradicciones de integridad reportadas.
- Se observaron `lib/arm64-v8a/libil2cpp.so`,
  `assets/bin/Data/Managed/Metadata/global-metadata.dat` y `assets/bin/Data/boot.config`.
  Estos elementos sustentan la identificación de una compilación Unity IL2CPP ARM64.
- Evidencia de inspección:
  `ev_55453f8b7fd760b70fe00884504bdafe71d07fbc46b0b4ecefa50386387722bb`.
- Vista de las entradas seleccionadas:
  `ev_fee1957152d08609a313dd4ba878d1f9924e6aff60e06c2a19288df08b7a9050`.

### Ensamblado de respaldo

- Archivo: `PuzzleCub_BackUpThisFolder_ButDontShipItWithYourGame/Managed/Assembly-CSharp.dll`.
- SHA-256: `abb469e4319828dfd283dff0d303539da9c0757634217f01851d7a63b1b8ca83`.
- REA inspeccionó los tipos `Board`, `GameManager`, `GamePiece` y `Tile`:
  **80 métodos**, **62 campos** y **471 relaciones de llamada** en esa selección.
- La página de llamadas examinada confirma el flujo estático
  `Board.Start → LevelDatabase.SelectedLevel → Board.LoadLevel`, seguido por
  `ApplyLevelData`, `InitializeBoardArrays` y `SetupBoard`.
- Evidencia de miembros:
  `ev_ed929a67a65f631130c471036801b26ecfd21ccff2910b04ec521b7fa2144119`.
- Página de llamadas:
  `ev_fc9b199850005ee5fc6a7f14d52bceab0852d90ded6abe63f3e80802a781cd98`.

El ensamblado es un artefacto de respaldo separado. No se ha demostrado su
equivalencia exacta con el código nativo del APK ni con los fuentes actuales.
La inspección fue estática; no se ejecutó el APK. REA reportó cobertura parcial
de cadenas por indicadores terminales de algunas entradas `#US`.

### Reglas verificadas en el proyecto actual

Los textos del tutorial se basan en estos puntos del código fuente:

| Regla | Implementación |
| --- | --- |
| Solo mover horizontalmente a destinos válidos vacíos | `Board.ReleaseTile`, `HighlightDirection`, `SwitchTilesRoutine` |
| Combinar desde dos piezas en fila o columna | `Board.FindMatches`, `FindHorizontalMatches`, `FindVerticalMatches` |
| Caídas y cascadas | `Board.ClearAndRefillBoardRoutine`, `CollapseColumn` |
| Obstáculos que interrumpen la caída | `Board.GetNextObstacleY` |
| Victoria al no quedar piezas | `Board.CheckWinCondition`, `GameManager.FinishGame` |
| Progreso de niveles | `LevelDatabase.CompleteLevel` |

Los niveles normales muestran una cuenta atrás `MM:SS` en el escudo del panel
inferior, mediante `LevelCountdown`, usando `time` del JSON del nivel cargado.
Se detiene al terminar la partida, durante una pausa o con la guía modal abierta.
Al llegar a cero se bloquea el tablero y se ofrece reintentar o volver a niveles.
Las tres lecciones de práctica no tienen límite de tiempo.
El enum actual de tiles solo diferencia
`Normal` y `Obstacle`; no se promete movimiento autónomo de obstáculos.

## Verificación

### Compatibilidad de Unity AI Assistant

Con Unity **6000.7.0a1**, mantener `com.unity.ai.assistant` en **2.17.0-pre.1**
(fijado en `Packages/manifest.json`). La versión 2.20.0-pre.2 producía dos errores
CS0117 en `MaterialUtilities.cs`: su rama `UNITY_6000_7_OR_NEWER` utiliza
`MaterialGlobalIlluminationFlags.BakedEmission`, pero este editor alfa todavía
expone `BakedEmissive`. También se comprobó ese cambio incompatible en las
versiones 2.18.0-pre.1, 2.18.0-pre.2 y 2.19.0-pre.2.

Después de cambiar el paquete, el editor abierto resolvió 2.17.0-pre.1 y registró
`Tundra build success (29.31 seconds)` y `Scripting: successfully reloaded assembly`
en `Logs/Editor.log`. Esta comprobación incluye la compilación de paquetes dentro
de Unity, además de la comprobación externa de scripts descrita a continuación.

- Compilación externa de todos los `Assets/Scripts/**/*.cs` con el SDK .NET
  incluido en Unity 6000.7.0a1 y referencias reales a UnityEngine, UnityEngine.UI
  e Input System: **0 errores**, 10 advertencias de campos existentes sin asignación
  explícita o sin uso.
- Proyecto temporal utilizado: `/tmp/opencode/PuzzleCub.Tutorial.Check.csproj`.
- `git diff --check`: sin errores.
- La compilación externa no valida renderizado, callbacks ni interacción real.
  No se ha realizado una prueba visual en Play Mode o dispositivo Android.

### Comprobaciones pendientes en Unity / Android

1. Primera entrada: aparece la guía y no se mueven piezas al pulsar detrás de ella.
2. Recorrer las cinco páginas; «Atrás» está desactivado en la primera.
3. Omitir, recargar el nivel y comprobar que no reaparece automáticamente.
4. Reabrir con «Ayuda» y finalizar; el tablero y el progreso se conservan.
5. Arrastrar con ratón y con dedo; el mensaje cambia solo tras un movimiento válido.
6. Hacer una combinación de dos piezas, una cascada y una caída sobre obstáculo.
7. Pulsar repetidamente durante la animación: no se aceptan movimientos adicionales.
8. Comprobar reinicio y selección de nivel, tanto con guía abierta como cerrada.
9. Ganar el nivel: la pantalla de victoria queda por encima y el recordatorio se oculta.
10. Revisar legibilidad en vertical, horizontal y una pantalla con notch.
