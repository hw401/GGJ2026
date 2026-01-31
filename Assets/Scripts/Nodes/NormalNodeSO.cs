using UnityEngine;

[CreateAssetMenu(fileName = "NewNormalNode", menuName = "叙事系统/普通节点")]
public class NormalNodeSO : BaseNodeSO
{
    [Header("节点内容")]
    public NodeContentData content = new NodeContentData();

    [Header("跳转设置")]
    [Tooltip("下一个节点")]
    public BaseNodeSO nextNode;

    public override NodeType GetNodeType() => NodeType.Normal;

    // 获取下一个节点的ID（用于序列化）
    public int GetNextNodeID()
    {
        return nextNode != null ? nextNode.NodeID : -1;
    }
}