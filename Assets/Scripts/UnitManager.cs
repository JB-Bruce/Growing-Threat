using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public Transform unitParent;

    public List<UnitLeader> unitLeaders = new List<UnitLeader>();

    public GameObject unitLeaderPrefab;

    int unitSpawned = 0;


    GridManager gridManager;

    [System.Serializable]
    public struct FactionColor
    {
        public Faction faction;
        public Color color;
    }

    public List<FactionColor> factionColors = new();

    (int, int) center;

    public static UnitManager instance;

    float fps;

    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        gridManager = GridManager.instance;

        center = ((int)gridManager.center.x, (int)gridManager.center.y);

        SpawnUnits(new Vector2(0, -1), 9, Faction.Player);
        SpawnUnits(new Vector2(0, -3), 5, Faction.Player);
        SpawnUnits(new Vector2(1, 0), 9, Faction.Player);

        InvokeRepeating("SpawnUnitAtBorder", 1f, 10f);
    }

    private void SpawnUnitAtBorder()
    {
        (int, int) spawnCell = gridManager.spawnableBorders[Random.Range(0, gridManager.spawnableBorders.Count)];

        var unit = SpawnUnits((spawnCell.Item1 - center.Item1, spawnCell.Item2 - center.Item2), 5, Faction.Barbarian);
        unit.SetDestination(new Vector2(0f, 0f));

        unitSpawned++;
    }


    public UnitLeader SpawnUnits(Cell cellOn, int amount, Faction faction)
    {
        GameObject go = Instantiate(unitLeaderPrefab, cellOn.transform.position, Quaternion.identity, unitParent);
        UnitLeader newUnitLeader = go.GetComponent<UnitLeader>();
        unitLeaders.Add(newUnitLeader);
        newUnitLeader.Init(faction, unitParent, amount);
        cellOn.TrySetUnit(newUnitLeader);

        return newUnitLeader;
    }
    public UnitLeader SpawnUnits((int, int) pos, int amount, Faction faction)
    {
        Cell cellOn = gridManager.GetCell(pos.Item1, pos.Item2);
        return SpawnUnits(cellOn, amount, faction);
    }

    public UnitLeader SpawnUnits(Vector2 pos, int amount, Faction faction)
    {
        Cell cellOn = GridManager.instance.GetCellFromPos(pos);
        return SpawnUnits(cellOn, amount, faction);
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
        List<Unit> entities = new();

        foreach (UnitLeader leader in unitLeaders)
        {
            if (leader.faction != baseUnit.leader.faction)
            {
                entities.AddRange(leader.units);
            }
        }
        Unit nearest = GetNearestEntityOf<Unit>(baseUnit, entities, range);

        return nearest;
    }

    public Building GetNearestBuildingOf(Unit baseUnit, Faction faction, float range = Mathf.Infinity)
    {
        Building nearest = GetNearestEntityOf<Building>(baseUnit, BuildingManager.instance.GetAllBuildings(faction), range); ;

        return nearest;
    }

    public List<Building> GetNearestBuildingsOf(Unit baseUnit, Faction faction, float range = Mathf.Infinity)
    {
        return GetOnlyEntityInRange(BuildingManager.instance.GetAllBuildings(faction, false), range, baseUnit.transform);

    }

    private List<T> GetOnlyEntityInRange<T>(List<T> l, float range, Transform origin) where T : Entity
    {
        List<T> list = new List<T>();

        foreach (var item in l)
        {
            if(Vector2.Distance(origin.position, item.transform.position) <= range)
                list.Add(item);
        }

        return list;
    }

    public void UpdatePaths(Cell updatedCell = null)
    {
        foreach(UnitLeader unitLeader in unitLeaders)
        {
            if(updatedCell == null || unitLeader.PathContainsCell(updatedCell))
                unitLeader.UpdatePathfinding();
        }
    }

    

    public T GetNearestEntityOf<T>(Unit baseUnit, List<T> entities, float range = Mathf.Infinity) where T : Entity
    {
        T nearest = null;
        float lowestDist = Mathf.Infinity;

        foreach (T entity in entities)
        {
            float newDist = Vector2.Distance(baseUnit.transform.position, entity.transform.position);
            if (newDist < range && newDist < lowestDist)
            {
                nearest = entity;
                lowestDist = newDist;
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
