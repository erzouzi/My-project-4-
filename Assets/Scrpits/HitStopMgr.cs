using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStopMgr : MonoBehaviour
{
    public static HitStopMgr Instance;
    Coroutine currentCoroutie;
    private void Awake()
    {
        Instance = this;
    }
    public void Stop(float duration)
    {
        if (currentCoroutie != null)
        {
            StopCoroutine(currentCoroutie);
        }

        currentCoroutie = StartCoroutine(DoStop(duration));
    }
    IEnumerator DoStop(float duration, float speed = 0.1f)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = speed;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTimeScale;
        currentCoroutie = null;

    }
}
