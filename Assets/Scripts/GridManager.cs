using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public bool changeMap = false;

    [SerializeField] int gridSize;
    public float cellSize;

    [SerializeField] GameObject cellPrefab;
    [SerializeField] Transform gridParent;

    List<Cell> cells = new List<Cell>();

    Cell selectedCell = null;

    [SerializeField] Camera cam;

    Vector2 gridStartPos;

    public float grassSizeAroundHub;
    public float stoneSeuil;
    public float noiseScale;

    [Header("River Settings")]

    public float RiverNoiseScale;
    public float riverWidth;
    public float grassAroundRiver;
    public float riverStrength;

    float offsetX;
    float offsetY;

    public Vector2 center;

    public List<(int, int)> spawnableBorders = new();

    BuildingManager buildingManager;

    [SerializeField] Transform selectionSquare;

    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        buildingManager = BuildingManager.instance;

        float totalLength = (gridSize - 1) * cellSize;
        gridStartPos = -Vector2.one * (totalLength / 2f);

        center = new(gridStartPos.x + totalLength / 2f, gridStartPos.y + totalLength / 2f);

        bool passed = false;
        int iteration = 0;

        offsetX = 0;
        offsetY = 0;

        do
        {
            spawnableBorders.Clear();
            offsetX = Random.Range(0f, 1000f);
            offsetY = Random.Range(0f, 1000f);

            if (iteration == 10) offsetX = 0; offsetY = 0;

            float[,] testGrid = new float[gridSize, gridSize];

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    testGrid[i, j] = GetHeight(i, j);
                }
            }

            (int, int) centerGrid = ((int)(gridSize/2), (int)(gridSize/2));
            
            List<(int, int)> accessibles = new();
            
            for (int i = 0; i < gridSize; i++)
            {
                if (CanBorderAccessCenter(testGrid, (i, 0), centerGrid, riverWidth, stoneSeuil)) accessibles.Add((i, 0));
                if (CanBorderAccessCenter(testGrid, (i, gridSize - 1), centerGrid, riverWidth, stoneSeuil)) accessibles.Add((i, gridSize - 1));
                if (CanBorderAccessCenter(testGrid, (0, i), centerGrid, riverWidth, stoneSeuil)) accessibles.Add((0, i));
                if (CanBorderAccessCenter(testGrid, (gridSize - 1, i), centerGrid, riverWidth, stoneSeuil)) accessibles.Add((gridSize - 1, i));
            }

            if(accessibles.Count > 0)
            {
                spawnableBorders = accessibles;
                passed = true;
            }

            iteration++;
        } while (!passed && iteration <= 10);




        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Vector2 newPos = gridStartPos + new Vector2(j * cellSize, i * cellSize);
                GameObject go = Instantiate(cellPrefab, newPos, Quaternion.identity, gridParent);
                go.name = "cell(" + i + "," + j + ")";
                var newCell = go.GetComponent<Cell>();
                newCell.Init(this, j, i);
                cells.Add(newCell);
            }
        }

        foreach (Cell cell in cells)
        {
            cell.InitAfter();

            float height = GetHeight(cell);

            Vector2 tmp = new Vector2(cell.transform.position.x, cell.transform.position.y);

            if (Vector2.Distance(tmp, center) > grassSizeAroundHub * cellSize)
            {
                if(height > stoneSeuil)
                {
                    cell.SetBlock(BlockType.Stone);
                    continue;
                }
                else if (height < riverWidth)
                {
                    cell.SetBlock(BlockType.Water);
                    continue;
                }
            }

            cell.SetBlock(BlockType.Grass);
            
        }

        foreach((int, int) pos in spawnableBorders)
        {
            //GetCell(pos.Item1, pos.Item2).Over();
        }

    }


    private float GetHeight(Cell cell)
    {
        return GetHeight(cell.x, cell.y);
    }

    private float GetHeight(int x, int y)
    {
        float cordX = (float)x * noiseScale + offsetX;
        float cordY = (float)y * noiseScale + offsetY;

        float height = Mathf.PerlinNoise(cordX, cordY);

        cordX = (float)x * RiverNoiseScale + offsetX + 69f;
        cordY = (float)y * RiverNoiseScale + offsetY + 69f;

        float height2 = Mathf.Abs(Mathf.PerlinNoise(cordX, cordY) - 0.5f) * 2f * riverStrength;

        return (height + height2) / (2f * riverStrength);
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void Update()
    {
        if (changeMap)
        {
            changeMap = false;
            ResetMap();
        }

        bool rMouse = Input.GetMouseButtonDown(1);
        bool lMouse = Input.GetMouseButtonDown(0);

        bool overUI = IsPointerOverUI();

        if (lMouse && !overUI)
        {
            HandleLeftMouse();
        }

        if (rMouse && !overUI)
        {
            HandleRightMouse();
        }

        /*if (selectedCell != null)
        {
            List<Cell> cells2 = FindPath(GetCell(4, 4), selectedCell, out bool completedPath);

            foreach (Cell n in cells)
            {
                n.UnOver();
                
            }

            foreach (Cell n in cells2)
            {
                n.Over();
            }
        }*/
 
    }

    private void ResetMap()
    {
        foreach (var item in cells)
        {
            Destroy(item.transform.gameObject);
        }

        cells.Clear();

        Init();
    }

    public Cell GetCell(int x, int y)
    {
        Cell returnCell = null;

        if(x >= 0 && x < gridSize && y >= 0 && y < gridSize) returnCell = cells[x + y * gridSize];

        return returnCell;
    }

    private void HandleLeftMouse()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (IsPosOverACell(mousePos) && !buildingManager.isBuildingSelected)
        {
            Cell overedCell = GetCellFromPos(mousePos);
            foreach (Cell n in cells) { n.UnOver(); }
            selectedCell = overedCell;
            selectedCell.Over();
            selectionSquare.position = selectedCell.transform.position;
            selectionSquare.gameObject.SetActive(true);
        }
        else
        {
            if (selectedCell != null)
            {
                selectionSquare.gameObject.SetActive(false);
                selectedCell.UnOver(); 
                selectedCell = null;
            }
        }
    }

    private void HandleRightMouse()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (IsPosOverACell(mousePos))
        {
            Cell overedCell = GetCellFromPos(mousePos);
            if (selectedCell != null && selectedCell.unitInCell != null)
            {
                List<Cell> path = FindPath(selectedCell, overedCell, out bool completedPath, selectedCell.unitInCell.faction);

                foreach (Cell n in cells) { n.UnOver(); }

                if (overedCell.unitInCell != null)
                {
                    overedCell.unitInCell.Merge(ref selectedCell.unitInCell);

                    overedCell.unitInCell.SetDestination(path);

                    selectionSquare.position = overedCell.transform.position;
                }

                if (selectedCell.TrySetDestination(path))
                {
                    foreach (Cell n in path)
                    {
                        n.Over();
                    }

                    selectionSquare.position = overedCell.transform.position;
                }


                selectedCell = overedCell.unitInCell != null ? overedCell : selectedCell;


            }
        }
    }

    private bool IsPosOverACell(Vector2 pos)
    {
        bool isIn = true;

        float gridLength = gridSize * cellSize;
        Vector2 realStart = gridStartPos + new Vector2(-.5f, -.5f);

        if (pos.x < realStart.x || pos.x > realStart.x + gridLength) isIn = false;
        if (pos.y < realStart.y || pos.y > realStart.y + gridLength) isIn = false; 

        return isIn;
    }

    public Cell SetElement(CellElement newElement, Vector2 pos)
    {
        Cell cell = GetCellFromPos(pos);

        cell.SetElement(newElement);
        newElement.transform.position = cell.transform.position;

        return cell;
    }

    public Vector2 GetCellPositionFromWorldPos(Vector2 pos)
    {
        Vector2Int indexs = GetCellIndexFromPosition(pos);

        Vector2 res = gridStartPos + new Vector2(indexs.x * cellSize, indexs.y * cellSize);

        return res;
    }

    public Cell GetCellFromPos(Vector2 pos)
    {
        Vector2Int indexs = GetCellIndexFromPosition(pos);
        indexs.y *= gridSize;

        Cell nearestCell = cells[indexs.x + indexs.y];

        return nearestCell;
    }

    private Vector2Int GetCellIndexFromPosition(Vector2 pos)
    {
        Vector2 newPos = pos - gridStartPos;

        int indexX = Mathf.Clamp(Mathf.RoundToInt(newPos.x / cellSize), 0, gridSize - 1);
        int indexY = Mathf.Clamp(Mathf.RoundToInt(newPos.y / cellSize), 0, gridSize - 1);

        return new(indexX, indexY);
    }

    public List<Cell> FindPath(Cell startCell, Cell targetCell, out bool completedPath, Faction faction)
    {

        List<Cell> openSet = new List<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();
        openSet.Add(startCell);

        startCell.hCost = GetDistance(startCell, targetCell);

        Cell closestCell = startCell;

        while (openSet.Count > 0)
        {
            Cell currentCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentCell.fCost || openSet[i].fCost == currentCell.fCost && openSet[i].hCost < currentCell.hCost)
                {
                    currentCell = openSet[i];
                }
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            if (currentCell.hCost < closestCell.hCost)
            {
                closestCell = currentCell;
            }

            if (currentCell == targetCell)
            {
                completedPath = true;
                return RetracePath(startCell, targetCell);
            }

            foreach (Cell neighbor in GetNeighbors(currentCell, faction))
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                float newMovementCostToNeighbor = currentCell.gCost + GetDistance(currentCell, neighbor) + neighbor.traversalCost;
                //if (neighbor.traversalCost > 0f) print(neighbor.traversalCost);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetCell);
                    neighbor.parentCell = currentCell;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        completedPath = false;
        return RetracePath(startCell, closestCell);
    }

    public bool CanBorderAccessCenter(float[,] testGrid, (int, int) start, (int, int) center, float min, float max)
    {
        int rows = testGrid.GetLength(0);
        int cols = testGrid.GetLength(1);

        bool IsValid(int x, int y) =>
            x >= 0 && x < rows && y >= 0 && y < cols &&
            testGrid[x, y] > min && testGrid[x, y] < max;

        if (!IsValid(start.Item1, start.Item2)) return false;

        (int, int)[] directions = new (int, int)[]
        {
        (-1, 0), // haut
        (1, 0),  // bas
        (0, -1), // gauche
        (0, 1)   // droite
        };

        Queue<(int, int)> openSet = new Queue<(int, int)>();
        HashSet<(int, int)> closedSet = new HashSet<(int, int)>();

        openSet.Enqueue(start);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == center)
            {
                return true;
            }

            closedSet.Add(current);

            foreach (var direction in directions)
            {
                int newX = current.Item1 + direction.Item1;
                int newY = current.Item2 + direction.Item2;
                var neighbor = (newX, newY);

                if (IsValid(newX, newY) && !closedSet.Contains(neighbor))
                {
                    openSet.Enqueue(neighbor);
                    closedSet.Add(neighbor);
                }
            }
        }

        return false;
    }

    int GetDistance(Cell nodeA, Cell nodeB)
    {
        int dstX = Mathf.Abs(nodeA.x - nodeB.x);
        int dstY = Mathf.Abs(nodeA.y - nodeB.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    List<Cell> GetNeighbors(Cell cell, Faction faction, bool takeDiags = true)
    {
        List<Cell> neighbors = new List<Cell>();

        bool top = false, bot = false, right = false, left = false;

        if(cell.topCell != null && cell.topCell.IsWalkableOrBreakable(faction, out bool breakableT))
        {
            neighbors.Add(cell.topCell);
            top = !breakableT;
        }
        if(cell.bottomCell != null && cell.bottomCell.IsWalkableOrBreakable(faction, out bool breakableB))
        {
            neighbors.Add(cell.bottomCell);
            bot = !breakableB;
        }
        if(cell.leftCell != null && cell.leftCell.IsWalkableOrBreakable(faction, out bool breakableL))
        {
            neighbors.Add(cell.leftCell);
            left = !breakableL;
        }
        if(cell.rightCell != null && cell.rightCell.IsWalkableOrBreakable(faction, out bool breakableR))
        {
            neighbors.Add(cell.rightCell);
            right = !breakableR;
        }

        if(!takeDiags) return neighbors;

        if(top && right && cell.topRightCell != null && cell.topRightCell.IsWalkable(faction))
        {
            neighbors.Add(cell.topRightCell);
        }
        if(top && left && cell.topLeftCell != null && cell.topLeftCell.IsWalkable(faction))
        {
            neighbors.Add(cell.topLeftCell);
        }
        if(bot && right && cell.bottomRightCell != null && cell.bottomRightCell.IsWalkable(faction))
        {
            neighbors.Add(cell.bottomRightCell);
        }
        if(bot && left && cell.bottomLeftCell != null && cell.bottomLeftCell.IsWalkable(faction))
        {
            neighbors.Add(cell.bottomLeftCell);
        }

        return neighbors;
    }

    List<Cell> RetracePath(Cell startCell, Cell endCell)
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = endCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = currentCell.parentCell;
        }

        if (currentCell == startCell)
        {
            path.Add(startCell);
        }

        path.Reverse();
        return path;
    }
}
