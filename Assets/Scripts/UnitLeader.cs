using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class UnitLeader : Entity
{
    public Vector2[] formationOffset;

    public Faction faction;

    [SerializeField] GameObject unitPrefab;

    public List<Unit> units = new List<Unit>();

    public TextMeshProUGUI unitNumberText;

    public UnitSpecialization unitSpecialization;

    GridManager gridManager;

    public List<Component> graphics = new();

    public void Init(Faction newFaction, Transform unitParent, int unitNumber = 5, UnitSpecialization newUnitSpe = UnitSpecialization.None)
    {
        gridManager = GridManager.instance;

        faction = newFaction;

        unitSpecialization = newUnitSpe;

        Color squadColor = UnitManager.instance.GetColorFromFaction(newFaction);

        for (int i = 0; i < unitNumber; i++)
        {
            GameObject go = Instantiate(unitPrefab, transform.position + new Vector3(formationOffset[i].x, formationOffset[i].y), Quaternion.identity, unitParent);
            Unit newUnit = go.GetComponent<Unit>();
            units.Add(newUnit);
            newUnit.Init(this, formationOffset[i], squadColor, faction == Faction.Barbarian);
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
    }

    public void SetDestination(List<Cell> path)
    {
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
        unitNumberText.text = units.Count.ToString();
        Destroy(unit.gameObject);

        if(units.Count == 0)
        {
            if(faction == Faction.Barbarian) RessourceManager.instance.AddCoin(1);
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