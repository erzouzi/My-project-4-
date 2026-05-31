using UnityEngine;

public class DefenceState : BaseState
{
    private PlayerController pc;
    private float enterTime;
    private bool parried;

    public DefenceState(FSMController fsm) : base(fsm)
    {
        pc = (PlayerController)fsm.unitController;
    }

    public override void OnEnter()
    {
        pc.defenceInput = false;
        enterTime = Time.time;
        parried = false;

        // 开启弹反窗口
        bb.Set("isParryWindow", true);
        bb.Set("parrySuccess", false);

        animator.CrossFade("Defence", 0.05f);
    }

    public override void OnUpdate()
    {
        // 弹反窗口超时关闭
        if (!parried && Time.time - enterTime > pc.parryWindowDuration)
        {
            bb.Set("isParryWindow", false);
        }

        // 弹反成功（由 PlayerController.OnDefenceHit 设置黑板标记）
        if (!parried && bb.Get<bool>("parrySuccess"))
        {
            parried = true;
            // 可在此播放弹反成功动画，如：
            animator.CrossFade("ParrySuccess", 0.05f);
        }

        // 闪避打断（弹反成功后仍可闪避取消后摇）
        if (pc.dodgeInput)
        {
            pc.dodgeInput = false;
            bb.Set("isParryWindow", false);
            fsm.SwitchType(StateType.Dodge);
            return;
        }

        // 动画播完回 Idle
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            fsm.SwitchType(StateType.Idle);
        }
    }

    public override void OnExit()
    {
        bb.Set("isParryWindow", false);
        bb.Set("parrySuccess", false);
    }
}
