using UnityEngine;

public class AttackState : BaseState
{
    private PlayerController pc;

    public AttackState(FSMController fsm) : base(fsm)
    {
        pc = (PlayerController)fsm.unitController;
    }

    public override void OnEnter()
    {
        pc.attackInput = false;
        pc.canCombo = false;
        animator.applyRootMotion = true;

        pc.hitBox.monsterOnHit += OnHitDetected;

        switch (pc.comboIndex)
        {
            case 0:
                pc.hitBox.damage = 10f;
                pc.hitBox.knockDown = false;
                pc.hitBox.knockback = 0.5f;
                animator.CrossFade("Attack01", 0.05f);
                break;
            case 1:
                pc.hitBox.damage = 12f;
                pc.hitBox.knockDown = false;
                pc.hitBox.knockback = 0.55f;
                animator.CrossFade("Attack02", 0.05f);
                break;
            case 2:
                pc.hitBox.damage = 14f;
                pc.hitBox.knockDown = false;
                pc.hitBox.knockback = 0.6f;
                animator.CrossFade("Attack03", 0.05f);
                break;
            case 3:
                pc.hitBox.damage = 16f;
                pc.hitBox.knockDown = false;
                pc.hitBox.knockback = 0.7f;
                animator.CrossFade("Attack04", 0.05f);
                break;
            case 4:
                pc.hitBox.damage = 16f;
                pc.hitBox.knockDown = false;
                pc.hitBox.knockback = 0.7f;
                animator.CrossFade("Attack05", 0.05f);
                break;
            case 5:
                pc.hitBox.damage = 16f;
                pc.hitBox.knockDown = false;
                pc.hitBox.knockback = 0.7f;
                animator.CrossFade("Attack06", 0.05f);
                break;
            case 6:
                pc.hitBox.damage = 16f;
                pc.hitBox.knockDown = false;
                pc.hitBox.knockback = 0.8f;
                animator.CrossFade("Attack07", 0.05f);
                break;
            case 7:
                pc.hitBox.damage = 25f;
                pc.hitBox.knockDown = true;
                pc.hitBox.knockback = 1.2f;
                animator.CrossFade("Attack08", 0.05f);
                break;
            default:
                pc.comboIndex = 0;
                animator.CrossFade("Attack01", 0.05f);
                break;
        }
    }

    public override void OnAnimatorMove()
    {
        pc.ApplyRootMotion(animator.deltaPosition);
    }

    public override void OnExit()
    {
        pc.hitBox.monsterOnHit -= OnHitDetected;
        animator.applyRootMotion = false;
    }

    public override void OnUpdate()
    {
        if (pc.canCombo && pc.attackInput)
        {
            pc.attackInput = false;
            pc.canCombo = false;

            pc.comboIndex++;
            if (pc.comboIndex > 7)
            {
                pc.comboIndex = 0;
            }
            fsm.SwitchType(StateType.Attack);
            return;
        }
        if (pc.skillInput)
        {
            pc.skillInput = false;
            pc.comboIndex = 0;
            fsm.SwitchType(StateType.Skill);
            return;
        }

        if (pc.dodgeInput)
        {
            pc.dodgeInput = false;
            pc.comboIndex = 0;
            fsm.SwitchType(StateType.Dodge);
            return;
        }

        AnimatorStateInfo info =
            animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.7f && pc.setMovement > 0.1f)
        {
            pc.comboIndex = 0;
            fsm.SwitchType(StateType.Move);
        }

        if (info.normalizedTime >= 0.95f)
        {
            pc.comboIndex = 0;
            fsm.SwitchType(StateType.Idle);
        }
    }

    private void OnHitDetected()
    {
        pc.PlayHitSound();
    }
}
