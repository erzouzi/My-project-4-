using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillState : BaseState
{
    private PlayerController pc;
    public SkillState(FSMController fsm) : base(fsm)
    {
        pc = (PlayerController)fsm.unitController;
    }

    public override void OnEnter()
    {
        pc.skillInput = false;
        pc.skillHitIndex = 0;
        animator.applyRootMotion = true;
        pc.hitBox.monsterOnHit += OnHitDetected;

        // Skill1: 近战多段攻击（后续段由动画事件 NextSkillHit 推进）
        // Skill2: 远程刀光，伤害由 WeaponVFX.ShootSlash 配置
        if (pc.SkillType == 1)
        {
            pc.hitBox.damage = 18f;
            pc.hitBox.knockDown = false;
            pc.hitBox.knockback = 0.35f;
            animator.CrossFade("Skill1", 0.05f);
        }
        else if (pc.SkillType == 2)
        {
            animator.CrossFade("Skill2", 0.05f);
        }
    }

    public override void OnExit()
    {
        pc.hitBox.monsterOnHit -= OnHitDetected;
        animator.applyRootMotion = false;
    }

    public override void OnUpdate()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.85f && pc.setMovement >= 0.1f)
        {
            fsm.SwitchType(StateType.Move);
        }
        if (info.normalizedTime >= 0.95f)
        {
            fsm.SwitchType(StateType.Idle);
        }
    }

    public override void OnAnimatorMove()
    {
        pc.ApplyRootMotion(animator.deltaPosition);
    }
    private void OnHitDetected()
    {
        pc.PlayHitSound();
    }
}
