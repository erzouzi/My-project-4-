using System;
using UnityEngine;

[Serializable]
public class SlashConfig
{
    public Transform spawnPoint;
    public GameObject slashPrefab; // 不填则用默认
}

public class WeaponVFX : MonoBehaviour
{
    public GameObject defaultSlashPrefab;
    public SlashConfig[] slashConfigs;

    [Header("远程刀光")]
    public float rangedSlashSpeed = 18f;
    public float rangedSlashLifetime = 2f;
    public float rangedSlashDamage = 20f;
    public bool rangedSlashKnockDown = false;
    public float rangedSlashKnockback = 1.0f;

    /// <summary>
    /// 近战刀光：在 spawnPoint 位置直接生成特效（由动画事件调用）
    /// </summary>
    public void PlaySlash(int index)
    {
        if (index < 0 || index >= slashConfigs.Length) return;

        SlashConfig cfg = slashConfigs[index];
        if (cfg.spawnPoint == null) return;

        GameObject prefab = cfg.slashPrefab != null ? cfg.slashPrefab : defaultSlashPrefab;
        if (prefab == null) return;

        GameObject slash = Instantiate(prefab, cfg.spawnPoint.position, cfg.spawnPoint.rotation);
        Destroy(slash, 1.5f);
    }

    /// <summary>
    /// 远程刀光：在 spawnPoint 位置生成刀光，朝最近敌人方向飞行（由动画事件依次调用）
    /// </summary>
    public void ShootSlash(int index)
    {
        if (index < 0 || index >= slashConfigs.Length) return;

        SlashConfig cfg = slashConfigs[index];
        if (cfg.spawnPoint == null) return;

        GameObject prefab = cfg.slashPrefab != null ? cfg.slashPrefab : defaultSlashPrefab;
        if (prefab == null) return;

        // 计算飞行方向：优先朝向最近敌人，否则朝角色前方
        Vector3 direction = transform.forward;
        EnemyController nearest = FindNearestEnemy();
        if (nearest != null)
        {
            Vector3 toEnemy = nearest.transform.position - cfg.spawnPoint.position;
            toEnemy.y = 0f;
            if (toEnemy.sqrMagnitude > 0.001f)
                direction = toEnemy.normalized;
        }

        GameObject slash = Instantiate(prefab, cfg.spawnPoint.position, Quaternion.LookRotation(direction));

        // 如果预制体上有 SlashProjectile 组件，初始化飞行参数
        SlashProjectile projectile = slash.GetComponent<SlashProjectile>();
        if (projectile != null)
        {
            projectile.Init(direction, rangedSlashDamage, rangedSlashKnockDown, rangedSlashKnockback);
            projectile.speed = rangedSlashSpeed;
            projectile.lifetime = rangedSlashLifetime;
        }

        Destroy(slash, rangedSlashLifetime);
    }

    private EnemyController FindNearestEnemy()
    {
        var enemies = FindObjectsOfType<EnemyController>();
        EnemyController nearest = null;
        float minDist = float.MaxValue;
        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }
}
