using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表示一段连续的文本选区
/// </summary>
[System.Serializable]
public class Range
{
    public int startIndex;      // 包含
    public int endIndex;        // 包含
    public List<GameObject> uiElements = new List<GameObject>(); // 这段选区对应的一个或多个 Image

    public Range(int start, int end)
    {
        startIndex = start;
        endIndex = end;
        if (startIndex > endIndex)
        {
            int t = startIndex;
            startIndex = endIndex;
            endIndex = t;
        }
    }

    public int Length => endIndex - startIndex + 1;

    public bool Contains(int charIndex)
    {
        return charIndex >= startIndex && charIndex <= endIndex;
    }

    public bool Overlaps(Range other)
    {
        return !(endIndex < other.startIndex || startIndex > other.endIndex);
    }

    /// <summary>
    /// 检查是否与另一个Range相邻（紧挨着）
    /// </summary>
    public bool IsAdjacent(Range other)
    {
        return (endIndex + 1 == other.startIndex) || (other.endIndex + 1 == startIndex);
    }

    public void Merge(Range other)
    {
        startIndex = Mathf.Min(startIndex, other.startIndex);
        endIndex = Mathf.Max(endIndex, other.endIndex);
    }

    /// <summary>
    /// 从Range中移除指定字符索引，可能拆分成两个Range
    /// </summary>
    public List<Range> RemoveChar(int charIndex)
    {
        var result = new List<Range>();
        
        if (!Contains(charIndex))
        {
            // 不包含该字符，返回自身
            result.Add(this);
            return result;
        }

        if (startIndex == endIndex)
        {
            // 单字符Range，删除后返回空列表
            return result;
        }

        if (charIndex == startIndex)
        {
            // 删除起始字符，返回 [startIndex+1, endIndex]
            result.Add(new Range(startIndex + 1, endIndex));
        }
        else if (charIndex == endIndex)
        {
            // 删除结束字符，返回 [startIndex, endIndex-1]
            result.Add(new Range(startIndex, endIndex - 1));
        }
        else
        {
            // 删除中间字符，拆分成两段
            result.Add(new Range(startIndex, charIndex - 1));
            result.Add(new Range(charIndex + 1, endIndex));
        }

        return result;
    }
}
