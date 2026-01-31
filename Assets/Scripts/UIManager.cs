using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI管理器
/// 管理整体UI，包括提交按钮等
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [Header("UI组件")]
    [Tooltip("提交按钮")]
    public Button submitButton;

    [Tooltip("剩余黑块数显示文本")]
    public TMP_Text remainingBlockCountText;

    [Tooltip("文本输入框（用于显示节点内容）")]
    public TMP_InputField contentInputField;

    [Tooltip("报纸图片")]
    public UnityEngine.UI.Image newspaperImage;

    [Tooltip("报纸动画控制器")]
    public Animator newspaperAnimator;

    [Tooltip("倒计时文本")]
    public TMP_Text countdownText;

    [Tooltip("注释文本")]
    public TMP_Text commentText;

    [Tooltip("贴纸GameObject")]
    public GameObject stickerObject;

    [Tooltip("进入下一关按钮")]
    public Button nextLevelButton;

    void Start()
    {
        // 绑定提交按钮的点击事件
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
        }

        // 绑定进入下一关按钮的点击事件
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
            // 初始状态：隐藏进入下一关按钮（必须点了提交后才显示）
            nextLevelButton.gameObject.SetActive(false);
        }

        // 初始化剩余黑块数显示
        UpdateRemainingBlockCount();

        // 初始化内容输入框
        UpdateContentInputField();

        // 初始化倒计时文本（初始状态隐藏）
        if (countdownText != null)
        {
            countdownText.enabled = false;
        }

        // 初始化注释文本
        UpdateCommentText();
    }

    /// <summary>
    /// 更新剩余黑块数显示
    /// </summary>
    public void UpdateRemainingBlockCount()
    {
        if (remainingBlockCountText != null && SelectionManager.instance != null)
        {
            int remaining = SelectionManager.instance.GetRemainingBlockCount();
            if (remaining <= 0)
            {
                remainingBlockCountText.text = "黑块数已达上限";
            }
            else
            {
                remainingBlockCountText.text = $"剩余黑块数：{remaining}";
            }
        }
    }

    /// <summary>
    /// 更新剩余黑块数显示（考虑临时Range）
    /// </summary>
    /// <param name="textComponent">文本组件</param>
    public void UpdateRemainingBlockCountWithTemp(TMP_Text textComponent)
    {
        if (remainingBlockCountText != null && SelectionManager.instance != null && textComponent != null)
        {
            // 获取当前已使用的黑块数
            int currentUsed = SelectionManager.instance.GetUsedBlockCount();
            
            // 获取临时Range
            Range tempRange = SelectionManager.instance.GetTempRange(textComponent);
            if (tempRange != null)
            {
                // 检查临时Range是否会超出限制（模拟添加）
                var (wouldExceed, totalAfterAdd) = SelectionManager.instance.CheckTempRangeLimit(textComponent, tempRange, true);
                int maxBlockCount = SelectionManager.instance.GetMaxBlockCount();
                int remaining = maxBlockCount - totalAfterAdd;
                if (remaining <= 0)
                {
                    remainingBlockCountText.text = "黑块数已达上限";
                }
                else
                {
                    remainingBlockCountText.text = $"剩余黑块数：{remaining}";
                }
            }
            else
            {
                // 没有临时Range，使用正常计算
                UpdateRemainingBlockCount();
            }
        }
    }

    /// <summary>
    /// 更新内容输入框，从当前节点获取并填入解析后的文本
    /// </summary>
    public void UpdateContentInputField()
    {
        if (contentInputField == null)
        {
            return;
        }

        // 获取当前节点
        if (LevelManager.instance == null || LevelManager.instance.currentNode == null)
        {
            contentInputField.text = "";
            return;
        }

        BaseNodeSO currentNode = LevelManager.instance.currentNode;
        NodeType nodeType = currentNode.GetNodeType();

        // 根据节点类型获取 content
        NodeContentData content = null;
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

        // 如果有 content，使用解析器解析并获取文本
        if (content != null)
        {
            if (ScriptableObjectParser.instance != null)
            {
                NodeContentParseResult parseResult = ScriptableObjectParser.instance.ParseNodeContent(content);
                if (parseResult != null && !string.IsNullOrEmpty(parseResult.text))
                {
                    contentInputField.text = parseResult.text;
                }
                else
                {
                    contentInputField.text = content.text;
                }
            }
            else
            {
                // 如果没有解析器，直接使用 content.text
                contentInputField.text = content.text ?? "";
            }
        }
        else
        {
            contentInputField.text = "";
        }
    }

    /// <summary>
    /// 更新注释文本，从当前节点获取并显示注释
    /// </summary>
    public void UpdateCommentText()
    {
        if (commentText == null)
        {
            return;
        }

        // 获取当前节点
        if (LevelManager.instance == null || LevelManager.instance.currentNode == null)
        {
            commentText.text = "";
            return;
        }

        BaseNodeSO currentNode = LevelManager.instance.currentNode;
        NodeType nodeType = currentNode.GetNodeType();

        // 根据节点类型获取 content
        NodeContentData content = null;
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

        // 如果有 content，使用解析器解析并获取注释
        if (content != null)
        {
            if (ScriptableObjectParser.instance != null)
            {
                NodeContentParseResult parseResult = ScriptableObjectParser.instance.ParseNodeContent(content);
                if (parseResult != null && !string.IsNullOrEmpty(parseResult.comment))
                {
                    commentText.text = parseResult.comment;
                }
                else
                {
                    commentText.text = content.comment ?? "";
                }
            }
            else
            {
                // 如果没有解析器，直接使用 content.comment
                commentText.text = content.comment ?? "";
            }
        }
        else
        {
            commentText.text = "";
        }
    }

    /// <summary>
    /// 更新贴纸位置，从当前节点获取并设置贴纸的Y轴坐标
    /// </summary>
    public void UpdateStickerPosition()
    {
        if (stickerObject == null)
        {
            return;
        }

        // 获取当前节点
        if (LevelManager.instance == null || LevelManager.instance.currentNode == null)
        {
            return;
        }

        BaseNodeSO currentNode = LevelManager.instance.currentNode;
        NodeType nodeType = currentNode.GetNodeType();

        // 根据节点类型获取 content
        NodeContentData content = null;
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

        // 如果有 content，使用解析器解析并获取贴纸Y轴坐标
        if (content != null)
        {
            float yPosition = 0f;
            if (ScriptableObjectParser.instance != null)
            {
                NodeContentParseResult parseResult = ScriptableObjectParser.instance.ParseNodeContent(content);
                if (parseResult != null)
                {
                    yPosition = parseResult.stickerYPosition;
                }
                else
                {
                    yPosition = content.stickerYPosition;
                }
            }
            else
            {
                // 如果没有解析器，直接使用 content.stickerYPosition
                yPosition = content.stickerYPosition;
            }

            // 更新贴纸的Y轴坐标（使用RectTransform的anchoredPosition）
            RectTransform rectTransform = stickerObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 currentAnchoredPosition = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = new Vector2(currentAnchoredPosition.x, yPosition);
            }
            else
            {
                Debug.LogWarning("UIManager: stickerObject 没有 RectTransform 组件，无法更新Y轴坐标");
            }
        }
    }

    /// <summary>
    /// 提交按钮点击事件
    /// </summary>
    private void OnSubmitButtonClicked()
    {
        if (LevelManager.instance == null)
        {
            Debug.LogWarning("UIManager: LevelManager.instance 为 null");
            return;
        }

        if (LevelManager.instance.currentNode == null)
        {
            Debug.LogWarning("UIManager: 当前没有节点");
            return;
        }

        // 获取 InputField 的文本组件（用于检查关键词状态）
        TMP_Text textComponent = null;
        if (contentInputField != null)
        {
            textComponent = contentInputField.textComponent;
        }

        // 1. 触发计分逻辑：处理当前节点的变量变更规则
        // 这会检查关键词条件，如果满足则应用变量变更
        LevelManager.VariableChangeResult result = LevelManager.instance.ProcessVariableChangesForCurrentNode(textComponent);

        // 打印满足的判定条件和数值变化
        Debug.Log("========== 提交结果 ==========");
        Debug.Log($"节点: {LevelManager.instance.currentNode.nodeName}");
        
        if (result.satisfiedConditions.Count > 0)
        {
            Debug.Log("满足的判定条件:");
            for (int i = 0; i < result.satisfiedConditions.Count; i++)
            {
                Debug.Log($"  [{i + 1}] {result.satisfiedConditions[i]}");
            }
        }
        else
        {
            Debug.Log("没有满足任何判定条件");
        }

        if (result.variableChanges.Count > 0)
        {
            Debug.Log("数值变化:");
            foreach (var change in result.variableChanges)
            {
                Debug.Log($"  • {change}");
            }
        }
        else
        {
            Debug.Log("没有数值变化");
        }
        Debug.Log("==============================");

        // 2. 检查是否为QTE节点，如果是则使用特殊的判定逻辑
        NodeType nodeType = LevelManager.instance.currentNode.GetNodeType();
        if (nodeType == NodeType.QTE)
        {
            // QTE节点：使用E变量判定逻辑
            LevelManager.instance.ProcessQTEResult();
        }
        else
        {
            // 非QTE节点：触发更新报纸的逻辑
            UpdateNewspaper(result);
        }
    }

    /// <summary>
    /// 更新报纸的逻辑
    /// </summary>
    /// <param name="result">变量变更处理结果</param>
    private void UpdateNewspaper(LevelManager.VariableChangeResult result)
    {
        if (newspaperImage == null)
        {
            Debug.LogWarning("UIManager: newspaperImage 为 null，无法更新报纸");
            return;
        }

        if (LevelManager.instance == null || LevelManager.instance.currentNode == null)
        {
            Debug.LogWarning("UIManager: 当前没有节点，无法获取报纸图片");
            return;
        }

        // 获取当前节点的content
        BaseNodeSO currentNode = LevelManager.instance.currentNode;
        NodeType nodeType = currentNode.GetNodeType();

        NodeContentData content = null;
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

        // 如果content中有报纸图片配置，则替换
        if (content != null && content.newspaperImage != null)
        {
            newspaperImage.sprite = content.newspaperImage;
            Debug.Log($"UIManager: 已更新报纸图片: {content.newspaperImage.name}");
        }
        else
        {
            Debug.LogWarning("UIManager: 当前节点没有配置报纸图片");
        }

        // 触发报纸动画
        if (newspaperAnimator != null)
        {
            newspaperAnimator.enabled = true;
            Debug.Log("UIManager: 已打开报纸动画");
        }

        // 显示进入下一关按钮
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 进入下一关按钮点击事件
    /// </summary>
    private void OnNextLevelButtonClicked()
    {
        if (LevelManager.instance == null)
        {
            Debug.LogWarning("UIManager: LevelManager.instance 为 null");
            return;
        }

        if (LevelManager.instance.currentNode == null)
        {
            Debug.LogWarning("UIManager: 当前没有节点");
            return;
        }

        // 跳转到下一个节点
        bool moved = LevelManager.instance.MoveToNextNode();
        
        if (!moved)
        {
            // 如果无法移动到下一个节点（可能是结局节点），给出提示
            if (LevelManager.instance.IsCurrentNodeEnd())
            {
                Debug.Log("UIManager: 已到达结局节点，无法继续");
            }
            else
            {
                Debug.LogWarning("UIManager: 无法移动到下一个节点");
            }
        }

        // 跳转节点后，隐藏进入下一关按钮（需要再次提交后才能显示）
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(false);
        }
    }

    // TODO: 实现其他UI功能
}
