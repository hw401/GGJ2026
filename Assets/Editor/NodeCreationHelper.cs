#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class NodeCreationHelper
{
    private static int _nextNodeID = 1;

    [MenuItem("叙事系统/创建普通节点")]
    public static void CreateNormalNode()
    {
        CreateNode<NormalNodeSO>("普通节点");
    }

    [MenuItem("叙事系统/创建关键节点")]
    public static void CreateKeyNode()
    {
        CreateNode<KeyNodeSO>("关键节点");
    }

    [MenuItem("叙事系统/创建QTE节点")]
    public static void CreateQTENode()
    {
        CreateNode<QTENodeSO>("QTE节点");
    }

    [MenuItem("叙事系统/创建结局节点")]
    public static void CreateEndNode()
    {
        CreateNode<EndNodeSO>("结局节点");
    }

    [MenuItem("叙事系统/创建节点图")]
    public static void CreateNodeGraph()
    {
        var graph = ScriptableObject.CreateInstance<NodeGraphManager>();

        // 选择保存路径
        string path = EditorUtility.SaveFilePanelInProject(
            "保存节点图",
            "NewNodeGraph",
            "asset",
            "请选择保存位置"
        );

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(graph, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = graph;
        }
    }

    public static void CreateNode<T>(string typeName) where T : BaseNodeSO
    {
        // 获取当前选中的文件夹
        string folderPath = "Assets/ScriptableObject/Nodes";
        if (Selection.activeObject != null)
        {
            folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                folderPath = Path.GetDirectoryName(folderPath);
            }
        }

        // 创建节点
        var node = ScriptableObject.CreateInstance<T>();
        node.ResetID(_nextNodeID++);

        // 保存节点
        string path = AssetDatabase.GenerateUniqueAssetPath(
            $"{folderPath}/{typeName}_{node.NodeID:000}.asset"
        );

        AssetDatabase.CreateAsset(node, path);
        AssetDatabase.SaveAssets();

        // 选中新创建的节点
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = node;

        Debug.Log($"已创建{typeName}节点: {path}");
    }

    [MenuItem("叙事系统/批量重设节点ID")]
    public static void ResetAllNodeIDs()
    {
        // 获取所有节点
        var guids = AssetDatabase.FindAssets("t:BaseNodeSO");
        int id = 1;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var node = AssetDatabase.LoadAssetAtPath<BaseNodeSO>(path);

            if (node != null)
            {
                Undo.RecordObject(node, "重设节点ID");
                node.ResetID(id++);
                EditorUtility.SetDirty(node);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"已重设 {guids.Length} 个节点的ID");
    }
}
#endif
