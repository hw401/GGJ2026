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

    [Header("黑块限制")]
    [Tooltip("最大黑块数量（一个黑块代表一个字符）")]
    [Min(1)]
    public int maxBlockCount = 100;

    [Header("报纸配置")]
    [Tooltip("报纸图片（提交后显示的图片）")]
    public Sprite newspaperImage;
}