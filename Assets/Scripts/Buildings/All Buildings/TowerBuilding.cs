using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBuilding : Building
{
    public float range;
    [SerializeField] Transform rangeTransform;

    UnitManager unitManager;

    public float damage;
    public float firerate;
    float lastFire;

    public GameObject projectile;

    new protected void Start()
    {
        base.Start();

        unitManager = UnitManager.instance;

        rangeTransform.localScale = Vector3.one * range;
        rangeTransform.gameObject.SetActive(false);
    }

    public override void Select()
    {
        base.Select();

        rangeTransform.gameObject.SetActive(true);
    }

    public override void UnSelect()
    {
        base.UnSelect();

        rangeTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        lastFire += Time.deltaTime;

        if (lastFire > firerate) 
        { 
            Unit unit = unitManager.GetNearestEnemyOf(transform, Faction.Player, range);

            if(unit != null)
            {
                lastFire = 0;

                GameObject go = Instantiate(projectile, transform.position, Quaternion.identity);
                go.GetComponent<Projectile>().Init(unit.transform, damage, dir: (transform.position - unit.transform.position).normalized, force: 0.3f);
            }
        }
    }
}
