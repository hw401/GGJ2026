using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BranchLogic
{
    [Tooltip("分支名称（用于调试）")]
    public string branchName = "新分支";

    [Tooltip("分支条件")]
    public List<VariableCondition> conditions = new List<VariableCondition>();

    [Tooltip("满足条件后跳转到的节点")]
    public BaseNodeSO targetNode;

    // 获取目标节点ID（用于序列化）
    public int GetTargetNodeID()
    {
        return targetNode != null ? targetNode.NodeID : -1;
    }
}

[CreateAssetMenu(fileName = "NewKeyNode", menuName = "叙事系统/关键节点")]
public class KeyNodeSO : BaseNodeSO
{
    [Header("节点内容")]
    public NodeContentData content = new NodeContentData();

    [Header("分支逻辑")]
    [Tooltip("分支列表（按顺序从前到后检查）")]
    public List<BranchLogic> branches = new List<BranchLogic>();

    [Header("默认跳转")]
    [Tooltip("所有分支都不满足时跳转的节点")]
    public BaseNodeSO defaultNode;

    public override NodeType GetNodeType() => NodeType.Key;

    // 获取默认节点ID
    public int GetDefaultNodeID()
    {
        return defaultNode != null ? defaultNode.NodeID : -1;
    }
}