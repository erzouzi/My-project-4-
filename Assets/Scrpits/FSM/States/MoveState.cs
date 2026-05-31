using UnityEngine;

public class MoveState : BaseState
{
    private PlayerController pc;

    public MoveState(FSMController fsm) : base(fsm)
    {
        pc = (PlayerController)fsm.unitController;
    }

    public override void OnEnter()
    {
        animator.CrossFade("Movement", 0.1f);
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

        animator.SetFloat("Movement", pc.setMovement);
        if (pc.setMovement < 0.1f)
        {
            fsm.SwitchType(StateType.Idle);
        }
    }
}
