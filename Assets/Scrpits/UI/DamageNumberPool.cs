using UnityEngine;

/// <summary>
/// 伤害数字对象池管理器。
/// 利用 PoolMgr 加载和管理 DamageNumber 实例。
/// 挂在一个持久化 GameObject 上（如 Main）。
/// </summary>
public class DamageNumberPool : MonoBehaviour
{
    [Header("加载配置")]
    [Tooltip("AB 包名（发布时使用）")]
    public string abName = "ui";
    [Tooltip("资源名（同时也是 Resources 路径和 PoolMgr key）")]
    public string resName = "DamageNumber";

    [Header("对象池")]
    [Tooltip("启动时预创建数量")]
    public int prewarmCount = 8;

    [Header("位置偏移")]
    [Tooltip("伤害数字相对于目标位置的偏移（头顶高度）")]
    public Vector3 spawnOffset = new Vector3(0f, 2f, 0f);

    [Tooltip("随机水平偏移范围，避免重叠")]
    public float randomXRange = 0.4f;
    public float randomZRange = 0.4f;

    private static DamageNumberPool instance;
    public static DamageNumberPool Instance => instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        Prewarm();
    }

    /// <summary>
    /// 预热对象池：预先创建指定数量的实例并放入池中。
    /// </summary>
    void Prewarm()
    {
        for (int i = 0; i < prewarmCount; i++)
        {
            // 同步加载一个实例
            GameObject obj = PoolMgr.Instance.GetObjSync(abName, resName);
            if (obj != null)
            {
                // 确保有 DamageNumber 组件
                if (obj.GetComponent<DamageNumber>() == null)
                    obj.AddComponent<DamageNumber>();

                // 放入池中
                PoolMgr.Instance.PushObj(resName, obj);
            }
            else
            {
                Debug.LogError($"DamageNumberPool: 预热失败！请确保 Resources/{resName}.prefab 存在，或 AB 包已构建。");
                break;
            }
        }
        Debug.Log($"DamageNumberPool: 预热完成，共 {prewarmCount} 个实例");
    }

    /// <summary>
    /// 在指定世界坐标显示伤害数字。
    /// </summary>
    public void ShowDamage(Vector3 worldPos, float damage, bool isKnockDown = false)
    {
        // 从池中获取（预热后池中有实例，此调用是同步的）
        PoolMgr.Instance.GetObj(abName, resName, (obj) =>
        {
            if (obj == null)
            {
                Debug.LogWarning("DamageNumberPool: 获取实例失败，池可能已耗尽");
                return;
            }

            // 添加随机偏移，避免多个伤害数字完全重叠
            Vector3 randomOffset = new Vector3(
                Random.Range(-randomXRange, randomXRange),
                0f,
                Random.Range(-randomZRange, randomZRange)
            );

            DamageNumber dn = obj.GetComponent<DamageNumber>();
            if (dn == null)
                dn = obj.AddComponent<DamageNumber>();

            dn.Show(worldPos + spawnOffset + randomOffset, damage, isKnockDown);
        });
    }

    /// <summary>
    /// 静态便捷方法。如果场景中没有 DamageNumberPool，会自动创建一个。
    /// </summary>
    public static void Show(Vector3 worldPos, float damage, bool isKnockDown = false)
    {
        if (Instance == null)
        {
            // 自动创建持久化 GameObject
            GameObject go = new GameObject("[Auto] DamageNumberPool");
            DontDestroyOnLoad(go);
            go.AddComponent<DamageNumberPool>();
        }

        if (Instance != null)
            Instance.ShowDamage(worldPos, damage, isKnockDown);
    }
}
