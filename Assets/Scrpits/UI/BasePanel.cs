using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.Video;

public abstract class BasePanel : MonoBehaviour
{
    // Start is called before the first frame update
    private CanvasGroup canvasGroup;
    private float alphaSpeed = 10f;
    public bool isShow = false;
    private UnityAction hideCallBack = null;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    protected virtual void Start()
    {
        Init();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isShow && canvasGroup.alpha != 1)
        {
            canvasGroup.alpha += alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha >= 1)
            {
                canvasGroup.alpha = 1;
            }

        }
        else if (!isShow && canvasGroup.alpha != 0)
        {
            canvasGroup.alpha -= alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;
                hideCallBack?.Invoke();
            }
        }
    }

    public abstract void Init();

    public virtual void ShowMe()
    {
        canvasGroup.alpha = 0;
        isShow = true;
    }

    public virtual void HideMe(UnityAction callBack)
    {
        canvasGroup.alpha = 1;
        isShow = false;
        hideCallBack = callBack;
    }
}
