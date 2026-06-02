using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : BasePanel
{
    [Header("玩家血条")]
    public Slider playerHp;

    [Header("怪物血条")]
    public GameObject monsterHpRoot;    // 怪物血条的父节点（控制显隐）
    public Image monsterFakeHP;         // 缓冲条（白色/黄色，慢速跟随）
    public Image monsterTureHP;         // 真实血量（红色，瞬间跳变）
    public float bufferSpeed = 3f;      // 缓冲跟随速度，越大越快

    public Button bagBtn;
    public Button equipmentBtn;
    private EnemyController currentEnemy;
    private float enemyFindTimer;

    public override void Init()
    {
        // 玩家 HP 通过 EventCenter 监听
        EventCenter.Instance.AddEventListener<HPChangeData>(EventCenter.PLAYER_HP_CHANGED, OnPlayerHPChanged);

        // 怪物 HP 通过 EventCenter 监听（无论哪个怪物受伤都更新）
        EventCenter.Instance.AddEventListener<HPChangeData>(EventCenter.ENEMY_HP_CHANGED, OnEnemyHPChanged);

        bagBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel<BagPanel>();
        });
        equipmentBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel<EquipmentPanel>();
        });

        if (monsterHpRoot != null)
            monsterHpRoot.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

        // 追踪怪物（仅用于显隐判断）
        enemyFindTimer -= Time.deltaTime;
        if (enemyFindTimer <= 0f)
        {
            enemyFindTimer = 0.5f;
            var enemy = FindObjectOfType<EnemyController>();
            if (enemy != currentEnemy)
            {
                currentEnemy = enemy;

                if (currentEnemy != null && monsterTureHP != null && monsterFakeHP != null)
                {
                    monsterTureHP.fillAmount = currentEnemy.CurrentHP / currentEnemy.MaxHP;
                    monsterFakeHP.fillAmount = monsterTureHP.fillAmount;
                }
            }
        }

        // 怪物血条显隐
        bool showMonster = false;
        if (currentEnemy != null)
            showMonster = currentEnemy.PlayerInDetectRange();

        if (monsterHpRoot != null && monsterHpRoot.activeSelf != showMonster)
            monsterHpRoot.SetActive(showMonster);

        // 缓冲条逐帧追赶
        if (showMonster && monsterFakeHP != null && monsterTureHP != null)
        {
            monsterFakeHP.fillAmount = Mathf.Lerp(
                monsterFakeHP.fillAmount,
                monsterTureHP.fillAmount,
                bufferSpeed * Time.deltaTime
            );
        }
    }

    private void OnPlayerHPChanged(HPChangeData data)
    {
        if (playerHp != null)
        {
            playerHp.maxValue = data.max;
            playerHp.value = data.current;
        }
    }

    private void OnEnemyHPChanged(HPChangeData data)
    {
        if (monsterTureHP != null)
            monsterTureHP.fillAmount = data.current / data.max;
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<HPChangeData>(EventCenter.PLAYER_HP_CHANGED, OnPlayerHPChanged);
        EventCenter.Instance.RemoveEventListener<HPChangeData>(EventCenter.ENEMY_HP_CHANGED, OnEnemyHPChanged);
    }
}
