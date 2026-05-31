
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EnemyController : UnitController
{
    private Animator animator;
    private CharacterController controller;

    [Header("属性配置")]
    public EnemyData data;

    [Header("运行时引用")]
    public Transform player;
    public HitBox hitBox;

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip[] whiffClips;
    public AudioClip hitClip;
    //攻击索引
    [HideInInspector] public int attackComboIndex;
    public const int MaxAttackCombo = 4;

    [HideInInspector] public float lastAttackTime = -999f;
    [HideInInspector] public float lastSkillTime = -999f;
    [HideInInspector] public float lastDefenceTime = -999f;
    [HideInInspector] public float strafeDir; // -1 左, 0 停, 1 右
    [HideInInspector] public int skillType; // 1 = combo01, 2 = combo02
    [HideInInspector] public int skillIndex;

    public override float MaxHP => data.maxHP;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        CurrentHP = data.maxHP;
        fsm = new FSMController(animator, this);
        fsm.AddState(StateType.Idle, new EnemyIdleState(fsm));
        fsm.AddState(StateType.Move, new EnemyCombatState(fsm));
        fsm.AddState(StateType.Attack, new EnemyAttackState(fsm));
        fsm.AddState(StateType.Defence, new EnemyDefenceState(fsm));
        fsm.AddState(StateType.Skill, new EneymySkillState(fsm));
        fsm.AddState(StateType.Hit, new HitState(fsm));
        fsm.AddState(StateType.Die, new DeadState(fsm));
        fsm.SwitchType(StateType.Idle);
    }


    void Update()
    {
        if (fsm.CurType == StateType.Die)
            return;
        fsm.OnUpdate();
        if (player == null)
            return;

        if (fsm.CurType == StateType.Hit && isKnockDown)
        {
            return;
        }
        bool isRootMotion = fsm.CurType == StateType.Attack
                        || fsm.CurType == StateType.Hit
                        || fsm.CurType == StateType.Skill
                        || fsm.CurType == StateType.Die;

        bool facePlayer = isRootMotion || fsm.CurType == StateType.Move || fsm.CurType == StateType.Defence;

        if (facePlayer)
        {
            FacePlayer();
        }

        if (fsm.CurType == StateType.Move)
        {
            HandleCombatMove();
        }
        else if (!isRootMotion)
        {
            setMovement = 0f;
        }
    }

    private void OnAnimatorMove()
    {
        fsm.OnAnimatorMove();
    }

    private void FacePlayer()
    {
        Vector3 diff = player.position - transform.position;
        diff.y = 0;
        if (diff.sqrMagnitude < 0.001f) return;
        Quaternion target = Quaternion.LookRotation(diff);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, data.rotateSpeed * Time.deltaTime);

    }

    private void HandleCombatMove()
    {
        float dist = DistanceToPlayer();
        if (dist > data.attackRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            controller.Move(dir * data.moveSpeed * Time.deltaTime);
            setMovement = 2f;
        }
        else
        {

            // 绕圈
            Vector3 diff = player.position - transform.position;
            diff.y = 0;
            Vector3 tangent = Vector3.Cross(diff.normalized, Vector3.up) * strafeDir;
            controller.Move(tangent * data.strafeSpeed * Time.deltaTime);
            setMovement = 1f;

        }
    }

    public float DistanceToPlayer()
    {
        if (player == null)
            return 999f;
        return Vector3.Distance(transform.position, player.position);
    }
    public bool PlayerInDetectRange() => DistanceToPlayer() <= data.detectRange;
    public bool PlayerInAttackRange() => DistanceToPlayer() <= data.attackRange;


    public bool CanAttack() => Time.time - lastAttackTime >= data.attackCD;
    public bool CanSkill() => Time.time - lastSkillTime >= data.skillCD;
    public bool CanDefence() => Time.time - lastDefenceTime >= data.defenceCD;

    protected override void OnTakeDamage(float damage)
    {
        base.OnTakeDamage(damage);
        if (CurrentHP <= 0f)
        {
            fsm.SwitchType(StateType.Die);
            Destroy(gameObject, 2f);
        }
    }

    public override void ApplyRootMotion(Vector3 deltaPosition)
    {
        controller.Move(deltaPosition);
    }

    public int GetSkillMaxHits()
    {
        return skillType == 1 ? 4 : 5;
    }

    public string GetSkillAnimName(int index)
    {
        return $"combo{skillType:D2}_{index + 1}";
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
}
