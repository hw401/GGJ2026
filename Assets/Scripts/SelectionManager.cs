using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 文本选择管理器，统一管理所有 Range
/// </summary>
public class SelectionManager : Singleton<SelectionManager>
{
    // 每个文本组件对应一个Range列表
    private Dictionary<TMP_Text, List<Range>> textRanges = new Dictionary<TMP_Text, List<Range>>();
    
    // 临时Range（用于拖拽时实时显示）
    private Dictionary<TMP_Text, Range> tempRanges = new Dictionary<TMP_Text, Range>();

    /// <summary>
    /// 获取指定文本组件的所有Range
    /// </summary>
    public List<Range> GetRanges(TMP_Text textComponent)
    {
        if (textComponent == null) return new List<Range>();
        
        if (!textRanges.ContainsKey(textComponent))
        {
            textRanges[textComponent] = new List<Range>();
        }
        
        return textRanges[textComponent];
    }

    /// <summary>
    /// 获取所有文本组件的所有Range
    /// </summary>
    public Dictionary<TMP_Text, List<Range>> GetAllRanges()
    {
        return textRanges;
    }

    /// <summary>
    /// 添加一个选区，如果与已有选区重叠或相邻则自动合并
    /// </summary>
    public void AddSelection(TMP_Text textComponent, int startIndex, int endIndex, bool autoMerge = true)
    {
        if (textComponent == null) return;

        // 确保索引有效
        int textLength = textComponent.textInfo?.characterCount ?? textComponent.text?.Length ?? 0;
        startIndex = Mathf.Clamp(startIndex, 0, textLength - 1);
        endIndex = Mathf.Clamp(endIndex, 0, textLength - 1);

        if (startIndex > endIndex)
        {
            int t = startIndex;
            startIndex = endIndex;
            endIndex = t;
        }

        Range newRange = new Range(startIndex, endIndex);

        if (!textRanges.ContainsKey(textComponent))
        {
            textRanges[textComponent] = new List<Range>();
        }

        var ranges = textRanges[textComponent];

        if (autoMerge)
        {
            // 查找重叠或相邻的Range并合并
            List<Range> toMerge = new List<Range>();
            for (int i = ranges.Count - 1; i >= 0; i--)
            {
                if (newRange.Overlaps(ranges[i]) || newRange.IsAdjacent(ranges[i]))
                {
                    toMerge.Add(ranges[i]);
                    ranges.RemoveAt(i);
                }
            }

            // 合并所有重叠或相邻的Range
            foreach (var range in toMerge)
            {
                newRange.Merge(range);
            }
        }

        ranges.Add(newRange);
        Debug.Log($"SelectionManager: 添加选区 - 文本:{textComponent.name}, 范围:[{newRange.startIndex}, {newRange.endIndex}]");

        // 通知UI更新
        RefreshUI(textComponent);
    }

    /// <summary>
    /// 移除指定字符索引所在的选区
    /// </summary>
    public void RemoveSelectionAt(TMP_Text textComponent, int charIndex)
    {
        if (textComponent == null || !textRanges.ContainsKey(textComponent))
            return;

        var ranges = textRanges[textComponent];
        List<Range> newRanges = new List<Range>();

        foreach (var range in ranges)
        {
            if (range.Contains(charIndex))
            {
                // 从Range中移除该字符，可能拆分成多个Range
                var splitRanges = range.RemoveChar(charIndex);
                newRanges.AddRange(splitRanges);
            }
            else
            {
                // 不包含该字符，保留原Range
                newRanges.Add(range);
            }
        }

        textRanges[textComponent] = newRanges;
        Debug.Log($"SelectionManager: 移除字符 {charIndex} 所在的选区");

        // 通知UI更新
        RefreshUI(textComponent);
    }

    /// <summary>
    /// 移除指定范围的选区
    /// </summary>
    public void RemoveSelection(TMP_Text textComponent, int startIndex, int endIndex)
    {
        if (textComponent == null || !textRanges.ContainsKey(textComponent))
            return;

        var ranges = textRanges[textComponent];
        List<Range> newRanges = new List<Range>();

        foreach (var range in ranges)
        {
            if (range.Overlaps(new Range(startIndex, endIndex)))
            {
                // 有重叠，需要拆分
                if (startIndex > range.startIndex)
                {
                    newRanges.Add(new Range(range.startIndex, startIndex - 1));
                }
                if (endIndex < range.endIndex)
                {
                    newRanges.Add(new Range(endIndex + 1, range.endIndex));
                }
            }
            else
            {
                // 无重叠，保留
                newRanges.Add(range);
            }
        }

        textRanges[textComponent] = newRanges;

        // 通知UI更新
        RefreshUI(textComponent);
    }

