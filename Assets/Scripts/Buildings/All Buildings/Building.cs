using UnityEngine;
using UnityEngine.Events;

public class Building : CellElement
{
    public bool attractEnemy;
    public float life;
    private float maxLife;
    public Faction faction = Faction.Player;

    public UnityEvent onBuildingDestroyed;
    public UnityEvent<float, float> onBuildingDamaged;

    protected Cell cellOn;


    private void Awake()
    {
        maxLife = life;
    }

    protected void Start()
    {
        if (!GetComponent<BoxCollider2D>().isTrigger && !blockFrienflyUnits)
            UnitManager.instance.DesactiveCollision(GetComponent<BoxCollider2D>(), Faction.Player);
    }

    public void SetFaction(Faction newFaction)
    {
        faction = newFaction;
    }

    public bool TakeDamage(float damage)
    {
        life -= damage;

        onBuildingDamaged.Invoke(life / maxLife, damage);

        if (life <= 0)
        {
            if (!GetComponent<BoxCollider2D>().isTrigger && !blockFrienflyUnits)
                UnitManager.instance.RemoveDesactiveCollision(GetComponent<BoxCollider2D>());

            BuildingManager.instance.RemoveBuilding(this);
            cellOn.SetElement(null);
            onBuildingDestroyed.Invoke();
            Destroy(gameObject);
            return true;
        }

        return false;
    }

    public void SetCell(Cell newCell)
    {
        cellOn = newCell;
    }
}
