using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitLeader : MonoBehaviour
{
    public Vector2[] formationOffset;

    public Faction faction;

    [SerializeField] GameObject unitPrefab;

    public List<Unit> units = new List<Unit>();

    public TextMeshProUGUI unitNumberText;

    public UnitSpecialization unitSpecialization;

    public void Init(Faction newFaction, Transform unitParent, int unitNumber = 5, UnitSpecialization newUnitSpe = UnitSpecialization.None)
    {
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
    }

    public void SetDestination(List<Cell> path)
    {
        transform.position = path[path.Count -1].transform.position;

        foreach (Unit unit in units)
        {
            unit.AddPath(path);
        }
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