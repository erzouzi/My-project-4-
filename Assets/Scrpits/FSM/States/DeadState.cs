using UnityEngine;

public class DeadState : BaseState
{
    public DeadState(FSMController fsm) : base(fsm)
    {
    }

    public override void OnEnter()
    {
        animator.CrossFade("Dead", 0.1f);

    }

    public override void OnUpdate()
    {
        // 死亡是终态，不做任何转换
    }

    public override void OnExit()
    {
    }
}
