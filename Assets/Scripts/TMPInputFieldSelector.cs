using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TMPInputFieldSelector : MonoBehaviour, IPointerUpHandler
{
    public TMP_InputField inputField;

    [Header("遮罩组件")]
    [Tooltip("拖入 TMPSelectionMaskUI 组件（如果脚本挂在同一 GameObject 上会自动获取）")]
    public TMPSelectionMaskUI maskUI;

    // 当前选中的字符串
    public string SelectedText { get; private set; }

    private int lastStartIndex = -1;
    private int lastEndIndex = -1;

    void Start()
    {
        // 如果没有手动赋值，尝试自动获取
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }

        // 如果没有手动指定遮罩组件，尝试自动获取
        if (maskUI == null)
        {
            maskUI = GetComponent<TMPSelectionMaskUI>();
            if (maskUI == null)
            {
                Debug.LogWarning("TMPInputFieldSelector: 找不到 TMPSelectionMaskUI 组件！\n" +
                    "请确保：\n" +
                    "1. TMPSelectionMaskUI 脚本已添加到同一个 GameObject 上\n" +
                    "2. 或者在 Inspector 中手动指定 Mask UI 字段");
            }
        }

        // 禁用 Unity 默认的蓝色选择高亮
        if (inputField != null)
        {
            inputField.selectionColor = new Color(0, 0, 0, 0);
        }

        // 添加选择事件监听
        if (inputField != null)
        {
            inputField.onSelect.AddListener(OnInputFieldSelect);
            inputField.onDeselect.AddListener(OnInputFieldDeselect);
        }
    }

    void Update()
    {
        // 持续检查是否有文本被选中
        if (inputField != null && inputField.isFocused)
        {
            CheckSelection();
        }
    }

    void OnInputFieldSelect(string text)
    {
        // 输入框获得焦点时
    }

    void OnInputFieldDeselect(string text)
    {
        // 输入框失去焦点时，清除遮罩
        if (maskUI != null)
        {
            maskUI.ClearSelectionMask();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 鼠标抬起时检查
        CheckSelection();
    }

    private void CheckSelection()
    {
        if (inputField == null) return;
        if (!inputField.isFocused) return;

        int anchorPos = inputField.selectionAnchorPosition;
        int focusPos = inputField.selectionFocusPosition;

        // 如果没有选择（两个位置相同），直接返回
        if (anchorPos == focusPos)
        {
            return;
        }

        // 如果选择位置没有变化，不重复打印
        int start = Mathf.Min(anchorPos, focusPos);
        int end = Mathf.Max(anchorPos, focusPos);

        if (start == lastStartIndex && end == lastEndIndex)
        {
            return;
        }

        // 更新记录
        lastStartIndex = start;
        lastEndIndex = end;

        // 提取选中的文本
        if (start >= 0 && end <= inputField.text.Length && start < end)
        {
            SelectedText = inputField.text.Substring(start, end - start);
            Debug.Log($"选中的文字：\"{SelectedText}\"  起始:{start}  结束:{end}");

            // 使用黑色遮罩方案
            if (maskUI != null)
            {
                maskUI.UpdateSelectionMask(start, end);
            }
            else
            {
                Debug.LogWarning("TMPInputFieldSelector: maskUI 为 null！请确保 TMPSelectionMaskUI 组件已添加。");
            }
        }
        else
        {
            // 如果没有选中文本，清除遮罩
            if (maskUI != null)
            {
                maskUI.ClearSelectionMask();
            }
        }
    }

    void OnDestroy()
    {
        // 清理事件监听
        if (inputField != null)
        {
            inputField.onSelect.RemoveListener(OnInputFieldSelect);
            inputField.onDeselect.RemoveListener(OnInputFieldDeselect);
        }
    }
}
