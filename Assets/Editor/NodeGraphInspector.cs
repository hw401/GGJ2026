// NodeGraphEnhancedInspector.cs (放在Editor文件夹下)
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(NodeGraphManager))]
public class NodeGraphEnhancedInspector : Editor
{
    private NodeGraphManager _graph;
    private Vector2 _scrollPos;
    private bool _showNodeList = true;
    private bool _showQuickActions = true;
    private bool _showStatistics = true;

    // 搜索功能
    private string _searchText = "";
    private List<BaseNodeSO> _filteredNodes = new List<BaseNodeSO>();

    private void OnEnable()
    {
        _graph = (NodeGraphManager)target;
        UpdateFilteredNodes();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);

        // 标题
        EditorGUILayout.LabelField("📊 节点图管理器", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 基本信息
        DrawBasicInfo();
        EditorGUILayout.Space(15);

        // 快速操作
        DrawQuickActions();
        EditorGUILayout.Space(15);

        // 节点列表
        DrawNodeList();
        EditorGUILayout.Space(15);

        // 统计信息
        DrawStatistics();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBasicInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("基本信息", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("graphName"));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("起始节点", GUILayout.Width(80));

        var startNodeProp = serializedObject.FindProperty("startNode");
        EditorGUILayout.PropertyField(startNodeProp, GUIContent.none);

        if (GUILayout.Button("设为选中", GUILayout.Width(60)))
        {
            if (_graph.startNode != null)
            {
                Selection.activeObject = _graph.startNode;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawQuickActions()
    {
        _showQuickActions = EditorGUILayout.Foldout(_showQuickActions, "🚀 快速操作", true);

        if (_showQuickActions)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            // 验证按钮
            if (GUILayout.Button("✅ 验证节点图", GUILayout.Height(30)))
            {
                _graph.ValidateGraph();
                EditorUtility.SetDirty(_graph);
                UpdateFilteredNodes();
            }

            // 收集按钮
            if (GUILayout.Button("🔄 收集所有节点", GUILayout.Height(30)))
            {
                CollectAllNodes();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            // 创建节点按钮
            if (GUILayout.Button("➕ 创建普通节点", GUILayout.Height(25)))
            {
                CreateAndAddNode<NormalNodeSO>("普通节点");
            }

            if (GUILayout.Button("🎯 创建关键节点", GUILayout.Height(25)))
            {
                CreateAndAddNode<KeyNodeSO>("关键节点");
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("⏱️ 创建QTE节点", GUILayout.Height(25)))
            {
                CreateAndAddNode<QTENodeSO>("QTE节点");
            }

            if (GUILayout.Button("🏁 创建结局节点", GUILayout.Height(25)))
            {
                CreateAndAddNode<EndNodeSO>("结局节点");
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawNodeList()
    {
        _showNodeList = EditorGUILayout.Foldout(_showNodeList, $"📋 节点列表 ({_graph.allNodes.Count}个)", true);

        if (_showNodeList)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 搜索框
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("🔍 搜索:", GUILayout.Width(50));
            string newSearchText = EditorGUILayout.TextField(_searchText);
            if (newSearchText != _searchText)
            {
                _searchText = newSearchText;
                UpdateFilteredNodes();
            }

            if (GUILayout.Button("清空", GUILayout.Width(50)))
            {
                _searchText = "";
                UpdateFilteredNodes();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 列表标题
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", GUILayout.Width(40));
            EditorGUILayout.LabelField("名称", GUILayout.Width(150));
            EditorGUILayout.LabelField("类型", GUILayout.Width(80));
            EditorGUILayout.LabelField("操作", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            // 节点列表
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(300));

            var nodesToShow = string.IsNullOrEmpty(_searchText) ? _graph.allNodes : _filteredNodes;

            for (int i = 0; i < nodesToShow.Count; i++)
            {
                var node = nodesToShow[i];
                if (node == null) continue;

                EditorGUILayout.BeginHorizontal();

                // ID（起始节点高亮）
                string idText = node.NodeID.ToString("000");
                if (node == _graph.startNode)
                {
                    EditorGUILayout.LabelField($"⭐{idText}", GUILayout.Width(40));
                }
                else
                {
                    EditorGUILayout.LabelField(idText, GUILayout.Width(40));
                }

                // 名称
                EditorGUILayout.LabelField(node.nodeName, GUILayout.Width(150));

                // 类型
                EditorGUILayout.LabelField(node.GetNodeTypeName(), GUILayout.Width(80));

                // 操作按钮
                EditorGUILayout.BeginHorizontal(GUILayout.Width(120));

                if (GUILayout.Button("选择", GUILayout.Width(40)))
                {
                    Selection.activeObject = node;
                }

                if (GUILayout.Button("设为起始", GUILayout.Width(50)))
                {
                    Undo.RecordObject(_graph, "设置起始节点");
                    _graph.startNode = node;
                    EditorUtility.SetDirty(_graph);
                }

                if (GUILayout.Button("移除", GUILayout.Width(40)))
                {
                    if (EditorUtility.DisplayDialog("确认移除",
                        $"确定要从节点图中移除节点 {node.nodeName} (ID: {node.NodeID}) 吗？\n\n注意：这不会删除资产文件，只是从列表中移除。",
                        "移除", "取消"))
                    {
                        Undo.RecordObject(_graph, "移除节点");
                        _graph.allNodes.Remove(node);
                        EditorUtility.SetDirty(_graph);
                        UpdateFilteredNodes();
                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();

                // 分隔线
                if (i < nodesToShow.Count - 1)
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    EditorGUILayout.Space(2);
                }
            }

            EditorGUILayout.EndScrollView();

            // 添加现有节点按钮
            EditorGUILayout.Space(10);
            if (GUILayout.Button("➕ 添加现有节点到列表", GUILayout.Height(30)))
            {
                AddExistingNode();
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawStatistics()
    {
        _showStatistics = EditorGUILayout.Foldout(_showStatistics, "📈 统计信息", true);

        if (_showStatistics)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int normalCount = _graph.allNodes.Count(n => n is NormalNodeSO);
            int keyCount = _graph.allNodes.Count(n => n is KeyNodeSO);
            int qteCount = _graph.allNodes.Count(n => n is QTENodeSO);
            int endCount = _graph.allNodes.Count(n => n is EndNodeSO);
            int nullCount = _graph.allNodes.Count(n => n == null);

            EditorGUILayout.LabelField($"总计节点: {_graph.allNodes.Count}");
            EditorGUILayout.LabelField($"普通节点: {normalCount}");
            EditorGUILayout.LabelField($"关键节点: {keyCount}");
            EditorGUILayout.LabelField($"QTE节点: {qteCount}");
            EditorGUILayout.LabelField($"结局节点: {endCount}");

            if (nullCount > 0)
            {
                EditorGUILayout.LabelField($"⚠️ 空引用: {nullCount}",
                    new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } });
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void UpdateFilteredNodes()
    {
        if (string.IsNullOrEmpty(_searchText))
        {
            _filteredNodes = new List<BaseNodeSO>(_graph.allNodes);
        }
        else
        {
            _filteredNodes = _graph.allNodes.Where(n =>
                n != null && (
                    n.nodeName.ToLower().Contains(_searchText.ToLower()) ||
                    n.NodeID.ToString().Contains(_searchText) ||
                    n.GetNodeTypeName().ToLower().Contains(_searchText.ToLower())
                )
            ).ToList();
        }
    }

    private void CollectAllNodes()
    {
        Undo.RecordObject(_graph, "收集所有节点");

        // 查找项目中的所有节点
        var guids = AssetDatabase.FindAssets("t:BaseNodeSO");
        _graph.allNodes.Clear();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var node = AssetDatabase.LoadAssetAtPath<BaseNodeSO>(path);

            if (node != null && !_graph.allNodes.Contains(node))
            {
                _graph.allNodes.Add(node);
            }
        }

        // 按ID排序
        _graph.allNodes = _graph.allNodes
            .Where(n => n != null)
            .OrderBy(n => n.NodeID)
            .ToList();

        // 如果没有起始节点，设置第一个为起始
        if (_graph.startNode == null && _graph.allNodes.Count > 0)
        {
            _graph.startNode = _graph.allNodes[0];
        }

        EditorUtility.SetDirty(_graph);
        UpdateFilteredNodes();

        Debug.Log($"✅ 已收集 {_graph.allNodes.Count} 个节点");
    }

    private void CreateAndAddNode<T>(string typeName) where T : BaseNodeSO
    {
        // 获取下一个可用的ID
        int nextID = 1;
        if (_graph.allNodes.Count > 0)
        {
            var maxID = _graph.allNodes.Where(n => n != null).Max(n => n.NodeID);
            nextID = maxID + 1;
        }

        // 选择保存路径
        string folderPath = "Assets/ScriptableObject/Nodes";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        // 创建节点
        var node = ScriptableObject.CreateInstance<T>();
        node.ResetID(nextID);
        node.nodeName = $"{typeName}_{nextID:000}";

        // 保存节点
        string path = AssetDatabase.GenerateUniqueAssetPath(
            $"{folderPath}/{typeName}_{nextID:000}.asset"
        );

        AssetDatabase.CreateAsset(node, path);
        AssetDatabase.SaveAssets();

        // 添加到列表
        Undo.RecordObject(_graph, "添加新节点");
        _graph.allNodes.Add(node);

        // 如果没有起始节点，设为起始
        if (_graph.startNode == null)
        {
            _graph.startNode = node;
        }

        EditorUtility.SetDirty(_graph);
        UpdateFilteredNodes();

        // 选中新节点
        Selection.activeObject = node;

        Debug.Log($"✅ 已创建并添加 {typeName}: {path}");
    }

    private void AddExistingNode()
    {
        // 打开文件选择窗口
        string path = EditorUtility.OpenFilePanel("选择节点文件", "Assets", "asset");

        if (!string.IsNullOrEmpty(path))
        {
            // 转换为相对路径
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            var node = AssetDatabase.LoadAssetAtPath<BaseNodeSO>(path);

            if (node != null)
            {
                if (!_graph.allNodes.Contains(node))
                {
                    Undo.RecordObject(_graph, "添加现有节点");
                    _graph.allNodes.Add(node);

                    // 排序
                    _graph.allNodes = _graph.allNodes
                        .Where(n => n != null)
                        .OrderBy(n => n.NodeID)
                        .ToList();

                    EditorUtility.SetDirty(_graph);
                    UpdateFilteredNodes();

                    Debug.Log($"✅ 已添加节点: {node.name}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ 节点已在列表中: {node.name}");
                }
            }
            else
            {
                Debug.LogError($"❌ 选择的文件不是有效的节点: {path}");
            }
        }
    }
}
#endif
