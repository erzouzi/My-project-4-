using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 背包里一个格子的运行时的数据
/// 纯数据结构体 ，用来存储格子里物品的状态。
/// </summary>
public struct InventorySlotData
{
    public ItemData itemData; //那种物品（null代表空格子）
    public int stackCount;   //堆叠数量（如果物品可堆叠的话）

    //是否是空格子
    public bool IsEmpty => itemData == null || stackCount <= 0;

    public InventorySlotData(ItemData data, int count)
    {
        itemData = data;
        stackCount = count;
    }

}
