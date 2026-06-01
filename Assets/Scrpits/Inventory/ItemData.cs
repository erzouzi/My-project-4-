using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 物品模板。一种物品就是一个 .asset 文件。
/// 右键 Create → Inventory → ItemData 创建。
/// </summary
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        Consumable,//消耗品
        Weapon,    //武器
        Armor,     //防具
        Material   //材料
    }

    [Header("基础信息")]
    public string itemName; //物品名字
    public Sprite icon;    //物品图标

    [TextArea(2, 4)]
    public string description; //物品描述

    [Header("类型和堆叠")]
    public ItemType itemType; //物品类型
    public int maxStack = 99; //最大堆叠数量，<=1代表不可堆叠

    [Header("消耗品效果")]
    public float hpRestore; //回血量

    [Header("装备属性")]
    public float attackBonus; //攻击加成
    public float defenseBonus; //防御加成
}
