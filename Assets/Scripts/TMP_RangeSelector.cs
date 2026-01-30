using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// 处理鼠标输入，实现左键选中和右键擦除功能
/// </summary>
public class TMP_RangeSelector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IDragHandler
{
    [Header("组件引用")]
    [Tooltip("要应用选择的 TMP_Text 组件（如果脚本挂在同一 GameObject 上会自动获取）")]
    public TMP_Text textComponent;

    private Camera eventCamera;
    private bool isDragging = false;
    private bool isRightDragging = false;
    private int dragStartIndex = -1;
    private int dragEndIndex = -1;
    private Range tempRange = null;

    void Start()
    {
        // 获取 TMP_Text 组件
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }

        if (textComponent == null)
        {
            Debug.LogError("TMP_RangeSelector: 找不到 TMP_Text 组件！请手动设置 Text Component 字段。");
        }
        else
        {
            // 确保 raycastTarget 开启
            if (!textComponent.raycastTarget)
            {
                textComponent.raycastTarget = true;
            }
        }

        // 获取事件相机
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }
        else
        {
            eventCamera = Camera.main;
        }
        
        // 检查事件系统
        if (EventSystem.current == null)
        {
            Debug.LogError("TMP_RangeSelector: 场景中没有 EventSystem！请添加 EventSystem 组件。");
        }
    }

    void Update()
    {
        if (textComponent == null) return;

        // 处理左键拖拽
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            if (isDragging && mouse.leftButton.isPressed)
            {
                HandleLeftDrag();
            }
            else if (isDragging && !mouse.leftButton.isPressed)
            {
                // 鼠标已释放，但isDragging还没重置（可能在Update中先于OnPointerUp执行）
                OnLeftDragEnd();
            }

            // 处理右键拖拽擦除
            if (isRightDragging && mouse.rightButton.isPressed)
            {
                HandleRightDrag();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (textComponent == null) return;

        int charIndex = GetCharacterIndexAtPosition(eventData.position);
        if (charIndex < 0) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 左键按下
            dragStartIndex = charIndex;
            dragEndIndex = charIndex;
            isDragging = true;

            // 默认支持多选，不清除之前的选择

            // 创建临时Range用于实时显示
            tempRange = new Range(dragStartIndex, dragEndIndex);
            
            // 立即设置临时Range，显示初始选择点
            if (SelectionManager.instance != null)
            {
                SelectionManager.instance.SetTempRange(textComponent, tempRange);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 右键按下，开始擦除
            isRightDragging = true;
            RemoveSelectionAt(charIndex);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftDragEnd();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            isRightDragging = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标移出时，如果还在拖拽，结束拖拽
        if (isDragging && eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftDragEnd();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (textComponent == null) return;

        if (isDragging && eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftDrag(eventData.position);
        }
        else if (isRightDragging && eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightDrag(eventData.position);
        }
    }

    private void OnLeftDragEnd()
    {
        if (!isDragging) return;

        isDragging = false;

        // 清除临时Range显示
        if (SelectionManager.instance != null)
        {
            SelectionManager.instance.SetTempRange(textComponent, null);
        }

        if (dragStartIndex >= 0 && dragEndIndex >= 0 && SelectionManager.instance != null)
        {
            // 确定最终范围
            int start = Mathf.Min(dragStartIndex, dragEndIndex);
            int end = Mathf.Max(dragStartIndex, dragEndIndex);

            // 如果范围有效，添加到SelectionManager
            if (start != end)
            {
                SelectionManager.instance.AddSelection(textComponent, start, end, true);
                Debug.Log($"选区: [{start}, {end}]");
            }
        }

        // 清除临时Range
        tempRange = null;
        dragStartIndex = -1;
        dragEndIndex = -1;
    }

    private void HandleLeftDrag()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos2D = mouse.position.ReadValue();
        Vector3 mousePosition = new Vector3(mousePos2D.x, mousePos2D.y, 0);
        HandleLeftDrag(mousePosition);
    }

    private void HandleLeftDrag(Vector3 screenPosition)
    {
        if (textComponent == null) return;

        int charIndex = GetCharacterIndexAtPosition(screenPosition);

        if (charIndex >= 0 && charIndex != dragEndIndex)
        {
            dragEndIndex = charIndex;
            
            // 更新临时Range
            int start = Mathf.Min(dragStartIndex, dragEndIndex);
            int end = Mathf.Max(dragStartIndex, dragEndIndex);
            tempRange = new Range(start, end);

            // 实时更新UI显示（使用临时Range）
            if (SelectionManager.instance != null)
            {
                SelectionManager.instance.SetTempRange(textComponent, tempRange);
            }
        }
    }

    private void HandleRightDrag()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos2D = mouse.position.ReadValue();
        Vector3 mousePosition = new Vector3(mousePos2D.x, mousePos2D.y, 0);
        HandleRightDrag(mousePosition);
    }

    private void HandleRightDrag(Vector3 screenPosition)
    {
        if (textComponent == null) return;

        int charIndex = GetCharacterIndexAtPosition(screenPosition);

        if (charIndex >= 0)
        {
            RemoveSelectionAt(charIndex);
        }
    }

    private void RemoveSelectionAt(int charIndex)
    {
        if (SelectionManager.instance != null)
        {
            SelectionManager.instance.RemoveSelectionAt(textComponent, charIndex);
        }
    }


    /// <summary>
    /// 根据屏幕位置获取字符索引
    /// </summary>
    private int GetCharacterIndexAtPosition(Vector3 screenPosition)
    {
        if (textComponent == null) return -1;

        // 确保文本已更新
        textComponent.ForceMeshUpdate();
        
        if (textComponent.textInfo == null || textComponent.textInfo.characterCount == 0)
        {
            return -1;
        }

        // 使用TMP_TextUtilities查找字符
        int charIndex = TMP_TextUtilities.FindIntersectingCharacter(
            textComponent, 
            screenPosition, 
            eventCamera, 
            true
        );

        return charIndex;
    }
}
