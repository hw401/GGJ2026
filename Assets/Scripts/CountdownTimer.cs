using UnityEngine;
using TMPro;

/// <summary>
/// 倒计时计时器
/// 用于显示倒计时并处理倒计时结束事件
/// 持续时间从ScriptableObject中读取
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class CountdownTimer : MonoBehaviour
{
    private TMP_Text countdownText;
    private float remainingTime = 0f;
    private float totalDuration = 0f;
    private bool isRunning = false;

    // 事件：倒计时结束
    public System.Action OnCountdownFinished;

    private void Awake()
    {
        countdownText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        // 初始状态隐藏文本
        if (countdownText != null)
        {
            countdownText.enabled = false;
        }
    }

    private void Update()
    {
        if (isRunning)
        {
            UpdateCountdown();
        }
    }

    /// <summary>
    /// 开始倒计时
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    public void StartCountdown(float duration)
    {
        if (duration <= 0f)
        {
            Debug.LogWarning("CountdownTimer: 持续时间必须大于0");
            return;
        }

        totalDuration = duration;
        remainingTime = duration;
        isRunning = true;
        
        // 显示倒计时文本
        if (countdownText != null)
        {
            countdownText.enabled = true;
        }
        
        // 立即更新一次显示
        UpdateDisplay();

        Debug.Log($"CountdownTimer: 开始倒计时，持续时间: {duration}秒，文本已激活");
    }

    /// <summary>
    /// 停止倒计时
    /// </summary>
    public void StopCountdown()
    {
        isRunning = false;
        remainingTime = 0f;
        
        // 隐藏倒计时文本
        if (countdownText != null)
        {
            countdownText.enabled = false;
        }
    }

    /// <summary>
    /// 暂停倒计时
    /// </summary>
    public void PauseCountdown()
    {
        isRunning = false;
    }

    /// <summary>
    /// 恢复倒计时
    /// </summary>
    public void ResumeCountdown()
    {
        if (remainingTime > 0f)
        {
            isRunning = true;
        }
    }

    /// <summary>
    /// 更新倒计时
    /// </summary>
    private void UpdateCountdown()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
            
            // 更新显示
            UpdateDisplay();
            
            // 触发结束事件
            OnCountdownFinished?.Invoke();
            
            Debug.Log("CountdownTimer: 倒计时结束");
        }
        else
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// 更新显示
    /// </summary>
    private void UpdateDisplay()
    {
        if (countdownText == null) return;

        // 显示剩余时间（保留1位小数）
        countdownText.text = remainingTime.ToString("F1");
    }

    /// <summary>
    /// 获取剩余时间
    /// </summary>
    public float GetRemainingTime()
    {
        return remainingTime;
    }

    /// <summary>
    /// 获取总持续时间
    /// </summary>
    public float GetTotalDuration()
    {
        return totalDuration;
    }

    /// <summary>
    /// 获取进度（0-1）
    /// </summary>
    public float GetProgress()
    {
        if (totalDuration <= 0f) return 0f;
        return 1f - (remainingTime / totalDuration);
    }

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning()
    {
        return isRunning;
    }
}
