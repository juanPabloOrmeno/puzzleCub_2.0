using System;
using UnityEngine;

public class PiecePrefabLibrary : ScriptableObject
{
    public GameObject[] prefabs;

    public GameObject GetPrefab(string pieceType)
    {
        if (string.IsNullOrEmpty(pieceType) || prefabs == null)
        {
            return null;
        }

        foreach (GameObject prefab in prefabs)
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

        return null;
    }

    private bool IsSameType(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
