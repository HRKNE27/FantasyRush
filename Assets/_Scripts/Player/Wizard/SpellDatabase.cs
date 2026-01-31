using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SpellEntry
{
    public SpellType type;
    public GameObject prefab;
}

[CreateAssetMenu(menuName = "Spell Database")]
public class SpellDatabase : ScriptableObject
{
    public List<SpellEntry> spells = new List<SpellEntry>();

    private Dictionary<SpellType, GameObject> lookup;

    private void OnEnable()
    {
        lookup = new Dictionary<SpellType, GameObject>();
        foreach (var entry in spells)
        {
            lookup[entry.type] = entry.prefab;
        }
    }

    public GameObject GetPrefab(SpellType type)
    {
        if (lookup.TryGetValue(type, out var prefab))
            return prefab;

        Debug.LogWarning($"No prefab assigned for spell type: {type}");
        return null;
    }
}
