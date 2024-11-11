using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarrackBuilding : Building
{
    [SerializeField] int unitCost = 5;

    [SerializeField] GameObject barrackMenu;

    bool createdUnit = false;

    public float timeDelayBetweenSpawn;
    float timer = 0f;

    [SerializeField] Image image;

    new protected void Start()
    {
        base.Start();

        image.fillAmount = 0f;
        image.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(!createdUnit) return;

        timer += Time.deltaTime;

        if(timer  > timeDelayBetweenSpawn)
        {
            createdUnit = false;
            timer = 0f;
            image.fillAmount = 0f;
            image.gameObject.SetActive(false);
            return;
        }

        image.fillAmount = 1f - (timer / timeDelayBetweenSpawn);
    }

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
        if (createdUnit) return;

        foreach (Cell cell in cellOn.GetAdjacentCells(false))
        {
            if(!cell.isOccupied && cell.unitInCell == null)
            {
                if (!RessourceManager.instance.TryRemoveCoin(unitCost)) return;

                UnitManager.instance.SpawnUnits(cell, 5, Faction.Player);
                createdUnit = true;
                image.gameObject.SetActive(true);

                return;
            }
        }
    }
}
