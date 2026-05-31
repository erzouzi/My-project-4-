using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

public class MonoMgr
{
    private static MonoMgr instance = new MonoMgr();
    public static MonoMgr Instance => instance;
    public MonoController monoController;

    public MonoMgr()
    {
        GameObject obj = new GameObject("MonoController");
        monoController = obj.AddComponent<MonoController>();
    }
    public void AddUpdateListener(UnityAction action)
    {
        monoController.AddUpdateListener(action);
    }


    public void RemoveUpdateListener(UnityAction action)
    {
        monoController.RemoveUpdateListener(action);
    }

    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return monoController.StartCoroutine(routine);
    }

    public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
    {
        return monoController.StartCoroutine(methodName, value);
    }

    public void StopCoroutine(IEnumerator routine)
    {
        monoController.StopCoroutine(routine);
    }

    public void StopCoroutine(string methodName)
    {
        monoController.StopCoroutine(methodName);
    }

    public void StopCoroutine(Coroutine routine)
    {
        monoController.StopCoroutine(routine);
    }
}
