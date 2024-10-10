using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public Animator buildUIAnimator;

    bool buildingUIOpened = true;

    public Transform buildingsParent;
    public GameObject buildingInitPrefab;

    public GameObject BuildingBtnPrefab;

    public List<BuildingElements> farmBuildings = new();
    List<GameObject> farmGOs = new();
    public List<BuildingElements> defensiveBuildings = new(); 
    List<GameObject> defensiveGOs = new();

    public Transform farmParent;
    public Transform defensiveParent;

    BuildingElements selectedBuilding;
    GameObject instantiatedBuilding = null;

    GridManager gridManager;

    public static BuildingManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gridManager = GridManager.instance;

        foreach (var building in farmBuildings)
        {
            farmGOs.Add(CreateBuildingButton(building, farmParent));
        }

        foreach (var def in defensiveBuildings)
        {
            defensiveGOs.Add(CreateBuildingButton(def, defensiveParent));
        }

        ClickFarm();
    }

    private void Update()
    {
        if(instantiatedBuilding != null)
        {
            instantiatedBuilding.transform.position = gridManager.GetCellPositionFromWorldPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (instantiatedBuilding != null)
        {
            if(Input.GetMouseButtonDown(0))
            {
                instantiatedBuilding.GetComponent<BuildingInit>().Place();
                SelectBuilding(selectedBuilding);
            }
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                
            }
        }
    }

    private GameObject CreateBuildingButton(BuildingElements building, Transform buildingParent)
    {
        var go = Instantiate(BuildingBtnPrefab, buildingParent);
        go.GetComponent<BuildingButton>().Init(building);

        return go;
    }


    public void OnBuildBtnClicked()
    {
        buildingUIOpened = !buildingUIOpened;
        buildUIAnimator.Play(buildingUIOpened ? "Close" : "Open");
    }

    public void ClickDefensive()
    {
        foreach (var go in farmGOs)
        {
            go.SetActive(false);
        }

        foreach (var go in defensiveGOs)
        {
            go.SetActive(true);
        }
    }

    public void ClickFarm()
    {
        foreach (var go in defensiveGOs)
        {
            go.SetActive(false);
        }

        foreach (var go in farmGOs)
        {
            go.SetActive(true);
        }
    }

    public void SelectBuilding(BuildingElements building)
    {
        selectedBuilding = building;
        instantiatedBuilding = Instantiate(buildingInitPrefab, buildingsParent);
        instantiatedBuilding.GetComponent<BuildingInit>().Init(building, Vector2.zero);
    }
}


[Serializable]
public struct BuildingElements
{
    public GameObject prefab;
    public GameObject preview;
    public Sprite sprite;
    public string name;
    public int cost;
}