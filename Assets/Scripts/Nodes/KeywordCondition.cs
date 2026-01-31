using System;
using UnityEngine;

[Serializable]
public struct KeywordCondition
{
    [Tooltip("关键词ID")]
    public string keywordID;

    [Tooltip("需要满足的状态（true=必须已抹黑，false=必须未抹黑）")]
    public bool requiredSelectedState;
}