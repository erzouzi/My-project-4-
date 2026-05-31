using UnityEngine;

public class IdleState : BaseState
{
    private PlayerController pc;

    public IdleState(FSMController fsm) : base(fsm)
    {
        pc = (PlayerController)fsm.unitController;
    }

    public override void OnEnter()
    {
        animator.CrossFadeInFixedTime("Idle", 0.1f);
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
        if (pc.attackInput)
        {
            pc.attackInput = false;
            fsm.SwitchType(StateType.Attack);
            return;
        }

        if (pc.dodgeInput)
        {
            pc.dodgeInput = false;
            fsm.SwitchType(StateType.Dodge);
            return;
        }

        if (pc.defenceInput)
        {
            fsm.SwitchType(StateType.Defence);
            return;
        }

        if (pc.setMovement >= 0.5)
        {
            fsm.SwitchType(StateType.Move);
        }
    }
}
