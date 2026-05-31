using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AssetBundleMgr
{
    private static AssetBundleMgr instance = new AssetBundleMgr();
    public static AssetBundleMgr Instance => instance;
    //存储加载的AB包 ，key是AB包的名字，value是AB包对象，因为AB包重复加载会报错
    Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();
    //主包
    private AssetBundle mainAB = null;
    //依赖包获取用的配置文件
    private AssetBundleManifest mainfest = null;

    /// <summary>
    /// AB包存放路径 方便以后修改
    /// </summary>
    private string PathUrl
    {
        get
        {
            return Application.streamingAssetsPath + "/";
        }
    }
    /// <summary>
    /// 主包名方便修改
    /// </summary>
    private string MainABName
    {
        get
        {
#if UNITY_IOS
          return "IOS";
#elif UNITY_ANDROID
          return "Android";
#else
            return "PC";
#endif
        }
    }

    //同步加载 不指定类型
    public UnityEngine.Object LoadRes(string abName, string resName)
    {
        //加载AB包
        LoadAB(abName);
        //加载资源
        UnityEngine.Object obj = abDic[abName].LoadAsset(resName);
        if (obj is GameObject)
        {
            return GameObject.Instantiate(obj);
        }
        else
            return obj;

    }

    //同步加载 指定type类型
    public UnityEngine.Object LoadRes(string abName, string resName, System.Type type)
    {
        //加载AB包
        LoadAB(abName);
        //加载资源
        UnityEngine.Object obj = abDic[abName].LoadAsset(resName, type);
        if (obj is GameObject)
        {
            return GameObject.Instantiate(obj);
        }
        else
            return obj;


    }

    //同步加载 根据泛型指定类型
    public T LoadRes<T>(string abName, string resName) where T : UnityEngine.Object
    {
        //加载AB包
        LoadAB(abName);
        //加载资源
        T obj = abDic[abName].LoadAsset<T>(resName);
        if (obj is GameObject)
        {
            return GameObject.Instantiate(obj);
        }
        else
            return obj;

    }

    //异步加载 这里的异步加载AB包并没有使用异步加载 只是AB包中加载资源使用异步

    //根据名字异步加载资源
    public void LoadResAsync(string abName, string resName, UnityAction<UnityEngine.Object> callBack)
    {
        MonoMgr.Instance.StartCoroutine(ReallyLoadResAsync(abName, resName, callBack));
    }
    private IEnumerator ReallyLoadResAsync(string abName, string resName, UnityAction<UnityEngine.Object> callBack)
    {
        //加载AB包
        LoadAB(abName);
        //加载资源
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(resName);
        yield return abr;
        //异步加载结束 通过委托传递给外部
        if (abr.asset is GameObject)
        {
            callBack(GameObject.Instantiate(abr.asset));
        }
        else
            callBack(abr.asset);
    }

    //根据Type异步加载资源
    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<UnityEngine.Object> callBack)
    {
        MonoMgr.Instance.StartCoroutine(ReallyLoadResAsync(abName, resName, type, callBack));
    }
    private IEnumerator ReallyLoadResAsync(string abName, string resName, System.Type type, UnityAction<UnityEngine.Object> callBack)
    {
        //加载AB包
        LoadAB(abName);
        //加载资源
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(resName, type);
        yield return abr;
        //异步加载结束 通过委托传递给外部
        if (abr.asset is GameObject)
        {
            callBack(GameObject.Instantiate(abr.asset));
        }
        else
            callBack(abr.asset);
    }

    //根据泛型异步加载资源
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callBack) where T : UnityEngine.Object
    {
        MonoMgr.Instance.StartCoroutine(ReallyLoadResAsync<T>(abName, resName, callBack));
    }
    private IEnumerator ReallyLoadResAsync<T>(string abName, string resName, UnityAction<T> callBack) where T : UnityEngine.Object
    {
        //加载AB包
        LoadAB(abName);
        //加载资源
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync<T>(resName);
        yield return abr;
        //异步加载结束 通过委托传递给外部
        if (abr.asset is GameObject)
        {
            callBack(GameObject.Instantiate(abr.asset) as T);
        }
        else
            callBack(abr.asset as T);
    }

    //加载指定AB包
    public void LoadAB(string abName)
    {
        //加载主包
        if (mainAB == null)
        {
            mainAB = AssetBundle.LoadFromFile(PathUrl + MainABName);
            mainfest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        //获取依赖包信息
        AssetBundle ab;
        string[] strs = mainfest.GetAllDependencies(abName);
        for (int i = 0; i < strs.Length; i++)
        {
            if (!abDic.ContainsKey(strs[i]))
            {
                ab = AssetBundle.LoadFromFile(PathUrl + strs[i]);
                abDic.Add(strs[i], ab);
            }
        }

        //加载要用的AB包
        if (!abDic.ContainsKey(abName))
        {
            ab = AssetBundle.LoadFromFile(PathUrl + abName);
            abDic.Add(abName, ab);
        }
    }

    //卸载指定AB包 默认false不卸载这个AB包加载的资源
    public void UnLoad(string abName, bool boolRes = false)
    {
        if (abDic.ContainsKey(abName))
        {
            abDic[abName].Unload(boolRes);
            abDic.Remove(abName);
        }
    }

    //卸载所有AB包 默认false 不卸载所有AB包加载的资源
    public void ClearAB(bool boolRes = false)
    {
        AssetBundle.UnloadAllAssetBundles(boolRes);
        abDic.Clear();
        mainAB = null;
        mainfest = null;
    }



}
