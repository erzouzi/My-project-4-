using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自定义网格布局。挂在 ScrollView 的 Content 物体上。
/// 自动把子物体排列成 N 列的网格，不用 Unity 的 GridLayoutGroup。
/// </summary>
public class GridLayoutHelper : MonoBehaviour
{
    [Header("列数")]
    public int columns = 5;

    [Header("格子大小")]
    public Vector2 cellSize = new Vector2(100, 100);

    [Header("间距")]
    public Vector2 spacing = new Vector2(10, 10);

    [Header("边距")]
    public Vector2 padding = new Vector2(10, 10);


    /// <summary>
    /// 子物体变化时自动重新排列。
    /// </summary>
    private void OnTransformChildrenChanged()
    {
        ArrangeChildren();

    }

    private void OnEnable()
    {
        ArrangeChildren();
    }

    /// <summary>
    /// 计算每个子物体的位置。
    /// </summary>
    [ContextMenu("手动排列")]
    public void ArrangeChildren()
    {
        if (columns <= 0) columns = 1;

        //只排列激活的子物体
        int visibleIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            RectTransform rt = child as RectTransform;
            if (rt == null) continue;

            //第几列 第几行
            int col = visibleIndex % columns;
            int row = visibleIndex / columns;

            //计算位置:锚点在左上角(0,1) 往下是负Y
            float x = padding.x + col * (cellSize.x + spacing.x) + cellSize.x * 0.5f;
            float y = -padding.y - row * (cellSize.y + spacing.y) - cellSize.y * 0.5f;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = cellSize;

            visibleIndex++;

        }

        if (visibleIndex > 0)
        {
            int totalRows = Mathf.CeilToInt((float)visibleIndex / columns);
            float contentHeight = padding.y * 2 + totalRows * cellSize.y + (totalRows - 1) * spacing.y;

            RectTransform contentRT = transform as RectTransform;
            if (contentRT != null)
            {
                contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, contentHeight);
            }
        }
    }

}
