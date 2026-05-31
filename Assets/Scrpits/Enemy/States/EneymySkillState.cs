using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EneymySkillState : BaseState
{
    private EnemyController ec;
    private int maxHits;
    private string currentAnim;
    public EneymySkillState(FSMController fsm) : base(fsm)
    {
        ec = (EnemyController)fsm.unitController;
    }

    public override void OnEnter()
    {
        ec.skillType = Random.value > 0.5f ? 1 : 2;
        ec.skillIndex = 0;
        ec.lastSkillTime = Time.time;
        ec.hitBox.playerOnHit += OnHitDetected;
        maxHits = ec.GetSkillMaxHits();
        animator.applyRootMotion = true;

        ApplySkillHitBoxConfig();
        currentAnim = ec.GetSkillAnimName(0);
        animator.CrossFade(currentAnim, 0.05f);
    }

    private void ApplySkillHitBoxConfig()
    {
        if (ec.hitBox == null) return;

        if (ec.skillType == 1)
        {
            ec.hitBox.damage = 18f;
            ec.hitBox.knockDown = false;
            ec.hitBox.knockback = 0.3f;
        }
        else
        {
            ec.hitBox.damage = 22f;
            ec.hitBox.knockDown = ec.skillIndex >= maxHits - 1;
            ec.hitBox.knockback = 0.5f;
        }
    }

    public override void OnExit()
    {
        animator.applyRootMotion = false;
        ec.hitBox.playerOnHit -= OnHitDetected;
    }

    public override void OnAnimatorMove()
    {
        ec.ApplyRootMotion(animator.deltaPosition);
    }
    public override void OnUpdate()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(currentAnim))
            return;

        if (info.normalizedTime > 0.8f)
        {
            ec.skillIndex++;
            if (ec.skillIndex >= maxHits)
            {
                fsm.SwitchType(StateType.Move);
                return;
            }
            ApplySkillHitBoxConfig();
            currentAnim = ec.GetSkillAnimName(ec.skillIndex);
            animator.CrossFade(currentAnim, 0.05f);
        }

        // 最后一段播完也回
        if (ec.skillIndex >= maxHits - 1 && info.normalizedTime >= 0.95f)
        {
            fsm.SwitchType(StateType.Move);
        }
    }


    private void OnHitDetected()
    {
        ec.PlayHitSound();
    }
}
