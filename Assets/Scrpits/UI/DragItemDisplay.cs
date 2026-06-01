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
    private bool isDragging = false; //是否正在拖动
    private ItemSlotUI lastHoveredSlot; //上一帧悬停的格子

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
        if (!isDragging) return;

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

        ItemSlotUI found = null;
        foreach (var r in result)
        {
            found = r.gameObject.GetComponentInParent<ItemSlotUI>();
            if (found != null) break;
        }

        //3. 跟上一帧对比，进入/离开不同的格子
        if (found != lastHoveredSlot)
        {
            if (lastHoveredSlot != null)
                lastHoveredSlot.OnDragExitTarget();
            if (found != null)
                found.OnDragEnterTarget();
            lastHoveredSlot = found;
        }
    }

    /// <summary>
    /// 开始拖拽 显示图标 记录来源
    /// </summary>
    public void StartDrag(int slotIndex, Sprite icon)
    {
        sourceSlotIndex = slotIndex;
        isDragging = true;

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
        isDragging = false;
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
    }
}