using System;
using UnityEngine;

[Serializable]
public class KeywordData
{
    [Tooltip("关键词的唯一标识符")]
    public string id;

    [Tooltip("关键词在文本中的起始索引（包含）")]
    public int startIndex;

    [Tooltip("关键词在文本中的结束索引（不包含）")]
    public int endIndex;

    // 构造函数
    public KeywordData() { }

    public KeywordData(string id, int start, int end)
    {
        this.id = id;
        this.startIndex = start;
        this.endIndex = end;
    }
}