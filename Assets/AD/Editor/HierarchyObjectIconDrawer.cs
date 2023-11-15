using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyObjectIconDrawer
{
    static HierarchyObjectIconDrawer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
    }

    static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
    {
        var objectContent = EditorGUIUtility.ObjectContent(EditorUtility.InstanceIDToObject(instanceID), null);
        if (objectContent.image != null && !IgnoredObjectIconNames.Contains(objectContent.image.name))
        {
            GUI.DrawTexture(new Rect(selectionRect.xMax - IconSize, selectionRect.yMin, IconSize, IconSize), objectContent.image);
        }
    }

    static readonly int IconSize = 16;

    static readonly List<string> IgnoredObjectIconNames = new List<string>
    {
        "d_GameObject Icon",
        "d_Prefab Icon",
        "d_PrefabVariant Icon",
        "d_PrefabModel Icon"
    };

}

[InitializeOnLoad]
public class CustomHierarchy
{
    static GUIStyle m_style;
    static int m_isShowInstanceIDTagValue;
    const string IsShowInstanceIDTag = "IsShowInstanceIDTag";

    public static bool isShowInstanceID { get; private set; } = true;

    static CustomHierarchy()
    {
        m_style = new GUIStyle();
        m_style.alignment = TextAnchor.MiddleRight;
        m_style.normal.textColor = Color.gray;

        m_isShowInstanceIDTagValue = PlayerPrefs.GetInt(IsShowInstanceIDTag, 0);
        isShowInstanceID = false;
        if (m_isShowInstanceIDTagValue == 1)
            OpenShowInstanceID();
    }

    public static void OpenShowInstanceID()
    {
        if (!isShowInstanceID)
        {
            isShowInstanceID = true;
            PlayerPrefs.SetInt(IsShowInstanceIDTag, 1);
            EditorApplication.hierarchyWindowItemOnGUI += ShowInstanceID;
            EditorApplication.RepaintHierarchyWindow();
        }
    }

    public static void CloseShowInstanceID()
    {
        if (isShowInstanceID)
        {
            isShowInstanceID = false;
            PlayerPrefs.SetInt(IsShowInstanceIDTag, 0);
            EditorApplication.hierarchyWindowItemOnGUI -= ShowInstanceID;
            EditorApplication.RepaintHierarchyWindow();
        }
    }

    static void ShowInstanceID(int instanceId, Rect selectionRect)
    {
        //��ʾHierarchyѡ�е�GameObject��InstanceID
        if (instanceId == Selection.activeInstanceID)
        {
            Rect rect = new Rect(50, selectionRect.y, selectionRect.width, selectionRect.height);
            GUI.Label(rect, instanceId.ToString(), m_style);
        }
    }
}

public class ToolsWindow : EditorWindow
{
    string m_ids = "";//�����InstanceID

    string m_log = "";//log��Ϣ
    Vector2 m_logScroll;

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        //����GameObject
        {
            GUILayout.Label("1.ͨ��GameObject��ΨһID(InstanceID)��λ");
            GUILayout.BeginHorizontal();
            m_ids = GUILayout.TextField(m_ids, GUILayout.Width(120), GUILayout.Height(20));
            if (GUILayout.Button("��λ", GUILayout.Width(60), GUILayout.Height(20)))
            {
                m_log = string.Empty;
                if (int.TryParse(m_ids, out int id))
                {
                    GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
                    if (go != null)
                    {
                        Selection.activeGameObject = go; //��Hierarchy����ѡ�и�GameObject
                        m_log = $"���ҳɹ� NameΪ{go.name}";
                    }
                    else
                        m_log = $"û���ҵ�IDΪ{id}��GameObject";
                }
                else
                    m_log = "��������ȷ��ID";
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.Space(10);
        //�Ƿ���Hierarchy��ʾInstanceID�Ĺ���
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("2.�Ƿ���Hierarchy�����ʾInstanceID�Ĺ���", GUILayout.Width(260), GUILayout.Height(20));

            if (GUILayout.Toggle(CustomHierarchy.isShowInstanceID, ""))
                CustomHierarchy.OpenShowInstanceID();
            else
                CustomHierarchy.CloseShowInstanceID();
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(10);
        //��ʾLog
        {
            if (!string.IsNullOrEmpty(m_log))
            {
                m_logScroll = GUILayout.BeginScrollView(m_logScroll);
                m_log = EditorGUILayout.TextArea(m_log, GUILayout.Height(80));
                EditorGUILayout.EndScrollView();
            }
        }
        GUILayout.EndVertical();
    }
}

public class EditorMenu
{
    [MenuItem("Tools/ToolsWindow")]
    static void ShowToolsWindow()
    {
        var window = (ToolsWindow)EditorWindow.GetWindow(typeof(ToolsWindow), false, "Tools Window");
        window.Show();
    }
}