    /// <summary>
    /// 清除指定文本组件的所有选区
    /// </summary>
    public void ClearAllSelections(TMP_Text textComponent)
    {
        if (textComponent == null) return;

        if (textRanges.ContainsKey(textComponent))
        {
            textRanges[textComponent].Clear();
        }

        // 通知UI更新
        RefreshUI(textComponent);
    }

    /// <summary>
    /// 清除所有文本组件的所有选区
    /// </summary>
    public void ClearAllSelections()
    {
        textRanges.Clear();

        // 通知所有UI更新
        foreach (var kvp in textRanges)
        {
            RefreshUI(kvp.Key);
        }
    }

    /// <summary>
    /// 获取指定文本组件中所有选中的文本
    /// </summary>
    public string GetSelectedText(TMP_Text textComponent)
    {
        if (textComponent == null) return string.Empty;

        var ranges = GetRanges(textComponent);
        if (ranges.Count == 0) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        string text = textComponent.text;

        foreach (var range in ranges)
        {
            if (range.startIndex < text.Length && range.endIndex < text.Length)
            {
                int length = range.endIndex - range.startIndex + 1;
                sb.Append(text.Substring(range.startIndex, length));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 获取指定文本组件中所有选中的字符索引列表
    /// </summary>
    public List<int> GetSelectedIndices(TMP_Text textComponent)
    {
        var indices = new List<int>();
        var ranges = GetRanges(textComponent);

        foreach (var range in ranges)
        {
            for (int i = range.startIndex; i <= range.endIndex; i++)
            {
                indices.Add(i);
            }
        }

        return indices;
    }

    /// <summary>
    /// 设置临时Range（用于拖拽时实时显示）
    /// </summary>
    public void SetTempRange(TMP_Text textComponent, Range tempRange)
    {
        if (textComponent == null) return;
        
        if (tempRange == null)
        {
            tempRanges.Remove(textComponent);
        }
        else
        {
            tempRanges[textComponent] = tempRange;
        }
        
        RefreshUI(textComponent);
    }

    /// <summary>
    /// 获取临时Range
    /// </summary>
    public Range GetTempRange(TMP_Text textComponent)
    {
        if (textComponent == null || !tempRanges.ContainsKey(textComponent))
            return null;
        
        return tempRanges[textComponent];
    }

    /// <summary>
    /// 刷新UI显示
    /// </summary>
    private void RefreshUI(TMP_Text textComponent)
    {
        if (textComponent == null) return;

        // 查找对应的SelectionMaskUI组件并更新
        SelectionMaskUI maskUI = textComponent.GetComponent<SelectionMaskUI>();
        if (maskUI == null)
        {
            // 尝试从父对象查找（InputField的情况）
            maskUI = textComponent.GetComponentInParent<SelectionMaskUI>();
        }
        
        if (maskUI == null)
        {
            // 尝试从场景中查找所有 SelectionMaskUI，看是否有匹配的
            SelectionMaskUI[] allMaskUIs = FindObjectsOfType<SelectionMaskUI>();
            foreach (var ui in allMaskUIs)
            {
                if (ui.textComponent == textComponent)
                {
                    maskUI = ui;
                    break;
                }
            }
        }

        if (maskUI != null)
        {
            maskUI.UpdateAllSelectionMasks();
        }
        else
        {
            Debug.LogWarning($"SelectionManager: 找不到 SelectionMaskUI 组件！textComponent = {textComponent.name}");
        }
    }

    /// <summary>
    /// 移除指定文本组件的所有Range（当文本组件被销毁时调用）
    /// </summary>
    public void RemoveTextComponent(TMP_Text textComponent)
    {
        if (textComponent != null && textRanges.ContainsKey(textComponent))
        {
            textRanges.Remove(textComponent);
        }
    }
}
