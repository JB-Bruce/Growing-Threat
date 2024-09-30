using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public Transform unitParent;

    public List<UnitLeader> unitLeaders = new List<UnitLeader>();

    public GameObject unitLeaderPrefab;

    [System.Serializable]
    public struct FactionColor
    {
        public Faction faction;
        public Color color;
    }

    public List<FactionColor> factionColors = new();

    public static UnitManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        SpawnUnits(new(0, -1), 9, Faction.Player);
        SpawnUnits(new(0, -3), 5, Faction.Player);
        SpawnUnits(new(1, 0), 9, Faction.Player);
        SpawnUnits(new(-1, 3), 9, Faction.Barbarian);
        SpawnUnits(new(1, 3), 9, Faction.Barbarian);
    }

    public void SpawnUnits(Vector2 pos, int amount, Faction faction)
    {
        Cell cellOn = GridManager.instance.GetCellFromPos(pos);

        GameObject go = Instantiate(unitLeaderPrefab, cellOn.transform.position, Quaternion.identity, unitParent);
        UnitLeader newUnitLeader = go.GetComponent<UnitLeader>();
        unitLeaders.Add(newUnitLeader);
        newUnitLeader.Init(faction, unitParent, amount);
        cellOn.TrySetUnit(newUnitLeader);
    }

    public List<Unit> GetEnemiesOf(Unit baseUnit, float range = Mathf.Infinity)
    {
        List<Unit> list = new List<Unit>();

        foreach (UnitLeader leader in unitLeaders)
        {
            if(leader.faction != baseUnit.leader.faction)
            {
                foreach (Unit unit in leader.units)
                {
                    if(Vector2.Distance(baseUnit.transform.position, unit.transform.position) < range)
                        list.Add(unit);
                }
            }
        }

        return list;
    }

    public Unit GetNearestEnemyOf(Unit baseUnit, float range = Mathf.Infinity)
    {
        Unit nearest = null;
        float lowestDist = Mathf.Infinity;

        foreach (UnitLeader leader in unitLeaders)
        {
            if (leader.faction != baseUnit.leader.faction)
            {
                foreach (Unit unit in leader.units)
                {
                    float newDist = Vector2.Distance(baseUnit.transform.position, unit.transform.position);
                    if (newDist < range && newDist < lowestDist)
                    {
                        nearest = unit;
                        lowestDist = newDist;
                    }
                        
                }
            }
        }

        return nearest;
    }

    public void LooseLeader(UnitLeader unitLeader)
    {
        unitLeaders.Remove(unitLeader);
        Destroy(unitLeader.gameObject);
    }

    public Color GetColorFromFaction(Faction faction)
    {
        foreach (FactionColor fc in factionColors)
        {
            if(fc.faction == faction)
                return fc.color;
        }

        return Color.white;
    }
}

public enum Faction
{
    Player,
    Barbarian,
    Neutral
}
