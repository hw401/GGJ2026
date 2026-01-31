using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VariableChangeRule
{
    [Tooltip("条件列表（所有条件必须同时满足）")]
    public List<KeywordCondition> conditions = new List<KeywordCondition>();

    [Tooltip("满足条件后执行的变量变更")]
    public List<VariableModifier> modifications = new List<VariableModifier>();
}