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

        GridManager.instance.SetElement(townPrefab.GetComponent<CellElement>(), new(0, 0));
    }
}
