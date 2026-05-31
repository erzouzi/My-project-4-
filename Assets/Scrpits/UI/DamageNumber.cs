using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 单个伤害数字组件。挂在一个带有 TextMeshPro 的 GameObject 上。
/// 由 DamageNumberPool 管理生命周期。
/// </summary>
public class DamageNumber : MonoBehaviour
{
    private TextMeshPro tmp;
    private Transform camTransform;
    private Vector3 startPos;
    private float elapsed;

    [Header("动画参数")]
    public float floatDistance = 1.5f;
    public float duration = 0.8f;

    [Header("普通伤害")]
    public Color normalColor = Color.white;
    public float normalFontSize = 4f;

    [Header("击倒/重击伤害")]
    public Color knockDownColor = new Color(1f, 0.5f, 0f);
    public float knockDownFontSize = 6f;

    // 默认曲线（未配置时使用）
    private static AnimationCurve _defaultAlphaCurve;
    private static AnimationCurve _defaultScaleCurve;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        if (tmp == null)
            tmp = gameObject.AddComponent<TextMeshPro>();

        // 设置 TMP 基础属性
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = normalFontSize;
        tmp.sortingOrder = 100; // 确保渲染在顶层

        if (camTransform == null)
            camTransform = Camera.main?.transform;

        InitDefaultCurves();
    }

    void InitDefaultCurves()
    {
        if (_defaultAlphaCurve == null)
        {
            // 0→1 快速显，0.3→1 保持，0.6→0 渐隐
            _defaultAlphaCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.05f, 1f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f)
            );
        }
        if (_defaultScaleCurve == null)
        {
            // 弹出：0→0.5→1.3→1.0→0.8
            _defaultScaleCurve = new AnimationCurve(
                new Keyframe(0f, 0.3f),
                new Keyframe(0.08f, 1.2f),
                new Keyframe(0.18f, 1f),
                new Keyframe(0.7f, 1f),
                new Keyframe(1f, 0.7f)
            );
        }
    }

    /// <summary>
    /// 显示伤害数字。由 DamageNumberPool 调用。
    /// </summary>
    public void Show(Vector3 worldPos, float damage, bool isKnockDown)
    {
        transform.position = worldPos;
        startPos = worldPos;
        elapsed = 0f;

        // 整数伤害
        tmp.text = Mathf.RoundToInt(damage).ToString();
        tmp.color = isKnockDown ? knockDownColor : normalColor;
        tmp.fontSize = isKnockDown ? knockDownFontSize : normalFontSize;

        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 向上浮动
            transform.position = startPos + Vector3.up * floatDistance * t;

            // 透明度
            Color c = tmp.color;
            c.a = _defaultAlphaCurve.Evaluate(t);
            tmp.color = c;

            // 缩放
            transform.localScale = Vector3.one * _defaultScaleCurve.Evaluate(t);

            // 面向摄像机
            FaceCamera();

            yield return null;
        }

        // 动画结束，回收进池
        gameObject.SetActive(false);
        PoolMgr.Instance.PushObj("DamageNumber", gameObject);
    }

    void FaceCamera()
    {
        if (camTransform != null)
        {
            // 世界空间文字始终面向摄像机
            Vector3 dir = transform.position - camTransform.position;
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
