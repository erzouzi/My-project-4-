using System;
using System.Collections;
using System.Collections.Generic;
using DarkPixelRPGUI.Scripts.UI.Equipment;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;


/// <summary>
/// 装备数据管理器（单例，非 MonoBehaviour）。
/// 内部用 Dictionary 存每个槽位装备了哪个 ItemData。
/// </summary>
public class EquipmentManager
{
    private static EquipmentManager instance = new EquipmentManager();
    public static EquipmentManager Instance => instance;

    //key:槽位ID（如"Weapon","Neck","Ringl")，value:装备的物品
    private Dictionary<string, ItemData> equippedItems;

    private EquipmentManager()
    {
        equippedItems = new Dictionary<string, ItemData>();
    }

    /// <summary>
    /// 给指定槽位装备物品，如果该槽位已有装备 先把旧的卸下来 回到背包。
    /// </summary>
    public void Equip(string slotId, ItemData item)
    {
        if (item == null) return;
        //如果槽位已有装备 就先把旧的卸下来 还回背包
        if (equippedItems.ContainsKey(slotId) && equippedItems[slotId] != null)
        {
            ItemData oldItem = equippedItems[slotId];
            InventoryManager.Instance.AddItem(oldItem, 1);
        }

        equippedItems[slotId] = item;
        EventCenter.Instance.EventTrigger(EventCenter.EVENT_EQUIPMENT_CHANGED,
        new EquipmentChangeData(slotId, item));

    }

    /// <summary>
    /// 卸下指定槽位的装备 归还到背包。
    /// </summary>
    /// <param name="slotId"></param>
    public void Unequip(string slotId)
    {
        if (!equippedItems.ContainsKey(slotId) || equippedItems[slotId] == null)
            return;

        ItemData item = equippedItems[slotId];
        InventoryManager.Instance.AddItem(item, 1);
        equippedItems[slotId] = null;

        //通知ui刷新
        EventCenter.Instance.EventTrigger(
            EventCenter.EVENT_EQUIPMENT_CHANGED,
            new EquipmentChangeData(slotId, null));
    }

    /// <summary>
    /// 获取指定槽位的装备 没装备返回 null
    /// </summary>
    /// <param name="slotId"></param>
    /// <returns></returns>
    public ItemData GetEquippedItem(string slotId)
    {
        equippedItems.TryGetValue(slotId, out ItemData item);
        return item;
    }

    /// <summary>
    /// 获取槽位是否有装备
    /// </summary>
    public bool IsEquipped(string slotId)
    {
        return equippedItems.ContainsKey(slotId) && equippedItems[slotId] != null;
    }

    /// <summary>
    /// 只清除装备槽数据，不归还背包（拖装备到背包时，AddItem 已先调用）。
    /// </summary>
    public void RemoveEquippedSilent(string slotId)
    {
        if (!equippedItems.ContainsKey(slotId)) return;

        equippedItems[slotId] = null;

        EventCenter.Instance.EventTrigger(
            EventCenter.EVENT_EQUIPMENT_CHANGED,
            new EquipmentChangeData(slotId, null));
    }

}
