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
    /// 获取黑块总数限制
    /// </summary>
    public int GetMaxBlockCount()
    {
        // 从LevelManager获取当前节点
        if (LevelManager.instance != null && LevelManager.instance.currentNode != null)
        {
            BaseNodeSO currentNode = LevelManager.instance.currentNode;
            
            // 根据节点类型获取content中的maxBlockCount
            NodeType nodeType = currentNode.GetNodeType();
            switch (nodeType)
            {
                case NodeType.Normal:
                    NormalNodeSO normalNode = currentNode as NormalNodeSO;
                    if (normalNode != null && normalNode.content != null)
                    {
                        return normalNode.content.maxBlockCount;
                    }
                    break;
                case NodeType.Key:
                    KeyNodeSO keyNode = currentNode as KeyNodeSO;
                    if (keyNode != null && keyNode.content != null)
                    {
                        return keyNode.content.maxBlockCount;
                    }
                    break;
                case NodeType.QTE:
                    QTENodeSO qteNode = currentNode as QTENodeSO;
                    if (qteNode != null && qteNode.content != null)
                    {
                        return qteNode.content.maxBlockCount;
                    }
                    break;
            }
        }
        
        // 如果无法获取，返回默认值
        return 100; // 默认值
    }

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
    /// 计算当前已使用的黑块数（字符数）
    /// </summary>
    public int GetUsedBlockCount()
    {
        int totalCount = 0;
        foreach (var kvp in textRanges)
        {
            foreach (var range in kvp.Value)
            {
                totalCount += range.Length;
            }
        }
        return totalCount;
    }

    /// <summary>
    /// 计算指定文本组件已使用的黑块数（字符数）
    /// </summary>
    public int GetUsedBlockCount(TMP_Text textComponent)
    {
        if (textComponent == null || !textRanges.ContainsKey(textComponent))
            return 0;

        int count = 0;
        foreach (var range in textRanges[textComponent])
        {
            count += range.Length;
        }
        return count;
    }

    /// <summary>
    /// 获取剩余黑块数
    /// </summary>
    public int GetRemainingBlockCount()
    {
        return GetMaxBlockCount() - GetUsedBlockCount();
    }

    /// <summary>
    /// 检查临时Range是否会超出黑块数限制
    /// </summary>
    /// <param name="textComponent">文本组件</param>
    /// <param name="tempRange">临时Range</param>
    /// <param name="autoMerge">是否自动合并</param>
    /// <returns>是否会超出限制，以及如果添加后的总字符数</returns>
    public (bool wouldExceed, int totalAfterAdd) CheckTempRangeLimit(TMP_Text textComponent, Range tempRange, bool autoMerge = true)
    {
        if (textComponent == null || tempRange == null)
        {
            return (false, GetUsedBlockCount());
        }

        // 计算当前已使用的黑块数（不包括临时Range）
        int currentUsedCount = GetUsedBlockCount();
        
        int maxBlockCount = GetMaxBlockCount();
        
        if (!textRanges.ContainsKey(textComponent))
        {
            // 没有已有选择，直接检查临时Range的长度
            int totalAfterAdd = currentUsedCount + tempRange.Length;
            return (totalAfterAdd > maxBlockCount, totalAfterAdd);
        }

        var ranges = textRanges[textComponent];
        Range simulatedRange = new Range(tempRange.startIndex, tempRange.endIndex);
        
        if (autoMerge)
        {
            // 模拟合并操作
            int mergedCharCount = 0;
            foreach (var range in ranges)
            {
                if (simulatedRange.Overlaps(range) || simulatedRange.IsAdjacent(range))
                {
                    mergedCharCount += range.Length;
                    simulatedRange.Merge(range);
                }
            }
            
            // 计算添加后的总字符数
            int totalAfterAdd = currentUsedCount - mergedCharCount + simulatedRange.Length;
            return (totalAfterAdd > maxBlockCount, totalAfterAdd);
        }
        else
        {
            // 不合并，直接检查
            int totalAfterAdd = currentUsedCount + tempRange.Length;
            return (totalAfterAdd > maxBlockCount, totalAfterAdd);
        }
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

        // 计算当前已使用的黑块数（在添加新Range之前）
        int currentUsedCount = GetUsedBlockCount();
        int maxBlockCount = GetMaxBlockCount();
        
        if (autoMerge)
        {
            // 查找重叠或相邻的Range并合并
            List<Range> toMerge = new List<Range>();
            int mergedCharCount = 0; // 被合并的Range的字符数
            
            for (int i = ranges.Count - 1; i >= 0; i--)
            {
                if (newRange.Overlaps(ranges[i]) || newRange.IsAdjacent(ranges[i]))
                {
                    toMerge.Add(ranges[i]);
                    mergedCharCount += ranges[i].Length; // 记录被合并的字符数
                    ranges.RemoveAt(i);
                }
            }

            // 合并所有重叠或相邻的Range
            foreach (var range in toMerge)
            {
                newRange.Merge(range);
            }
            
            // 计算添加新Range后的总字符数
            // 当前已使用 - 被合并的字符数 + 新Range的字符数
            int totalAfterAdd = currentUsedCount - mergedCharCount + newRange.Length;
            if (totalAfterAdd > maxBlockCount)
            {
                // 超出限制，恢复ranges列表
                ranges.AddRange(toMerge);
                Debug.LogWarning($"SelectionManager: 超出黑块限制！当前已使用: {currentUsedCount}, 尝试添加: {newRange.Length}, 被合并: {mergedCharCount}, 限制: {maxBlockCount}");
                return; // 超出限制，不添加
            }
        }
        else
        {
            // 没有合并，直接检查新增的字符数
            int totalAfterAdd = currentUsedCount + newRange.Length;
            if (totalAfterAdd > maxBlockCount)
            {
                Debug.LogWarning($"SelectionManager: 超出黑块限制！当前已使用: {currentUsedCount}, 尝试添加: {newRange.Length}, 限制: {maxBlockCount}");
                return; // 超出限制，不添加
            }
        }

        ranges.Add(newRange);

        // 通知UI更新
        RefreshUI(textComponent);
        
        // 更新剩余黑块数显示
        UpdateRemainingBlockCount();
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

        // 通知UI更新
        RefreshUI(textComponent);
        
        // 更新剩余黑块数显示
        UpdateRemainingBlockCount();
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
        
        // 更新剩余黑块数显示
        UpdateRemainingBlockCount();
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
        
        // 更新剩余黑块数显示
        UpdateRemainingBlockCount();
    }

    /// <summary>
    /// 清除所有文本组件的所有选区
    /// </summary>
    public void ClearAllSelections()
    {
        // 在清除之前，先保存所有文本组件的引用，以便后续更新UI
        List<TMP_Text> textComponents = new List<TMP_Text>(textRanges.Keys);
        
        // 也保存临时Range中的文本组件（可能不在textRanges中）
        foreach (var kvp in tempRanges)
        {
            if (!textComponents.Contains(kvp.Key))
            {
                textComponents.Add(kvp.Key);
            }
        }
        
        // 清除所有Range
        textRanges.Clear();
        
        // 清除所有临时Range
        tempRanges.Clear();

        // 通知所有UI更新（清除黑框）
        foreach (var textComponent in textComponents)
        {
            RefreshUI(textComponent);
        }
        
        // 额外：确保清除所有场景中的 SelectionMaskUI（以防有遗漏）
        SelectionMaskUI[] allMaskUIs = FindObjectsOfType<SelectionMaskUI>();
        foreach (var maskUI in allMaskUIs)
        {
            if (maskUI != null)
            {
                maskUI.UpdateAllSelectionMasks();
            }
        }
        
        // 更新剩余黑块数显示
        UpdateRemainingBlockCount();
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
        
        // 更新剩余黑块数显示
        UpdateRemainingBlockCount();
    }

    /// <summary>
    /// 更新剩余黑块数显示
    /// </summary>
    private void UpdateRemainingBlockCount()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateRemainingBlockCount();
        }
    }
}
