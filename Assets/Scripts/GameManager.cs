using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] BuildingPlacer buildingPlacer;
    [SerializeField] UnitManager unitManager;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gridManager.Init();
        buildingPlacer.Init();
        unitManager.Init();
    }
}
