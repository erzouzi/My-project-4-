using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private static UIManager instance = new UIManager();
    public static UIManager Instance => instance;
    //存储显示着的面板的字典 每显示一个就存入字典
    //隐藏面板时 直接获取字典中的对应面板 进行隐藏
    private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

    private Transform canvasTrans;

    // UI面板所在的AB包名，所有面板统一用一个包
    private const string UI_AB_NAME = "ui";

    private UIManager()
    {
        // Canvas 全局唯一，保留 Resources 加载
        GameObject canvas = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas"));
        canvasTrans = canvas.transform;
        //通过过场景不移除保证整个游戏过程中只有一个canvas对象
        GameObject.DontDestroyOnLoad(canvas);
    }
    //显示面板
    public T ShowPanel<T>() where T : BasePanel
    {
        //我们只需要保证 泛型T的类型 和面板预设体名字一样 就可以非常方便的使用了
        string panelName = typeof(T).Name;

        //首先要判断面板是否已经显示了 如果已经显示了 就直接返回
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName] as T;
        }

        //通过 AssetBundle 加载面板预设体
        GameObject panelObj = AssetBundleMgr.Instance.LoadRes<GameObject>(UI_AB_NAME, panelName);
        //把这个对象放到场景中的canvas下面
        panelObj.transform.SetParent(canvasTrans, false);

        //执行面板上的显示逻辑 并且保存到字典里
        T panel = panelObj.GetComponent<T>();
        panelDic.Add(panelName, panel);
        panel.ShowMe();

        return panel;
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isFade">是否要隐藏完毕后再删除</param>
    public void HidePanel<T>(bool isFade = true) where T : BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            if (isFade)
            {
                //让面板隐藏完毕后再删除
                panelDic[panelName].HideMe(() =>
                {
                    //当面板完全隐藏后才删除对象和字典中的数据
                    GameObject.Destroy(panelDic[panelName].gameObject);
                    panelDic.Remove(panelName);
                });
            }
            else
            {
                //删除对象
                GameObject.Destroy(panelDic[panelName].gameObject);
                //删除字典里面存储的面板脚本
                panelDic.Remove(panelName);
            }
        }
    }
    //获取面板
    public T GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName] as T;
        }
        else
        {
            return null;
        }
    }

}
