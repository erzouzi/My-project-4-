using UnityEngine;

public class DodgeState : BaseState
{
    public DodgeState(FSMController fsm) : base(fsm)
    {
    }

    public override void OnEnter()
    {
        animator.applyRootMotion = true;
        animator.CrossFade("Dodge", 0.05f);
    }

    public override void OnAnimatorMove()
    {
        fsm.unitController.ApplyRootMotion(animator.deltaPosition);
    }

    public override void OnExit()
    {
        animator.applyRootMotion = false;
    }

    public override void OnUpdate()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.9f)
        {
            fsm.SwitchType(StateType.Idle);
        }
        if (info.normalizedTime >= 0.85f && fsm.unitController.setMovement >= 0.1f)
        {
            fsm.SwitchType(StateType.Move);
        }
    }
}
