using System.Collections.Generic;
using UnityEngine;

public class VariableManager : MonoBehaviour
{
    public static VariableManager Instance { get; private set; }

    private Dictionary<GameVariable, float> _variables = new Dictionary<GameVariable, float>();

    [Header("变量初始值")]
    [Tooltip("政府信任度初始值")]
    [SerializeField]
    private float governmentTrustInitialValue = 0f;

    [Tooltip("政府公信力初始值")]
    [SerializeField]
    private float governmentCredibilityInitialValue = 0f;

    [Tooltip("民众信任度初始值")]
    [SerializeField]
    private float publicTrustInitialValue = 0f;

    // 事件系统：当变量改变时通知UI更新
    public event System.Action<GameVariable, float> OnVariableChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初始化变量，使用配置的初始值
        foreach (GameVariable varType in System.Enum.GetValues(typeof(GameVariable)))
        {
            float initialValue = 0f;
            
            // 根据变量类型设置对应的初始值
            switch (varType)
            {
                case GameVariable.政府信任度:
                    initialValue = governmentTrustInitialValue;
                    break;
                case GameVariable.政府公信力:
                    initialValue = governmentCredibilityInitialValue;
                    break;
                case GameVariable.民众信任度:
                    initialValue = publicTrustInitialValue;
                    break;
            }
            
            _variables[varType] = initialValue;
        }
    }

    public float GetValue(GameVariable varType) => _variables[varType];

    public void ModifyValue(GameVariable varType, OperationType op, float value)
    {
        float current = _variables[varType];
        switch (op)
        {
            case OperationType.Add: current += value; break;
            case OperationType.Subtract: current -= value; break;
            case OperationType.Multiply: current *= value; break;
            case OperationType.DivideAndFloor: current = Mathf.Floor(current / value); break;
            case OperationType.Set: current = value; break;
        }
        _variables[varType] = current;
        OnVariableChanged?.Invoke(varType, current);

        Debug.Log($"Variable {varType} changed to {current}");
    }

    public bool EvaluateCondition(VariableCondition condition)
    {
        float val = _variables[condition.variable];
        float targetA = condition.valueA;
        float targetB = condition.valueB;

        switch (condition.comparison)
        {
            case ComparisonOperator.Equal: return Mathf.Approximately(val, targetA);
            case ComparisonOperator.NotEqual: return !Mathf.Approximately(val, targetA);
            case ComparisonOperator.Greater: return val > targetA;
            case ComparisonOperator.Less: return val < targetA;
            case ComparisonOperator.GreaterOrEqual: return val >= targetA;
            case ComparisonOperator.LessOrEqual: return val <= targetA;
            case ComparisonOperator.Range: return val >= targetA && val <= targetB;
            default: return false;
        }
    }

    public bool EvaluateBranch(BranchLogic branch)
    {
        foreach (var cond in branch.conditions)
        {
            if (!EvaluateCondition(cond)) return false;
        }
        return true;
    }
}