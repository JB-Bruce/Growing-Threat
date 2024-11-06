using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    [System.Serializable]
    public struct ColorBlock
    {
        public Sprite sprite;
        public Color color;
        public Color colorHighlight;
        public BlockType type;
        public bool isWalkable;
    }

    public ColorBlock selectedBlock { get; private set; }
    [SerializeField] List<ColorBlock> blockList;


    public UnitLeader unitInCell;

    [SerializeField] SpriteRenderer cellRenderer;
    CellElement cellElement;

    public bool isOccupied { get { return cellElement != null || unitInCell != null || selectedBlock.type != BlockType.Grass; } private set { }}

    [HideInInspector] public Cell topCell;
    [HideInInspector] public Cell bottomCell;
    [HideInInspector] public Cell rightCell;
    [HideInInspector] public Cell leftCell;

    [HideInInspector] public Cell topRightCell;
    [HideInInspector] public Cell bottomRightCell;
    [HideInInspector] public Cell bottomLeftCell;
    [HideInInspector] public Cell topLeftCell;

    [SerializeField] BoxCollider2D box2d;

    public int x, y;

    GridManager grid;

    public float gCost;
    public float hCost;
    public float fCost
    {
        get { return gCost + hCost; }
    }


    public float traversalCostMultiplier;

    public float traversalCost
    {
        get { return cellElement != null && cellElement is Building b ? b.life * traversalCostMultiplier : 0f; }
    }


    public Cell parentCell;

    public void Init(GridManager gridManager, int indexX, int indexY)
    {
        grid = gridManager;

        x = indexX;
        y = indexY;
    }

    public void InitAfter()
    {
        topCell = grid.GetCell(x, y + 1);
        bottomCell = grid.GetCell(x, y - 1);
        rightCell = grid.GetCell(x + 1, y);
        leftCell = grid.GetCell(x - 1, y);

        topRightCell = grid.GetCell(x + 1, y + 1);
        bottomRightCell = grid.GetCell(x + 1, y - 1);
        bottomLeftCell = grid.GetCell(x - 1, y - 1);
        topLeftCell = grid.GetCell(x - 1, y + 1);
    }

    public bool TryGetBuilding(out Building building)
    {
        building = null;

        if (cellElement is Building newBuilding)
        {
            building = newBuilding;
            return true;
        }
        return false;
    }

    public void Over()
    {
        cellRenderer.color = selectedBlock.colorHighlight;
        cellElement?.Select();
    }

    public void UnOver()
    {
        cellRenderer.color = selectedBlock.color;
        cellElement?.UnSelect();
    }

    public void SetElement(CellElement newCellElement)
    {
        cellElement = newCellElement;
    }

    public bool TrySetDestination(List<Cell> path)
    {
        if(unitInCell != null)
        {
            if (path[path.Count -1].TrySetUnit(unitInCell))
            {
                unitInCell.SetDestination(path);
                unitInCell = null;
                return true;
            }
            
        }

        return false;
    }

    public List<Cell> GetAdjacentCells(bool takeDiagonals = true)
    {
        List<Cell> cells = new List<Cell>()
        {
            topCell,
            bottomCell,
            rightCell,
            leftCell
        };

        if(!takeDiagonals) return FilterNonExistingCells(cells);

        cells.AddRange(new List<Cell>(){
            topLeftCell,
            topRightCell,
            bottomLeftCell,
            bottomRightCell
        });

        return FilterNonExistingCells(cells);
    }

    public List<Cell> FilterNonExistingCells(List<Cell> cells)
    {
        List<Cell> existingCells = new();

        foreach(Cell cell in cells)
        {
            if(cell != null)
                existingCells.Add(cell);
        }

        return existingCells;
    }

    public bool DoesIntersectWithCircle(Vector2 circleCenter, float radius)
    {
        Vector2 pos = transform.position;
        Vector2 scale = transform.localScale;

        // Trouver le point le plus proche du centre du cercle sur le carré
        float closestX = Mathf.Clamp(circleCenter.x, pos.x - scale.x / 2, pos.x + scale.x / 2);
        float closestY = Mathf.Clamp(circleCenter.y, pos.y - scale.y / 2, pos.y + scale.y / 2);

        // Calculer la distance entre le centre du cercle et ce point le plus proche
        float distanceX = circleCenter.x - closestX;
        float distanceY = circleCenter.y - closestY;

        // Calculer la distance au carré
        float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

        // Vérifier si la distance est inférieure ou égale au carré du rayon
        return distanceSquared <= (radius * radius);
    }

    public void SetBlock(BlockType type)
    {
        foreach (ColorBlock block in blockList)
        {
            if(block.type == type)
            {
                selectedBlock = block;
                cellRenderer.sprite = selectedBlock.sprite;
                cellRenderer.color = selectedBlock.color;
                box2d.enabled = !selectedBlock.isWalkable;
                return;
            }
        }
    }

    public bool TrySetUnit(UnitLeader unit)
    {
        if(unitInCell == null)
        {
            unitInCell = unit;
            return true;
        }
        return false;
    }

    public bool IsWalkableOrBreakable(Faction faction, out bool breakable)
    {
        breakable = (cellElement != null) ? cellElement.canBeDestroyed : false;
        return (cellElement == null || (faction == Faction.Player ? !cellElement.blockFrienflyUnits : (!cellElement.blockEnemyUnits || cellElement.canBeDestroyed))) && selectedBlock.isWalkable;
    }
    public bool IsWalkable(Faction faction)
    {
        return (cellElement == null || (faction == Faction.Player ? !cellElement.blockFrienflyUnits : !cellElement.blockEnemyUnits)) && selectedBlock.isWalkable;
    }
}

[System.Serializable]
public enum BlockType
{
    Grass,
    Stone,
    Water,
    None
}
