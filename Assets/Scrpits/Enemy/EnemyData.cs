using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("基础属性")]
    public float maxHP = 100f;
    public float attackPower = 10f;

    [Header("索敌")]
    public float detectRange = 10f;
    public float attackRange = 2f;

    [Header("移动")]
    public float moveSpeed = 3f;
    public float strafeSpeed = 1.5f;
    public float rotateSpeed = 8f;

    [Header("冷却")]
    public float attackCD = 2f;
    public float skillCD = 5f;
    public float defenceCD = 4f;

    [Header("战斗决策间隔")]
    public float decisionMin = 1.5f;
    public float decisionMax = 3f;
}
