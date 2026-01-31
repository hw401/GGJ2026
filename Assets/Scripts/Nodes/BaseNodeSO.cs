using UnityEngine;

public abstract class BaseNodeSO : ScriptableObject
{
    [Header("节点基本信息")]
    [Tooltip("节点ID（数字）")]
    [SerializeField] private int _nodeID;

    [Tooltip("节点名称（用于调试）")]
    public string nodeName = "未命名节点";

    [TextArea(2, 4)]
    [Tooltip("节点描述（用于调试）")]
    public string description = "";

    // 公共属性访问器
    public int NodeID
    {
        get => _nodeID;
        set => _nodeID = value;
    }

    // 抽象方法
    public abstract NodeType GetNodeType();

    // 获取节点类型名称（用于显示）
    public virtual string GetNodeTypeName()
    {
        return GetNodeType().ToString();
    }

    // 重置节点ID（在创建时调用）
    public void ResetID(int newID)
    {
        _nodeID = newID;
        name = $"{GetNodeTypeName()}_{newID:000}";
    }
}