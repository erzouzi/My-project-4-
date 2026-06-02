using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BagPanel : BasePanel
{

    public ScrollRect scrollRect;
    public Transform contentParent; //Content物体

    private List<ItemSlotUI> slotUIs = new List<ItemSlotUI>();
    private bool slotsCreated;

    public override void Init()
    {
        //1.监听背包变化事件 数据变了就刷新UI
        EventCenter.Instance.AddEventListener<InventoryChangeData>(
            EventCenter.EVENT_INVENTORY_CHANGED, OnInventoryChanged);

        //2. 创建30个格子UI
        CreateSlots();
    }

    private void CreateSlots()
    {
        int capacity = InventoryManager.Instance.Capacity;
        int createdCount = 0;

        for (int i = 0; i < capacity; i++)
        {
            int slotIdx = i;//闭包捕获 防止lambda里i变最终值
            PoolMgr.Instance.GetObj("ui", "ItemGrid", (obj) =>
            {
                //把ItemGrid 实例放到Content下
                obj.transform.SetParent(contentParent, false);
                obj.SetActive(true);

                //获取或添加ItemSlotUI组件
                ItemSlotUI slotUI = obj.GetComponent<ItemSlotUI>();
                if (slotUI == null)
                {
                    //预制体上还没挂ItemSlotUI，运行时 自动加
                    slotUI = obj.AddComponent<ItemSlotUI>();

                    //自动查找子物体
                    Transform img = obj.transform.Find("ItemImage");
                    if (img != null) slotUI.itemIcon = img.GetComponent<Image>();

                    Transform txt = obj.transform.Find("Text (TMP)");
                    if (txt != null) slotUI.stackText = txt.GetComponent<TMPro.TMP_Text>();

                    Transform back = obj.transform.Find("Back");
                    if (back != null) slotUI.backImage = back.GetComponent<Image>();
                }

                slotUI.SetSlotIndex(slotIdx);
                slotUIs.Add(slotUI);

                createdCount++;
                if (createdCount >= capacity)
                {
                    // 全部创建完，按索引排序 + 刷新
                    slotUIs.Sort((a, b) => a.SlotIndex.CompareTo(b.SlotIndex));
                    slotsCreated = true;
                    RefreshAllSlots();
                }
            });
        }
    }
    /// <summary>
    /// 遍历所有格子，从 InventoryManager 读数据刷新显示。
    /// </summary>
    private void RefreshAllSlots()
    {
        if (!slotsCreated) return;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            InventorySlotData data = InventoryManager.Instance.GetSlot(i);
            slotUIs[i].SetData(data, i);
        }
    }
    private void OnInventoryChanged(InventoryChangeData data)
    {
        RefreshAllSlots();
    }

    protected override void Update()
    {
        base.Update();

        if (isShow && Keyboard.current != null
            && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            UIManager.Instance.HidePanel<BagPanel>();
        }
    }

    private void OnDestroy()
    {
        // 注销事件
        EventCenter.Instance.RemoveEventListener<InventoryChangeData>(
            EventCenter.EVENT_INVENTORY_CHANGED, OnInventoryChanged);

        // 归还所有格子到对象池
        foreach (var slotUI in slotUIs)
        {
            if (slotUI != null)
            {
                slotUI.transform.SetParent(null, false);
                PoolMgr.Instance.PushObj("ItemGrid", slotUI.gameObject);
            }
        }
        slotUIs.Clear();
    }
}
