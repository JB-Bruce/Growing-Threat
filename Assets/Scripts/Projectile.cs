using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    ProjectileInfo projectileInfo = new();

    public float projSpeed;

    public void Init(Transform target, float damage, float speed = 0f, Vector2? dir = null, float force = 0f)
    {
        projectileInfo.target = target;
        projectileInfo.damage = damage;
        projectileInfo.speed = speed == 0f ? projSpeed : speed;
        projectileInfo.dir = (Vector2)(dir != null ? dir : transform.position - target.position);
        projectileInfo.force = force;
    }

    private void Update()
    {
        if (projectileInfo.target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, projectileInfo.target.position, projectileInfo.speed * Time.deltaTime);

        if(Vector2.Distance(transform.position, projectileInfo.target.position) < 0.01f)
        {
            projectileInfo.target.GetComponent<Unit>().TakeDamage(projectileInfo.damage, projectileInfo.dir.normalized * projectileInfo.force);
            Destroy(gameObject);
        }
    }
}

public struct ProjectileInfo
{
    public Transform target;
    public float damage;
    public float speed;
    public Vector2 dir;
    public float force;
}
