using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 用里氏替换原则 避免装箱拆箱
/// </summary>
public interface IEventInfo
{

}
public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;
    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

/// <summary>
/// HP 变化事件数据
/// </summary>
public struct HPChangeData
{
    public float current;
    public float max;
    public HPChangeData(float c, float m) { current = c; max = m; }
}

public enum InventoryChangeType
{
    Add,
    Remove,
    Updated,
    Swapped
}
/// 背包格子变化事件数据
public struct InventoryChangeData
{
    public int slotIndex;
    public ItemData itemData;
    public int stackCount;
    public InventoryChangeType changeType;
    public InventoryChangeData(int index, ItemData data, int count, InventoryChangeType type)
    {
        slotIndex = index;
        itemData = data;
        stackCount = count;
        changeType = type;
    }
}

/// <summary>
/// 物品被使用事件数据
/// </summary>
public struct ItemUsedData
{
    public ItemData item;
    public ItemUsedData(ItemData data)
    {
        item = data;
    }
}

/// <summary>
/// 物品被装备事件数据
/// </summary>
public struct ItemEquippedData
{
    public ItemData item;
    public int fromSlotIndex;
    public ItemEquippedData(ItemData data, int index)
    {
        item = data;
        fromSlotIndex = index;
    }
}

public struct EquipmentChangeData
{
    public string slotId; //那个部位
    public ItemData itemData; //装备的物品 null表示空
    public EquipmentChangeData(string slotId, ItemData itemData)
    {
        this.slotId = slotId;
        this.itemData = itemData;
    }

}

/// <summary>
/// 事件中心模块
/// </summary>
public class EventCenter
{
    private static EventCenter instance = new EventCenter();
    public static EventCenter Instance => instance;
    private EventCenter()
    {

    }
    public Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();
    public void AddEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions += action;

        }
        else
        {
            eventDic.Add(eventName, new EventInfo<T>(action));
        }
    }
    public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions -= action;
        }
    }
    public void EventTrigger<T>(string eventName, T info)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions?.Invoke(info);
        }
    }

    public void Clear()
    {
        eventDic.Clear();
    }

    // ---- 事件名常量 ----
    public const string PLAYER_HP_CHANGED = "PlayerHPChanged";
    public const string ENEMY_HP_CHANGED = "EnemyHPChanged";
    public const string EVENT_INVENTORY_CHANGED = "InventoryChanged";
    public const string EVENT_ITEM_USED = "ItemUsed";
    public const string EVENT_ITEM_EQUIPPED = "ItemEquipped";
    public const string EVENT_EQUIPMENT_CHANGED = "EquipmentChanged";
}

