using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeContentData
{
    [TextArea(5, 20)]
    [Tooltip("节点显示的文本内容")]
    public string text = "";

    [Tooltip("文本字体大小")]
    public float textSize = 14f;

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

    [Header("注释")]
    [TextArea(3, 10)]
    [Tooltip("节点注释（用于显示提示信息）")]
    public string comment = "";

    [Header("贴纸配置")]
    [Tooltip("贴纸的Y轴坐标")]
    public float stickerYPosition = 0f;
}