using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NodeEditorWindow : EditorWindow
{
    private Color backgroundColor;
    private Color gridColor;

    private Vector2 preMousePosition;
    private Vector2 offset = Vector2.zero;
    private Vector2 drag = Vector2.zero;

    [MenuItem("Window/Node Editor")]
    static void OpenEditor()
    {
        NodeEditorWindow editor = EditorWindow.GetWindow<NodeEditorWindow>();
        editor.Init();
    }

    void Init()
    {
        backgroundColor = new Color(0.4f, 0.4f, 0.4f);
        gridColor = new Color(0.1f, 0.1f, 0.1f);
    }

    private void OnInspectorUpdate()
    {
        Debug.Log("x");

        offset += new Vector2(1f, 1);
        EditorUtility.SetDirty(this);

        this.Show();
    }

    private void OnGUI()
    {
        Debug.Log("y");
        DrawBackground();
        DrawGrid(20, 0.4f);
        DrawGrid(100, 0.8f);
        if (Event.current.type == EventType.ContextClick)
        {

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("1"), false, null, null);

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("2"), false, null, null);

            menu.ShowAsContext();

            //设置该事件被使用

            Event.current.Use();
        }
    }

    private void DrawBackground()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), backgroundColor);
    }

    private void DrawGrid(float gridSpacing, float gridOpacity)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);
        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);
        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }
        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }
        Handles.color = Color.white;
        Handles.EndGUI();
    }
}