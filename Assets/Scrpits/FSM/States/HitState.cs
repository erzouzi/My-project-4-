using UnityEngine;

public class HitState : BaseState
{
    private Vector3 knockVelocity;
    private float knockDecay = 10f;
    private bool useRootMotion;     // 击倒动画用帧位移，普通受击用手动击退

    public HitState(FSMController fsm) : base(fsm)
    {
    }

    public override void OnEnter()
    {
        UnitController unit = fsm.unitController;

        Vector3 dir = unit.hitSourcePos - unit.transform.position;
        dir.y = 0;

        string animName;
        if (unit.isKnockDown)
        {
            animName = "KnockDown";
            // 击倒动画自带位移，走 root motion
            useRootMotion = true;
            animator.applyRootMotion = true;
            knockVelocity = Vector3.zero;
        }
        else
        {
            useRootMotion = false;
            knockDecay = 10f;
            Vector3 localDir = unit.transform.InverseTransformDirection(dir.normalized);

            if (localDir.z > 0.5f)
                animName = "Hit_Back";
            else if (localDir.z < -0.5f)
                animName = "Hit_Front";
            else if (localDir.x > 0)
                animName = "Hit_Right";
            else
                animName = "Hit_Left";

            // 普通受击：初速度逐帧衰减（消除瞬移感）
            Vector3 knockDir = -dir.normalized;
            knockVelocity = knockDir * unit.knockbackDistance * knockDecay;
        }

        animator.Play(animName);
    }

    public override void OnUpdate()
    {
        // 普通受击：逐帧衰减击退
        if (!useRootMotion)
        {
            var cc = fsm.unitController.GetComponent<CharacterController>();
            if (cc != null && knockVelocity.sqrMagnitude > 0.001f)
            {
                cc.Move(knockVelocity * Time.deltaTime);
                knockVelocity *= 1f - knockDecay * Time.deltaTime;
            }
        }

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            fsm.SwitchType(StateType.Idle);
        }
    }

    public override void OnAnimatorMove()
    {
        if (useRootMotion)
        {
            fsm.unitController.ApplyRootMotion(animator.deltaPosition);
        }
    }

    public override void OnExit()
    {
        fsm.unitController.isKnockDown = false;
        knockVelocity = Vector3.zero;
        if (useRootMotion)
        {
            animator.applyRootMotion = false;
            useRootMotion = false;
        }
    }
}
