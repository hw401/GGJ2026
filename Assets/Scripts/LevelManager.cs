using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡管理器
/// 负责选择当前节点，管理节点图的执行流程
/// </summary>
public class LevelManager : Singleton<LevelManager>
{
    [Header("节点图配置")]
    [Tooltip("当前使用的节点图")]
    public NodeGraphManager nodeGraph;

    [Header("当前状态")]
    [Tooltip("当前选中的节点")]
    public BaseNodeSO currentNode;

    /// <summary>
    /// 初始化节点图，从起始节点开始
    /// </summary>
    public void InitializeNodeGraph()
    {
        if (nodeGraph == null)
        {
            Debug.LogWarning("LevelManager: nodeGraph 为 null，无法初始化");
            return;
        }

        if (nodeGraph.startNode == null)
        {
            Debug.LogWarning("LevelManager: nodeGraph.startNode 为 null，无法初始化");
            return;
        }

        currentNode = nodeGraph.startNode;
        Debug.Log($"LevelManager: 已初始化节点图 '{nodeGraph.graphName}'，当前节点: {currentNode.nodeName} (ID: {currentNode.NodeID})");
        
        // 通知UI更新
        OnNodeChanged();
    }

    /// <summary>
    /// 移动到下一个节点（自动处理所有节点类型）
    /// 对于普通节点：直接移动到nextNode
    /// 对于关键节点：自动评估分支条件，选择满足条件的分支
    /// </summary>
    /// <param name="skipVariableProcessing">是否跳过变量变更处理（如果已经在外部处理过）</param>
    /// <returns>是否成功移动到下一个节点</returns>
    public bool MoveToNextNode(bool skipVariableProcessing = false)
    {
        if (currentNode == null)
        {
            Debug.LogWarning("LevelManager: currentNode 为 null");
            return false;
        }

        NodeType nodeType = currentNode.GetNodeType();

        switch (nodeType)
        {
            case NodeType.Normal:
                NormalNodeSO normalNode = currentNode as NormalNodeSO;
                if (normalNode != null && normalNode.nextNode != null)
                {
                    return MoveToNode(normalNode.nextNode, skipVariableProcessing);
                }
                else
                {
                    Debug.LogWarning($"LevelManager: 普通节点 {normalNode?.nodeName} 没有下一个节点");
                    return false;
                }

            case NodeType.Key:
                // 关键节点：自动评估分支条件
                return ProcessKeyNodeAutomatically(skipVariableProcessing);

            case NodeType.End:
                Debug.Log($"LevelManager: 已到达结局节点: {currentNode.nodeName}");
                return false;

            default:
                Debug.LogWarning($"LevelManager: 节点类型 {nodeType} 不支持 MoveToNextNode，请使用对应的移动方法");
                return false;
        }
    }

    /// <summary>
    /// 移动到指定节点（通过节点ID）
    /// </summary>
    /// <param name="nodeID">目标节点ID</param>
    /// <returns>是否成功移动</returns>
    public bool MoveToNode(int nodeID)
    {
        if (nodeGraph == null)
        {
            Debug.LogWarning("LevelManager: nodeGraph 为 null");
            return false;
        }

        BaseNodeSO targetNode = nodeGraph.GetNodeByID(nodeID);
        if (targetNode != null)
        {
            // 使用统一的 MoveToNode 方法，确保变量变更和UI更新都被正确处理
            return MoveToNode(targetNode);
        }
        else
        {
            Debug.LogWarning($"LevelManager: 找不到ID为 {nodeID} 的节点");
            return false;
        }
    }

    /// <summary>
    /// 移动到指定节点（直接指定节点对象）
    /// </summary>
    /// <param name="targetNode">目标节点</param>
    /// <param name="skipVariableProcessing">是否跳过变量变更处理（如果已经在外部处理过）</param>
    /// <param name="textComponent">可选的文本组件，用于检查关键词状态</param>
    /// <returns>是否成功移动</returns>
    public bool MoveToNode(BaseNodeSO targetNode, bool skipVariableProcessing = false, TMPro.TMP_Text textComponent = null)
    {
        if (targetNode == null)
        {
            Debug.LogWarning("LevelManager: targetNode 为 null");
            return false;
        }

        // 验证节点是否在节点图中
        if (nodeGraph != null && !nodeGraph.allNodes.Contains(targetNode))
        {
            Debug.LogWarning($"LevelManager: 节点 {targetNode.nodeName} 不在当前节点图中");
            return false;
        }

        // 在切换节点前，处理当前节点的变量变更规则（如果当前节点有内容）
        // 如果 skipVariableProcessing 为 true，则跳过（已经在外部处理过）
        if (!skipVariableProcessing)
        {
            ProcessVariableChangesForCurrentNode(textComponent);
        }

        currentNode = targetNode;
        Debug.Log($"LevelManager: 移动到节点: {currentNode.nodeName} (ID: {currentNode.NodeID})");
        
        // 通知UI更新
        OnNodeChanged();
        
        return true;
    }

