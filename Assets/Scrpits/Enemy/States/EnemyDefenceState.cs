using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class EnemyDefenceState : BaseState
{
    private EnemyController ec;

    public EnemyDefenceState(FSMController fsm) : base(fsm)
    {
        ec = (EnemyController)fsm.unitController;
    }

    public override void OnEnter()
    {
        ec.lastDefenceTime = Time.time;
        animator.CrossFade("Defence", 0.1f);

    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName("Defence"))
            return;

        if (info.normalizedTime >= 0.95f)
        {
            fsm.SwitchType(StateType.Move);
        }
    }
}
