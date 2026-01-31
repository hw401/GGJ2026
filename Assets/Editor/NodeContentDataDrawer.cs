#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomPropertyDrawer(typeof(NodeContentData))]
public class NodeContentDataDrawer : PropertyDrawer
{
    // 用于存储每个属性的状态
    private class PropertyState
    {
        public Vector2 scrollPosition = Vector2.zero;
        public bool showKeywords = true;
        public bool showVariableChanges = true;
        public bool showAdvanced = false;
        public string lastSelectedText = "";
        public int lastSelectionStart = -1;
        public int lastSelectionEnd = -1;
        public Dictionary<int, bool> keywordEditStates = new Dictionary<int, bool>(); // 记录每个关键词是否处于编辑模式
    }

    private Dictionary<string, PropertyState> propertyStates = new Dictionary<string, PropertyState>();

    private PropertyState GetState(SerializedProperty property)
    {
        string key = property.propertyPath;
        if (!propertyStates.ContainsKey(key))
        {
            propertyStates[key] = new PropertyState();
        }
        return propertyStates[key];
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var state = GetState(property);

        float height = EditorGUIUtility.singleLineHeight * 1.5f; // 标题高度

        // text字段高度
        var textProp = property.FindPropertyRelative("text");
        height += EditorGUI.GetPropertyHeight(textProp) + EditorGUIUtility.standardVerticalSpacing;

        // 添加关键词按钮高度
        height += EditorGUIUtility.singleLineHeight * 1.5f + EditorGUIUtility.standardVerticalSpacing;

        // keywords字段高度
        var keywordsProp = property.FindPropertyRelative("keywords");
        height += EditorGUIUtility.singleLineHeight * 1.5f; // 折叠标题
        if (state.showKeywords)
        {
            height += EditorGUIUtility.singleLineHeight * 1.5f; // 关键词数量显示

            for (int i = 0; i < keywordsProp.arraySize; i++)
            {
                var keyword = keywordsProp.GetArrayElementAtIndex(i);
                bool isEditing = state.keywordEditStates.ContainsKey(i) && state.keywordEditStates[i];

                if (isEditing)
                {
                    // 编辑模式：显示所有字段
                    height += EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 3;
                }
                else
                {
                    // 预览模式：显示预览信息
                    height += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            height += EditorGUIUtility.singleLineHeight * 1.5f; // 添加按钮
        }

        // variableChanges字段高度
        height += EditorGUIUtility.singleLineHeight * 1.5f; // 折叠标题
        if (state.showVariableChanges)
        {
            var changesProp = property.FindPropertyRelative("variableChanges");
            height += EditorGUI.GetPropertyHeight(changesProp) + EditorGUIUtility.standardVerticalSpacing;
        }

        // 其他字段高度
        var maxBlockProp = property.FindPropertyRelative("maxBlockCount");
        var newspaperProp = property.FindPropertyRelative("newspaperImage");
        var commentProp = property.FindPropertyRelative("comment");
        var stickerYProp = property.FindPropertyRelative("stickerYPosition");

        height += EditorGUI.GetPropertyHeight(maxBlockProp) + EditorGUIUtility.standardVerticalSpacing;
        height += EditorGUI.GetPropertyHeight(newspaperProp) + EditorGUIUtility.standardVerticalSpacing;
        height += EditorGUI.GetPropertyHeight(commentProp) + EditorGUIUtility.standardVerticalSpacing;
        height += EditorGUI.GetPropertyHeight(stickerYProp) + EditorGUIUtility.standardVerticalSpacing;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var state = GetState(property);

        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float width = position.width;

        // 标题
        Rect titleRect = new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight * 1.5f);
        EditorGUI.LabelField(titleRect, "节点内容配置", EditorStyles.boldLabel);
        y += titleRect.height;

        // text字段
        var textProp = property.FindPropertyRelative("text");
        Rect textRect = new Rect(position.x, y, width, EditorGUI.GetPropertyHeight(textProp));
        EditorGUI.PropertyField(textRect, textProp);
        y += textRect.height + EditorGUIUtility.standardVerticalSpacing;

        // 常驻的添加关键词按钮
        Rect addButtonRect = new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight * 1.5f);
        if (GUI.Button(addButtonRect, "🎯 请先选中文本并Ctrl+C，然后点击以添加选中文本为关键词"))
        {
            AddKeywordFromCurrentSelection(property);
        }
        y += addButtonRect.height + EditorGUIUtility.standardVerticalSpacing;

        // keywords字段
        var keywordsProp = property.FindPropertyRelative("keywords");
        Rect keywordsHeaderRect = new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight * 1.5f);
        state.showKeywords = EditorGUI.Foldout(keywordsHeaderRect, state.showKeywords, $"关键词列表 ({keywordsProp.arraySize}个)", true);
        y += keywordsHeaderRect.height;

