using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 自动滚动ScrollView脚本
/// 挂载到ScrollView上，可以自动向下滚动文本
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class AutoScrollView : MonoBehaviour
{
    [Header("滚动设置")]
    [Tooltip("滚动速度（每秒滚动的像素数）")]
    public float scrollSpeed = 100f;

    private ScrollRect scrollRect;
    private RectTransform content;
    private bool isScrolling = false;
    private Coroutine scrollCoroutine;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            Debug.LogError("AutoScrollView: 找不到 ScrollRect 组件");
            return;
        }

        content = scrollRect.content;
        if (content == null)
        {
            Debug.LogError("AutoScrollView: ScrollRect 的 content 为 null");
        }
    }

    void Start()
    {
        StartAutoScroll();
    }

    void OnEnable()
    {
        if (!isScrolling)
        {
            StartAutoScroll();
        }
    }

    void OnDisable()
    {
        StopAutoScroll();
    }

    /// <summary>
    /// 开始自动滚动
    /// </summary>
    public void StartAutoScroll()
    {
        if (scrollRect == null || content == null)
        {
            return;
        }

        if (isScrolling)
        {
            return;
        }

        isScrolling = true;
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
        }
        scrollCoroutine = StartCoroutine(AutoScrollCoroutine());
    }

    /// <summary>
    /// 停止自动滚动
    /// </summary>
    public void StopAutoScroll()
    {
        isScrolling = false;
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
    }

    /// <summary>
    /// 自动滚动协程
    /// </summary>
    private IEnumerator AutoScrollCoroutine()
    {
        while (isScrolling)
        {
            // 计算可滚动的高度
            float scrollableHeight = content.rect.height - scrollRect.viewport.rect.height;
            
            if (scrollableHeight > 0)
            {
                // 获取当前滚动位置（0 = 顶部, 1 = 底部）
                float currentPosition = scrollRect.verticalNormalizedPosition;

                // 向下滚动
                float scrollDelta = scrollSpeed * Time.deltaTime / scrollableHeight;
                float newPosition = currentPosition - scrollDelta;

                // 检查是否到达底部
                if (newPosition <= 0f)
                {
                    scrollRect.verticalNormalizedPosition = 0f;
                    // 停止滚动
                    isScrolling = false;
                    break;
                }
                else
                {
                    scrollRect.verticalNormalizedPosition = newPosition;
                }
            }
            else
            {
                // 内容不足以滚动，等待一下再检查
                yield return new WaitForSeconds(0.5f);
            }

            yield return null;
        }

        scrollCoroutine = null;
    }

    /// <summary>
    /// 立即滚动到底部
    /// </summary>
    public void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    /// <summary>
    /// 立即滚动到顶部
    /// </summary>
    public void ScrollToTop()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    /// <summary>
    /// 当内容更新时调用（可以在外部调用，比如文本更新后）
    /// </summary>
    public void OnContentUpdated()
    {
        // 延迟一帧，确保布局更新完成
        StartCoroutine(ScrollToBottomDelayed());
    }

    private IEnumerator ScrollToBottomDelayed()
    {
        yield return null; // 等待一帧，确保布局更新
        ScrollToBottom();
    }

    /// <summary>
    /// 设置滚动速度
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = Mathf.Max(0f, speed);
    }

}
