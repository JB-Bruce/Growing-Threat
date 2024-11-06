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

    public BuildingUIDescription buildingUIDescription;

    GridManager gridManager;

    RessourceManager ressourceManager;

    BuildingButton overedBuildingUI;
    bool overingBuildingUI;

    public bool isBuildingSelected { get { return instantiatedBuilding != null; } private set { } }

    public static BuildingManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gridManager = GridManager.instance;
        ressourceManager = RessourceManager.instance;

        buildingUIDescription.Active(false);

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
        bool foundBuildingUI = false;

        if (IsPointerOverUI())
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
            };

            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                if (results[0].gameObject.tag == "UIBuilding")
                {
                    overedBuildingUI = results[0].gameObject.GetComponent<BuildingButton>();
                    if(!overedBuildingUI)
                        overedBuildingUI = results[0].gameObject.GetComponentInParent<BuildingButton>();
                    BuildingElements bElements = overedBuildingUI.building;
                    buildingUIDescription.Init(bElements.name, bElements.description, bElements.cost);
                    overingBuildingUI = true;
                    foundBuildingUI = true;
                    buildingUIDescription.Active(true);
                    buildingUIDescription.SetPosition(Input.mousePosition.x);
                }
            }
        }

        if (overingBuildingUI && !foundBuildingUI)
        {
            overingBuildingUI = false;
            buildingUIDescription.Active(false);
        }


        if (instantiatedBuilding == null)
            return;


        Vector2 cellUnderMouse = gridManager.GetCellPositionFromWorldPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Cell cellOn = gridManager.GetCellFromPos(cellUnderMouse);

        bool possible = !cellOn.isOccupied && GetPossibility(selectedBuilding, cellOn);


        instantiatedBuilding.transform.position = cellUnderMouse;
        instantiatedBuilding.GetComponent<BuildingInit>().SetPossibility(possible);


        if (Input.GetMouseButtonDown(0) && possible && !IsPointerOverUI())
        {
            if (!ressourceManager.TryRemoveCoin(selectedBuilding.cost)) return;

            instantiatedBuilding.GetComponent<BuildingInit>().Place();
            Building newBuilding = instantiatedBuilding.GetComponentInChildren<Building>();
            AddBuilding(newBuilding);


            cellOn.SetElement(newBuilding);
            newBuilding.SetCell(cellOn);

            SelectBuilding(selectedBuilding, false);
            UnitManager.instance.UpdatePaths(cellOn);
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(instantiatedBuilding);
            instantiatedBuilding = null;
        }

    }

    private bool GetPossibility(BuildingElements building, Cell cellOn)
    {
        foreach (var item in building.buildingsProximityRequirements)
        {
            int amount = GetBuildingsInRange(item.building, item.distance, cellOn);
            if (item.reverse ? amount >= item.amount : amount < item.amount)
                return false;
        }

        foreach (var item in building.terrainsProximityRequirements)
        {
            int amount = GetTerrainInRange(item.block, item.distance, cellOn);
            if (item.reverse ? amount >= item.amount : amount < item.amount)
                return false;
        }

        return true;
    }

    private int GetBuildingsInRange(string building, float range, Cell cellOn)
    {
        return GetConditionInRange((Cell testCell) => { return testCell.TryGetBuilding(out Building b) && b.elementName == building; }, range, cellOn);
    }

    private int GetTerrainInRange(BlockType type, float range, Cell cellOn)
    {
        return GetConditionInRange((Cell testCell) => { return testCell.selectedBlock.type == type; }, range, cellOn);
    }

    private int GetConditionInRange(Func<Cell, bool> action, float range, Cell cellOn)
    {
        int amount = 0;

        HashSet<Cell> testedCells = new();

        List<Cell> cellsToTest = new() { cellOn };

        while (cellsToTest.Count > 0)
        {
            Cell testCell = cellsToTest[0];
            cellsToTest.RemoveAt(0);

            testedCells.Add(testCell);

            if (action(testCell))
                amount++;

            foreach (var item in testCell.GetAdjacentCells(true))
            {
                if (Vector2.Distance(item.transform.position, cellOn.transform.position) <= range && !testedCells.Contains(item))
                {
                    cellsToTest.Add(item);
                }
            }
        }

        return amount;
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

    public void SelectBuilding(BuildingElements building, bool destroyPrevious = true)
    {
        if (instantiatedBuilding != null && destroyPrevious)
        {
            Destroy(instantiatedBuilding);
        }

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
    public string name;
    public GameObject prefab;
    public GameObject preview;
    public Sprite sprite;
    [TextArea(2, 10)]
    public string description;
    public int cost;
    public List<BuildingProximityRequirement> buildingsProximityRequirements;
    public List<TerrainProximityRequirement> terrainsProximityRequirements;
}

[Serializable]
public struct BuildingProximityRequirement
{
    public string building;
    public float distance;
    public int amount;
    public bool reverse;
}

[Serializable]
public struct TerrainProximityRequirement
{
    public BlockType block;
    public float distance;
    public int amount;
    public bool reverse;
}