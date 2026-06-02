using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// 拖拽时跟随鼠标的浮动图标
/// 挂载在BagPanel下的一个子物体上
/// 该物体需要Image+CanvasGroup+此脚本
/// </summary>
public class DragItemDisplay : MonoBehaviour
{
    public static DragItemDisplay Instance { get; private set; }

    public Image dragIcon; // 浮动物品图标
    public CanvasGroup canvasGroup; //用于控制透明度

    private int sourceSlotIndex; //拖动来源格子索引
    public bool IsDragging { get; private set; } //是否正在拖动
    public EquipmentSlotUI dragSourceEquipSlot;   // 拖拽来源是装备槽（非背包时用）
    private ItemSlotUI lastHoveredSlot; //上一帧悬停的格子
    public ItemSlotUI LastHoveredSlot => lastHoveredSlot;
    private EquipmentSlotUI lastHoveredEquipSlot; //上一帧悬停的装备格子
    private Transform originalParent; // 拖拽前的父级，结束时还原

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //初始状态:不可见 
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false; // 不阻挡射线，这样鼠标事件可以穿透到下面的格子
            canvasGroup.interactable = false; // 不可交互
        }
    }

    private void Update()
    {
        if (!IsDragging) return;

        //1. 图标跟随鼠标
        transform.position = Input.mousePosition;
        //2. 射线检测鼠标下方有没有ItemSlotUI
        var eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        var pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        var result = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, result);

        //检测背包格子
        ItemSlotUI foundSlot = null;
        EquipmentSlotUI foundEquipSlot = null;
        foreach (var r in result)
        {
            foundSlot = r.gameObject.GetComponentInParent<ItemSlotUI>();
            if (foundSlot != null) break;

            foundEquipSlot = r.gameObject.GetComponentInParent<EquipmentSlotUI>();
            if (foundEquipSlot != null) break;
        }

        // 背包格子：进入/离开
        if (foundSlot != lastHoveredSlot)
        {
            if (lastHoveredSlot != null)
                lastHoveredSlot.OnDragExitTarget();
            if (foundSlot != null)
                foundSlot.OnDragEnterTarget();
            lastHoveredSlot = foundSlot;
        }

        // 装备格子：重置悬停状态（OnPointerEnter/Exit 会自动处理）
        if (foundSlot == null && foundEquipSlot != lastHoveredEquipSlot)
        {
            if (lastHoveredEquipSlot != null)
                lastHoveredEquipSlot.OnDragExitTarget();
            if (foundEquipSlot != null)
                foundEquipSlot.OnDragEnterTarget();
            lastHoveredEquipSlot = foundEquipSlot;
        }
        else if (foundSlot != null && lastHoveredEquipSlot != null)
        {
            // 鼠标从装备格子移到背包格子上，清除装备格子的高亮
            lastHoveredEquipSlot.OnDragExitTarget();
            lastHoveredEquipSlot = null;
        }
    }

    /// <summary>
    /// 开始拖拽 显示图标 记录来源
    /// </summary>
    public void StartDrag(int slotIndex, Sprite icon)
    {
        sourceSlotIndex = slotIndex;
        IsDragging = true;

        // 移到根 Canvas 最顶层，防止被装备面板等 UI 遮挡
        originalParent = transform.parent;
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform);
            transform.SetAsLastSibling();
        }

        if (dragIcon != null)
        {
            dragIcon.sprite = icon;
            dragIcon.enabled = true;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.8f;
        }
    }

    /// <summary>
    /// 结束拖拽隐藏图标
    /// </summary>
    public void EndDrag()
    {
        IsDragging = false;
        sourceSlotIndex = -1;

        if (dragIcon != null)
        {
            dragIcon.sprite = null;
            dragIcon.enabled = false;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        //清理悬停状态
        if (lastHoveredSlot != null)
        {
            lastHoveredSlot.OnDragExitTarget();
            lastHoveredSlot = null;
        }

        if (lastHoveredEquipSlot != null)
        {
            lastHoveredEquipSlot.OnDragExitTarget();
            lastHoveredEquipSlot = null;
        }

        dragSourceEquipSlot = null;

        // 移回原来的父级
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            originalParent = null;
        }
    }
}