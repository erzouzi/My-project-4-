using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 背包数据管理器（单例，非 MonoBehaviour）。
/// 内部用 List 存 30 个 InventorySlotData。
/// 所有写操作都发事件通知 UI。
/// </summary>
public class InventoryManager
{
    private static InventoryManager instance = new InventoryManager();
    public static InventoryManager Instance => instance;

    //背包格子数据列表
    private List<InventorySlotData> slots;
    private const int DEFAULT_CAPACITY = 30;
    public int Capacity => slots.Count;

    private InventoryManager()
    {
        //初始化30个格子
        slots = new List<InventorySlotData>(DEFAULT_CAPACITY);
        for (int i = 0; i < DEFAULT_CAPACITY; i++)
        {
            slots.Add(new InventorySlotData(null, 0));
        }
    }

    /// <summary>
    /// 往背包里添加物品。先尝试堆叠到已有的同类物品上，剩余的再放到空格子里。
    /// 返回true == 成功 false == 背包满了
    /// </summary>
    public bool AddItem(ItemData item, int count)
    {
        if (item == null || count <= 0) return false;
        int remaining = count;

        //第1步先尝试堆叠 优先堆叠到已有的同类物品上
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemData == item && slots[i].stackCount < item.maxStack)
            {
                int canAdd = item.maxStack - slots[i].stackCount;
                int toAdd = remaining <= canAdd ? remaining : canAdd;
                slots[i] = new InventorySlotData(item, slots[i].stackCount + toAdd);
                remaining -= toAdd;

                EventCenter.Instance.EventTrigger(EventCenter.EVENT_INVENTORY_CHANGED,
                new InventoryChangeData(i, item, slots[i].stackCount, InventoryChangeType.Updated));

                if (remaining <= 0) return true; //全部添加完了
            }
        }

        //第2步多余的塞进空格子里
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                int toAdd = remaining <= item.maxStack ? remaining : item.maxStack;
                slots[i] = new InventorySlotData(item, toAdd);
                remaining -= toAdd;

                EventCenter.Instance.EventTrigger(EventCenter.EVENT_INVENTORY_CHANGED,
                new InventoryChangeData(i, item, slots[i].stackCount, InventoryChangeType.Add));

                if (remaining <= 0) return true; //全部添加完了
            }
        }

        return false; //没添加完 格子满了
    }

    /// <summary>
    /// 从指定格子移除count个物品，归零就清空格子
    /// </summary>
    public void RemoveItem(int slotIndex, int count = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;
        if (slots[slotIndex].IsEmpty) return;

        int newCount = slots[slotIndex].stackCount - count;
        if (newCount <= 0)
        {
            slots[slotIndex] = new InventorySlotData(null, 0);
        }
        else
        {
            slots[slotIndex] = new InventorySlotData(slots[slotIndex].itemData, newCount);
        }

        EventCenter.Instance.EventTrigger(EventCenter.EVENT_INVENTORY_CHANGED,
            new InventoryChangeData(slotIndex, slots[slotIndex].itemData,
            slots[slotIndex].stackCount,
            slots[slotIndex].IsEmpty ? InventoryChangeType.Remove : InventoryChangeType.Updated));
    }

    /// <summary>
    /// 交换两个格子（拖拽松手时调用
    /// </summary>
    public void SwapSlots(int a, int b)
    {
        if (a < 0 || a >= slots.Count || b < 0 || b >= slots.Count || a == b) return;
        InventorySlotData temp = slots[a];
        slots[a] = slots[b];
        slots[b] = temp;

        EventCenter.Instance.EventTrigger(EventCenter.EVENT_INVENTORY_CHANGED,
            new InventoryChangeData(a, slots[a].itemData, slots[a].stackCount, InventoryChangeType.Swapped));
        EventCenter.Instance.EventTrigger(EventCenter.EVENT_INVENTORY_CHANGED,
            new InventoryChangeData(b, slots[b].itemData, slots[b].stackCount, InventoryChangeType.Swapped));
    }

    /// <summary>
    /// 使用物品 消耗品数量-1+触发事件：装备只触发事件
    /// </summary>
    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;
        if (slots[slotIndex].IsEmpty) return;

        ItemData item = slots[slotIndex].itemData;

        switch (item.itemType)
        {
            case ItemData.ItemType.Consumable:
                RemoveItem(slotIndex, 1);
                EventCenter.Instance.EventTrigger(
                    EventCenter.EVENT_ITEM_USED, new ItemUsedData(item));
                break;

            case ItemData.ItemType.Weapon:
            case ItemData.ItemType.Armor:
                EventCenter.Instance.EventTrigger(
                    EventCenter.EVENT_ITEM_EQUIPPED, new ItemEquippedData(item, slotIndex));
                break;

        }
    }

    /// <summary>
    /// 读取指定格子的数据
    /// </summary>
    public InventorySlotData GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
            return new InventorySlotData(null, 0);


        return slots[index];
    }

}
