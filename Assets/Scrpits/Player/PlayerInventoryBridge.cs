using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物品效果桥接器。监听"物品被使用/装备"事件，应用到 PlayerController。
/// 挂载在 Player 的 GameObject 上。
/// </summary>
public class PlayerInventoryBridge : MonoBehaviour
{
    private PlayerController pc;

    private void Start()
    {
        pc = GetComponent<PlayerController>();

        // 监听两个事件
        EventCenter.Instance.AddEventListener<ItemUsedData>(
            EventCenter.EVENT_ITEM_USED, OnItemUsed);
        EventCenter.Instance.AddEventListener<ItemEquippedData>(
            EventCenter.EVENT_ITEM_EQUIPPED, OnItemEquipped);
    }

    /// <summary>
    /// 消耗品被使用了 → 回血
    /// </summary>
    private void OnItemUsed(ItemUsedData data)
    {
        if (pc == null) return;

        ItemData item = data.item;

        // 回血
        if (item.hpRestore > 0)
        {
            pc.RestoreHP(item.hpRestore);
        }

        // 后续可以加其他消耗品效果：buff、加速等
    }

    /// <summary>
    /// 装备被穿上了 → 加属性（后续扩展）
    /// </summary>
    private void OnItemEquipped(ItemEquippedData data)
    {
        Debug.Log($"装备了 {data.item.itemName}");
        // TODO: 后续实现装备系统时，在此处加攻防属性
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<ItemUsedData>(
            EventCenter.EVENT_ITEM_USED, OnItemUsed);
        EventCenter.Instance.RemoveEventListener<ItemEquippedData>(
            EventCenter.EVENT_ITEM_EQUIPPED, OnItemEquipped);
    }
}