    /// <summary>
    /// 变量变更处理结果
    /// </summary>
    public class VariableChangeResult
    {
        public List<string> satisfiedConditions = new List<string>();
        public List<string> variableChanges = new List<string>();
    }

    /// <summary>
    /// 处理当前节点的变量变更规则
    /// 检查关键词条件，如果满足则应用变量变更
    /// </summary>
    /// <param name="textComponent">可选的文本组件，用于检查关键词状态</param>
    /// <returns>处理结果，包含满足的条件和数值变化</returns>
    public VariableChangeResult ProcessVariableChangesForCurrentNode(TMPro.TMP_Text textComponent = null)
    {
        VariableChangeResult result = new VariableChangeResult();
        
        if (currentNode == null)
        {
            return result;
        }

        // 检查 VariableManager 是否存在
        if (VariableManager.Instance == null)
        {
            return result;
        }

        // 获取节点内容（NormalNodeSO, KeyNodeSO, QTENodeSO 都有 content）
        NodeContentData content = null;
        NodeType nodeType = currentNode.GetNodeType();
        
        switch (nodeType)
        {
            case NodeType.Normal:
                NormalNodeSO normalNode = currentNode as NormalNodeSO;
                content = normalNode?.content;
                break;
            case NodeType.Key:
                KeyNodeSO keyNode = currentNode as KeyNodeSO;
                content = keyNode?.content;
                break;
            case NodeType.QTE:
                QTENodeSO qteNode = currentNode as QTENodeSO;
                content = qteNode?.content;
                break;
        }

        if (content == null || content.variableChanges == null || content.variableChanges.Count == 0)
        {
            return result;
        }

        // 检查 SelectionManager 是否存在（用于检查关键词状态）
        bool canCheckKeywords = SelectionManager.instance != null && textComponent != null;

        // 遍历所有变量变更规则
        foreach (var rule in content.variableChanges)
        {
            if (rule == null)
            {
                continue;
            }

            // 收集条件信息
            List<string> conditionDescriptions = new List<string>();
            bool allConditionsMet = true;

            if (rule.conditions == null || rule.conditions.Count == 0)
            {
                // 如果没有条件，直接应用变量变更
                conditionDescriptions.Add("无条件（自动触发）");
            }
            else
            {
                // 检查所有关键词条件是否满足
                foreach (var condition in rule.conditions)
                {
                    if (string.IsNullOrEmpty(condition.keywordID))
                    {
                        continue;
                    }

                    // 检查关键词是否被选中
                    bool keywordSelected = CheckKeywordSelected(condition.keywordID, content, textComponent);
                    
                    // 构建条件描述
                    string conditionDesc = $"关键词 '{condition.keywordID}' {(condition.requiredSelectedState ? "已选中" : "未选中")}";
                    conditionDescriptions.Add(conditionDesc);
                    
                    // 检查条件是否满足（requiredSelectedState: true=必须已选中, false=必须未选中）
                    if (condition.requiredSelectedState != keywordSelected)
                    {
                        allConditionsMet = false;
                        break;
                    }
                }
            }

            // 如果所有条件都满足，应用变量变更
            if (allConditionsMet && rule.modifications != null)
            {
                // 记录满足的条件
                string conditionsStr = string.Join(" 且 ", conditionDescriptions);
                result.satisfiedConditions.Add(conditionsStr);

                // 应用变量变更并记录变化
                foreach (var modifier in rule.modifications)
                {
                    // 获取变更前的值
                    float oldValue = VariableManager.Instance.GetValue(modifier.targetVariable);
                    
                    // 应用变更
                    VariableManager.Instance.ModifyValue(
                        modifier.targetVariable,
                        modifier.operation,
                        modifier.value
                    );
                    
                    // 获取变更后的值
                    float newValue = VariableManager.Instance.GetValue(modifier.targetVariable);
                    
                    // 记录变化
                    string operationStr = GetOperationString(modifier.operation);
                    string changeDesc = $"{modifier.targetVariable}: {oldValue} {operationStr} {modifier.value} = {newValue}";
                    result.variableChanges.Add(changeDesc);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 获取操作符的字符串表示
    /// </summary>
    private string GetOperationString(OperationType op)
    {
        switch (op)
        {
            case OperationType.Add: return "+";
            case OperationType.Subtract: return "-";
            case OperationType.Multiply: return "×";
            case OperationType.DivideAndFloor: return "÷";
            case OperationType.Set: return "=";
            default: return "?";
        }
    }

    /// <summary>
    /// 检查关键词是否被选中
    /// </summary>
    /// <param name="keywordID">关键词ID</param>
    /// <param name="content">节点内容数据</param>
    /// <param name="textComponent">文本组件</param>
    /// <returns>关键词是否被选中</returns>
    private bool CheckKeywordSelected(string keywordID, NodeContentData content, TMPro.TMP_Text textComponent)
    {
        if (string.IsNullOrEmpty(keywordID) || content == null || content.keywords == null)
        {
            return false;
        }

        // 查找对应的关键词
        KeywordData keyword = null;
        foreach (var kw in content.keywords)
        {
            if (kw != null && kw.id == keywordID)
            {
                keyword = kw;
                break;
            }
        }

        if (keyword == null)
        {
            Debug.LogWarning($"LevelManager: 找不到ID为 '{keywordID}' 的关键词");
            return false;
        }

        // 如果没有文本组件或 SelectionManager，无法检查，返回 false
        if (textComponent == null || SelectionManager.instance == null)
        {
            return false;
        }

        // 获取当前选中的范围
        var selectedRanges = SelectionManager.instance.GetRanges(textComponent);
        if (selectedRanges == null || selectedRanges.Count == 0)
        {
            return false;
        }

        // 检查关键词的范围是否被完全覆盖
        // 关键词范围：startIndex 到 endIndex（endIndex 不包含，所以实际字符范围是 startIndex 到 endIndex-1）
        int keywordStart = keyword.startIndex;
        int keywordEnd = keyword.endIndex - 1; // endIndex 不包含，所以减1
        Range keywordRange = new Range(keywordStart, keywordEnd);
        
        foreach (var selectedRange in selectedRanges)
        {
            // 检查关键词是否被完全覆盖
            // 即：selectedRange.startIndex <= keywordRange.startIndex && selectedRange.endIndex >= keywordRange.endIndex
            if (selectedRange.startIndex <= keywordRange.startIndex && selectedRange.endIndex >= keywordRange.endIndex)
            {
                return true;
            }
            
            // 检查是否由多个Range组合覆盖
            // 检查关键词的起点和终点是否都在选中的Range中
            bool startCovered = false;
            bool endCovered = false;
            
            foreach (var range in selectedRanges)
            {
                if (range.Contains(keywordRange.startIndex))
                {
                    startCovered = true;
                }
                if (range.Contains(keywordRange.endIndex))
                {
                    endCovered = true;
                }
            }
            
            // 如果起点和终点都被覆盖，检查中间的所有字符是否都被覆盖
            if (startCovered && endCovered)
            {
                bool allCharsCovered = true;
                for (int charIdx = keywordRange.startIndex; charIdx <= keywordRange.endIndex; charIdx++)
                {
                    bool charCovered = false;
                    foreach (var range in selectedRanges)
                    {
                        if (range.Contains(charIdx))
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
                if (allCharsCovered)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 应用变量修改
    /// </summary>
    private void ApplyVariableModifications(System.Collections.Generic.List<VariableModifier> modifications)
    {
        if (modifications == null || VariableManager.Instance == null)
        {
            return;
        }

        foreach (var modifier in modifications)
        {
            VariableManager.Instance.ModifyValue(
                modifier.targetVariable,
                modifier.operation,
                modifier.value
            );
        }
    }

    /// <summary>
    /// 自动处理关键节点的分支选择（根据变量条件自动选择）
    /// </summary>
    /// <param name="skipVariableProcessing">是否跳过变量变更处理（如果已经在外部处理过）</param>
    /// <returns>是否成功移动到分支目标节点</returns>
    private bool ProcessKeyNodeAutomatically(bool skipVariableProcessing = false)
    {
        if (currentNode == null || currentNode.GetNodeType() != NodeType.Key)
        {
            Debug.LogWarning("LevelManager: 当前节点不是关键节点");
            return false;
        }

        KeyNodeSO keyNode = currentNode as KeyNodeSO;
        if (keyNode == null)
        {
            Debug.LogWarning("LevelManager: 无法将当前节点转换为 KeyNodeSO");
            return false;
        }

        // 检查 VariableManager 是否存在
        if (VariableManager.Instance == null)
        {
            Debug.LogWarning("LevelManager: VariableManager.Instance 为 null，无法评估分支条件");
            // 如果没有 VariableManager，尝试使用默认节点
            if (keyNode.defaultNode != null)
            {
                return MoveToNode(keyNode.defaultNode);
            }
            return false;
        }

        // 按顺序检查每个分支的条件
        for (int i = 0; i < keyNode.branches.Count; i++)
        {
            BranchLogic branch = keyNode.branches[i];
            if (branch == null) continue;

            // 使用 VariableManager 评估分支条件
            bool conditionMet = VariableManager.Instance.EvaluateBranch(branch);
            
            if (conditionMet)
            {
                Debug.Log($"LevelManager: 分支 '{branch.branchName}' (索引 {i}) 条件满足，切换到目标节点");
                if (branch.targetNode != null)
                {
                    return MoveToNode(branch.targetNode, skipVariableProcessing);
                }
                else
                {
                    Debug.LogWarning($"LevelManager: 分支 '{branch.branchName}' 满足条件但没有目标节点");
                }
            }
        }

        // 所有分支都不满足，使用默认节点
        if (keyNode.defaultNode != null)
        {
            Debug.Log("LevelManager: 所有分支条件都不满足，使用默认节点");
            return MoveToNode(keyNode.defaultNode, skipVariableProcessing);
        }
        else
        {
            Debug.LogWarning("LevelManager: 所有分支条件都不满足，且没有默认节点");
            return false;
        }
    }

    /// <summary>
    /// 处理关键节点的分支选择（根据分支索引，手动选择）
    /// </summary>
    /// <param name="branchIndex">分支索引</param>
    /// <returns>是否成功移动到分支目标节点</returns>
    public bool SelectKeyNodeBranch(int branchIndex)
    {
        if (currentNode == null)
        {
            Debug.LogWarning("LevelManager: currentNode 为 null");
            return false;
        }

        if (currentNode.GetNodeType() != NodeType.Key)
        {
            Debug.LogWarning($"LevelManager: 当前节点不是关键节点，无法选择分支");
            return false;
        }

        KeyNodeSO keyNode = currentNode as KeyNodeSO;
        if (keyNode == null)
        {
            Debug.LogWarning("LevelManager: 无法将当前节点转换为 KeyNodeSO");
            return false;
        }

        if (branchIndex < 0 || branchIndex >= keyNode.branches.Count)
        {
            Debug.LogWarning($"LevelManager: 分支索引 {branchIndex} 超出范围 (0-{keyNode.branches.Count - 1})");
            // 尝试使用默认节点
            if (keyNode.defaultNode != null)
            {
                return MoveToNode(keyNode.defaultNode);
            }
            return false;
        }

        BranchLogic branch = keyNode.branches[branchIndex];
        if (branch.targetNode != null)
        {
            return MoveToNode(branch.targetNode);
        }
        else
        {
            // 分支没有目标节点，尝试使用默认节点
            if (keyNode.defaultNode != null)
            {
                return MoveToNode(keyNode.defaultNode);
            }
            Debug.LogWarning($"LevelManager: 分支 {branchIndex} 没有目标节点，且没有默认节点");
            return false;
        }
    }

    /// <summary>
    /// 处理QTE节点的结果
    /// </summary>
    /// <param name="success">QTE是否成功</param>
    /// <returns>是否成功移动到结果节点</returns>
    public bool HandleQTEResult(bool success)
    {
        if (currentNode == null)
        {
            Debug.LogWarning("LevelManager: currentNode 为 null");
            return false;
        }

        if (currentNode.GetNodeType() != NodeType.QTE)
        {
            Debug.LogWarning($"LevelManager: 当前节点不是QTE节点，无法处理QTE结果");
            return false;
        }

        QTENodeSO qteNode = currentNode as QTENodeSO;
        if (qteNode == null)
        {
            Debug.LogWarning("LevelManager: 无法将当前节点转换为 QTENodeSO");
            return false;
        }

        BaseNodeSO targetNode = success ? qteNode.successNode : qteNode.failureNode;
        if (targetNode != null)
        {
            return MoveToNode(targetNode);
        }
        else
        {
            Debug.LogWarning($"LevelManager: QTE节点没有{(success ? "成功" : "失败")}目标节点");
            return false;
        }
    }

    /// <summary>
    /// 检查当前节点是否为结局节点
    /// </summary>
    public bool IsCurrentNodeEnd()
    {
        return currentNode != null && currentNode.GetNodeType() == NodeType.End;
    }

    /// <summary>
    /// 重置到起始节点
    /// </summary>
    public void ResetToStart()
    {
        InitializeNodeGraph();
    }

    /// <summary>
    /// 节点切换后的回调，通知UI更新并重置选择
    /// </summary>
    private void OnNodeChanged()
    {
        // 重置所有选择（清除被选中的列表、UI元素以及黑框）
        if (SelectionManager.instance != null)
        {
            SelectionManager.instance.ClearAllSelections();
        }

        // 取消之前的倒计时订阅（如果有）
        if (UIManager.instance != null && UIManager.instance.countdownText != null)
        {
            CountdownTimer countdownTimer = UIManager.instance.countdownText.GetComponent<CountdownTimer>();
            if (countdownTimer != null)
            {
                countdownTimer.OnCountdownFinished -= OnQTECountdownFinished;
            }
        }

        // 更新UIManager的内容输入框
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateContentInputField();
        }

        // 处理QTE节点的倒计时
        HandleQTECountdown();
    }

    /// <summary>
    /// 处理QTE节点的倒计时
    /// </summary>
    private void HandleQTECountdown()
    {
        if (currentNode == null)
        {
            Debug.LogWarning("LevelManager: currentNode 为 null，无法处理倒计时");
            return;
        }

        // 检查是否为QTE节点
        if (currentNode.GetNodeType() == NodeType.QTE)
        {
            QTENodeSO qteNode = currentNode as QTENodeSO;
            if (qteNode == null)
            {
                Debug.LogWarning("LevelManager: 无法将当前节点转换为 QTENodeSO");
                return;
            }

            if (UIManager.instance == null)
            {
                Debug.LogWarning("LevelManager: UIManager.instance 为 null");
                return;
            }

            if (UIManager.instance.countdownText == null)
            {
                Debug.LogWarning("LevelManager: UIManager.instance.countdownText 为 null");
                return;
            }

            CountdownTimer countdownTimer = UIManager.instance.countdownText.GetComponent<CountdownTimer>();
            if (countdownTimer == null)
            {
                Debug.LogWarning("LevelManager: countdownText 上没有 CountdownTimer 组件");
                return;
            }

            // 订阅倒计时结束事件
            countdownTimer.OnCountdownFinished -= OnQTECountdownFinished;
            countdownTimer.OnCountdownFinished += OnQTECountdownFinished;

            // 启动倒计时，使用QTE节点的持续时间
            countdownTimer.StartCountdown(qteNode.duration);
            Debug.Log($"LevelManager: QTE节点，启动倒计时: {qteNode.duration}秒");
        }
        else
        {
            // 非QTE节点，停止倒计时并取消订阅
            if (UIManager.instance != null && UIManager.instance.countdownText != null)
            {
                CountdownTimer countdownTimer = UIManager.instance.countdownText.GetComponent<CountdownTimer>();
                if (countdownTimer != null)
                {
                    countdownTimer.OnCountdownFinished -= OnQTECountdownFinished;
                    countdownTimer.StopCountdown();
                }
            }
        }
    }

    /// <summary>
    /// 处理QTE节点的判定逻辑
    /// 根据E变量的值判定成功或失败：E=0为失败，E=1为成功
    /// </summary>
    public void ProcessQTEResult()
    {
        if (currentNode == null || currentNode.GetNodeType() != NodeType.QTE)
        {
            Debug.LogWarning("LevelManager: 当前节点不是QTE节点，无法处理QTE判定");
            return;
        }

        // 检查VariableManager是否存在
        if (VariableManager.Instance == null)
        {
            Debug.LogWarning("LevelManager: VariableManager.Instance 为 null，无法判定QTE结果");
            return;
        }

        // 获取E变量的值
        float eValue = VariableManager.Instance.GetValue(GameVariable.E);

        // 根据E的值判定：E=0为失败，E=1为成功
        bool success = Mathf.Approximately(eValue, 1f);
        
        Debug.Log($"LevelManager: QTE判定，E值={eValue}，判定为{(success ? "成功" : "失败")}");

        // 处理QTE结果
        HandleQTEResult(success);
    }

    /// <summary>
    /// QTE倒计时结束回调
    /// 根据E变量的值判定成功或失败
    /// </summary>
    private void OnQTECountdownFinished()
    {
        ProcessQTEResult();
    }

    protected override void Awake()
    {
        base.Awake();
        
        // 如果配置了节点图，自动初始化
        if (nodeGraph != null)
        {
            InitializeNodeGraph();
        }
    }
}
