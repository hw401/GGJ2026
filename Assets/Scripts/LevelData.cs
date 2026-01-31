using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡数据配置（ScriptableObject）
/// 用于在编辑器中配置关卡的关键词表、判分规则和黑块限制
/// </summary>
[CreateAssetMenu(fileName = "New Level Data", menuName = "Text Selection/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("黑块限制")]
    [Tooltip("黑块总数限制（一个黑块代表一个字符）")]
    public int maxBlockCount = 100;

    [Header("关键词表")]
    [Tooltip("关键词列表，每个关键词包含起点和终点")]
    public List<Keyword> keywords = new List<Keyword>();

    [Header("判分规则")]
    [Tooltip("判分规则列表，每个规则定义哪些关键词必须被覆盖/未被覆盖")]
    public List<ResultRule> resultRules = new List<ResultRule>();
}

/// <summary>
/// 判分规则
/// 定义：哪些关键词必须被覆盖，哪些必须未被覆盖，才能产生对应的结果
/// </summary>
[System.Serializable]
public class ResultRule
{
    [Tooltip("结果名称/ID")]
    public string resultName;

    [Tooltip("必须被覆盖的关键词名称列表")]
    public List<string> requiredCoveredKeywords = new List<string>();

    [Tooltip("必须未被覆盖的关键词名称列表")]
    public List<string> requiredUncoveredKeywords = new List<string>();

    [Header("属性变化")]
    [Tooltip("公信力变化值（可以为正数或负数）")]
    public int credibilityChange = 0;

    [Tooltip("民众支持度变化值（可以为正数或负数）")]
    public int publicSupportChange = 0;

    [Tooltip("反对党变化值（可以为正数或负数）")]
    public int oppositionChange = 0;

    [Tooltip("法院变化值（可以为正数或负数）")]
    public int courtChange = 0;

    /// <summary>
    /// 检查当前覆盖状态是否满足此规则
    /// </summary>
    /// <param name="coveredKeywords">被覆盖的关键词索引集合</param>
    /// <param name="keywordNameToIndex">关键词名字到索引的映射字典</param>
    /// <returns>是否满足规则</returns>
    public bool IsSatisfied(HashSet<int> coveredKeywords, Dictionary<string, int> keywordNameToIndex)
    {
        // 检查所有必须被覆盖的关键词是否都在覆盖集合中
        foreach (string keywordName in requiredCoveredKeywords)
        {
            if (!keywordNameToIndex.ContainsKey(keywordName))
            {
                Debug.LogWarning($"ResultRule: 找不到关键词名称 '{keywordName}'");
                return false;
            }
            int index = keywordNameToIndex[keywordName];
            if (!coveredKeywords.Contains(index))
            {
                return false; // 必须被覆盖的关键词未被覆盖
            }
        }

        // 检查所有必须未被覆盖的关键词是否都不在覆盖集合中
        foreach (string keywordName in requiredUncoveredKeywords)
        {
            if (!keywordNameToIndex.ContainsKey(keywordName))
            {
                Debug.LogWarning($"ResultRule: 找不到关键词名称 '{keywordName}'");
                return false;
            }
            int index = keywordNameToIndex[keywordName];
            if (coveredKeywords.Contains(index))
            {
                return false; // 必须未被覆盖的关键词被覆盖了
            }
        }

        return true;
    }
}
