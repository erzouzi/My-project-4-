using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    // 由状态机在播动画前设置
    [HideInInspector] public float damage = 10f;
    [HideInInspector] public bool knockDown;
    [HideInInspector] public float knockback = 0.3f;

    [Header("目标Tag")]
    public string targetTag = "Player";

    [Header("顿帧")]
    public float normalHitStop = 0.04f;
    public float knockDownHitStop = 0.12f;

    [Header("屏幕抖动")]
    public float normalShakeIntensity = 0.12f;
    public float normalShakeDuration = 0.15f;
    public float knockDownShakeIntensity = 0.35f;
    public float knockDownShakeDuration = 0.3f;


    public Collider col;
    private Transform owner;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    public bool HasHitThisAttack { get; private set; }
    public System.Action monsterOnHit;
    public System.Action playerOnHit;

    void Awake()
    {
        col.isTrigger = true;
        col.enabled = false;
        owner = GetComponent<UnitController>().transform;

    }

    // 动画Event调用：开始检测
    public void EnableHitbox()
    {
        hitTargets.Clear();
        HasHitThisAttack = false;
        col.enabled = true;
    }

    // 动画Event调用：停止检测
    public void DisableHitbox()
    {
        col.enabled = false;
    }

    public void HandleTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        if (hitTargets.Contains(other.gameObject))
            return;

        hitTargets.Add(other.gameObject);

        UnitController target = other.GetComponent<UnitController>();
        if (target != null)
        {
            HasHitThisAttack = true;
            if (other.CompareTag("Enemy"))
                monsterOnHit?.Invoke();
            if (other.CompareTag("Player"))
                playerOnHit?.Invoke();

            target.TakeDamage(owner.position, damage, knockDown, knockback);

            // 在受害者身上弹出伤害数字
            DamageNumberPool.Show(other.transform.position, damage, knockDown);

            float stopTime = knockDown ? knockDownHitStop : normalHitStop;
            HitStopMgr.Instance.Stop(stopTime);

            // 屏幕抖动
            float shakeIntensity = knockDown ? knockDownShakeIntensity : normalShakeIntensity;
            float shakeDuration = knockDown ? knockDownShakeDuration : normalShakeDuration;
            CameraFollow.Trigger(shakeIntensity, shakeDuration);

        }
    }
}
