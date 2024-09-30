using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [SerializeField] int gridSize;
    [SerializeField] float cellSize;

    [SerializeField] GameObject cellPrefab;
    [SerializeField] Transform gridParent;

    List<Cell> cells = new List<Cell>();

    Cell selectedCell = null;

    [SerializeField] Camera cam;

    Vector2 gridStartPos;

    public float grassSizeAroundHub;
    public float stoneSeuil;
    public float noiseScale;

    float offsetX;
    float offsetY;

    Vector2 center;

    private void Awake()
    {
        instance = this;

        offsetX = Random.Range(0f, 1000f);
        offsetY = Random.Range(0f, 1000f);
    }

    public void Init()
    {
        float totalLength = (gridSize - 1) * cellSize;
        gridStartPos = -Vector2.one * (totalLength / 2f);

        center = new(gridStartPos.x + totalLength / 2f, gridStartPos.y + totalLength / 2f);

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Vector2 newPos = gridStartPos + new Vector2(j * cellSize, i * cellSize);
                GameObject go = Instantiate(cellPrefab, newPos, Quaternion.identity, gridParent);
                var newCell = go.GetComponent<Cell>();
                newCell.Init(this, j, i);
                cells.Add(newCell);
                
            }
        }

        foreach (Cell cell in cells)
        {
            cell.InitAfter();

            float cordX = cell.x / (float)gridSize * noiseScale + offsetX;
            float cordY = cell.y / (float)gridSize * noiseScale + offsetY;

            float height = Mathf.PerlinNoise(cordX, cordY);

            Vector2 tmp = new Vector2(cell.transform.position.x, cell.transform.position.y);

            if (Vector2.Distance(tmp, center) > grassSizeAroundHub * cellSize && height > stoneSeuil)
            {
                cell.SetBlock(BlockType.Stone);
            }
            else
            {
                cell.SetBlock(BlockType.Grass);
            }
        }

    }

    private void Update()
    {
        bool rMouse = Input.GetMouseButtonDown(1);
        bool lMouse = Input.GetMouseButtonDown(0);

        if (lMouse)
        {
            HandleLeftMouse();
        }

        if (rMouse)
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

    public Cell GetCell(int x, int y)
    {
        Cell returnCell = null;

        if(x >= 0 && x < gridSize && y >= 0 && y < gridSize) returnCell = cells[x + y * gridSize];

        return returnCell;
    }

    private void HandleLeftMouse()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (IsPosOverACell(mousePos))
        {
            Cell overedCell = GetCellFromPos(mousePos);
            foreach (Cell n in cells) { n.UnOver(); }
            selectedCell = overedCell;
            selectedCell.Over();
        }
        else
        {
            if (selectedCell != null)
            {
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
            if (selectedCell != null)
            {
                List<Cell> path = FindPath(selectedCell, overedCell, out bool completedPath);

                foreach (Cell n in cells) { n.UnOver(); }


                if (selectedCell.TrySetDestination(path))
                {
                    foreach (Cell n in path)
                    {
                        n.Over();
                    }
                }
                selectedCell = null;


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

    public void SetElement(CellElement newElement, Vector2 pos)
    {
        Cell cell = GetCellFromPos(pos);

        cell.SetElement(newElement);
        newElement.transform.position = cell.transform.position;
    }

    private Vector2 GetCellPositionFromWorldPos(Vector2 pos)
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

    public List<Cell> FindPath(Cell startCell, Cell targetCell, out bool completedPath)
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

            foreach (Cell neighbor in GetNeighbors(currentCell))
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newMovementCostToNeighbor = currentCell.gCost + GetDistance(currentCell, neighbor);
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

    int GetDistance(Cell nodeA, Cell nodeB)
    {
        int dstX = Mathf.Abs(nodeA.x - nodeB.x);
        int dstY = Mathf.Abs(nodeA.y - nodeB.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    List<Cell> GetNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();

        bool top = false, bot = false, right = false, left = false;

        if(cell.topCell != null && cell.topCell.IsWalkable())
        {
            neighbors.Add(cell.topCell);
            top = true;
        }
        if(cell.bottomCell != null && cell.bottomCell.IsWalkable())
        {
            neighbors.Add(cell.bottomCell);
            bot = true;
        }
        if(cell.leftCell != null && cell.leftCell.IsWalkable())
        {
            neighbors.Add(cell.leftCell);
            left = true;
        }
        if(cell.rightCell != null && cell.rightCell.IsWalkable())
        {
            neighbors.Add(cell.rightCell);
            right = true;
        }

        if(top && right && cell.topRightCell != null && cell.topRightCell.IsWalkable())
        {
            neighbors.Add(cell.topRightCell);
        }
        if(top && left && cell.topLeftCell != null && cell.topLeftCell.IsWalkable())
        {
            neighbors.Add(cell.topLeftCell);
        }
        if(bot && right && cell.bottomRightCell != null && cell.bottomRightCell.IsWalkable())
        {
            neighbors.Add(cell.bottomRightCell);
        }
        if(bot && left && cell.bottomLeftCell != null && cell.bottomLeftCell.IsWalkable())
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
