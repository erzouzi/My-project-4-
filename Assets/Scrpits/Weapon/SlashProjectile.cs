using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 远程刀光飞行组件。不需要 Rigidbody / Collider，用 OverlapSphere 手动检测命中。
/// 挂到刀光预制体上，由 WeaponVFX.ShootSlash 初始化方向。
/// </summary>
public class SlashProjectile : MonoBehaviour
{
    [Header("飞行")]
    public float speed = 15f;
    public float lifetime = 2f;

    [Header("命中检测")]
    public float hitRadius = 0.5f;
    public LayerMask enemyLayer = -1; // 默认检测所有层，可在 Inspector 设为 Enemy 层

    [Header("伤害")]
    public float damage = 20f;
    public bool knockDown = false;
    public float knockback = 1.0f;

    [Header("穿透")]
    [Tooltip("-1 = 无限穿透，>0 = 穿透 N 个敌人后消失")]
    public int pierceCount = -1;

    [Header("音效")]
    public AudioClip hitClip;

    private Vector3 direction;
    private float timer;
    private bool initialized;
    private HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

    /// <summary>
    /// 由 WeaponVFX.ShootSlash 调用，传入飞行方向和伤害参数。
    /// </summary>
    public void Init(Vector3 dir, float dmg, bool kd, float kb)
    {
        direction = dir.normalized;
        damage = dmg;
        knockDown = kd;
        knockback = kb;
        timer = 0f;
        initialized = true;
        hitEnemies.Clear();

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Update()
    {
        if (!initialized) return;

        // 飞行
        Vector3 move = direction * speed * Time.deltaTime;
        transform.position += move;
        timer += Time.deltaTime;

        // 每帧检测命中（不需要 Rigidbody / Collider）
        CheckHit();

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    private void CheckHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, enemyLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            var enemy = hit.GetComponent<EnemyController>();
            if (enemy == null || hitEnemies.Contains(enemy)) continue;

            enemy.TakeDamage(transform.position, damage, knockDown, knockback);
            hitEnemies.Add(enemy);

            // 伤害数字
            DamageNumberPool.Show(enemy.transform.position, damage, knockDown);

            // 顿帧
            float stopTime = knockDown ? 0.12f : 0.04f;
            HitStopMgr.Instance.Stop(stopTime);

            // 屏幕抖动
            float shakeIntensity = knockDown ? 0.35f : 0.12f;
            float shakeDuration = knockDown ? 0.3f : 0.15f;
            CameraFollow.Trigger(shakeIntensity, shakeDuration);

            if (hitClip != null)
                AudioSource.PlayClipAtPoint(hitClip, transform.position);

            if (pierceCount > 0 && hitEnemies.Count >= pierceCount)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
