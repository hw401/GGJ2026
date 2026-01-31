using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 关键词数据，包含名字、起点和终点
/// </summary>
[System.Serializable]
public class Keyword
{
    [Tooltip("关键词名称")]
    public string name;

    [Tooltip("起点索引")]
    public int startIndex;

    [Tooltip("终点索引")]
    public int endIndex;

    public Keyword(string keywordName, int start, int end)
    {
        name = keywordName;
        startIndex = start;
        endIndex = end;
    }
}

/// <summary>
/// 关卡管理器
/// 管理关卡数据（关键词表、判分规则等），并实现关键词与当前选择的重合判定逻辑
/// </summary>
public class KeywordManager : Singleton<KeywordManager>
{
    [Header("关卡配置")]
    [Tooltip("关卡数据配置（ScriptableObject）")]
    public LevelData levelData;

    [Header("文本组件")]
    [Tooltip("要检查的文本组件（用于获取当前选择）")]
    public TMP_Text textComponent;

    [Header("属性值")]
    [Tooltip("公信力")]
    public int credibility = 0;

    [Tooltip("民众支持度")]
    public int publicSupport = 0;

    [Tooltip("反对党")]
    public int opposition = 0;

    [Tooltip("法院")]
    public int court = 0;

    /// <summary>
    /// 判分逻辑
    /// 检查当前选择是否与关键词表重合，并根据规则返回结果
    /// </summary>
    public void CheckKeywords()
    {
        if (SelectionManager.instance == null)
        {
            Debug.LogWarning("KeywordManager: SelectionManager.instance 为 null");
            return;
        }

        if (levelData == null)
        {
            Debug.LogWarning("KeywordManager: levelData 为 null，请配置关卡数据");
            return;
        }

        // 1. 获取当前所有选择（从 SelectionManager）
        if (textComponent == null)
        {
            Debug.LogWarning("KeywordManager: textComponent 为 null，请指定要检查的文本组件");
            return;
        }

        List<Range> currentRanges = SelectionManager.instance.GetRanges(textComponent);
        
        if (currentRanges.Count == 0)
        {
            Debug.Log("KeywordManager: 当前没有选择任何文本");
            return;
        }

        // 2. 创建关键词名字到索引的映射字典
        Dictionary<string, int> keywordNameToIndex = new Dictionary<string, int>();
        for (int i = 0; i < levelData.keywords.Count; i++)
        {
            Keyword keyword = levelData.keywords[i];
            if (!string.IsNullOrEmpty(keyword.name))
            {
                keywordNameToIndex[keyword.name] = i;
            }
        }

        // 3. 遍历关键词表，检查每个关键词是否被覆盖
        HashSet<int> coveredKeywords = new HashSet<int>();
        
        for (int i = 0; i < levelData.keywords.Count; i++)
        {
            Keyword keyword = levelData.keywords[i];
            // 创建关键词对应的Range对象，用于判断是否重合
            Range keywordRange = new Range(keyword.startIndex, keyword.endIndex);
            
            // 检查关键词是否被完全覆盖
            // 必须完全覆盖：当前选择的Range必须完全包含关键词（可以多出来，但不能少）
            // 即：currentRange.startIndex <= keyword.startIndex && currentRange.endIndex >= keyword.endIndex
            bool isCovered = false;
            foreach (Range currentRange in currentRanges)
            {
                // 检查当前Range是否完全包含关键词
                if (currentRange.startIndex <= keyword.startIndex && currentRange.endIndex >= keyword.endIndex)
                {
                    isCovered = true;
                    break;
                }
            }
            
            // 如果单个Range无法完全覆盖，检查是否由多个Range组合覆盖
            if (!isCovered)
            {
                // 检查关键词的起点和终点是否都在当前选择的Range中
                bool startCovered = false;
                bool endCovered = false;
                
                foreach (Range currentRange in currentRanges)
                {
                    if (currentRange.Contains(keyword.startIndex))
                    {
                        startCovered = true;
                    }
                    if (currentRange.Contains(keyword.endIndex))
                    {
                        endCovered = true;
                    }
                }
                
                // 如果起点和终点都被覆盖，还需要检查中间的所有字符是否都被覆盖
                if (startCovered && endCovered)
                {
                    bool allCharsCovered = true;
                    for (int charIdx = keyword.startIndex; charIdx <= keyword.endIndex; charIdx++)
                    {
                        bool charCovered = false;
                        foreach (Range currentRange in currentRanges)
                        {
                            if (currentRange.Contains(charIdx))
                            {
                                charCovered = true;
                                break;
                            }
                        }
                        if (!charCovered)
                        {
                            allCharsCovered = false;
                            break;
                        }
                    }
                    isCovered = allCharsCovered;
                }
            }
            
            if (isCovered)
            {
                coveredKeywords.Add(i);
                string keywordName = string.IsNullOrEmpty(keyword.name) ? $"关键词{i}" : keyword.name;
                Debug.Log($"关键词 '{keywordName}' [{keyword.startIndex}, {keyword.endIndex}] 被覆盖");
            }
        }
        
        // 输出被覆盖的关键词名字
        List<string> coveredKeywordNames = new List<string>();
        foreach (int index in coveredKeywords)
        {
            if (index < levelData.keywords.Count)
            {
                Keyword keyword = levelData.keywords[index];
                string keywordName = string.IsNullOrEmpty(keyword.name) ? $"关键词{index}" : keyword.name;
                coveredKeywordNames.Add(keywordName);
            }
        }
        Debug.Log($"被覆盖的关键词: {string.Join(", ", coveredKeywordNames)}");
        
        // 4. 根据覆盖情况，检查所有判分规则
        bool anyRuleSatisfied = false;
        foreach (var rule in levelData.resultRules)
        {
            if (rule.IsSatisfied(coveredKeywords, keywordNameToIndex))
            {
                anyRuleSatisfied = true;
                
                // 规则满足，应用属性变化
                credibility += rule.credibilityChange;
                publicSupport += rule.publicSupportChange;
                opposition += rule.oppositionChange;
                court += rule.courtChange;
                
                Debug.Log($"判分结果: {rule.resultName} - 公信力: {rule.credibilityChange}, 民众支持度: {rule.publicSupportChange}, 反对党: {rule.oppositionChange}, 法院: {rule.courtChange}");
                Debug.Log($"当前属性值 - 公信力: {credibility}, 民众支持度: {publicSupport}, 反对党: {opposition}, 法院: {court}");
            }
        }
        
        if (!anyRuleSatisfied)
        {
            Debug.Log("KeywordManager: 没有满足任何判分规则");
        }
    }
}
