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

    private static LevelCollection LoadCollection()
    {
        if (s_collection != null)
        {
            return s_collection;
        }

        TextAsset json = Resources.Load<TextAsset>(ResourcePath);
        if (json == null)
        {
            Debug.LogWarning("No se encontró el archivo de niveles en Resources/" + ResourcePath);
            return null;
        }

        // JsonUtility necesita una clase contenedora para deserializar listas desde JSON.
        s_collection = JsonUtility.FromJson<LevelCollection>(json.text);
        return s_collection;
    }
}
