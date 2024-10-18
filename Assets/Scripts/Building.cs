using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : CellElement
{
    public bool attractEnemy;
    public float life;
    public Faction faction = Faction.Player;

    Cell cellOn;


    public void SetFaction(Faction newFaction)
    {
        faction = newFaction;
    }

    public bool TakeDamage(float damage)
    {
        life -= damage;

        if (life <= 0)
        {
            BuildingManager.instance.RemoveBuilding(this);
            cellOn.SetElement(null);
            Destroy(gameObject);
            return true;
        }

        return false;
    }

    public void SetCell(Cell newCell)
    {
        cellOn = newCell;
    }
}
