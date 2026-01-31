using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellType
{
    // Fire Spells
    Fireball,
    Firebolt,
    FirePillar,
    FireElemental,

    // Water Spells
    Waterball,
    WaterWhip,
    WaterBubbles,
    WaterElemental,

    // Wind Spells
    Windblast,
    WindTrap,
    WindTornado,
    WindElemental,

    //Lightning Spells
    LightningBolt,
    LightningCloud,
    LightningBall,
    LightningElemental
}


public class SpawnSpellOnFrame : MonoBehaviour
{
    
    public SpellDatabase spellPrefabs;
    public Transform spawnPoint;

    private SpellType currentSpell;
    private bool isCasting;

    public void CastSpell(SpellType spell)
    {
        /*GameObject spellPrefab = spellPrefabs.GetPrefab(spell);
        if(spellPrefab != null)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(spellPrefab, spawnPos, Quaternion.identity);
        }*/

        /*if (isCasting)
            return;
        */
        currentSpell = spell;
        isCasting = true;
    }

    public void SpawnSpell()
    {
        GameObject spellPrefab = spellPrefabs.GetPrefab(currentSpell);
        if(spellPrefab != null)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(spellPrefab, spawnPos, Quaternion.identity);
            Debug.Log($"Spawned {currentSpell}!");
        }
    }

    public void EndCast()
    {
        isCasting = false;
    }
}
