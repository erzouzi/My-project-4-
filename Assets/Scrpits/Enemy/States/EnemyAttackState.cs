using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EnemyAttackState : BaseState
{
    private EnemyController ec;
    private string currentAnim;
    public EnemyAttackState(FSMController fsm) : base(fsm)
    {
        ec = (EnemyController)fsm.unitController;
    }

    public override void OnEnter()
    {
        ec.attackComboIndex = 0;
        ec.hitBox.playerOnHit += OnHitDetected;
        animator.applyRootMotion = true;
        ec.lastAttackTime = Time.time;


        ApplyHitBoxConfig(ec.attackComboIndex);
        currentAnim = "Attack01";
        animator.CrossFade(currentAnim, 0.05f);
    }

    private void ApplyHitBoxConfig(int comboIndex)
    {
        if (ec.hitBox == null) return;

        switch (comboIndex)
        {
            case 0:
                ec.hitBox.damage = 10f;
                ec.hitBox.knockDown = false;
                ec.hitBox.knockback = 0.2f;
                break;
            case 1:
                ec.hitBox.damage = 12f;
                ec.hitBox.knockDown = false;
                ec.hitBox.knockback = 0.25f;
                break;
            case 2:
                ec.hitBox.damage = 14f;
                ec.hitBox.knockDown = false;
                ec.hitBox.knockback = 0.3f;
                break;
            case 3:
                ec.hitBox.damage = 20f;
                ec.hitBox.knockDown = true;
                ec.hitBox.knockback = 0.5f;
                break;
        }
    }

    public override void OnExit()
    {
        animator.applyRootMotion = false;
        ec.hitBox.playerOnHit -= OnHitDetected;
    }

    public override void OnUpdate()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(currentAnim))
            return;

        if (info.normalizedTime >= 0.8f)
        {
            ec.attackComboIndex++;
            if (ec.attackComboIndex >= EnemyController.MaxAttackCombo)
            {
                fsm.SwitchType(StateType.Move);
                return;
            }
            ApplyHitBoxConfig(ec.attackComboIndex);
            currentAnim = $"Attack{ec.attackComboIndex + 1:D2}";
            animator.CrossFade(currentAnim, 0.05f);
        }

        if (ec.attackComboIndex >= EnemyController.MaxAttackCombo - 1 && info.normalizedTime >= 0.95f)
        {
            fsm.SwitchType(StateType.Move);
        }
    }
    public override void OnAnimatorMove()
    {
        ec.ApplyRootMotion(animator.deltaPosition);
    }
    private void OnHitDetected()
    {
        ec.PlayHitSound();
    }
}
