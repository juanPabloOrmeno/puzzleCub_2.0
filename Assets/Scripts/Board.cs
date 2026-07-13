using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    private const int TileSortingOrder = 0;
    private const int GamePieceSortingOrder = 10;

    [Header("Tablero")]
    public int width;
    public int height;
    public GameObject tilePrefab;
    public StartingTile[] startingTiles;
    public GameObject tileNormalPrefab;
    public GameObject tileObtaclePrefab;

    public GameObject tileObtacleMovilPrefab;
    public StartingObject[] piezasComienzo;
    Tile[,] m_allTiles;

    GamePiece[,] m_allGamePieces;

    public GameObject[] gamePiecePrefabs;

    [Header("Camera")]
    public float borderSize;
    BoardCameraSetup m_cameraSetup;

    [Header("Piezas")]
    public GameObject Blanc;
    public GameObject Cyan;
    public GameObject Green;
    public GameObject Indigo;
    public GameObject Magenta;
    public GameObject Orange;
    public GameObject Red;
    public GameObject Teal;
    public GameObject Yellow;

    [Header("Gameplay")]
    public int Level = 1;
    public bool m_playerInputEnabled = true;

    private readonly float swapTime = 0.15f;
    private Tile m_clickedTile;
    private Tile m_targetTile;
    private readonly HashSet<Vector2Int> m_validMoveTargets = new HashSet<Vector2Int>();
    private int m_rightMoveLimit;
    private int m_leftMoveLimit;
    private int m_lastScreenWidth;
    private int m_lastScreenHeight;
    private Rect m_lastSafeArea;
    public ParticleManager m_particleManager;
    private SoundManager m_soundManager;

    void Start()
    {
        m_particleManager = FindAnyObjectByType<ParticleManager>();
        m_soundManager = SoundManager.Instance;
        LoadLevel(LevelDatabase.SelectedLevel);
    }

    public void LoadLevel(int level)
    {
        Level = level;
        ApplyLevelData(LevelDatabase.GetLevelData(Level));
        InitializeBoardArrays();
        SetupBoard();
    }

    void Update()
    {
        if (Screen.width != m_lastScreenWidth || Screen.height != m_lastScreenHeight || Screen.safeArea != m_lastSafeArea)
        {
            SetupCamera();
        }

        UpdateDragTargetFromPointer();

        if (m_clickedTile != null && Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ReleaseTile();
        }
    }

    private void InitializeBoardArrays()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
    }

    public void SetupBoard()
    {
        SetupTiles();
        SetupCamera();
        SetupGamePieces();
    }

    public void ClickTile(Tile tile)
    {
        if (!m_playerInputEnabled || tile == null)
        {
            return;
        }

        if (m_clickedTile == null && GetGamePieceAt(tile.xIndex, tile.yIndex) != null)
        {
            HighlightAvailableMoves(tile);
            m_clickedTile = tile;
        }
    }

    public void ClickGamePiece(GamePiece gamePiece)
    {
        if (gamePiece != null && IsWithinBounds(gamePiece.xIndex, gamePiece.yIndex))
        {
            ClickTile(m_allTiles[gamePiece.xIndex, gamePiece.yIndex]);
        }
    }

    public void DragToTile(Tile tile)
    {
        if (!m_playerInputEnabled || m_clickedTile == null || tile == null)
        {
            return;
        }

        if (IsValidMoveTarget(tile))
        {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        ResetTileColors();

        if (m_clickedTile != null && m_targetTile != null && IsValidMoveTarget(m_targetTile))
        {
            // La mecánica original permite mover piezas solo en horizontal.
            if (m_clickedTile.yIndex == m_targetTile.yIndex)
            {
                SwitchTiles(m_clickedTile, m_targetTile);
            }
        }

        m_clickedTile = null;
        m_targetTile = null;
        m_validMoveTargets.Clear();
    }

    private void UpdateDragTargetFromPointer()
    {
        if (m_clickedTile == null || Mouse.current == null || !Mouse.current.leftButton.isPressed || Camera.main == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider == null)
        {
            return;
        }

        Tile tile = hit.collider.GetComponent<Tile>();
        if (tile == null)
        {
            GamePiece piece = hit.collider.GetComponent<GamePiece>();
            if (piece != null && IsWithinBounds(piece.xIndex, piece.yIndex))
            {
                tile = m_allTiles[piece.xIndex, piece.yIndex];
            }
        }

        if (tile != null)
        {
            DragToTile(tile);
        }
    }

    private void SetupTiles()
    {
        // Primero coloca los tiles especiales del nivel y luego rellena los huecos con tiles normales.
        if (startingTiles != null)
        {
            foreach (StartingTile sTile in startingTiles)
            {
                if (sTile != null)
                {
                    MakeTile(sTile.tilePrefab, sTile.x, sTile.y);
                }
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allTiles[i, j] == null)
                {
                    MakeTile(GetDefaultTilePrefab(), i, j);
                }
            }
        }
    }

    private void MakeTile(GameObject prefab, int x, int y)
    {
        if (prefab == null || !IsWithinBounds(x, y))
        {
            return;
        }

        GameObject tile = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
        Tile tileComponent = tile.GetComponent<Tile>();
        if (tileComponent == null)
        {
            Debug.LogWarning("Prefab de tile inválido en (" + x + "," + y + "): " + prefab.name);
            Destroy(tile);
            return;
        }

        tile.name = "Tile (" + x + "," + y + ")";
        tile.transform.parent = transform;
        SetSpriteRenderers(tile, TileSortingOrder, false);
        tileComponent.Init(x, y, this);
        m_allTiles[x, y] = tileComponent;
    }

    private GameObject GetDefaultTilePrefab()
    {
        return tileNormalPrefab != null ? tileNormalPrefab : tilePrefab;
    }

    private void SetupGamePieces()
    {
        if (piezasComienzo == null)
        {
            return;
        }

        foreach (StartingObject sPiece in piezasComienzo)
        {
            if (sPiece != null && sPiece.prefab != null)
            {
                if (sPiece.prefab.GetComponent<GamePiece>() == null)
                {
                    Debug.LogWarning("Prefab de pieza inválido en (" + sPiece.x + "," + sPiece.y + "): " + sPiece.prefab.name);
                    continue;
                }

                GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity);
                MakeGamePiece(piece, sPiece.x, sPiece.y);
            }
        }
    }

    private void ApplyLevelData(LevelData levelData)
    {
        if (levelData == null)
        {
            return;
        }

        width = levelData.width;
        height = levelData.height;
        borderSize = levelData.borderSize;

        // Convierte los datos del JSON en las estructuras que el tablero ya sabe instanciar.
        List<StartingObject> levelPieces = new List<StartingObject>();
        if (levelData.pieces != null)
        {
            foreach (LevelPieceData pieceData in levelData.pieces)
            {
                GameObject prefab = GetPiecePrefab(pieceData.type);
                if (prefab == null)
                {
                    Debug.LogWarning("No se encontró prefab para pieza: " + pieceData.type);
                    continue;
                }

                levelPieces.Add(new StartingObject
                {
                    prefab = prefab,
                    x = pieceData.x,
                    y = pieceData.y
                });
            }
        }

        List<StartingTile> levelTiles = new List<StartingTile>();
        if (levelData.tiles != null)
        {
            foreach (LevelTileData tileData in levelData.tiles)
            {
                GameObject prefab = GetTilePrefab(tileData.type);
                if (prefab == null)
                {
                    Debug.LogWarning("No se encontró prefab para tile: " + tileData.type);
                    continue;
                }

                levelTiles.Add(new StartingTile
                {
                    tilePrefab = prefab,
                    x = tileData.x,
                    y = tileData.y
                });
            }
        }

        piezasComienzo = levelPieces.ToArray();
        startingTiles = levelTiles.ToArray();
    }

    private void MakeGamePiece(GameObject prefab, int x, int y, float moveTime = 0.1f)
    {
        if (prefab == null || !IsWithinBounds(x, y))
        {
            return;
        }

        GamePiece gamePiece = prefab.GetComponent<GamePiece>();
        if (gamePiece == null)
        {
            Debug.LogWarning("Prefab de pieza inválido en (" + x + "," + y + "): " + prefab.name);
            Destroy(prefab);
            return;
        }

        gamePiece.Init(this);
        PlaceGamePiece(gamePiece, x, y);
        gamePiece.SnapTo(x, y);

        prefab.transform.parent = transform;
    }

    private GameObject GetTilePrefab(string tileType)
    {
        if (IsSameType(tileType, "Normal"))
        {
            return tileNormalPrefab;
        }

        if (IsSameType(tileType, "ObstacleMovil") ||
            IsSameType(tileType, "ObtacleMovil") ||
            IsSameType(tileType, "MobileObstacle") ||
            IsSameType(tileType, "TileObtacleMovil"))
        {
            return tileObtacleMovilPrefab != null ? tileObtacleMovilPrefab : tileObtaclePrefab;
        }

        if (IsSameType(tileType, "Obstacle") ||
            IsSameType(tileType, "Obtacle") ||
            IsSameType(tileType, "TileObtacle"))
        {
            return tileObtaclePrefab;
        }

        return FindTilePrefabInSceneDefaults(tileType) ?? tileObtaclePrefab;
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD:  Invalid GamePiece!");
            return;
        }

        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        SetSpriteRenderers(gamePiece.gameObject, GamePieceSortingOrder, true);

        if (IsWithinBounds(x, y))
        {
            m_allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoord(x, y);
    }

    private void SetSpriteRenderers(GameObject target, int sortingOrder, bool forceOpaque)
    {
        SpriteRenderer[] spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingOrder = sortingOrder;

            if (forceOpaque)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }
        }
    }

    private GamePiece GetGamePieceAt(int x, int y)
    {
        return IsWithinBounds(x, y) ? m_allGamePieces[x, y] : null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    private IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (!m_playerInputEnabled)
        {
            yield break;
        }

        GamePiece clickedPiece = GetGamePieceAt(clickedTile.xIndex, clickedTile.yIndex);
        GamePiece targetPiece = GetGamePieceAt(targetTile.xIndex, targetTile.yIndex);
        if (clickedPiece == null || targetPiece != null)
        {
            yield break;
        }

        clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
        m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex] = null;
        m_allGamePieces[targetTile.xIndex, targetTile.yIndex] = clickedPiece;
        clickedPiece.SetCoord(targetTile.xIndex, targetTile.yIndex);

        yield return new WaitForSeconds(swapTime);

        List<GamePiece> matches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
        yield return StartCoroutine(ClearAndRefillBoardRoutine(matches));
    }

    private IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> matches = gamePieces;

        do
        {
            m_playerInputEnabled = false;
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return StartCoroutine(CollapseBoardRoutine());

            matches = FindAllMatches();
            yield return null;
        }
        while (matches.Count != 0);

        m_playerInputEnabled = true;
        CheckWinCondition();
    }

    private IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        if (gamePieces == null || gamePieces.Count == 0)
        {
            yield break;
        }

        if (m_soundManager != null)
        {
            m_soundManager.PlaySoundPiece();
        }

        yield return new WaitForSeconds(0.15f);
        ClearPieceAt(gamePieces);

        List<GamePiece> movingPieces = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            movingPieces = movingPieces.Union(CollapseColumn(i)).ToList();
        }

        while (!IsCollapsed(movingPieces))
        {
            yield return null;
        }

        List<GamePiece> newMatches = FindMatchesAt(movingPieces);
        if (newMatches.Count > 0)
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(newMatches));
        }
    }

    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = GetGamePieceAt(x, y);
        if (pieceToClear == null)
        {
            return;
        }

        m_allGamePieces[x, y] = null;
        if (m_particleManager != null)
        {
            m_particleManager.ClearPieceFXAt(x, y, -1);
        }

        pieceToClear.ScorePoints();
        Destroy(pieceToClear.gameObject);
    }

    private void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    private IEnumerator CollapseBoardRoutine()
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            movingPieces = movingPieces.Union(CollapseColumn(i)).ToList();
        }

        while (!IsCollapsed(movingPieces))
        {
            yield return null;
        }
    }

    private bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null && (piece.IsMoving || Vector3.Distance(piece.transform.position, new Vector3(piece.xIndex, piece.yIndex, 0f)) > 0.001f))
            {
                return false;
            }
        }

        return true;
    }

    private List<GamePiece> CollapseColumn(int column, float collapseTime = 0.3f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int y = 0; y < height - 1; y++)
        {
            if (GetGamePieceAt(column, y) != null || m_allTiles[column, y].tileType == TileType.Obstacle)
            {
                continue;
            }

            for (int nextY = y + 1; nextY < GetNextObstacleY(column, nextY); nextY++)
            {
                GamePiece piece = GetGamePieceAt(column, nextY);
                if (piece == null)
                {
                    continue;
                }

                piece.Move(column, y, collapseTime * (nextY - y));
                m_allGamePieces[column, y] = piece;
                m_allGamePieces[column, nextY] = null;
                piece.SetCoord(column, y);

                if (!movingPieces.Contains(piece))
                {
                    movingPieces.Add(piece);
                }

                break;
            }
        }

        return movingPieces;
    }

    private int GetNextObstacleY(int column, int startY)
    {
        for (int y = startY; y < height; y++)
        {
            if (m_allTiles[column, y].tileType == TileType.Obstacle)
            {
                return y;
            }
        }

        return height;
    }

    private void HighlightAvailableMoves(Tile startTile)
    {
        m_validMoveTargets.Clear();
        m_rightMoveLimit = HighlightDirection(startTile, 1);
        m_leftMoveLimit = HighlightDirection(startTile, -1);
    }

    private int HighlightDirection(Tile startTile, int direction)
    {
        int count = 0;
        GamePiece movingPiece = GetGamePieceAt(startTile.xIndex, startTile.yIndex);

        for (int x = startTile.xIndex + direction; x >= 0 && x < width; x += direction)
        {
            bool isEmptyNormalTile = GetGamePieceAt(x, startTile.yIndex) == null && m_allTiles[x, startTile.yIndex].tileType == TileType.Normal;
            bool hasSupportBelow = HasSupportBelow(x, startTile.yIndex);
            bool hasMatchingPieceBelow = HasMatchingPieceBelow(x, startTile.yIndex, movingPiece);

            if (isEmptyNormalTile)
            {
                m_allTiles[x, startTile.yIndex].tileColor();
                m_validMoveTargets.Add(new Vector2Int(x, startTile.yIndex));
            }

            if (isEmptyNormalTile && hasMatchingPieceBelow)
            {
                return count + 1;
            }

            if (isEmptyNormalTile && hasSupportBelow)
            {
                count++;
                continue;
            }

            return count + 1;
        }

        return count + 1;
    }

    private bool HasMatchingPieceBelow(int x, int y, GamePiece movingPiece)
    {
        if (movingPiece == null || y <= 0 || !IsWithinBounds(x, y - 1))
        {
            return false;
        }

        GamePiece supportPiece = GetGamePieceAt(x, y - 1);
        return supportPiece != null && supportPiece.matchValue == movingPiece.matchValue;
    }

    private bool IsValidMoveTarget(Tile tile)
    {
        return tile != null && m_validMoveTargets.Contains(new Vector2Int(tile.xIndex, tile.yIndex));
    }

    private bool HasSupportBelow(int x, int y)
    {
        if (y <= 0)
        {
            return true;
        }

        if (!IsWithinBounds(x, y - 1))
        {
            return false;
        }

        return GetGamePieceAt(x, y - 1) != null || m_allTiles[x, y - 1].tileType == TileType.Obstacle;
    }

    private void ResetTileColors()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (m_allTiles[x, y] != null && m_allTiles[x, y].tileType == TileType.Normal)
                {
                    m_allTiles[x, y].tileColorN();
                }
            }
        }
    }

    private bool IsNextTo(Tile start, Tile end)
    {
        if (start.yIndex != end.yIndex)
        {
            return false;
        }

        int limit = end.xIndex > start.xIndex ? m_rightMoveLimit : m_leftMoveLimit;
        return Mathf.Abs(start.xIndex - end.xIndex) <= limit;
    }

    private List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                combinedMatches = combinedMatches.Union(FindMatchesAt(x, y)).ToList();
            }
        }

        return combinedMatches;
    }

    private List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 2)
    {
        GamePiece startPiece = GetGamePieceAt(startX, startY);
        if (startPiece == null)
        {
            return null;
        }

        List<GamePiece> matches = new List<GamePiece> { startPiece };
        int maxValue = Mathf.Max(width, height);

        for (int i = 1; i < maxValue; i++)
        {
            int nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            int nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = GetGamePieceAt(nextX, nextY);
            if (nextPiece == null || nextPiece.matchValue != startPiece.matchValue || matches.Contains(nextPiece))
            {
                break;
            }

            matches.Add(nextPiece);
        }

        return matches.Count >= minLength ? matches : null;
    }

    private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 2)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, Vector2.up, 2) ?? new List<GamePiece>();
        List<GamePiece> downwardMatches = FindMatches(startX, startY, Vector2.down, 2) ?? new List<GamePiece>();
        List<GamePiece> combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return combinedMatches.Count >= minLength ? combinedMatches : null;
    }

    private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 2)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, Vector2.right, 2) ?? new List<GamePiece>();
        List<GamePiece> leftMatches = FindMatches(startX, startY, Vector2.left, 2) ?? new List<GamePiece>();
        List<GamePiece> combinedMatches = rightMatches.Union(leftMatches).ToList();

        return combinedMatches.Count >= minLength ? combinedMatches : null;
    }

    private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 2)
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y, minLength) ?? new List<GamePiece>();
        List<GamePiece> verticalMatches = FindVerticalMatches(x, y, minLength) ?? new List<GamePiece>();

        return horizontalMatches.Union(verticalMatches).ToList();
    }

    private List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 2)
    {
        List<GamePiece> matches = new List<GamePiece>();
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
            }
        }

        return matches;
    }

    private void CheckWinCondition()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (m_allGamePieces[x, y] != null)
                {
                    return;
                }
            }
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.ganar();
        }

        m_playerInputEnabled = false;
    }

    private bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;
        public int x;
        public int y;
    }

    [System.Serializable]
    public class StartingTile
    {
        public GameObject tilePrefab;
        public int x;
        public int y;
    }

    private GameObject GetPiecePrefab(string pieceType)
    {
        if (string.IsNullOrEmpty(pieceType))
        {
            return null;
        }

        GameObject namedPrefab = GetNamedPiecePrefab(pieceType);
        if (namedPrefab != null)
        {
            return namedPrefab;
        }

        if (gamePiecePrefabs != null)
        {
            foreach (GameObject prefab in gamePiecePrefabs)
            {
                if (prefab == null)
                {
                    continue;
                }

                string prefabName;
                try
                {
                    prefabName = prefab.name;
                }
                catch (MissingReferenceException)
                {
                    continue;
                }

                if (prefabName.IndexOf(pieceType, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return prefab;
                }

                GamePiece piece;
                try
                {
                    piece = prefab.GetComponent<GamePiece>();
                }
                catch (MissingReferenceException)
                {
                    continue;
                }

                if (piece != null && IsSameType(piece.matchValue.ToString(), pieceType))
                {
                    return prefab;
                }
            }
        }

        // Fallback para prefabs que no estén asignados directamente en el Inspector.
        PiecePrefabLibrary library = Resources.Load<PiecePrefabLibrary>("PiecePrefabLibrary");
        if (library != null)
        {
            GameObject libraryPrefab = library.GetPrefab(pieceType);
            if (libraryPrefab != null)
            {
                return libraryPrefab;
            }
        }

        return null;
    }

    private GameObject GetNamedPiecePrefab(string pieceType)
    {
        switch (pieceType.ToLowerInvariant())
        {
            case "blanc":
                return Blanc;
            case "cyan":
                return Cyan;
            case "green":
                return Green;
            case "indigo":
                return Indigo;
            case "magenta":
                return Magenta;
            case "orange":
                return Orange;
            case "red":
                return Red;
            case "teal":
                return Teal;
            case "yellow":
                return Yellow;
            default:
                return null;
        }
    }

    private GameObject FindTilePrefabInSceneDefaults(string tileType)
    {
        if (startingTiles == null)
        {
            return null;
        }

        foreach (StartingTile tile in startingTiles)
        {
            if (tile == null || tile.tilePrefab == null)
            {
                continue;
            }

            if (IsSameType(GetTileDataType(tile.tilePrefab), tileType))
            {
                return tile.tilePrefab;
            }
        }

        return null;
    }

    private string GetTileDataType(GameObject tilePrefab)
    {
        if (tilePrefab == null)
        {
            return "Obstacle";
        }

        string prefabName = tilePrefab.name.ToLowerInvariant();
        if (prefabName.Contains("movil") || prefabName.Contains("mobile"))
        {
            return "ObstacleMovil";
        }

        if (prefabName.Contains("obtacle") || prefabName.Contains("obstacle"))
        {
            return "Obstacle";
        }

        Tile tile = tilePrefab.GetComponent<Tile>();
        if (tile != null && tile.tileType == TileType.Normal)
        {
            return "Normal";
        }

        return "Obstacle";
    }
    private bool IsSameType(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private void SetupCamera()
    {
        m_cameraSetup = GetComponent<BoardCameraSetup>();
        if (m_cameraSetup == null)
        {
            m_cameraSetup = gameObject.AddComponent<BoardCameraSetup>();
        }

        m_cameraSetup.SetupCamera(width, height, borderSize);
        m_lastScreenWidth = Screen.width;
        m_lastScreenHeight = Screen.height;
        m_lastSafeArea = Screen.safeArea;
    }
}
