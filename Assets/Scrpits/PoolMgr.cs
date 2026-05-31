using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 缓存池里的抽屉类，包含一个父物体和一个抽屉列表
/// </summary>
public class PoolData
{
    public GameObject fatherObj;
    public List<GameObject> poolList;
    public PoolData(GameObject obj, GameObject poolObj)
    {
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = poolObj.transform;
        poolList = new List<GameObject>();
        PushObj(obj);
    }

    public void PushObj(GameObject obj)
    {
        obj.SetActive(false);
        poolList.Add(obj);
        obj.transform.parent = fatherObj.transform;
    }

    public GameObject GetObj()
    {
        GameObject obj = null;
        obj = poolList[0];
        poolList.RemoveAt(0);
        obj.SetActive(true);
        obj.transform.parent = null;
        return obj;
    }

}

/// <summary>
/// 缓存池模块
/// </summary>
public class PoolMgr
{
    private static PoolMgr instance = new PoolMgr();
    public static PoolMgr Instance => instance;
    public Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();

    //池父物体
    private GameObject poolObj;

    public void GetObj(string abName, string name, UnityAction<GameObject> callBack)
    {
        //有抽屉并且抽屉里有东西
        if (poolDic.ContainsKey(name) && poolDic[name].poolList.Count > 0)
        {
            callBack(poolDic[name].GetObj());
        }
        //没有抽屉或者抽屉里没有东西
        else
        {
#if UNITY_EDITOR
            // 编辑器下走 Resources，方便开发调试
            GameObject prefab = Resources.Load<GameObject>(name);
            if (prefab != null)
            {
                GameObject obj = GameObject.Instantiate(prefab);
                obj.name = name;
                callBack(obj);
            }
            else
            {
                Debug.LogError($"PoolMgr: 编辑器下 Resources.Load 失败，路径: {name}");
            }
#else
            // 发布后走 AB 包异步加载
            AssetBundleMgr.Instance.LoadResAsync<GameObject>(abName, name, (obj) =>
            {
                obj.name = name;
                callBack(obj);
            });
#endif
        }
    }

    /// <summary>
    /// 同步获取对象。编辑器下走 Resources，发布后走 AB 同步加载。
    /// 注意：AB 同步加载会卡顿，仅在编辑器或小资源场景使用。
    /// </summary>
    public GameObject GetObjSync(string abName, string name)
    {
        // 有抽屉并且抽屉里有东西，直接拿
        if (poolDic.ContainsKey(name) && poolDic[name].poolList.Count > 0)
        {
            return poolDic[name].GetObj();
        }

#if UNITY_EDITOR
        // 编辑器下走 Resources 同步加载
        GameObject prefab = Resources.Load<GameObject>(name);
        if (prefab != null)
        {
            GameObject obj = GameObject.Instantiate(prefab);
            obj.name = name;
            return obj;
        }
        Debug.LogError($"PoolMgr: 编辑器下 Resources.Load 失败，路径: {name}");
        return null;
#else
        // 发布后走 AB 同步加载
        GameObject abObj = AssetBundleMgr.Instance.LoadRes<GameObject>(abName, name);
        if (abObj != null)
        {
            abObj.name = name;
            return abObj;
        }
        return null;
#endif
    }

    public void PushObj(string name, GameObject obj)
    {
        //池父物体不存在就创建一个
        if (poolObj == null)
            poolObj = new GameObject("Pool");

        //有抽屉就放进去，没有抽屉就创建一个抽屉再放进去
        if (poolDic.ContainsKey(name))
        {
            poolDic[name].PushObj(obj);

        }
        else
        {
            poolDic.Add(name, new PoolData(obj, poolObj));
        }

    }

    //清空池子
    public void Clear()
    {
        poolDic.Clear();
        poolObj = null;
    }


}

