using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    List<Building> buildings = new();

    public Transform farmParent;
    public Transform defensiveParent;

    BuildingElements selectedBuilding;
    GameObject instantiatedBuilding = null;

    GridManager gridManager;

    public bool isBuildingSelected { get { return instantiatedBuilding != null; } private set { } }

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

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void Update()
    {
        Vector2 cellUnderMouse = gridManager.GetCellPositionFromWorldPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Cell cellOn = gridManager.GetCellFromPos(cellUnderMouse);

        bool possible = !cellOn.isOccupied;

        if (instantiatedBuilding != null)
        {
            instantiatedBuilding.transform.position = cellUnderMouse;
            instantiatedBuilding.GetComponent<BuildingInit>().SetPossibility(possible);
        }

        if (instantiatedBuilding != null)
        {
            if(Input.GetMouseButtonDown(0) && possible && !IsPointerOverUI())
            {
                print("ff");
                instantiatedBuilding.GetComponent<BuildingInit>().Place();
                Building newBuilding = instantiatedBuilding.GetComponentInChildren<Building>();
                AddBuilding(newBuilding);

                cellOn.SetElement(newBuilding);
                newBuilding.SetCell(cellOn);

                SelectBuilding(selectedBuilding);
                UnitManager.instance.UpdatePaths(cellOn);
            }
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(instantiatedBuilding);
                instantiatedBuilding = null;
            }
        }
    }

    public void AddBuilding(Building building)
    {
        buildings.Add(building);
    }

    public void RemoveBuilding(Building building)
    {
        buildings.Remove(building);
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

    public List<Building> GetAllBuildings(Faction faction, bool selectOnlyAttractable = true)
    {
        List<Building> newBuildings = new List<Building>();

        foreach (var go in buildings)
        {
            if(go.faction == faction)
                if (selectOnlyAttractable && !go.attractEnemy) continue;
                newBuildings.Add(go);
        }

        return newBuildings;
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