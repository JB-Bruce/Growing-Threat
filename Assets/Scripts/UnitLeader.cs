using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitLeader : Entity
{
    public Vector2[] formationOffset;

    public Faction faction;

    [SerializeField] GameObject unitPrefab;

    public List<Unit> units = new List<Unit>();

    public TextMeshProUGUI unitNumberText;

    public UnitSpecialization unitSpecialization;

    public GridManager gridManager;

    public List<Component> graphics = new();


    public List<Building> authorizedNonAttractableBuildingsToDestroy { get; private set; } = new();

    public List<Unit> Init(Faction newFaction, Transform unitParent, int unitNumber = 5, UnitSpecialization newUnitSpe = UnitSpecialization.None)
    {
        gridManager = GridManager.instance;

        faction = newFaction;

        unitSpecialization = newUnitSpe;

        Sprite squadSprite = UnitManager.instance.GetSpriteFromFaction(newFaction);

        for (int i = 0; i < unitNumber; i++)
        {
            GameObject go = Instantiate(unitPrefab, transform.position + new Vector3(formationOffset[i].x, formationOffset[i].y), Quaternion.identity, unitParent);
            Unit newUnit = go.GetComponent<Unit>();
            go.name = "Unit - " + i;
            units.Add(newUnit);
            newUnit.Init(this, formationOffset[i], squadSprite, faction == Faction.Barbarian);
        }

        unitNumberText.text = unitNumber.ToString();

        if(faction != Faction.Player)
        {
            foreach (var item in graphics)
            {
                if(item is Renderer) ((Renderer)item).enabled = false;
                else if(item is Behaviour) ((Behaviour)item).enabled = false;
            }
        }

        return units;
    }

    public void Merge(ref UnitLeader newLeader)
    {
        if (unitSpecialization != newLeader.unitSpecialization)
            return;

        for (int i = units.Count; i < 9; i++)
        {
            if (newLeader.units.Count <= 0)
                break;


            if(newLeader.TryRemoveUnit(out Unit unit))
            {
                units.Add(unit);
                unit.leader = this;
            }
        }

        ActualiseUnits();
    }

    public bool TryRemoveUnit(out Unit unit)
    {
        unit = null;
        if(units.Count > 0)
        {
            unit = units[0];
            units.RemoveAt(0);

            ActualiseUnits();

            return true;
        }

        return false;
    }

    public void SetDestination(List<Cell> path)
    {
        authorizedNonAttractableBuildingsToDestroy.Clear();
        foreach (var cell in path)
        {
            if (cell.TryGetBuilding(out Building building)) authorizedNonAttractableBuildingsToDestroy.Add(building);
        }

        transform.position = path[path.Count -1].transform.position;

        foreach (Unit unit in units)
        {
            unit.AddPath(path);
        }
    }

    public void SetDestination((int, int) pos)
    {
        SetDestination(gridManager.FindPath(gridManager.GetCellFromPos(transform.position), gridManager.GetCell(pos.Item1, pos.Item2), out bool finished, faction));
    }

    public void SetDestination(Vector2 pos)
    {
        SetDestination(gridManager.FindPath(gridManager.GetCellFromPos(transform.position), gridManager.GetCellFromPos(pos), out bool finished, faction));
    }

    public void UpdatePathfinding()
    {
        List<Cell> newPath = gridManager.FindPath(gridManager.GetCellFromPos(units[0].transform.position), gridManager.GetCellFromPos(transform.position), out bool finished, faction);

        authorizedNonAttractableBuildingsToDestroy.Clear();
        foreach (var cell in newPath)
        {
            if (cell.TryGetBuilding(out Building building)) authorizedNonAttractableBuildingsToDestroy.Add(building);
        }

        foreach (Unit unit in units)
        {
            unit.ClearPath();
            unit.AddPath(newPath);
        }
    }

    public bool PathContainsCell(Cell cell)
    {
        return units[0].path.Contains(cell);
    }

    public void LooseUnit(Unit unit)
    {
        units.Remove(unit);
        Destroy(unit.gameObject);
        
        if(faction == Faction.Barbarian) RessourceManager.instance.AddCoin(1);

        ActualiseUnits();
    }

    private void ActualiseUnits()
    {
        unitNumberText.text = units.Count.ToString();

        if (units.Count == 0)
        {
            UnitManager.instance.LooseLeader(this);
        }

        for (int i = 0; i < units.Count; i++)
        {
            units[i].ChangeOffset(formationOffset[i]);
        }
    }
}


[System.Serializable]
public enum UnitSpecialization
{
    None,
    Guardian,
    Ranger
}