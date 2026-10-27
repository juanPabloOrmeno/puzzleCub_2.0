using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelCollection
{
    public List<LevelData> levels = new List<LevelData>();
}

[Serializable]
public class LevelData
{
    public int level;
    public int width;
    public int height;
    public float borderSize;
    public int time;
    public List<LevelPieceData> pieces = new List<LevelPieceData>();
    public List<LevelTileData> tiles = new List<LevelTileData>();
}

[Serializable]
public class LevelPieceData
{
    public string type;
    public int x;
    public int y;
}

[Serializable]
public class LevelTileData
{
    public string type;
    public int x;
    public int y;
}

public static class LevelDatabase
{
    public const string SelectedLevelKey = "SelectedLevel";
    public const string HighestCompletedLevelKey = "HighestCompletedLevel";
    public const string ResourcePath = "Levels/levels";

    private static LevelCollection s_collection;
#if UNITY_EDITOR
    private static string s_sourceJson;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        s_collection = null;
#if UNITY_EDITOR
        s_sourceJson = null;
#endif
    }

    public static int SelectedLevel
    {
        get { return PlayerPrefs.GetInt(SelectedLevelKey, 1); }
    }

    public static int HighestCompletedLevel
    {
        get { return PlayerPrefs.GetInt(HighestCompletedLevelKey, 0); }
    }

    public static void SelectLevel(int level)
    {
        PlayerPrefs.SetInt(SelectedLevelKey, level);
        PlayerPrefs.Save();
    }

    public static void CompleteLevel(int level)
    {
        if (level <= HighestCompletedLevel)
        {
            return;
        }

        PlayerPrefs.SetInt(HighestCompletedLevelKey, level);
        PlayerPrefs.Save();
    }

    public static void ResetProgressIfOutOfRange()
    {
        int highestLevel = GetHighestLevelNumber();
        if (highestLevel <= 0 || HighestCompletedLevel <= highestLevel)
        {
            return;
        }

        PlayerPrefs.SetInt(HighestCompletedLevelKey, 0);
        PlayerPrefs.SetInt(SelectedLevelKey, GetFirstLevelNumber());
        PlayerPrefs.Save();
    }

    public static LevelData GetSelectedLevelData()
    {
        return GetLevelData(SelectedLevel);
    }

    public static LevelData GetLevelData(int level)
    {
        LevelCollection collection = LoadCollection();
        if (collection == null || collection.levels == null)
        {
            return null;
        }

        foreach (LevelData levelData in collection.levels)
        {
            if (levelData != null && levelData.level == level)
            {
                return levelData;
            }
        }

        return null;
    }

    public static int GetNextLevel(int currentLevel)
    {
        LevelCollection collection = LoadCollection();
        if (collection == null || collection.levels == null)
        {
            return 0;
        }

        int nextLevel = int.MaxValue;
        foreach (LevelData levelData in collection.levels)
        {
            if (levelData != null && levelData.level > currentLevel && levelData.level < nextLevel)
            {
                nextLevel = levelData.level;
            }
        }

        return nextLevel == int.MaxValue ? 0 : nextLevel;
    }

    public static int GetFirstLevelNumber()
    {
        return GetBoundaryLevelNumber(int.MaxValue, true);
    }

    public static int GetHighestLevelNumber()
    {
        return GetBoundaryLevelNumber(int.MinValue, false);
    }

    private static int GetBoundaryLevelNumber(int initialValue, bool findLowest)
    {
        LevelCollection collection = LoadCollection();
        if (collection == null || collection.levels == null)
        {
            return 0;
        }

        int boundaryLevel = initialValue;
        foreach (LevelData levelData in collection.levels)
        {
            if (levelData == null)
            {
                continue;
            }

            if ((findLowest && levelData.level < boundaryLevel) || (!findLowest && levelData.level > boundaryLevel))
            {
                boundaryLevel = levelData.level;
            }
        }

        return boundaryLevel == initialValue ? 0 : boundaryLevel;
    }

    private static LevelCollection LoadCollection()
    {
#if !UNITY_EDITOR
        if (s_collection != null)
        {
            return s_collection;
        }
#endif

        TextAsset json = Resources.Load<TextAsset>(ResourcePath);
        if (json == null)
        {
            Debug.LogWarning("No se encontró el archivo de niveles en Resources/" + ResourcePath);
            return null;
        }

        string source = json.text;
#if UNITY_EDITOR
        // Reimporting the JSON must invalidate editor data even with domain reload disabled.
        if (s_collection != null && s_sourceJson == source) return s_collection;
#endif
        // JsonUtility necesita una clase contenedora para deserializar listas desde JSON.
        s_collection = JsonUtility.FromJson<LevelCollection>(source);
#if UNITY_EDITOR
        s_sourceJson = source;
#endif
        return s_collection;
    }
}
