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

    Cell cellOn;


    private void Awake()
    {
        maxLife = life;
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
