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
            hidePaths = false;

            foreach (Cell cell in tmpPathList)
            {
                cell.UnOver();
            }

            tmpPathList.Clear();
        }
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

            /*RaycastHit2D hit = Physics2D.Raycast(transform.position, newDir, newDir.magnitude, movementLayerMask);

            if (hit.collider == null)
            {
                tmpTarget = path[i].transform.position;
                break;
            }*/

            List<Cell> cells = new();

            if (FindGridIntersections2(transform.position, newDir, newDir.magnitude, ref cells))
            {
                tmpTarget = path[i].transform.position;
                break;
            }
        }


        
        if(MoveTo(tmpTarget + offsetFromLeader) && tmpTarget == path[path.Count - 1].transform.position)
        {
            ClearPath();
        }
            
    }

    public bool CanReachDestinationForward(Vector2 start, Vector2 direction, float length)
    {
        if (length < 0 || direction == Vector2.zero) return false;

        // Normaliser la direction pour obtenir une ligne droite unitaire
        direction.Normalize();

        // Initialisation des variables de Bresenham
        Vector2 currentPos = start;
        Vector2 endPos = start + direction * length;

        float x0 = Mathf.Round(currentPos.x);
        float y0 = Mathf.Round(currentPos.y);
        float x1 = Mathf.Round(endPos.x);
        float y1 = Mathf.Round(endPos.y);

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
            if (!IsCellWalkable(new Vector2(x0, y0), leader.faction) ||
            !IsCellWalkable(new Vector2(x0 + sx, y0), leader.faction) ||
            !IsCellWalkable(new Vector2(x0, y0 + sy), leader.faction) ||
            !IsCellWalkable(new Vector2(x0 + sx, y0 + sy), leader.faction))
            {
                return false;
            }


            // Vérifier si nous avons atteint le point final (ou très proche)
            if (Mathf.Abs(x0 - x1) < 0.01f && Mathf.Abs(y0 - y1) < 0.01f) break;

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

    private bool IsCellWalkable(Vector2 pos, Faction faction)
    {
        Cell cell = leader.gridManager.GetCellFromPos(pos);
        return cell != null && cell.IsWalkable(faction);
    }

    public void PathReachDestinationForward(Vector2 start, Vector2 direction, float length)
    {
        if (length < 0 || direction == Vector2.zero) return;

        // Normaliser la direction pour obtenir une ligne droite unitaire
        direction.Normalize();

        // Calcul des positions de départ et d'arrivée
        Vector2 currentPos = start;
        Vector2 endPos = start + direction * length;

        float x0 = Mathf.Round(currentPos.x);
        float y0 = Mathf.Round(currentPos.y);
        float x1 = Mathf.Round(endPos.x);
        float y1 = Mathf.Round(endPos.y);

        // Déterminer les directions d'avancement
        int stepX = (x1 > x0) ? 1 : -1;
        int stepY = (y1 > y0) ? 1 : -1;

        // Calculer les distances jusqu'aux prochaines intersections de grille
        float tMaxX = (stepX > 0 ? Mathf.Ceil(x0) - x0 : x0 - Mathf.Floor(x0)) / Mathf.Abs(direction.x);
        float tMaxY = (stepY > 0 ? Mathf.Ceil(y0) - y0 : y0 - Mathf.Floor(y0)) / Mathf.Abs(direction.y);

        // Calculer les distances entre les intersections de grille
        float tDeltaX = 1 / Mathf.Abs(direction.x);
        float tDeltaY = 1 / Mathf.Abs(direction.y);

        int index = 0;

        // Parcourir la grille
        while (true)
        {
            index++;
            if (index >= 1000)
            {
                print("ERROR StackOverflow");
                return;
            }

            // Vérifier si la cellule actuelle est praticable
            Cell currentCell = leader.gridManager.GetCellFromPos(new Vector2(Mathf.Floor(x0), Mathf.Floor(y0)));
            tmpPathList.Add(currentCell);
            currentCell.Over();
            if (!currentCell.IsWalkable(leader.faction))
            {
                return;
            }

            // Vérifier si nous avons atteint le point final
            if (Mathf.Abs(x0 - x1) < 0.01f && Mathf.Abs(y0 - y1) < 0.01f) break;

            // Avancer à la prochaine frontière de grille en fonction de tMaxX et tMaxY
            if (tMaxX < tMaxY)
            {
                x0 += stepX;
                tMaxX += tDeltaX;
            }
            else
            {
                y0 += stepY;
                tMaxY += tDeltaY;
            }
        }

        return;
    }

    public bool CanReachDestinationForward2(Vector2 start, Vector2 direction, float length, float radius)
    {
        if (length < 0 || direction == Vector2.zero) return false;

        // Normaliser la direction pour obtenir une ligne droite unitaire
        direction.Normalize();

        // Initialisation des variables de Bresenham
        Vector2 currentPos = start;
        Vector2 endPos = start + direction * length;

        float x0 = Mathf.Round(currentPos.x);
        float y0 = Mathf.Round(currentPos.y);
        float x1 = Mathf.Round(endPos.x);
        float y1 = Mathf.Round(endPos.y);

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

            Vector2 pos = new Vector2(x0, y0);

            Cell currentCell = leader.gridManager.GetCellFromPos(pos);
            if (!currentCell.IsWalkable(leader.faction)) return false;

            foreach (var item in currentCell.GetAdjacentCells())
            {
                if(!item.IsWalkable(leader.faction) && item.DoesIntersectWithCircle(pos, radius))
                    return false;
            }


            // Vérifier si nous avons atteint le point final (ou très proche)
            if (Mathf.Abs(x0 - x1) < 0.01f && Mathf.Abs(y0 - y1) < 0.01f) break;

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

    public bool CanReachDestinationForward3(Vector2 start, Vector2 direction, float length)
    {
        if (length < 0 || direction == Vector2.zero) return false;

        // Normaliser la direction pour obtenir une ligne droite unitaire
        direction.Normalize();

        // Calcul des positions de départ et d'arrivée
        Vector2 currentPos = start;
        Vector2 endPos = start + direction * length;

        float x0 = currentPos.x;
        float y0 = currentPos.y;
        float x1 = endPos.x;
        float y1 = endPos.y;

        // Déterminer les directions d'avancement
        int stepX = (x1 > x0) ? 1 : -1;
        int stepY = (y1 > y0) ? 1 : -1;

        // Calculer les distances jusqu'aux prochaines intersections de grille
        float tMaxX = (direction.x != 0) ?
                      (stepX > 0 ? Mathf.Ceil(x0) - x0 : x0 - Mathf.Floor(x0)) / Mathf.Abs(direction.x)
                      : float.PositiveInfinity;
        float tMaxY = (direction.y != 0) ?
                      (stepY > 0 ? Mathf.Ceil(y0) - y0 : y0 - Mathf.Floor(y0)) / Mathf.Abs(direction.y)
                      : float.PositiveInfinity;

        // Calculer les distances entre les intersections de grille
        float tDeltaX = (direction.x != 0) ? 1 / Mathf.Abs(direction.x) : float.PositiveInfinity;
        float tDeltaY = (direction.y != 0) ? 1 / Mathf.Abs(direction.y) : float.PositiveInfinity;

        int index = 0;
        int maxIterations = 1000; // Limite pour éviter les boucles infinies

        // Parcourir la grille
        while (index < maxIterations)
        {
            index++;

            // Vérifier si la cellule actuelle est praticable
            Cell currentCell = leader.gridManager.GetCellFromPos(new Vector2(Mathf.Floor(x0), Mathf.Floor(y0)));
            if (!currentCell.IsWalkable(leader.faction))
            {
                return false;
            }

            // Vérifier si nous avons atteint le point final (ou que nous sommes très proches)
            if (Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1)) < 0.01f)
                break;

            // Avancer à la prochaine frontière de grille en fonction de tMaxX et tMaxY
            if (tMaxX < tMaxY)
            {
                x0 += stepX;
                tMaxX += tDeltaX;
            }
            else
            {
                y0 += stepY;
                tMaxY += tDeltaY;
            }
        }

        // Si on atteint la limite d'itérations, cela signifie que le chemin est trop long
        if (index >= maxIterations)
        {
            print("ERROR: Iteration limit reached, possible infinite loop.");
            return false;
        }

        return true;
    }

    public bool FindGridIntersections(Vector2 aLineStart, Vector2 aDir, float aDistance, ref List<Cell> aResult)
    {
        aResult.Clear();
        aDir = aDir.normalized;
        aDistance *= aDistance;

        Vector2 aGridSize = Vector2.one * leader.gridManager.cellSize;

        // vertical grid lines
        float x = aLineStart.x / aGridSize.x;
        if (aDir.x > 0.0001f)
        {
            Vector2 v = new Vector2((x * aGridSize.x) - aLineStart.x, 0f);
            v.y = (v.x / aDir.x) * aDir.y;
            while (v.sqrMagnitude < aDistance)
            {
                aResult.Add(leader.gridManager.GetCellFromPos(v));

                v.x += aGridSize.x;
                v.y = (v.x / aDir.x) * aDir.y;
            }
        }
        else if (aDir.x < -0.0001f)
        {
            Vector2 v = new Vector2((x * aGridSize.x) - aLineStart.x, 0f);
            v.y = (v.x / aDir.x) * aDir.y;
            while (v.sqrMagnitude < aDistance)
            {
                aResult.Add(leader.gridManager.GetCellFromPos(v));
                v.x -= aGridSize.x;
                v.y = (v.x / aDir.x) * aDir.y;
            }
        }

        // horizontal grid lines
        float y = aLineStart.y / aGridSize.y;
        if (aDir.y > 0.0001f)
        {
            Vector2 v = new Vector2(0f, (y * aGridSize.y) - aLineStart.y);
            v.x = (v.y / aDir.y) * aDir.x;
            while (v.sqrMagnitude < aDistance)
            {
                aResult.Add(leader.gridManager.GetCellFromPos(v));
                v.y += aGridSize.y;
                v.x = (v.y / aDir.y) * aDir.x;
            }
        }
        else if (aDir.y < -0.0001f)
        {
            Vector2 v = new Vector2(0f, (y * aGridSize.y) - aLineStart.y);
            v.x = (v.y / aDir.y) * aDir.x;
            while (v.sqrMagnitude < aDistance)
            {
                aResult.Add(leader.gridManager.GetCellFromPos(v));
                v.y -= aGridSize.y;
                v.x = (v.y / aDir.y) * aDir.x;
            }
        }
        aResult.Sort((a, b) => a.transform.position.sqrMagnitude.CompareTo(b.transform.position.sqrMagnitude));

        foreach (var item in aResult)
        {
            if (!item.IsWalkable(leader.faction))
                return false;
        }

        return true;
    }

    public bool FindGridIntersections2(Vector2 aLineStart, Vector2 aDir, float aDistance, ref List<Cell> aResult)
    {
        aResult.Clear();
        aDir = aDir.normalized; // Normaliser la direction
        Vector2 aGridSize = Vector2.one * leader.gridManager.cellSize;
        Vector2 endPos = aLineStart + aDir * aDistance;

        // Position initiale de la ligne
        Vector2 currentPos = aLineStart;

        // Trouver la première cellule
        Cell currentCell = leader.gridManager.GetCellFromPos(currentPos);
        aResult.Add(currentCell);

        // Boucle tant que la ligne n'atteint pas la fin
        while (Vector2.Distance(currentPos, endPos) > 0.001f)
        {
            // Coordonnées des bords de la cellule actuelle
            float left = currentCell.transform.position.x - (aGridSize.x / 2f);
            float right = left + aGridSize.x;
            float bottom = currentCell.transform.position.y - (aGridSize.y / 2f);
            float top = bottom + aGridSize.y;

            // Initialisation pour trouver la prochaine intersection
            float tMin = float.MaxValue;
            Vector2 nextIntersection = currentPos;
            Cell nextCell = null;

            // Vérifier l'intersection avec le côté gauche
            if (aDir.x != 0)
            {
                float t = (left - currentPos.x) / aDir.x;
                Vector2 intersection = currentPos + t * aDir;
                if (t > 0 && intersection.y >= bottom && intersection.y <= top && t < tMin)
                {
                    tMin = t;
                    nextIntersection = intersection;
                    nextCell = leader.gridManager.GetCell(currentCell.x - 1, currentCell.y);
                }
            }

            // Vérifier l'intersection avec le côté droit
            if (aDir.x != 0)
            {
                float t = (right - currentPos.x) / aDir.x;
                Vector2 intersection = currentPos + t * aDir;
                if (t > 0 && intersection.y >= bottom && intersection.y <= top && t < tMin)
                {
                    tMin = t;
                    nextIntersection = intersection;
                    nextCell = leader.gridManager.GetCell(currentCell.x + 1, currentCell.y);
                }
            }

            // Vérifier l'intersection avec le côté bas
            if (aDir.y != 0)
            {
                float t = (bottom - currentPos.y) / aDir.y;
                Vector2 intersection = currentPos + t * aDir;
                if (t > 0 && intersection.x >= left && intersection.x <= right && t < tMin)
                {
                    tMin = t;
                    nextIntersection = intersection;
                    nextCell = leader.gridManager.GetCell(currentCell.x, currentCell.y - 1);
                }
            }

            // Vérifier l'intersection avec le côté haut
            if (aDir.y != 0)
            {
                float t = (top - currentPos.y) / aDir.y;
                Vector2 intersection = currentPos + t * aDir;
                if (t > 0 && intersection.x >= left && intersection.x <= right && t < tMin)
                {
                    tMin = t;
                    nextIntersection = intersection;
                    nextCell = leader.gridManager.GetCell(currentCell.x, currentCell.y + 1);
                }
            }

            // Si aucune nouvelle cellule n'a été trouvée, on sort pour éviter une boucle infinie
            if (nextCell == null)
            {
                Debug.Log("Aucune intersection trouvée, fin du parcours");
                break;
            }

            // Mettre à jour la position actuelle et la cellule actuelle
            currentPos = nextIntersection;
            currentCell = nextCell;

            // Ajouter la nouvelle cellule si elle n'est pas déjà dans la liste
            if (!aResult.Contains(currentCell))
            {
                aResult.Add(currentCell);
            }

            // Vérifier si la cellule est accessible
            if (!currentCell.IsWalkable(leader.faction))
            {
                Debug.Log("RETURNED FALSE");
                return false;
            }
        }

        print("return eTRUEEE");

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
