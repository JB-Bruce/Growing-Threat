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

    public List<Cell> path { get; private set; } = new();

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
        if (CheckForEnemies(out Unit enemy))
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

        if(leader.faction != Faction.Player && CheckNearestAttractableBuilding(out Building building, true) && building != null)
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
        

        for (int i = path.Count - 1; i > 0; i--)
        {
            Vector3 newDir = path[i].transform.position - (transform.position - offsetFromLeader);
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, newDir, newDir.magnitude, movementLayerMask);

            if(CanReachDestinationForward2(transform.position, newDir, newDir.magnitude))
            {
                tmpTarget = path[i].transform.position;
                break;
            }

            /*if (hit.collider == null)
            {
                tmpTarget = path[i].transform.position;
                break;
            }*/
        }


        
        if(MoveTo(tmpTarget + offsetFromLeader))
        {
            ClearPath();
        }
            
    }

    public bool CanReachDestinationForward(Vector2 start, Vector2 direction, float length)
    {
        direction = direction.normalized;

        Vector2 currentPos = start;

        float cellSize = leader.gridManager.cellSize;
        float semiSize = cellSize / 2f;

        int index = 0;

        while (length > 0)
        {
            index++;
            if(index >= 100)
            {
                print("WARNNG : reached max index");
                return false;
            }


            Cell currentCell = leader.gridManager.GetCellFromPos(new(currentPos.x, currentPos.y));
            if (!currentCell.IsWalkable(leader.faction))
            {
                return false;
            }


            // Déterminer les bordures de la cellule actuelle
            float leftBorder = currentCell.transform.position.x - semiSize;           // Bord gauche
            float rightBorder = currentCell.transform.position.x + semiSize; // Bord droit
            float bottomBorder = currentCell.transform.position.y - semiSize;         // Bord bas
            float topBorder = currentCell.transform.position.y + semiSize; // Bord haut

            // Calculer les bordures en fonction de la direction
            float nextVerticalBorder = (direction.x > 0) ? rightBorder : leftBorder;
            float nextHorizontalBorder = (direction.y > 0) ? topBorder : bottomBorder;

            // Calculer tMax
            float tMaxX = (direction.x == 0) ? float.MaxValue : (nextVerticalBorder - currentPos.x) / direction.x;
            float tMaxY = (direction.y == 0) ? float.MaxValue : (nextHorizontalBorder - currentPos.y) / direction.y;


            float t = Mathf.Min(tMaxX, tMaxY);
            currentPos += direction * t;

            if (t < 0.0001f)
            {
                print("WARNNG : t est trop petit, arrêt de la boucle");
                return false;
            }
            else
            {
                print("T est assez grand");
            }

            length = Mathf.Max(length - t, 0.001f);
        }

        return true;
    }

    public bool CanReachDestinationForward2(Vector2 start, Vector2 direction, float length)
    {
        if (length < 0 || direction == Vector2.zero) return false;

        // Normaliser la direction pour obtenir une ligne droite unitaire
        direction.Normalize();

        // Initialisation des variables de Bresenham
        Vector2 currentPos = start;
        Vector2 endPos = start + direction * length;

        float x0 = currentPos.x;
        float y0 = currentPos.y;
        float x1 = endPos.x;
        float y1 = endPos.y;

        float dx = Mathf.Abs(x1 - x0);
        float dy = Mathf.Abs(y1 - y0);
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        float err = dx - dy;

        int index = 0;

        // Parcourir la grille en suivant l'algorithme de Bresenham
        while (true)
        {
            index++;
            if (index >= 1000)
            {
                print("ERROR StackOverflow");
                return false;
            }

            // Vérifier si la cellule actuelle est praticable
            Cell currentCell = leader.gridManager.GetCellFromPos(new Vector2(x0, y0));
            if (!currentCell.IsWalkable(leader.faction))
            {
                return false;
            }

            // Vérifier si nous avons atteint le point final (ou très proche)
            if (Mathf.Abs(x0 - x1) < 0.001f && Mathf.Abs(y0 - y1) < 0.001f) break;

            // Calculer l'erreur pour déterminer la prochaine cellule
            float e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return true;
    }

    private bool CheckNearestAttractableBuilding(out Building building, bool includePathBlockers = false)
    {
        building = null;

        if(CheckForBuildings(out List<Building> buildings, Faction.Player))
        {
            Building nearestBuilding = null;
            float nearestDistance = Mathf.Infinity;

            foreach (var item in buildings)
            {
                if(!item.attractEnemy && !leader.authorizedNonAttractableBuildingsToDestroy.Contains(item)) continue;

                float newDistance = Vector2.Distance(item.transform.position, transform.position);
                if (newDistance < nearestDistance)
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

        return enemy != null;
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

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent<Building>(out Building building))
        {
            touchedBuildings.Remove(building);
        }
    }
}
