using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public class PlayerController : UnitController
{
    [Header("移动速度")]
    public float moveSpeed = 5f;

    [Header("旋转速度")]
    public float rotateSpeed = 10f;

    [Header("主摄像机")]
    public Transform cameraTransform;
    public HitBox hitBox;

    [Header("索敌")]
    public float detectRange = 10f;

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip[] whiffClips;
    public AudioClip hitClip;

    [Header("血量")]
    public float maxHP = 200f;
    public override float MaxHP => maxHP;

    [Header("弹反")]
    public float parryWindowDuration = 0.35f;   // 弹反窗口时长（秒）
    public float parryKnockback = 3f;           // 弹反成功击飞距离
    public float parryDamage = 15f;             // 弹反成功对敌人造成的伤害
    public AudioClip parryClip;                 // 弹反成功音效


    private Animator animator;
    private Vector2 moveInput;
    private CharacterController controller;
    private float targetMovement;
    private float velocity;
    [HideInInspector]
    public int comboIndex;
    [HideInInspector]
    public bool attackInput;
    [HideInInspector]
    public bool canCombo;
    [HideInInspector]
    public bool dodgeInput;
    [HideInInspector]
    public bool defenceInput;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        CurrentHP = maxHP;
        fsm = new FSMController(animator, this);
        fsm.AddState(StateType.Idle, new IdleState(fsm));
        fsm.AddState(StateType.Move, new MoveState(fsm));
        fsm.AddState(StateType.Attack, new AttackState(fsm));
        fsm.AddState(StateType.Dodge, new DodgeState(fsm));
        fsm.AddState(StateType.Hit, new HitState(fsm));
        fsm.AddState(StateType.Defence, new DefenceState(fsm));
        fsm.AddState(StateType.Die, new DeadState(fsm));
        fsm.SwitchType(StateType.Idle);
    }

    // Update is called once per frame
    void Update()
    {

        setMovement = Mathf.SmoothDamp(
            setMovement,
            targetMovement,
            ref velocity,
            0.1f
        );


        fsm.OnUpdate();

        if (fsm.CurType == StateType.Attack || fsm.CurType == StateType.Defence)
        {
            var nearest = FindNearestEnemy();
            if (nearest != null)
                FaceTarget(nearest.transform.position);
        }

        if (fsm.CurType == StateType.Attack || fsm.CurType == StateType.Dodge ||
            fsm.CurType == StateType.Hit || fsm.CurType == StateType.Defence ||
            fsm.CurType == StateType.Die)
            return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir =
            camForward * moveInput.y +
            camRight * moveInput.x;



        if (moveDir.sqrMagnitude > 0.01f)
        {
            controller.Move(
                moveDir.normalized *
                moveSpeed *
                Time.deltaTime
            );

            Quaternion targetRot =
                Quaternion.LookRotation(moveDir);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    10f * Time.deltaTime
                );
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        targetMovement = moveInput.magnitude * 3;
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (fsm.CurType == StateType.Die) return;
        if (context.performed)
        {
            attackInput = true;
        }
    }
    public void OnDodge(InputAction.CallbackContext context)
    {
        if (fsm.CurType == StateType.Die) return;
        if (context.performed)
        {
            dodgeInput = true;
        }
    }
    public void OnDefence(InputAction.CallbackContext context)
    {
        if (fsm.CurType == StateType.Die) return;
        if (context.performed)
        {
            defenceInput = true;
        }
    }
    private void OnAnimatorMove()
    {
        fsm.OnAnimatorMove();
    }

    public override void ApplyRootMotion(Vector3 deltaPosition)
    {
        controller.Move(deltaPosition);
    }

    public override void Die()
    {
        base.Die();
    }

    public void OpenCombo()
    {
        canCombo = true;
        Debug.Log("cancombo为true");
    }

    private EnemyController FindNearestEnemy()
    {
        var enemies = FindObjectsOfType<EnemyController>();
        EnemyController nearest = null;
        float minDist = detectRange;
        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 diff = targetPos - transform.position;
        diff.y = 0;
        if (diff.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(diff);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    public void PlayRandomWhiffSound()
    {
        if (audioSource == null || whiffClips == null || whiffClips.Length == 0) return;
        int idx = Random.Range(0, whiffClips.Length);
        audioSource.PlayOneShot(whiffClips[idx]);
    }

    public void PlayHitSound()
    {
        if (audioSource == null || hitClip == null) return;
        audioSource.PlayOneShot(hitClip);
    }

    protected override void OnTakeDamage(float damage)
    {
        base.OnTakeDamage(damage);
        if (CurrentHP <= 0f)
        {
            fsm.SwitchType(StateType.Die);
            Destroy(gameObject, 2f);
        }
    }

    /// <summary>
    /// 弹反判定：在弹反窗口内受击则弹反成功，反击敌人
    /// </summary>
    protected override bool OnDefenceHit(Vector3 attackerPos, float damage)
    {
        if (fsm.blackboard.Get<bool>("isParryWindow"))
        {
            // 弹反成功，关闭窗口
            fsm.blackboard.Set("isParryWindow", false);
            fsm.blackboard.Set("parrySuccess", true);

            // 击飞敌人（knockDown=true 播放击倒动画）
            var enemy = FindNearestEnemy();
            if (enemy != null)
            {
                enemy.TakeDamage(transform.position, parryDamage, true, parryKnockback);
            }

            // 弹反成功音效
            if (audioSource != null && parryClip != null)
            {
                audioSource.PlayOneShot(parryClip);
            }

            // 弹反屏幕抖动
            CameraFollow.Trigger(0.4f, 0.35f);

            return true; // 免伤
        }

        return false; // 弹反窗口已过，正常受伤
    }
}
