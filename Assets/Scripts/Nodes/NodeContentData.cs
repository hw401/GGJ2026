using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeContentData
{
    [TextArea(5, 20)]
    [Tooltip("节点显示的文本内容")]
    public string text = "";

    [Tooltip("关键词列表")]
    public List<KeywordData> keywords = new List<KeywordData>();

    [Tooltip("变量变更规则列表")]
    public List<VariableChangeRule> variableChanges = new List<VariableChangeRule>();
}