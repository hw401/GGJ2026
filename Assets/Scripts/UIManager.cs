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

    void Start()
    {
        // 绑定提交按钮的点击事件
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
        }

        // 初始化剩余黑块数显示
        UpdateRemainingBlockCount();
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
    /// 提交按钮点击事件
    /// </summary>
    private void OnSubmitButtonClicked()
    {
        // 触发判分逻辑
        if (KeywordManager.instance != null)
        {
            KeywordManager.instance.CheckKeywords();
        }
    }

    // TODO: 实现其他UI功能
}
