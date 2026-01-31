using System;
using UnityEngine;

[Serializable]
public struct VariableCondition
{
    [Tooltip("目标变量")]
    public GameVariable variable;

    [Tooltip("比较运算符")]
    public ComparisonOperator comparison;

    [Tooltip("比较值A（对于Range操作，这是最小值）")]
    public float valueA;

    [Tooltip("比较值B（仅用于Range操作，这是最大值）")]
    public float valueB;
}