using System.Collections.Generic;
using UnityEngine;

public class NodeReferenceResolver : MonoBehaviour
{
    [Header("节点图")]
    public NodeGraphManager nodeGraph;

    // 运行时缓存
    private Dictionary<int, BaseNodeSO> _nodeCache;

    private void Awake()
    {
        BuildNodeCache();
    }

    /// <summary>
    /// 构建节点缓存
    /// </summary>
    private void BuildNodeCache()
    {
        if (nodeGraph == null)
        {
            Debug.LogError("NodeGraphManager未设置！");
            return;
        }

        _nodeCache = new Dictionary<int, BaseNodeSO>();
        foreach (var node in nodeGraph.allNodes)
        {
            if (node != null)
            {
                _nodeCache[node.NodeID] = node;
            }
        }
    }

    /// <summary>
    /// 根据ID获取节点
    /// </summary>
    public BaseNodeSO GetNode(int nodeID)
    {
        if (_nodeCache == null) BuildNodeCache();

        if (_nodeCache.TryGetValue(nodeID, out var node))
        {
            return node;
        }

        Debug.LogError($"找不到ID为 {nodeID} 的节点");
        return null;
    }

    /// <summary>
    /// 获取起始节点
    /// </summary>
    public BaseNodeSO GetStartNode()
    {
        if (nodeGraph != null && nodeGraph.startNode != null)
        {
            return nodeGraph.startNode;
        }

        // 如果没有设置起始节点，返回第一个节点
        if (_nodeCache != null && _nodeCache.Count > 0)
        {
            foreach (var node in _nodeCache.Values)
            {
                return node;
            }
        }

        return null;
    }

    /// <summary>
    /// 验证所有节点引用
    /// </summary>
    public bool ValidateReferences()
    {
        if (nodeGraph == null) return false;

        bool allValid = true;

        foreach (var node in nodeGraph.allNodes)
        {
            if (node == null) continue;

            switch (node)
            {
                case NormalNodeSO normalNode:
                    if (normalNode.nextNode != null && !_nodeCache.ContainsValue(normalNode.nextNode))
                    {
                        Debug.LogWarning($"普通节点 {node.NodeID} 引用了不存在的节点");
                        allValid = false;
                    }
                    break;

                case KeyNodeSO keyNode:
                    if (keyNode.defaultNode != null && !_nodeCache.ContainsValue(keyNode.defaultNode))
                    {
                        Debug.LogWarning($"关键节点 {node.NodeID} 的默认节点不存在");
                        allValid = false;
                    }

                    foreach (var branch in keyNode.branches)
                    {
                        if (branch.targetNode != null && !_nodeCache.ContainsValue(branch.targetNode))
                        {
                            Debug.LogWarning($"关键节点 {node.NodeID} 的分支引用了不存在的节点");
                            allValid = false;
                        }
                    }
                    break;

                case QTENodeSO qteNode:
                    if (qteNode.successNode != null && !_nodeCache.ContainsValue(qteNode.successNode))
                    {
                        Debug.LogWarning($"QTE节点 {node.NodeID} 的成功节点不存在");
                        allValid = false;
                    }

                    if (qteNode.failureNode != null && !_nodeCache.ContainsValue(qteNode.failureNode))
                    {
                        Debug.LogWarning($"QTE节点 {node.NodeID} 的失败节点不存在");
                        allValid = false;
                    }
                    break;
            }
        }

        return allValid;
    }
}