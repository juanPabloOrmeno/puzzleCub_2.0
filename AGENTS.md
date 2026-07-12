# AGENTS.md

## Project Shape
- Unity project, editor version `6000.7.0a1` from `ProjectSettings/ProjectVersion.txt`; current build target in generated csproj is Android.
- Runtime scripts live in `Assets/Scripts/`; gameplay scene in build settings is `Assets/Scenes/Level.unity`.
- `Assembly-CSharp*.csproj` and `puzzleCub2.0.slnx` are Unity-generated. Do not hand-edit them; edit assets/scripts instead.

## Verification
- Fast C# compile check: `dotnet build Assembly-CSharp.csproj`.
- There are no repo test files currently; `com.unity.test-framework` is installed but no `TestFixture`/`UnityTest` sources are present.
- For scene/serialization-sensitive changes, prefer opening/running in Unity `6000.7.0a1`; `dotnet build` only validates C# compilation.

## Level Data
- Runtime level loading uses `Resources.Load<TextAsset>("Levels/levels")`, so the authoritative level JSON for gameplay is `Assets/Resources/Levels/levels.json`.
- `Assets/Levels/levels.json` is a duplicate/non-runtime copy unless code is changed to load it.
- Level selection is stored in `PlayerPrefs` key `SelectedLevel`; `LevelDatabase.GetSelectedLevelData()` reads that key.
- Completed progress is stored in `PlayerPrefs` key `HighestCompletedLevel`; `GameManager.ganar()` updates it.
- Level JSON schema is defined by `Assets/Scripts/LevelData.cs`: `levels[]` with `level`, `width`, `height`, `borderSize`, `time`, `pieces[]`, and `tiles[]`.

## Gameplay Wiring
- `Board.Start()` calls `LoadLevel(LevelDatabase.SelectedLevel)`, so level select must set `PlayerPrefs` via `LevelDatabase.SelectLevel(level)` before loading `Level`.
- `Board.ApplyLevelData()` converts JSON piece/tile strings into inspector/prefab references, then `SetupBoard()` creates tiles, camera, and pieces.
- Piece prefab lookup order is direct `Board` fields, then `gamePiecePrefabs`, then `Resources/PiecePrefabLibrary.asset`.
- Tile type compatibility includes legacy misspellings: `Obtacle`, `TileObtacle`, `ObtacleMovil`, and `TileObtacleMovil`.
- Board coordinates map directly to world positions `(x, y, 0)` with arrays indexed `[x, y]`.

## Assets And Generated Output
- Avoid editing generated/build output unless explicitly requested: `Library/`, `Temp/`, `Logs/`, `build/`, `PuzzleCub.apk`, and `PuzzleCub_BackUpThisFolder_ButDontShipItWithYourGame/`.
- Keep Unity `.meta` files with asset additions/removals; broken GUIDs will break scene and prefab references.
- `.gitignore` currently ignores `Packages/packages-lock.json` and generated csproj/sln files even though copies exist in the worktree; check `git status` before assuming these are intentional changes.
