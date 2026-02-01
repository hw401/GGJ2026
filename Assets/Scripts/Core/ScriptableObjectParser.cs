using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject解析器
/// 负责解析各种ScriptableObject数据，特别是节点相关的数据
/// </summary>
public class ScriptableObjectParser : Singleton<ScriptableObjectParser>
{
    /// <summary>
    /// 解析节点内容数据
    /// </summary>
    /// <param name="contentData">节点内容数据</param>
    /// <returns>解析结果，包含文本、关键词列表等信息</returns>
    public NodeContentParseResult ParseNodeContent(NodeContentData contentData)
    {
        if (contentData == null)
        {
            Debug.LogWarning("ScriptableObjectParser: contentData 为 null");
            return null;
        }

        NodeContentParseResult result = new NodeContentParseResult
        {
            text = contentData.text,
            textSize = contentData.textSize,
            keywords = new List<KeywordParseResult>(),
            variableChangeRules = new List<VariableChangeRuleParseResult>(),
            maxBlockCount = contentData.maxBlockCount,
            newspaperImage = contentData.newspaperImage,
            comment = contentData.comment,
            stickerYPosition = contentData.stickerYPosition
        };

        // 解析关键词
        if (contentData.keywords != null)
        {
            foreach (var keyword in contentData.keywords)
            {
                if (keyword != null)
                {
                    result.keywords.Add(new KeywordParseResult
                    {
                        id = keyword.id,
                        startIndex = keyword.startIndex,
                        endIndex = keyword.endIndex
                    });
                }
            }
        }

        // 解析变量变更规则
        if (contentData.variableChanges != null)
        {
            foreach (var rule in contentData.variableChanges)
            {
                if (rule != null)
                {
                    VariableChangeRuleParseResult ruleResult = new VariableChangeRuleParseResult
                    {
                        conditions = new List<KeywordConditionParseResult>(),
                        modifications = new List<VariableModifierParseResult>()
                    };

                    // 解析条件
                    if (rule.conditions != null)
                    {
                        foreach (var condition in rule.conditions)
                        {
                            ruleResult.conditions.Add(new KeywordConditionParseResult
                            {
                                keywordID = condition.keywordID,
                                requiredSelectedState = condition.requiredSelectedState
                            });
                        }
                    }

                    // 解析修改器
                    if (rule.modifications != null)
                    {
                        foreach (var modifier in rule.modifications)
                        {
                            ruleResult.modifications.Add(new VariableModifierParseResult
                            {
                                targetVariable = modifier.targetVariable,
                                operation = modifier.operation,
                                value = modifier.value
                            });
                        }
                    }

                    result.variableChangeRules.Add(ruleResult);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 解析普通节点
    /// </summary>
    public NormalNodeParseResult ParseNormalNode(NormalNodeSO node)
    {
        if (node == null)
        {
            Debug.LogWarning("ScriptableObjectParser: node 为 null");
            return null;
        }

        NormalNodeParseResult result = new NormalNodeParseResult
        {
            nodeID = node.NodeID,
            nodeName = node.nodeName,
            description = node.description,
            content = ParseNodeContent(node.content),
            nextNodeID = node.GetNextNodeID(),
            nextNode = node.nextNode
        };

        return result;
    }

    /// <summary>
    /// 解析关键节点
    /// </summary>
    public KeyNodeParseResult ParseKeyNode(KeyNodeSO node)
    {
        if (node == null)
        {
            Debug.LogWarning("ScriptableObjectParser: node 为 null");
            return null;
        }

        KeyNodeParseResult result = new KeyNodeParseResult
        {
            nodeID = node.NodeID,
            nodeName = node.nodeName,
            description = node.description,
            content = ParseNodeContent(node.content),
            branches = new List<BranchLogicParseResult>(),
            defaultNodeID = node.GetDefaultNodeID(),
            defaultNode = node.defaultNode
        };

        // 解析分支逻辑
        if (node.branches != null)
        {
            foreach (var branch in node.branches)
            {
                if (branch != null)
                {
                    BranchLogicParseResult branchResult = new BranchLogicParseResult
                    {
                        branchName = branch.branchName,
                        conditions = new List<VariableConditionParseResult>(),
                        targetNodeID = branch.GetTargetNodeID(),
                        targetNode = branch.targetNode
                    };

                    // 解析分支条件
                    if (branch.conditions != null)
                    {
                        foreach (var condition in branch.conditions)
                        {
                            branchResult.conditions.Add(new VariableConditionParseResult
                            {
                                variable = condition.variable,
                                comparison = condition.comparison,
                                valueA = condition.valueA,
                                valueB = condition.valueB
                            });
                        }
                    }

                    result.branches.Add(branchResult);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 解析结局节点
    /// </summary>
    public EndNodeParseResult ParseEndNode(EndNodeSO node)
    {
        if (node == null)
        {
            Debug.LogWarning("ScriptableObjectParser: node 为 null");
            return null;
        }

        EndNodeParseResult result = new EndNodeParseResult
        {
            nodeID = node.NodeID,
            nodeName = node.nodeName,
            description = node.description,
            endingType = node.endingType,
            cgImage = node.cgImage,
            endingDescription = node.endingDescription
        };

        return result;
    }

    /// <summary>
    /// 解析QTE节点
    /// </summary>
    public QTENodeParseResult ParseQTENode(QTENodeSO node)
    {
        if (node == null)
        {
            Debug.LogWarning("ScriptableObjectParser: node 为 null");
            return null;
        }

        QTENodeParseResult result = new QTENodeParseResult
        {
            nodeID = node.NodeID,
            nodeName = node.nodeName,
            description = node.description,
            content = ParseNodeContent(node.content),
            duration = node.duration,
            successNodeID = node.GetSuccessNodeID(),
            successNode = node.successNode,
            failureNodeID = node.GetFailureNodeID(),
            failureNode = node.failureNode
        };

        return result;
    }

    /// <summary>
    /// 解析基础节点（通用方法，根据类型自动选择解析方法）
    /// </summary>
    public BaseNodeParseResult ParseBaseNode(BaseNodeSO node)
    {
        if (node == null)
        {
            Debug.LogWarning("ScriptableObjectParser: node 为 null");
            return null;
        }

        NodeType nodeType = node.GetNodeType();

        switch (nodeType)
        {
            case NodeType.Normal:
                return ParseNormalNode(node as NormalNodeSO);
            case NodeType.Key:
                return ParseKeyNode(node as KeyNodeSO);
            case NodeType.End:
                return ParseEndNode(node as EndNodeSO);
            case NodeType.QTE:
                return ParseQTENode(node as QTENodeSO);
            default:
                Debug.LogWarning($"ScriptableObjectParser: 未知的节点类型 {nodeType}");
                return new BaseNodeParseResult
                {
                    nodeID = node.NodeID,
                    nodeName = node.nodeName,
                    description = node.description,
                    nodeType = nodeType
                };
        }
    }
}

/// <summary>
/// 节点内容解析结果
/// </summary>
[System.Serializable]
public class NodeContentParseResult
{
    public string text;
    public float textSize;
    public List<KeywordParseResult> keywords;
    public List<VariableChangeRuleParseResult> variableChangeRules;
    public int maxBlockCount;
    public Sprite newspaperImage;
    public string comment;
    public float stickerYPosition;
}

/// <summary>
/// 关键词解析结果
/// </summary>
[System.Serializable]
public class KeywordParseResult
{
    public string id;
    public int startIndex;
    public int endIndex;
}

/// <summary>
/// 关键词条件解析结果
/// </summary>
[System.Serializable]
public class KeywordConditionParseResult
{
    public string keywordID;
    public bool requiredSelectedState;
}

/// <summary>
/// 变量变更规则解析结果
/// </summary>
[System.Serializable]
public class VariableChangeRuleParseResult
{
    public List<KeywordConditionParseResult> conditions;
    public List<VariableModifierParseResult> modifications;
}

/// <summary>
/// 变量修改器解析结果
/// </summary>
[System.Serializable]
public class VariableModifierParseResult
{
    public GameVariable targetVariable;
    public OperationType operation;
    public float value;
}

/// <summary>
/// 变量条件解析结果
/// </summary>
[System.Serializable]
public class VariableConditionParseResult
{
    public GameVariable variable;
    public ComparisonOperator comparison;
    public float valueA;
    public float valueB;
}

/// <summary>
/// 基础节点解析结果
/// </summary>
[System.Serializable]
public class BaseNodeParseResult
{
    public int nodeID;
    public string nodeName;
    public string description;
    public NodeType nodeType;
}

/// <summary>
/// 普通节点解析结果
/// </summary>
[System.Serializable]
public class NormalNodeParseResult : BaseNodeParseResult
{
    public NodeContentParseResult content;
    public int nextNodeID;
    public BaseNodeSO nextNode;
}

/// <summary>
/// 分支逻辑解析结果
/// </summary>
[System.Serializable]
public class BranchLogicParseResult
{
    public string branchName;
    public List<VariableConditionParseResult> conditions;
    public int targetNodeID;
    public BaseNodeSO targetNode;
}

/// <summary>
/// 关键节点解析结果
/// </summary>
[System.Serializable]
public class KeyNodeParseResult : BaseNodeParseResult
{
    public NodeContentParseResult content;
    public List<BranchLogicParseResult> branches;
    public int defaultNodeID;
    public BaseNodeSO defaultNode;
}

/// <summary>
/// 结局节点解析结果
/// </summary>
[System.Serializable]
public class EndNodeParseResult : BaseNodeParseResult
{
    public EndingType endingType;
    public Sprite cgImage;
    public string endingDescription;
}

/// <summary>
/// QTE节点解析结果
/// </summary>
[System.Serializable]
public class QTENodeParseResult : BaseNodeParseResult
{
    public NodeContentParseResult content;
    public float duration;
    public int successNodeID;
    public BaseNodeSO successNode;
    public int failureNodeID;
    public BaseNodeSO failureNode;
}
