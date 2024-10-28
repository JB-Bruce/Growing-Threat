using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrackBuilding : Building
{
    [SerializeField] int unitCost = 5;

    [SerializeField] GameObject barrackMenu;

    public override void Select()
    {
        barrackMenu.SetActive(true);
    }

    public override void UnSelect()
    {
        barrackMenu.SetActive(false);
    }

    public void TryCreateUnit()
    {
        foreach (Cell cell in cellOn.GetAdjacentCells(false))
        {
            if(!cell.isOccupied && cell.unitInCell == null)
            {
                if (!RessourceManager.instance.TryRemoveCoin(unitCost)) return;

                UnitManager.instance.SpawnUnits(cell, 5, Faction.Player);

                return;
            }
        }
    }
}
