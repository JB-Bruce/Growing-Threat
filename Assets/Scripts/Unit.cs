using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Entity
{
    public float acceleration;
    public float maxSpeed;

    public UnitLeader leader;
    Vector3 offsetFromLeader;

    public SpriteRenderer sp;

    Rigidbody2D rb;

    public float stopingDistance;
    public AnimationCurve stopingCurve;

    public AnimationCurve randomSpeedCurve;

    bool initialized = false;

    float spawnRandom;

    public bool showPath = false;
    public bool showChosenDirection = false;
    public bool hidePaths = false;

    [SerializeField] public List<Cell> path { get; set; } = new();

    List<Cell> tmpPathList = new();

    Vector3 tmpTarget;

    public LayerMask movementLayerMask;

    UnitManager manager;

    [Header("Combat")]

    public float health;
    public float damage;
    public float attackInterval;
    public float attackDistance;
    public float viewDistance;

    public float attackDistanceFromTheDestination;
    public bool attackOnSight = false;

    bool canAttack = true;

    List<Building> touchedBuildings = new();

    [Header("Skipping times")]

    public float followPathSkip;
    float followPathTime;
    public float lookingForEnemySkip;
    float lookingForEnemyTime;
    public float lookingForBuildingSkip;
    float lookingForBuildingTime;


    private void OnValidate()
    {
        if (showPath)
        {
            showPath = false;
            foreach (Cell cell in path) {
                tmpPathList.Add(cell);
                cell.Over();
            }
        }

        if (showChosenDirection) {
            showChosenDirection = false;
            for (int i = path.Count - 1; i > 0; i--)
            {
                Vector3 newDir = path[i].transform.position - (transform.position - offsetFromLeader);

                /*RaycastHit2D hit = Physics2D.Raycast(transform.position, newDir, newDir.magnitude, movementLayerMask);

                if (hit.collider == null)
                {
                    tmpTarget = path[i].transform.position;
                    break;
                }*/

                var t = new List<Cell>();

                if (FindGridIntersections2(transform.position, newDir, newDir.magnitude, ref t))
                {
                    foreach (Cell cell in t)
                    {
                        tmpPathList.Add(cell);
                        cell.Over();
                    }
                    break;
                }
            }
        }

        if (hidePaths)
        {
            HidePath();
        }
    }

    private void HidePath()
    {
        hidePaths = false;

        foreach (Cell cell in tmpPathList)
        {
            cell.UnOver();
        }

        tmpPathList.Clear();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        spawnRandom = Random.Range(0f, 5f);

        InvokeRepeating("ChangeRandom", 0, .15f);
    }

    private void Start()
    {
        manager = UnitManager.instance;
    }

    private void ChangeRandom()
    {
        spawnRandom += Random.Range(-0.1f, .1f);
    }

    public void Init(UnitLeader newLeader, Vector2 posToKeep, Color newColor, bool _attackOnSight = false)
    {
        transform.position = newLeader.transform.position + new Vector3(posToKeep.x, posToKeep.y, 0);
        leader = newLeader;
        offsetFromLeader = posToKeep;
        initialized = true;
        tmpTarget = leader.transform.position;

        path.Add(GridManager.instance.GetCellFromPos(tmpTarget));

        attackOnSight = _attackOnSight;

        sp.color = newColor;
    }

    public void ChangeOffset(Vector2 newOffset)
    {
        offsetFromLeader = newOffset;
    }

    private void Update()
    {
        followPathTime += Time.deltaTime;
        lookingForEnemyTime += Time.deltaTime;
        lookingForBuildingTime += Time.deltaTime;

        if(lookingForEnemyTime >= lookingForEnemySkip)
        {
            lookingForEnemyTime -= lookingForEnemySkip;
        }
        else if (CheckForEnemies(out Unit enemy))
        {
            if(attackOnSight || Vector2.Distance(enemy.transform.position, path[path.Count - 1].transform.position) < attackDistanceFromTheDestination)
            {
                if (MoveTo(enemy.transform.position, attackDistance))
                {
                    if (canAttack)
                    {
                        enemy.TakeDamage(damage, transform.position - enemy.transform.position);
                        canAttack = false;
                        Invoke("ResetAttack", attackInterval);
                    }
                }
                return;
            }
        }


        if (lookingForBuildingTime >= lookingForBuildingSkip)
        {
            lookingForBuildingTime -= lookingForBuildingSkip;
        }
        else if (leader.faction != Faction.Player && CheckNearestAttractableBuilding(out Building building, true) && building != null)
        {
            MoveTo(building.transform.position, attackDistance);
            if (canAttack && touchedBuildings.Contains(building))
            {
                building.TakeDamage(damage);
                canAttack = false;
                Invoke("ResetAttack", attackInterval);
            }
            return;
        }
        
        if(followPathTime >= followPathSkip)
        {
            followPathTime -= followPathSkip;
            return;
        }

        for (int i = path.Count - 1; i > 0; i--)
        {
            Vector3 newDir = path[i].transform.position - (transform.position - offsetFromLeader);
            Vector3 newDir2 = path[i].transform.position - (transform.position);

            /*RaycastHit2D hit = Physics2D.Raycast(transform.position, newDir, newDir.magnitude, movementLayerMask);

            if (hit.collider == null)
            {
                tmpTarget = path[i].transform.position;
                break;
            }*/

            List<Cell> cells = new();

            if (FindGridIntersections2(transform.position, newDir2, newDir2.magnitude, ref cells))
            {
                HidePath();
                tmpTarget = path[i].transform.position;
                
                Debug.DrawLine(transform.position, path[i].transform.position);
                break;
            }
        }


        
        if(MoveTo(tmpTarget + offsetFromLeader) && tmpTarget == path[path.Count - 1].transform.position)
        {
            ClearPath();
        }
            
    }


    public bool FindGridIntersections2(Vector2 aLineStart, Vector2 aDir, float aDistance, ref List<Cell> aResult)
    {
        aResult.Clear();
        aDir = aDir.normalized; 
        Vector2 aGridSize = Vector2.one * leader.gridManager.cellSize;
        Vector2 endPos = aLineStart + aDir * aDistance;

        Vector2 currentPos = aLineStart;

        Cell endCell = leader.gridManager.GetCellFromPos(endPos);

        Cell currentCell = leader.gridManager.GetCellFromPos(currentPos);
        aResult.Add(currentCell);

        int index = 0;
        float epsilon = 0.0001f; 

        while (currentCell != endCell && index < 1000) 
        {
            index++;

            float left = currentCell.transform.position.x - (aGridSize.x / 2f);
            float right = left + aGridSize.x;
            float bottom = currentCell.transform.position.y - (aGridSize.y / 2f);
            float top = bottom + aGridSize.y;

            float tMin = float.MaxValue;
            Vector2 nextIntersection = currentPos;
            Cell nextCell = null;

            if (aDir.x != 0)
            {
                float t = (left - currentPos.x) / aDir.x;
                Vector2 intersection = currentPos + t * aDir;
                if (t > epsilon && intersection.y >= bottom - epsilon && intersection.y <= top + epsilon && t < tMin)
                {
                    tMin = t;
                    nextIntersection = intersection;
                    nextCell = leader.gridManager.GetCell(currentCell.x - 1, currentCell.y);
                }
            }

            if (aDir.x != 0)
            {
                float t = (right - currentPos.x) / aDir.x;
                Vector2 intersection = currentPos + t * aDir;
                if (t > epsilon && intersection.y >= bottom - epsilon && intersection.y <= top + epsilon && t < tMin)
                {
                    tMin = t;
                    nextIntersection = intersection;
                    nextCell = leader.gridManager.GetCell(currentCell.x + 1, currentCell.y);
                }
            }

            if (aDir.y != 0)
            {
                float t = (bottom - currentPos.y) / aDir.y;
                Vector2 intersection = currentPos + t * aDir;
                if (t > epsilon && intersection.x >= left - epsilon && intersection.x <= right + epsilon && t < tMin)
                {
                    tMin = t;
                    nextIntersection = intersection;
                    nextCell = leader.gridManager.GetCell(currentCell.x, currentCell.y - 1);
                }
            }

            if (aDir.y != 0)
            {
                float t = (top - currentPos.y) / aDir.y;
                Vector2 intersection = currentPos + t * aDir;
                if (t > epsilon && intersection.x >= left - epsilon && intersection.x <= right + epsilon && t < tMin)
                {
                    tMin = t;
                    nextIntersection = intersection;
                    nextCell = leader.gridManager.GetCell(currentCell.x, currentCell.y + 1);
                }
            }

            if (nextCell == null)
            {
                return false;
            }

            currentPos = nextIntersection;
            currentCell = nextCell;

            if (!aResult.Contains(currentCell))
            {
                aResult.Add(currentCell);
            }

            if (!currentCell.IsWalkable(leader.faction))
            {
                return false;
            }
        }

        return true;
    }

    public void DesactiveCollision(Collider2D collision)
    {
        Physics2D.IgnoreCollision(collision, GetComponent<CircleCollider2D>());
    }

    private bool CheckNearestAttractableBuilding(out Building building, bool includePathBlockers = false)
    {
        building = null;

        if(CheckForBuildings(out List<Building> buildings, Faction.Player))
        {
            Building nearestBuilding = null;
            float nearestDistance = Mathf.Infinity;

            List<Cell> cells = new List<Cell>();

            foreach (var item in buildings)
            {
                if(!item.attractEnemy && !leader.authorizedNonAttractableBuildingsToDestroy.Contains(item)) continue;

                float newDistance = Vector2.Distance(item.transform.position, transform.position);
                if (newDistance < nearestDistance && (FindGridIntersections2(transform.position, item.transform.position - transform.position, (item.transform.position - transform.position).magnitude, ref cells) || leader.authorizedNonAttractableBuildingsToDestroy.Contains(item)))
                {
                    nearestDistance = newDistance;
                    nearestBuilding = item;
                }
            }

            building = nearestBuilding;

            return true;
        }

        return false;
    }

    private void ResetAttack()
    {
        canAttack = true;
    }

    private bool MoveTo(Vector3 target, float stoppingDistance = 0.05f)
    {
        Vector3 dir = target - (transform.position);

        float dist = dir.magnitude;

        if (dist > stoppingDistance)
        {
            rb.AddForce(dir.normalized * Time.deltaTime * acceleration * (dist < stopingDistance ? stopingCurve.Evaluate(dist / stopingDistance) : 1f) * randomSpeedCurve.Evaluate((Time.time + spawnRandom) % 5));
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
            return false;
        }
        return true;
    }

    private bool CheckForEnemies(out Unit enemy)
    {
        enemy = manager.GetNearestEnemyOf(this, viewDistance);

        List<Cell> cells = new List<Cell>();

        return enemy != null && FindGridIntersections2(transform.position, enemy.transform.position - transform.position, (enemy.transform.position - transform.position).magnitude, ref cells);
    }

    private bool CheckForBuildings(out List<Building> buildings, Faction faction)
    {
        buildings = manager.GetNearestBuildingsOf(this, faction, viewDistance);

        return buildings.Count > 0;
    }

    public void AddPath(List<Cell> newPath)
    {
        path.AddRange(newPath);
    }

    public void ClearPath()
    {
        Cell lastCell = path[path.Count - 1];

        path.Clear();

        path.Add(lastCell);
    }

    public void TakeDamage(float damageTaken, Vector2 dir)
    {
        health -= damageTaken;
        rb.AddForce(-dir * 10f, ForceMode2D.Impulse);

        if(health <= 0)
        {
            leader.LooseUnit(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent<Building>(out Building building))
        {
            touchedBuildings.Add(building);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent<Building>(out Building building))
        {
            touchedBuildings.Add(building);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent<Building>(out Building building))
        {
            touchedBuildings.Remove(building);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent<Building>(out Building building))
        {
            touchedBuildings.Remove(building);
        }
    }
}
