using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : BaseState
{
    private EnemyController ec;
    public EnemyIdleState(FSMController fsm) : base(fsm)
    {
        ec = (EnemyController)fsm.unitController;
    }

    public override void OnEnter()
    {
        animator.CrossFade("Idle", 0.2f);
    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {
        if (ec.player != null && ec.PlayerInDetectRange())
            fsm.SwitchType(StateType.Move);
    }
}
