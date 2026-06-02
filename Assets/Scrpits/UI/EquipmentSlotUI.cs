using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// 装备槽位的UI组件 挂在每个装备格子GameObject 上。
/// 负责： 显示空/装备状态 + 接收拖拽悬停检测
/// </summary>
public class EquipmentSlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [Header("槽位配置")]
    public string slotId;  //槽位唯一ID，如 "Weapon", "Neck"
    public ItemData.ItemType acceptedItemType; //接受的物品类型

    [Header("子物体引用")]
    public Image defaultImage; //空槽时显示的默认图标
    public Image slotBackground; //槽位背景（始终显示）


    //-----动态创建的装备图标-----
    private GameObject itemImageObj;
    private Image itemImage;

    //----静态：当前被拖拽悬停的装备槽----
    public static EquipmentSlotUI HoverTarget { get; private set; }

    private void Start()
    {
        //根据当前存档数据刷新显示
        ItemData equipped = EquipmentManager.Instance.GetEquippedItem(slotId);
        RefreshDisplay(equipped);

        //监听装备变化事件
        EventCenter.Instance.AddEventListener<EquipmentChangeData>(
            EventCenter.EVENT_EQUIPMENT_CHANGED, OnEquipmentChanged);
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<EquipmentChangeData>(
            EventCenter.EVENT_EQUIPMENT_CHANGED, OnEquipmentChanged);

    }


    /// <summary>
    /// 装备变化回调--只处理自己槽位的变化
    /// </summary>
    /// <param name="data"></param>
    private void OnEquipmentChanged(EquipmentChangeData data)
    {
        if (data.slotId == slotId)
            RefreshDisplay(data.itemData);
    }

    public void RefreshDisplay(ItemData item)
    {
        if (item != null)
        {
            //隐藏默认图
            if (defaultImage != null) defaultImage.enabled = false;
            //没有itemImage 子物体就创建一个
            if (itemImageObj == null)
            {
                itemImageObj = new GameObject("itemImage", typeof(RectTransform), typeof(Image));
                itemImageObj.transform.SetParent(transform, false);
                itemImage = itemImageObj.GetComponent<Image>();

                // RectTransform 填满父级
                RectTransform rt = itemImageObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(10, 10);
                rt.offsetMax = new Vector2(-10, -10);
                rt.localScale = Vector3.one;
            }
            itemImage.sprite = item.icon;
            itemImageObj.SetActive(true);
        }
        else
        {
            //显示默认图标
            if (defaultImage != null) defaultImage.enabled = true;
            if (itemImageObj != null) itemImageObj.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DragItemDisplay.Instance != null && DragItemDisplay.Instance.IsDragging)
        {
            OnDragEnterTarget();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (DragItemDisplay.Instance != null && DragItemDisplay.Instance.IsDragging)
        {
            OnDragExitTarget();
        }
    }

    public void OnDragEnterTarget()
    {
        HoverTarget = this;
        HighlightSlot(true);
    }
    public void OnDragExitTarget()
    {
        if (HoverTarget == this)
            HoverTarget = null;
        HighlightSlot(false);
    }
    /// <summary>
    /// 悬停时高亮槽位背景
    /// </summary>
    /// <param name="on"></param>
    public void HighlightSlot(bool on)
    {
        if (slotBackground != null)
        {
            slotBackground.color = on ? new Color(1, 1, 0.6f, 0.5f) : Color.white;
        }
    }

    // ==================== 装备槽拖出 ====================

    public void OnBeginDrag(PointerEventData eventData)
    {
        ItemData equipped = EquipmentManager.Instance.GetEquippedItem(slotId);
        if (equipped == null) return;

        DragItemDisplay.Instance.dragSourceEquipSlot = this;
        DragItemDisplay.Instance.StartDrag(-1, equipped.icon);

        // 临时隐藏装备显示
        if (itemImageObj != null) itemImageObj.SetActive(false);
        if (defaultImage != null) defaultImage.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // DragItemDisplay 负责跟随鼠标，这里什么都不用做
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragItemDisplay.Instance.dragSourceEquipSlot != this)
            return;

        // 检查是否落在背包格子上
        ItemSlotUI targetSlot = DragItemDisplay.Instance.LastHoveredSlot;

        if (targetSlot != null)
        {
            ItemData equipped = EquipmentManager.Instance.GetEquippedItem(slotId);
            if (equipped != null)
            {
                // 先尝试加入背包，成功再卸下
                bool added = InventoryManager.Instance.AddItem(equipped, 1);
                if (added)
                {
                    // AddItem 成功了，手动清除装备（不用 Unequip，否则会重复 AddItem）
                    EquipmentManager.Instance.RemoveEquippedSilent(slotId);
                }
            }
        }

        // 恢复显示
        ItemData current = EquipmentManager.Instance.GetEquippedItem(slotId);
        RefreshDisplay(current);

        DragItemDisplay.Instance.EndDrag();
    }
}
