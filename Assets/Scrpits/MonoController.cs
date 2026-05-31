using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoController : MonoBehaviour
{

    public event UnityAction updataEvent;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (updataEvent != null)
            updataEvent.Invoke();
    }

    public void AddUpdateListener(UnityAction action)
    {
        updataEvent += action;
    }

    public void RemoveUpdateListener(UnityAction action)
    {
        updataEvent -= action;
    }
}
