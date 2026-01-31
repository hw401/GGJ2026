using UnityEngine;

[CreateAssetMenu(fileName = "NewQTENode", menuName = "叙事系统/QTE节点")]
public class QTENodeSO : BaseNodeSO
{
    [Header("节点内容")]
    public NodeContentData content = new NodeContentData();

    [Header("QTE设置")]
    [Tooltip("QTE持续时间（秒）")]
    [Min(0.1f)]
    public float duration = 5f;

    [Header("跳转设置")]
    [Tooltip("QTE成功时跳转的节点")]
    public BaseNodeSO successNode;

    [Tooltip("QTE失败时跳转的节点")]
    public BaseNodeSO failureNode;

    public override NodeType GetNodeType() => NodeType.QTE;

    // 获取成功/失败节点ID
    public int GetSuccessNodeID() => successNode != null ? successNode.NodeID : -1;
    public int GetFailureNodeID() => failureNode != null ? failureNode.NodeID : -1;
}
