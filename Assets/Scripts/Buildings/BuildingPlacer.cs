using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    public static BuildingPlacer instance;

    public GameObject townPrefab;

    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        GameObject town = Instantiate(townPrefab);

        Building townBuilding = town.GetComponentInChildren<Building>();

        townBuilding.SetCell(GridManager.instance.SetElement(town.GetComponentInChildren<CellElement>(), new(0, 0)));
        BuildingManager.instance.AddBuilding(townBuilding);

    }
}
