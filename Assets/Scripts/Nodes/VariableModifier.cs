using System;
using UnityEngine;

[Serializable]
public struct VariableModifier
{
    [Tooltip("目标变量")]
    public GameVariable targetVariable;

    [Tooltip("操作类型")]
    public OperationType operation;

    [Tooltip("数值")]
    public float value;
}