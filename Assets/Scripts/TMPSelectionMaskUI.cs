using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 使用 UI Image 遮罩来实现选中文本的黑色矩形遮罩效果
/// </summary>
public class TMPSelectionMaskUI : MonoBehaviour
{
    [Header("组件引用")]
    [Tooltip("要应用遮罩的 TMP_InputField 组件")]
    public TMP_InputField inputField;
    
    [Header("遮罩设置")]
    [Tooltip("遮罩颜色（默认黑色）")]
    public Color maskColor = Color.black;
    
    [Tooltip("遮罩的父对象（通常是 Canvas 或 InputField 的父对象）")]
    public RectTransform maskParent;

    private TMP_Text textComponent;
    private RectTransform textRectTransform;
    private Canvas canvas;
    private GameObject maskObject;
    private Image maskImage;
    private RectTransform maskRectTransform;
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;

        // 获取组件
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }

        if (inputField == null)
        {
            Debug.LogError("TMPSelectionMaskUI: 找不到 TMP_InputField 组件！");
            return;
        }

        textComponent = inputField.textComponent;

        if (textComponent == null)
        {
            Debug.LogError("TMPSelectionMaskUI: 找不到 TMP_Text 组件！");
            return;
        }

        textRectTransform = textComponent.rectTransform;

        // 获取 Canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        if (canvas == null)
        {
            Debug.LogError("TMPSelectionMaskUI: 找不到 Canvas！");
            return;
        }

        // 确定遮罩的父对象
        // 最好使用文本的父对象，这样坐标转换更简单
        if (maskParent == null)
        {
            maskParent = textRectTransform.parent as RectTransform;
            if (maskParent == null)
            {
                maskParent = canvas.transform as RectTransform;
            }
        }
        
        Debug.Log($"TMPSelectionMaskUI: 遮罩父对象设置\n" +
            $"  - maskParent: {(maskParent != null ? maskParent.name : "null")}\n" +
            $"  - textRectTransform.parent: {(textRectTransform.parent != null ? textRectTransform.parent.name : "null")}");

        // 创建遮罩对象
        CreateMaskObject();

        isInitialized = true;
        Debug.Log("TMPSelectionMaskUI: 初始化完成！");
    }

    /// <summary>
    /// 创建遮罩 UI 对象
    /// </summary>
    private void CreateMaskObject()
    {
        if (maskObject != null) return;

        // 创建 GameObject
        maskObject = new GameObject("SelectionMask");
        maskObject.transform.SetParent(maskParent, false);

        // 添加 RectTransform
        maskRectTransform = maskObject.AddComponent<RectTransform>();
        // 设置 anchor 和 pivot 与父对象相同，这样坐标转换更简单
        // 或者使用 stretch 模式，但这里我们用左上角对齐
        maskRectTransform.anchorMin = new Vector2(0, 1);
        maskRectTransform.anchorMax = new Vector2(0, 1);
        maskRectTransform.pivot = new Vector2(0, 1); // 左上角对齐

        // 添加 Image 组件
        maskImage = maskObject.AddComponent<Image>();
        maskImage.color = maskColor;
        maskImage.raycastTarget = false; // 不阻挡鼠标事件

        // 初始隐藏
        maskObject.SetActive(false);

        // 设置层级：确保遮罩在文本之上
        // 将遮罩放在父对象的最后（最上层）
        maskRectTransform.SetAsLastSibling();
        
        // 如果文本有 CanvasGroup，遮罩也需要考虑
        CanvasGroup maskCanvasGroup = maskObject.AddComponent<CanvasGroup>();
        maskCanvasGroup.blocksRaycasts = false; // 不阻挡射线检测
        maskCanvasGroup.ignoreParentGroups = false;

        Debug.Log($"TMPSelectionMaskUI: 遮罩对象已创建\n" +
            $"  - 遮罩颜色: {maskColor}\n" +
            $"  - 遮罩层级: {maskRectTransform.GetSiblingIndex()}\n" +
            $"  - 文本层级: {textRectTransform.GetSiblingIndex()}");
    }

    /// <summary>
    /// 更新选中区域的遮罩
    /// </summary>
    /// <param name="startIndex">选中文本的起始字符索引</param>
    /// <param name="endIndex">选中文本的结束字符索引</param>
    public void UpdateSelectionMask(int startIndex, int endIndex)
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (!isInitialized || textComponent == null || maskObject == null)
        {
            return;
        }

        // 确保索引有效
        int start = Mathf.Clamp(Mathf.Min(startIndex, endIndex), 0, textComponent.text.Length);
        int end = Mathf.Clamp(Mathf.Max(startIndex, endIndex), 0, textComponent.text.Length);

        if (start >= end)
        {
            ClearSelectionMask();
            return;
        }

        // 确保文本已更新
        textComponent.ForceMeshUpdate();

        // 获取文本信息
        TMP_TextInfo textInfo = textComponent.textInfo;
        if (textInfo == null || textInfo.characterCount == 0)
        {
            ClearSelectionMask();
            return;
        }

        // 计算选中区域的边界框（在文本的局部坐标空间）
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        bool hasSelection = false;

        // 遍历选中的字符
        for (int i = start; i < end && i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            
            // 跳过不可见字符
            if (!charInfo.isVisible)
                continue;

            hasSelection = true;

            // 获取字符的四个顶点位置（在文本的局部坐标空间）
            Vector3 bottomLeft = charInfo.bottomLeft;
            Vector3 topRight = charInfo.topRight;

            minX = Mathf.Min(minX, bottomLeft.x);
            maxX = Mathf.Max(maxX, topRight.x);
            minY = Mathf.Min(minY, bottomLeft.y);
            maxY = Mathf.Max(maxY, topRight.y);
        }

        if (!hasSelection)
        {
            ClearSelectionMask();
            return;
        }

        // 计算遮罩的位置和大小（在文本的局部坐标空间）
        float width = maxX - minX;
        float height = maxY - minY;
        
        // TMP 的坐标系统：Y 轴向上，原点在左下角
        // 计算左上角的位置（在文本的局部坐标空间）
        Vector2 topLeft = new Vector2(minX, maxY);

        // 计算遮罩的位置
        // 将文本局部坐标转换为遮罩父对象的坐标空间
        Vector3 selectionTopLeftLocal = new Vector3(topLeft.x, topLeft.y, 0);
        Vector3 selectionTopLeftWorld = textRectTransform.TransformPoint(selectionTopLeftLocal);
        Vector3 selectionTopLeftParentLocal = maskParent.InverseTransformPoint(selectionTopLeftWorld);
        
        // 计算大小（考虑缩放）
        // 将文本局部空间的大小转换为遮罩父对象的局部空间大小
        Vector3 sizeWorld = textRectTransform.TransformVector(new Vector3(width, height, 0));
        Vector3 sizeParentLocal = maskParent.InverseTransformVector(sizeWorld);
        Vector2 size = new Vector2(Mathf.Abs(sizeParentLocal.x), Mathf.Abs(sizeParentLocal.y));
        
        // 更新遮罩
        // 由于遮罩的 anchor 和 pivot 都是 (0, 1)，anchoredPosition 就是相对于父对象左上角的位置
        // 但是需要考虑父对象的 anchor 和 pivot
        // 如果父对象的 anchor 是拉伸的，我们需要计算相对于父对象左上角的偏移
        Vector2 maskPosition;
        
        // 获取父对象的左上角世界坐标
        Vector3[] parentCorners = new Vector3[4];
        maskParent.GetWorldCorners(parentCorners);
        Vector3 parentTopLeftWorld = parentCorners[1]; // 索引1是左上角
        
        // 计算相对于父对象左上角的局部偏移
        Vector3 offsetWorld = selectionTopLeftWorld - parentTopLeftWorld;
        Vector3 offsetParentLocal = maskParent.InverseTransformVector(offsetWorld);
        maskPosition = new Vector2(offsetParentLocal.x, offsetParentLocal.y);
        
        // 更新遮罩
        maskRectTransform.anchoredPosition = maskPosition;
        maskRectTransform.sizeDelta = size;
        
        // 确保遮罩在最上层（在文本之上）
        maskRectTransform.SetAsLastSibling();
        
        // 确保颜色正确
        maskImage.color = maskColor;
        
        // 显示遮罩
        maskObject.SetActive(true);
        
        // 强制更新 Canvas
        Canvas.ForceUpdateCanvases();
        
        // 调试：检查遮罩是否在可见区域
        Vector3[] maskCorners = new Vector3[4];
        maskRectTransform.GetWorldCorners(maskCorners);
        
        // 获取文本的世界坐标用于对比
        Vector3[] textCorners = new Vector3[4];
        textRectTransform.GetWorldCorners(textCorners);
        
        Debug.Log($"TMPSelectionMaskUI: 遮罩已更新\n" +
            $"  - 选中范围: [{start}, {end})\n" +
            $"  - 边界框: minX={minX:F2}, maxX={maxX:F2}, minY={minY:F2}, maxY={maxY:F2}\n" +
            $"  - 文本局部坐标 (topLeft): ({topLeft.x:F2}, {topLeft.y:F2})\n" +
            $"  - 遮罩位置: ({maskPosition.x:F2}, {maskPosition.y:F2})\n" +
            $"  - 大小: {size}\n" +
            $"  - 遮罩世界坐标 - 左下: ({maskCorners[0].x:F2}, {maskCorners[0].y:F2})\n" +
            $"  - 遮罩世界坐标 - 左上: ({maskCorners[1].x:F2}, {maskCorners[1].y:F2})\n" +
            $"  - 遮罩世界坐标 - 右上: ({maskCorners[2].x:F2}, {maskCorners[2].y:F2})\n" +
            $"  - 遮罩世界坐标 - 右下: ({maskCorners[3].x:F2}, {maskCorners[3].y:F2})\n" +
            $"  - 文本世界坐标 - 左上: ({textCorners[1].x:F2}, {textCorners[1].y:F2})\n" +
            $"  - 遮罩颜色: {maskImage.color}\n" +
            $"  - 遮罩是否激活: {maskObject.activeSelf}\n" +
            $"  - 遮罩层级: {maskRectTransform.GetSiblingIndex()}\n" +
            $"  - 文本层级: {textRectTransform.GetSiblingIndex()}\n" +
            $"  - maskParent anchor: {maskParent.anchorMin} ~ {maskParent.anchorMax}\n" +
            $"  - maskParent pivot: {maskParent.pivot}\n" +
            $"  - maskRectTransform anchor: {maskRectTransform.anchorMin} ~ {maskRectTransform.anchorMax}\n" +
            $"  - maskRectTransform pivot: {maskRectTransform.pivot}");
    }

    /// <summary>
    /// 清除选中区域的遮罩
    /// </summary>
    public void ClearSelectionMask()
    {
        if (maskObject != null)
        {
            maskObject.SetActive(false);
            Debug.Log("TMPSelectionMaskUI: 遮罩已清除");
        }
    }

    void OnDestroy()
    {
        // 清理遮罩对象
        if (maskObject != null)
        {
            Destroy(maskObject);
        }
    }

    void OnDisable()
    {
        ClearSelectionMask();
    }
}
