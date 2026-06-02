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
        // 受击僵直期间不能动
        if (Time.time < ec.stunnedUntil) return;

        if (ec.player != null && ec.PlayerInDetectRange())
            fsm.SwitchType(StateType.Move);
    }
}