        if (state.showKeywords)
        {
            // 显示关键词数量和预览
            Rect keywordsInfoRect = new Rect(position.x + 20, y, width - 20, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(keywordsInfoRect, $"当前有 {keywordsProp.arraySize} 个关键词");
            y += keywordsInfoRect.height;

            // 显示每个关键词的预览或编辑界面
            for (int i = 0; i < keywordsProp.arraySize; i++)
            {
                var keyword = keywordsProp.GetArrayElementAtIndex(i);
                bool isEditing = state.keywordEditStates.ContainsKey(i) && state.keywordEditStates[i];

                if (isEditing)
                {
                    DrawKeywordEditor(property, keyword, i, position, ref y, state);
                }
                else
                {
                    DrawKeywordPreview(property, keyword, i, position, ref y, state);
                }
            }

            // 手动添加关键词按钮
            Rect manualAddRect = new Rect(position.x + 20, y, width - 20, EditorGUIUtility.singleLineHeight * 1.5f);
            if (GUI.Button(manualAddRect, "➕ 手动添加关键词"))
            {
                keywordsProp.arraySize++;
                var newKeyword = keywordsProp.GetArrayElementAtIndex(keywordsProp.arraySize - 1);
                newKeyword.FindPropertyRelative("id").stringValue = GetNextKeywordID(property);
                newKeyword.FindPropertyRelative("startIndex").intValue = 0;
                newKeyword.FindPropertyRelative("endIndex").intValue = 0;

                // 新添加的关键词默认进入编辑模式
                state.keywordEditStates[keywordsProp.arraySize - 1] = true;
            }
            y += manualAddRect.height + EditorGUIUtility.standardVerticalSpacing;
        }

        // variableChanges字段
        var changesProp = property.FindPropertyRelative("variableChanges");
        Rect changesHeaderRect = new Rect(position.x, y, width, EditorGUIUtility.singleLineHeight * 1.5f);
        state.showVariableChanges = EditorGUI.Foldout(changesHeaderRect, state.showVariableChanges, "变量变更规则", true);
        y += changesHeaderRect.height;

        if (state.showVariableChanges)
        {
            Rect changesRect = new Rect(position.x, y, width, EditorGUI.GetPropertyHeight(changesProp));
            EditorGUI.PropertyField(changesRect, changesProp);
            y += changesRect.height + EditorGUIUtility.standardVerticalSpacing;
        }

        // 其他字段
        var maxBlockProp = property.FindPropertyRelative("maxBlockCount");
        var newspaperProp = property.FindPropertyRelative("newspaperImage");
        var commentProp = property.FindPropertyRelative("comment");
        var stickerYProp = property.FindPropertyRelative("stickerYPosition");

        Rect maxBlockRect = new Rect(position.x, y, width, EditorGUI.GetPropertyHeight(maxBlockProp));
        EditorGUI.PropertyField(maxBlockRect, maxBlockProp);
        y += maxBlockRect.height + EditorGUIUtility.standardVerticalSpacing;

        Rect newspaperRect = new Rect(position.x, y, width, EditorGUI.GetPropertyHeight(newspaperProp));
        EditorGUI.PropertyField(newspaperRect, newspaperProp);
        y += newspaperRect.height + EditorGUIUtility.standardVerticalSpacing;

        Rect commentRect = new Rect(position.x, y, width, EditorGUI.GetPropertyHeight(commentProp));
        EditorGUI.PropertyField(commentRect, commentProp);
        y += commentRect.height + EditorGUIUtility.standardVerticalSpacing;

        Rect stickerYRect = new Rect(position.x, y, width, EditorGUI.GetPropertyHeight(stickerYProp));
        EditorGUI.PropertyField(stickerYRect, stickerYProp);

        EditorGUI.EndProperty();
    }

