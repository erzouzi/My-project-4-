using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatState : BaseState
{
    private EnemyController ec;
    public EnemyCombatState(FSMController fsm) : base(fsm)
    {
        ec = (EnemyController)fsm.unitController;
    }

    private float decisionTimer;
    private float strafeTimer;


    public override void OnEnter()
    {
        animator.CrossFade("Movement", 0.1f);
        ec.strafeDir = Random.value > 0.5f ? 1f : -1f;
        decisionTimer = Random.Range(ec.data.decisionMin, ec.data.decisionMax);
        strafeTimer = Random.Range(2f, 4f);
    }

    public override void OnUpdate()
    {
        animator.SetFloat("Movement", ec.setMovement);
        animator.SetFloat("StrafeDir", ec.strafeDir);

        //没有玩家目标 或者玩家跑出范围圈 原地待机;
        if (ec.player == null || !ec.PlayerInDetectRange())
        {
            fsm.SwitchType(StateType.Idle);
            return;
        }

        //计时器始终走，避免边界振荡导致永远触发不了决策
        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0f)
        {
            decisionTimer = Random.Range(ec.data.decisionMin, ec.data.decisionMax);
            if (ec.PlayerInAttackRange())
            {
                TryAction();
            }
        }

        //改变一次转圈方向
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeTimer = Random.Range(2f, 4f);
            ec.strafeDir = Random.value > 0.5f ? 1f : -1f;

        }
    }
    private void TryAction()
    {
        float rool = Random.value;
        if (rool < 0.4f && ec.CanAttack())
        {
            fsm.SwitchType(StateType.Attack);
            return;
        }

        if (rool < 0.65 && ec.CanSkill())
        {
            fsm.SwitchType(StateType.Skill);
            return;
        }

        if (rool < 0.85 && ec.CanDefence())
        {
            fsm.SwitchType(StateType.Defence);
            return;
        }

        //剩下15%概率是继续绕圈
    }

    public override void OnExit()
    {
        ec.strafeDir = 0f;
    }
}
