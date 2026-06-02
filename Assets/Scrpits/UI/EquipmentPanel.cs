using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// 装备界面面板。继承 BasePanel，挂载在 EquipmentPanel 预制体根节点上。
/// </summary>
public class EquipmentPanel : BasePanel
{

    private EquipmentSlotUI[] slots;

    public override void Init()
    {
        slots = GetComponentsInChildren<EquipmentSlotUI>();

        foreach (var slot in slots)
        {
            var item = EquipmentManager.Instance.GetEquippedItem(slot.slotId);
            slot.RefreshDisplay(item);
        }
    }



    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!isShow) return;

        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            UIManager.Instance.HidePanel<EquipmentPanel>();
        }
    }
}
