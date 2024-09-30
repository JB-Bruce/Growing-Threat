using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
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

    List<Cell> path = new();

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
        

        for (int i = path.Count - 1; i > 0; i--)
        {
            Vector3 newDir = path[i].transform.position - (transform.position - offsetFromLeader);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, newDir, newDir.magnitude, movementLayerMask);
            if (hit.collider == null)
            {
                tmpTarget = path[i].transform.position;
                break;
            }
        }


        
        if(MoveTo(tmpTarget + offsetFromLeader))
        {
            ClearPath();
        }
            
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

    public void AddPath(List<Cell> newPath)
    {
        path.AddRange(newPath);
    }

    private void ClearPath()
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
}
