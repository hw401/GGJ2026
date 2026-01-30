using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 使用 UI Image 遮罩来实现选中文本的黑色矩形遮罩效果
/// 按行拆分Range，每行一个矩形
/// </summary>
public class SelectionMaskUI : MonoBehaviour
{
    [Header("组件引用")]
    [Tooltip("要应用遮罩的 TMP_Text 组件")]
    public TMP_Text textComponent;
    
    [Header("遮罩设置")]
    [Tooltip("遮罩颜色（默认黑色）")]
    public Color maskColor = Color.black;
    
    [Tooltip("遮罩透明度")]
    [Range(0f, 1f)]
    public float maskAlpha = 0.3f;

    private RectTransform maskParent;
    private RectTransform textRectTransform;
    private Canvas canvas;
    
    // 每个Range对应的遮罩对象列表（一个Range可能对应多个遮罩，因为跨行）
    private Dictionary<Range, List<GameObject>> rangeMaskObjects = new Dictionary<Range, List<GameObject>>();
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;

        // 获取组件
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }

        // 如果没有TMP_Text，尝试从TMP_InputField获取
        if (textComponent == null)
        {
            TMP_InputField inputField = GetComponent<TMP_InputField>();
            if (inputField != null)
            {
                textComponent = inputField.textComponent;
            }
        }

        if (textComponent == null)
        {
            Debug.LogError($"SelectionMaskUI: 找不到 TMP_Text 组件！GameObject = {gameObject.name}");
            return;
        }

        textRectTransform = textComponent.rectTransform;

        // 获取 Canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        if (canvas == null)
        {
            Debug.LogError("SelectionMaskUI: 找不到 Canvas！");
            return;
        }

        // 按照思路.txt的建议：不再把黑框作为Text的子物体
        // 创建或查找 SelectionMaskContainer 作为遮罩的父对象
        GameObject containerObj = GameObject.Find("SelectionMaskContainer");
        if (containerObj == null)
        {
            // 创建新的容器
            containerObj = new GameObject("SelectionMaskContainer");
            containerObj.transform.SetParent(canvas.transform, false);
            
            // 设置容器的RectTransform，尽量和Text Area对齐
            RectTransform containerRT = containerObj.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0, 0);
            containerRT.anchorMax = new Vector2(1, 1);
            containerRT.sizeDelta = Vector2.zero;
            containerRT.anchoredPosition = Vector2.zero;
        }
        
        maskParent = containerObj.GetComponent<RectTransform>();
        if (maskParent == null)
        {
            maskParent = containerObj.AddComponent<RectTransform>();
        }
        
        isInitialized = true;
    }

    /// <summary>
    /// 创建遮罩 UI 对象
    /// </summary>
    private GameObject CreateMaskObject()
    {
        // 创建 GameObject，直接作为 textRectTransform 的子对象
        // 这样可以直接使用 TMP 的字符坐标，无需坐标转换
        GameObject maskObject = new GameObject("SelectionMask");
        maskObject.transform.SetParent(textRectTransform, false);

        // 添加 RectTransform
        RectTransform maskRectTransform = maskObject.AddComponent<RectTransform>();
        // 将遮罩的anchor和pivot设置为与文本的pivot相同，这样可以直接使用字符坐标
        Vector2 textPivot = textRectTransform.pivot;
        maskRectTransform.anchorMin = textPivot;
        maskRectTransform.anchorMax = textPivot;
        maskRectTransform.pivot = textPivot;

        // 添加 Image 组件
        Image maskImage = maskObject.AddComponent<Image>();
        Color color = maskColor;
        color.a = maskAlpha;
        maskImage.color = color;
        maskImage.raycastTarget = false; // 不阻挡鼠标事件

        // 设置层级：确保遮罩在文本之上
        maskRectTransform.SetAsLastSibling();
        
        // 添加 CanvasGroup
        CanvasGroup maskCanvasGroup = maskObject.AddComponent<CanvasGroup>();
        maskCanvasGroup.blocksRaycasts = false;
        maskCanvasGroup.ignoreParentGroups = false;

        return maskObject;
    }

    /// <summary>
    /// 计算一个Range对应的所有矩形（按行拆分）
    /// 返回每行的最小/最大坐标（相对于pivot）
    /// </summary>
    private List<(float minX, float maxX, float minY, float maxY)> CalculateRangeRects(Range range, TMP_Text tmpText)
    {
        var rects = new List<(float minX, float maxX, float minY, float maxY)>();

        if (tmpText == null || tmpText.textInfo == null) return rects;

        // 确保文本已更新
        tmpText.ForceMeshUpdate();

        int currentLine = -1;
        List<int> indicesInThisLine = new List<int>();

        for (int i = range.startIndex; i <= range.endIndex; i++)
        {
            if (i < 0 || i >= tmpText.textInfo.characterCount) continue;
            
            var charInfo = tmpText.textInfo.characterInfo[i];
            int line = charInfo.lineNumber;

            if (line != currentLine)
            {
                if (indicesInThisLine.Count > 0)
                {
                    var (minX, maxX, minY, maxY) = CalcOneLineRect(indicesInThisLine, tmpText);
                    float width = maxX - minX;
                    float height = maxY - minY;
                    if (width > 0 && height > 0)
                    {
                        rects.Add((minX, maxX, minY, maxY));
                    }
                    indicesInThisLine.Clear();
                }

                currentLine = line;
            }
            indicesInThisLine.Add(i);
        }

        if (indicesInThisLine.Count > 0)
        {
            var (minX, maxX, minY, maxY) = CalcOneLineRect(indicesInThisLine, tmpText);
            float width = maxX - minX;
            float height = maxY - minY;
            if (width > 0 && height > 0)
            {
                rects.Add((minX, maxX, minY, maxY));
            }
        }

        return rects;
    }

    /// <summary>
    /// 计算一行中指定字符索引列表的矩形
    /// 返回 (minX, maxX, minY, maxY) 相对于 pivot
    /// </summary>
    private (float minX, float maxX, float minY, float maxY) CalcOneLineRect(List<int> indices, TMP_Text tmpText)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        bool hasVisibleChar = false;

        foreach (int idx in indices)
        {
            if (idx < 0 || idx >= tmpText.textInfo.characterCount) continue;
            
            var c = tmpText.textInfo.characterInfo[idx];
            if (!c.isVisible) continue; // 跳过不可见字符

            // 使用字符的顶点坐标（这些坐标是相对于文本 RectTransform 的局部坐标）
            Vector3 topLeft = c.topLeft;
            Vector3 topRight = c.topRight;
            Vector3 bottomLeft = c.bottomLeft;
            Vector3 bottomRight = c.bottomRight;

            // 验证坐标是否有效
            if (Mathf.Abs(topLeft.x) > 10000f || Mathf.Abs(bottomRight.x) > 10000f ||
                Mathf.Abs(topLeft.y) > 10000f || Mathf.Abs(bottomRight.y) > 10000f)
            {
                continue;
            }

            if (!hasVisibleChar)
            {
                minX = Mathf.Min(topLeft.x, bottomLeft.x);
                maxX = Mathf.Max(topRight.x, bottomRight.x);
                minY = Mathf.Min(bottomLeft.y, bottomRight.y); // y 轴向下，所以 bottom 是 minY
                maxY = Mathf.Max(topLeft.y, topRight.y); // top 是 maxY
                hasVisibleChar = true;
            }
            else
            {
                minX = Mathf.Min(minX, topLeft.x, bottomLeft.x);
                maxX = Mathf.Max(maxX, topRight.x, bottomRight.x);
                minY = Mathf.Min(minY, bottomLeft.y, bottomRight.y);
                maxY = Mathf.Max(maxY, topLeft.y, topRight.y);
            }
        }

        if (!hasVisibleChar)
        {
            return (0, 0, 0, 0);
        }

        return (minX, maxX, minY, maxY);
    }

    /// <summary>
    /// 更新单个Range的遮罩显示
    /// </summary>
    private void UpdateRangeMask(Range range)
    {
        if (!isInitialized || textComponent == null)
        {
            Debug.LogWarning($"SelectionMaskUI UpdateRangeMask: 未初始化或 textComponent 为 null");
            return;
        }

        // 确保文本已更新
        textComponent.ForceMeshUpdate();
        
        if (textComponent.textInfo == null) return;

        // 按行分组字符
        Dictionary<int, List<int>> lineIndices = new Dictionary<int, List<int>>();
        for (int i = range.startIndex; i <= range.endIndex; i++)
        {
            if (i < 0 || i >= textComponent.textInfo.characterCount) continue;
            
            var charInfo = textComponent.textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;
            
            int line = charInfo.lineNumber;
            if (!lineIndices.ContainsKey(line))
            {
                lineIndices[line] = new List<int>();
            }
            lineIndices[line].Add(i);
        }

        if (lineIndices.Count == 0) return;

        // 获取或创建该Range的遮罩对象列表
        if (!rangeMaskObjects.ContainsKey(range))
        {
            rangeMaskObjects[range] = new List<GameObject>();
        }

        List<GameObject> maskObjects = rangeMaskObjects[range];

        // 确保有足够数量的遮罩对象（每行一个）
        while (maskObjects.Count < lineIndices.Count)
        {
            maskObjects.Add(CreateMaskObject());
        }

        // 更新每个遮罩对象的位置和大小（每行一个遮罩）
        int lineIndex = 0;
        foreach (var kvp in lineIndices)
        {
            int line = kvp.Key;
            List<int> indices = kvp.Value;
            
            GameObject maskObject = maskObjects[lineIndex];
            RectTransform maskRectTransform = maskObject.GetComponent<RectTransform>();
            Image maskImage = maskObject.GetComponent<Image>();

            // 直接使用 TMP 的字符坐标（已经是相对于 textRectTransform 的本地坐标）
            // 因为遮罩对象是 textRectTransform 的子对象，所以可以直接使用
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            bool hasVisibleChar = false;

            foreach (int idx in indices)
            {
                var c = textComponent.textInfo.characterInfo[idx];
                if (!c.isVisible) continue;

                // 直接使用 TMP 提供的字符顶点坐标（相对于 textRectTransform 的本地坐标）
                // topLeft, topRight, bottomLeft, bottomRight 都是相对于 textRectTransform 的本地坐标
                Vector3 topLeft = c.topLeft;
                Vector3 topRight = c.topRight;
                Vector3 bottomLeft = c.bottomLeft;
                Vector3 bottomRight = c.bottomRight;

                // 计算该行的边界框
                if (!hasVisibleChar)
                {
                    minX = Mathf.Min(topLeft.x, bottomLeft.x);
                    maxX = Mathf.Max(topRight.x, bottomRight.x);
                    minY = Mathf.Min(bottomLeft.y, bottomRight.y);  // 较小的y值（底部）
                    maxY = Mathf.Max(topLeft.y, topRight.y);      // 较大的y值（顶部）
                    hasVisibleChar = true;
                }
                else
                {
                    minX = Mathf.Min(minX, topLeft.x, bottomLeft.x);
                    maxX = Mathf.Max(maxX, topRight.x, bottomRight.x);
                    minY = Mathf.Min(minY, bottomLeft.y, bottomRight.y);
                    maxY = Mathf.Max(maxY, topLeft.y, topRight.y);
                }
            }

            if (!hasVisibleChar)
            {
                maskObject.SetActive(false);
                lineIndex++;
                continue;
            }

            // 计算遮罩位置和大小
            // 由于遮罩的anchor和pivot都与文本的pivot相同，字符坐标可以直接使用
            // 字符坐标是相对于textRectTransform的pivot的
            // 遮罩的anchoredPosition也是相对于pivot的（因为anchor=pivot）
            //
            // 但是，遮罩的pivot在中心，所以我们需要计算遮罩中心的位置
            // 遮罩应该覆盖从(minX, minY)到(maxX, maxY)的区域
            // 遮罩的中心应该在 ((minX + maxX) / 2, (minY + maxY) / 2)
            
            float width = maxX - minX;
            float height = maxY - minY;
            float centerX = (minX + maxX) / 2f;
            float centerY = (minY + maxY) / 2f;
            
            // 由于遮罩的pivot在中心，anchoredPosition就是遮罩中心的位置
            Vector2 maskPosition = new Vector2(centerX, centerY);
            Vector2 size = new Vector2(width, height);
            
            // 更新遮罩
            maskRectTransform.anchoredPosition = maskPosition;
            maskRectTransform.sizeDelta = size;
            maskRectTransform.SetAsLastSibling();
            
            Color color = maskColor;
            color.a = maskAlpha;
            maskImage.color = color;
            maskObject.SetActive(true);
            
            lineIndex++;
        }

        // 隐藏多余的遮罩对象
        for (int i = lineIndices.Count; i < maskObjects.Count; i++)
        {
            maskObjects[i].SetActive(false);
        }
    }

    /// <summary>
    /// 更新所有选择段的遮罩显示
    /// </summary>
    public void UpdateAllSelectionMasks()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (!isInitialized)
        {
            Debug.LogWarning("SelectionMaskUI UpdateAllSelectionMasks: 初始化失败");
            return;
        }
        
        if (SelectionManager.instance == null)
        {
            Debug.LogWarning("SelectionMaskUI UpdateAllSelectionMasks: SelectionManager.instance 为 null");
            return;
        }
        
        if (textComponent == null)
        {
            Debug.LogWarning("SelectionMaskUI UpdateAllSelectionMasks: textComponent 为 null");
            return;
        }

        // 获取当前文本组件的所有Range
        var ranges = SelectionManager.instance.GetRanges(textComponent);
        
        // 获取临时Range（如果有）
        Range tempRange = SelectionManager.instance.GetTempRange(textComponent);
        
        // 收集当前应该显示的Range（包括临时Range）
        HashSet<Range> activeRanges = new HashSet<Range>(ranges);
        if (tempRange != null)
        {
            activeRanges.Add(tempRange);
        }
        
        // 更新每个Range的遮罩
        foreach (var range in ranges)
        {
            UpdateRangeMask(range);
        }
        
        // 更新临时Range的遮罩（如果有）
        if (tempRange != null)
        {
            UpdateRangeMask(tempRange);
        }
        
        // 移除不再使用的Range的遮罩对象
        List<Range> rangesToRemove = new List<Range>();
        foreach (var kvp in rangeMaskObjects)
        {
            if (!activeRanges.Contains(kvp.Key))
            {
                // 销毁该Range的所有遮罩对象
                foreach (var maskObject in kvp.Value)
                {
                    if (maskObject != null)
                    {
                        Destroy(maskObject);
                    }
                }
                rangesToRemove.Add(kvp.Key);
            }
        }
        
        // 从字典中移除
        foreach (var range in rangesToRemove)
        {
            rangeMaskObjects.Remove(range);
        }
        
        // 强制更新 Canvas
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// 清除所有遮罩
    /// </summary>
    public void ClearSelectionMask()
    {
        foreach (var maskObjects in rangeMaskObjects.Values)
        {
            foreach (var maskObject in maskObjects)
            {
                if (maskObject != null)
                {
                    maskObject.SetActive(false);
                }
            }
        }
    }

    void OnDestroy()
    {
        // 清理所有遮罩对象
        foreach (var maskObjects in rangeMaskObjects.Values)
        {
            foreach (var maskObject in maskObjects)
            {
                if (maskObject != null)
                {
                    Destroy(maskObject);
                }
            }
        }
        rangeMaskObjects.Clear();
    }

    void OnDisable()
    {
        ClearSelectionMask();
    }
}
