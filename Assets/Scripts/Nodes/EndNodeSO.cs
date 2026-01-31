using UnityEngine;

[CreateAssetMenu(fileName = "NewEndNode", menuName = "叙事系统/结局节点")]
public class EndNodeSO : BaseNodeSO
{
    [Header("结局设置")]
    [Tooltip("结局类型")]
    public EndingType endingType = EndingType.Normal;

    [Tooltip("结局CG图片")]
    public Sprite cgImage;

    [TextArea(3, 6)]
    [Tooltip("结局描述（调试）")]
    public string endingDescription = "";

    public override NodeType GetNodeType() => NodeType.End;
}