    private void DrawKeywordPreview(SerializedProperty property, SerializedProperty keyword, int index, Rect position, ref float y, PropertyState state)
    {
        var textProp = property.FindPropertyRelative("text");
        string fullText = textProp.stringValue;

        string id = keyword.FindPropertyRelative("id").stringValue;
        int startIndex = keyword.FindPropertyRelative("startIndex").intValue;
        int endIndex = keyword.FindPropertyRelative("endIndex").intValue;

        // 检查索引是否有效
        bool isValid = startIndex >= 0 && endIndex <= fullText.Length && startIndex < endIndex;
        string previewText = isValid ? fullText.Substring(startIndex, endIndex - startIndex) : "[索引无效]";

        Rect keywordRect = new Rect(position.x + 20, y, position.width - 40, EditorGUIUtility.singleLineHeight * 2);

        // 背景色
        Color bgColor = isValid ? new Color(0.2f, 0.3f, 0.4f, 0.1f) : new Color(0.8f, 0.3f, 0.3f, 0.1f);
        EditorGUI.DrawRect(keywordRect, bgColor);

        // 关键词信息
        Rect idRect = new Rect(keywordRect.x + 5, keywordRect.y + 2, 50, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(idRect, $"ID: {id}");

        Rect indexRect = new Rect(keywordRect.x + 60, keywordRect.y + 2, 100, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(indexRect, $"[{startIndex}-{endIndex})");

        Rect textRect = new Rect(keywordRect.x + 165, keywordRect.y + 2, keywordRect.width - 170, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(textRect, $"文本: \"{previewText}\"");

        // 操作按钮 - 使用独立的Rect而不是BeginHorizontal
        Rect editButtonRect = new Rect(keywordRect.x + keywordRect.width - 140, keywordRect.y + keywordRect.height - 20, 50, 18);
        Rect deleteButtonRect = new Rect(keywordRect.x + keywordRect.width - 85, keywordRect.y + keywordRect.height - 20, 50, 18);

        if (GUI.Button(editButtonRect, "编辑"))
        {
            state.keywordEditStates[index] = true;
        }

        if (GUI.Button(deleteButtonRect, "删除"))
        {
            var keywordsProp = property.FindPropertyRelative("keywords");
            keywordsProp.DeleteArrayElementAtIndex(index);
            // 清理编辑状态
            state.keywordEditStates.Remove(index);
            // 重新索引
            var newEditStates = new Dictionary<int, bool>();
            foreach (var kvp in state.keywordEditStates)
            {
                if (kvp.Key > index)
                    newEditStates[kvp.Key - 1] = kvp.Value;
                else if (kvp.Key < index)
                    newEditStates[kvp.Key] = kvp.Value;
            }
            state.keywordEditStates = newEditStates;
        }

        y += keywordRect.height + EditorGUIUtility.standardVerticalSpacing;
    }

    private void DrawKeywordEditor(SerializedProperty property, SerializedProperty keyword, int index, Rect position, ref float y, PropertyState state)
    {
        var textProp = property.FindPropertyRelative("text");
        string fullText = textProp.stringValue;

        Rect editorRect = new Rect(position.x + 20, y, position.width - 40, EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 3);

        // 编辑模式背景色
        EditorGUI.DrawRect(editorRect, new Color(0.3f, 0.5f, 0.3f, 0.1f));

        float editorY = editorRect.y + 5;

        // ID字段
        Rect idLabelRect = new Rect(editorRect.x + 5, editorY, 30, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(idLabelRect, "ID:");

        Rect idFieldRect = new Rect(editorRect.x + 40, editorY, 100, EditorGUIUtility.singleLineHeight);
        string newId = EditorGUI.TextField(idFieldRect, keyword.FindPropertyRelative("id").stringValue);
        if (newId != keyword.FindPropertyRelative("id").stringValue)
        {
            keyword.FindPropertyRelative("id").stringValue = newId;
        }

        editorY += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // 起始索引
        Rect startLabelRect = new Rect(editorRect.x + 5, editorY, 60, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(startLabelRect, "起始索引:");

        Rect startFieldRect = new Rect(editorRect.x + 70, editorY, 70, EditorGUIUtility.singleLineHeight);
        int newStart = EditorGUI.IntField(startFieldRect, keyword.FindPropertyRelative("startIndex").intValue);
        if (newStart != keyword.FindPropertyRelative("startIndex").intValue)
        {
            keyword.FindPropertyRelative("startIndex").intValue = Mathf.Clamp(newStart, 0, fullText.Length - 1);
        }

        editorY += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // 结束索引
        Rect endLabelRect = new Rect(editorRect.x + 5, editorY, 60, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(endLabelRect, "结束索引:");

        Rect endFieldRect = new Rect(editorRect.x + 70, editorY, 70, EditorGUIUtility.singleLineHeight);
        int newEnd = EditorGUI.IntField(endFieldRect, keyword.FindPropertyRelative("endIndex").intValue);
        if (newEnd != keyword.FindPropertyRelative("endIndex").intValue)
        {
            keyword.FindPropertyRelative("endIndex").intValue = Mathf.Clamp(newEnd, 1, fullText.Length);
        }

        editorY += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // 预览文本
        int startIndex = keyword.FindPropertyRelative("startIndex").intValue;
        int endIndex = keyword.FindPropertyRelative("endIndex").intValue;
        bool isValid = startIndex >= 0 && endIndex <= fullText.Length && startIndex < endIndex;
        string previewText = isValid ? fullText.Substring(startIndex, endIndex - startIndex) : "[索引无效]";

        Rect previewLabelRect = new Rect(editorRect.x + 5, editorY, 40, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(previewLabelRect, "预览:");

        Rect previewTextRect = new Rect(editorRect.x + 50, editorY, editorRect.width - 55, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(previewTextRect, $"\"{previewText}\"",
            isValid ? EditorStyles.label : new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } });

        // 操作按钮 - 使用独立的Rect
        Rect saveButtonRect = new Rect(editorRect.x + editorRect.width - 100, editorRect.y + editorRect.height - 25, 50, 20);
        Rect cancelButtonRect = new Rect(editorRect.x + editorRect.width - 45, editorRect.y + editorRect.height - 25, 45, 20);

        if (GUI.Button(saveButtonRect, "保存"))
        {
            state.keywordEditStates[index] = false;
        }

        if (GUI.Button(cancelButtonRect, "取消"))
        {
            state.keywordEditStates[index] = false;
            // 可以在这里添加恢复原始值的逻辑
        }

        y += editorRect.height + EditorGUIUtility.standardVerticalSpacing;
    }

    private void AddKeywordFromCurrentSelection(SerializedProperty property)
    {
        var textProp = property.FindPropertyRelative("text");
        string fullText = textProp.stringValue;

        // 获取当前选中的文本
        string selectedText = EditorGUIUtility.systemCopyBuffer;

        if (string.IsNullOrEmpty(selectedText))
        {
            EditorUtility.DisplayDialog("没有选中文本", "请先在文本中选择一段内容，然后再点击添加按钮。", "确定");
            return;
        }

        // 在完整文本中查找选中的部分
        int startIndex = fullText.IndexOf(selectedText);

        if (startIndex == -1)
        {
            EditorUtility.DisplayDialog("未找到选中文本", "选中的文本在完整文本中不存在，请确认选择是否正确。", "确定");
            return;
        }

        int endIndex = startIndex + selectedText.Length;

        var keywordsProp = property.FindPropertyRelative("keywords");

        // 检查是否已经存在相同位置的关键词
        for (int i = 0; i < keywordsProp.arraySize; i++)
        {
            var keyword = keywordsProp.GetArrayElementAtIndex(i);
            int existingStart = keyword.FindPropertyRelative("startIndex").intValue;
            int existingEnd = keyword.FindPropertyRelative("endIndex").intValue;

            if (existingStart == startIndex && existingEnd == endIndex)
            {
                EditorUtility.DisplayDialog("重复关键词", "该位置已经有关键词了！", "确定");
                return;
            }
        }

        // 添加新关键词
        keywordsProp.arraySize++;
        var newKeyword = keywordsProp.GetArrayElementAtIndex(keywordsProp.arraySize - 1);

        newKeyword.FindPropertyRelative("id").stringValue = GetNextKeywordID(property);
        newKeyword.FindPropertyRelative("startIndex").intValue = startIndex;
        newKeyword.FindPropertyRelative("endIndex").intValue = endIndex;

        // 标记为已修改
        property.serializedObject.ApplyModifiedProperties();

        Debug.Log($"已添加关键词: ID={newKeyword.FindPropertyRelative("id").stringValue}, 文本=\"{selectedText}\"");
    }

    private string GetNextKeywordID(SerializedProperty property)
    {
        var keywordsProp = property.FindPropertyRelative("keywords");
        int maxID = 0;

        for (int i = 0; i < keywordsProp.arraySize; i++)
        {
            var keyword = keywordsProp.GetArrayElementAtIndex(i);
            string idStr = keyword.FindPropertyRelative("id").stringValue;

            if (int.TryParse(idStr, out int id))
            {
                if (id > maxID)
                {
                    maxID = id;
                }
            }
        }

        return (maxID + 1).ToString();
    }
}
#endif
