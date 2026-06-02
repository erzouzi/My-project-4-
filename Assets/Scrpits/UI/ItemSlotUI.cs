using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// 背包格子UI组件 实现拖拽和点击
/// 挂载在ItemGrid 预制体的根节点上
/// </summary>
public class ItemSlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image itemIcon;//拖入物品图标用
    public TMP_Text stackText;//堆叠数量UI组件 
    public Image backImage;//拖入悬停高亮用   

    // ---- 运行时状态 ----
    private int slotIndex;//格子索引
    private bool isEmpty;

    public int SlotIndex => slotIndex;

    // ---- 静态变量：所有格子共享一份拖拽状态 ----
    private static ItemSlotUI dragSource;//谁被拖了
    private static ItemSlotUI dragTarget;//鼠标正悬停在谁上面

    // ==================== 显示逻辑 ====================
    public void SetSlotIndex(int index) { slotIndex = index; }

    /// <summary>
    /// 根据数据刷新格子外观
    /// </summary>
    public void SetData(InventorySlotData data, int index)
    {
        slotIndex = index;
        isEmpty = data.IsEmpty;

        if (isEmpty)
        {
            if (itemIcon != null) { itemIcon.sprite = null; itemIcon.enabled = false; }
            if (stackText != null)
                stackText.text = data.stackCount > 1 ? data.stackCount.ToString() : "";
        }
        else
        {
            if (itemIcon != null) { itemIcon.sprite = data.itemData.icon; itemIcon.enabled = true; }
            if (stackText != null)
                stackText.text = data.stackCount > 1 ? data.stackCount.ToString() : "";
        }
    }

    /// <summary>
    /// 拖拽悬停时背景变亮黄色
    /// </summary>
    public void SetHighlight(bool on)
    {
        if (backImage != null)
            backImage.color = on ? new Color(1, 1, 0.6f, 0.5f) : Color.white;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isEmpty) return; //空格子不能拖

        dragSource = this;
        dragTarget = null;

        //告诉DragItemDisplay 开始显示浮动图标
        DragItemDisplay.Instance.StartDrag(slotIndex, itemIcon.sprite);

        //源格子变半透明
        if (itemIcon != null)
        {
            var c = itemIcon.color;
            c.a = 0.4f;
            itemIcon.color = c;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //啥都不做,DragItemDisplay会跟着鼠标走
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragSource == null) return;
        //恢复透明度
        if (dragSource.itemIcon != null)
        {
            var c = dragSource.itemIcon.color;
            c.a = 1f;
            dragSource.itemIcon.color = c;
        }
        //清除落点高亮
        if (dragTarget != null)
        {
            dragTarget.SetHighlight(false);
        }


        // 检查是否拖到了装备格子
        EquipmentSlotUI equipTarget = EquipmentSlotUI.HoverTarget;
        if (equipTarget != null)
        {
            var slotData = InventoryManager.Instance.GetSlot(dragSource.slotIndex);
            if (!slotData.IsEmpty && slotData.itemData.itemType == equipTarget.acceptedItemType)
            {
                EquipmentManager.Instance.Equip(equipTarget.slotId, slotData.itemData);
                InventoryManager.Instance.RemoveItem(dragSource.slotIndex, 1);
            }
        }
        else if (dragTarget != null && dragTarget != dragSource)
        {
            // 拖到另一个背包格子上 → 交换
            InventoryManager.Instance.SwapSlots(dragSource.slotIndex, dragTarget.slotIndex);
        }

        DragItemDisplay.Instance.EndDrag();
        dragSource = null;
        dragTarget = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isEmpty) return;
        InventoryManager.Instance.UseItem(slotIndex);
    }

    // ==================== 落点检测（由 DragItemDisplay 调用） ====================

    public void OnDragEnterTarget()
    {
        //取消上一次的高亮，设置新的高亮
        if (dragTarget != null && dragTarget != this)
            dragTarget.SetHighlight(false);

        dragTarget = this;
        SetHighlight(true);
    }

    public void OnDragExitTarget()
    {
        if (dragTarget == this) { SetHighlight(false); dragTarget = null; }
    }

    private void OnDestroy()
    {
        if (dragSource == this) dragSource = null;
        if (dragTarget == this) dragTarget = null;
    }
}
