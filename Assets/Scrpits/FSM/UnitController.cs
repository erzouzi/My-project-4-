using UnityEngine;

public class UnitController : MonoBehaviour
{
    protected FSMController fsm;
    public float setMovement;
    public Vector3 hitSourcePos;
    public bool isKnockDown;
    public float knockbackDistance;

    // ---- HP ----
    public virtual float MaxHP => 100f;
    public float CurrentHP { get; protected set; }

    // ---- 受击僵直 ----
    [HideInInspector] public float hitStunDuration = 0f;
    [HideInInspector] public float stunnedUntil;

    public virtual void ApplyRootMotion(Vector3 deltaPosition) { }
    public virtual void Die()
    {
        if (fsm.CurType == StateType.Die) return;
        fsm.SwitchType(StateType.Die);
    }
    public virtual void TakeDamage(Vector3 attackerPos, float damage, bool knockDown = false, float knockback = 0.3f)
    {
        if (fsm.CurType == StateType.Die) return;

        // 弹反判定：防御状态下由子类决定是否弹反成功
        if (fsm.CurType == StateType.Defence)
        {
            if (OnDefenceHit(attackerPos, damage))
                return; // 弹反成功，免伤
            // 弹反失败，继续受伤流程
        }

        hitSourcePos = attackerPos;
        isKnockDown = knockDown;
        knockbackDistance = knockback;
        OnTakeDamage(damage);

        if (fsm.CurType != StateType.Die)
            fsm.SwitchType(StateType.Hit);
    }

    /// <summary>
    /// 防御状态下受击回调。返回 true 表示弹反成功（免伤），false 表示防御失败（正常受伤）
    /// </summary>
    protected virtual bool OnDefenceHit(Vector3 attackerPos, float damage) => false;

    /// <summary>
    /// 实际扣血逻辑。子类重写时务必调用 base.OnTakeDamage(damage)
    /// </summary>
    protected virtual void OnTakeDamage(float damage)
    {
        CurrentHP -= damage;

        // 通过 EventCenter 广播 HP 变化，解耦 UI 和战斗逻辑
        var data = new HPChangeData(CurrentHP, MaxHP);
        if (CompareTag("Player"))
            EventCenter.Instance.EventTrigger(EventCenter.PLAYER_HP_CHANGED, data);
        else if (CompareTag("Enemy"))
            EventCenter.Instance.EventTrigger(EventCenter.ENEMY_HP_CHANGED, data);
    }

    /// <summary>
    /// 回复HP的方法 通用的 可以靠物品或者技能恢复
    /// </summary>
    public void RestoreHP(float amount)
    {
        if (amount <= 0 || fsm.CurType == StateType.Die) return;
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        var data = new HPChangeData(CurrentHP, MaxHP);
        if (CompareTag("Player"))
            EventCenter.Instance.EventTrigger(EventCenter.PLAYER_HP_CHANGED, data);
        else if (CompareTag("Enemy"))
            EventCenter.Instance.EventTrigger(EventCenter.ENEMY_HP_CHANGED, data);
    }
}
