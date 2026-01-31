using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NodeGraph", menuName = "叙事系统/节点图")]
public class NodeGraphManager : ScriptableObject
{
    [Header("节点图信息")]
    public string graphName = "新节点图";

    [Tooltip("起始节点")]
    public BaseNodeSO startNode;

    [Header("所有节点")]
    [Tooltip("节点列表（按ID排序）")]
    public List<BaseNodeSO> allNodes = new List<BaseNodeSO>();

    // 节点ID到节点的映射（用于快速查找）
    private Dictionary<int, BaseNodeSO> _nodeCache;

    /// <summary>
    /// 根据ID获取节点
    /// </summary>
    public BaseNodeSO GetNodeByID(int nodeID)
    {
        if (_nodeCache == null || _nodeCache.Count != allNodes.Count)
        {
            BuildNodeCache();
        }

        if (_nodeCache.TryGetValue(nodeID, out var node))
        {
            return node;
        }

        Debug.LogWarning($"找不到ID为 {nodeID} 的节点");
        return null;
    }

    /// <summary>
    /// 构建节点缓存
    /// </summary>
    private void BuildNodeCache()
    {
        _nodeCache = new Dictionary<int, BaseNodeSO>();
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                _nodeCache[node.NodeID] = node;
            }
        }
    }

    /// <summary>
    /// 验证节点图
    /// </summary>
    public void ValidateGraph()
    {
        BuildNodeCache();

        // 检查起始节点
        if ((startNode == null || !allNodes.Contains(startNode)) && allNodes.Count > 0)
        {
            startNode = allNodes[0];
            Debug.Log("已设置第一个节点为起始节点");
        }

        // 检查节点ID唯一性
        var idSet = new HashSet<int>();
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                if (idSet.Contains(node.NodeID))
                {
                    Debug.LogError($"节点ID重复: {node.NodeID} - {node.name}");
                }
                else
                {
                    idSet.Add(node.NodeID);
                }
            }
        }
    }

    /// <summary>
    /// 获取起始节点ID（用于序列化）
    /// </summary>
    public int GetStartNodeID()
    {
        return startNode != null ? startNode.NodeID : -1;
    }
}
