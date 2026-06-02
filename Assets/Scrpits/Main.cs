using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.ShowPanel<GamePanel>();


        var potion = Resources.Load<ItemData>("Potion_HP");
        InventoryManager.Instance.AddItem(potion, 5);
        InventoryManager.Instance.AddItem(Resources.Load<ItemData>("Sword"), 1);

    }

    // Update is called once per frame
    void Update()
    {
        // 打开/关闭装备界面
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (UIManager.Instance.GetPanel<EquipmentPanel>() != null)
                UIManager.Instance.HidePanel<EquipmentPanel>();
            else
                UIManager.Instance.ShowPanel<EquipmentPanel>();
        }
        // 打开/关闭背包
        if (Keyboard.current != null
            && Keyboard.current.bKey.wasPressedThisFrame)
        {
            if (UIManager.Instance.GetPanel<BagPanel>() != null)
                UIManager.Instance.HidePanel<BagPanel>();
            else
                UIManager.Instance.ShowPanel<BagPanel>();
        }

    }
}
