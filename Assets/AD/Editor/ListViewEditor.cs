using AD.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(inspectedType: typeof(AD.UI.ListView), editorForChildClasses: true, isFallback = false), CanEditMultipleObjects]
public class ListViewEditor : ADUIEditor
{
    private ListView that;

    SerializedProperty _Scroll;
    SerializedProperty _List;
    SerializedProperty _Title;
    SerializedProperty Prefab;
    SerializedProperty index;

    public override void OnContentGUI()
    {

    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(_Scroll);
        EditorGUILayout.PropertyField(_List);
        EditorGUILayout.PropertyField(_Title);
        EditorGUILayout.PropertyField(Prefab);
        HorizontalBlockWithBox(() => OnNotChangeGUI(() => EditorGUILayout.PropertyField(index)));
    }

    public override void OnSettingsGUI()
    {
        if (GUILayout.Button("Init ListView")) that.Init();
        if (GUILayout.Button("Generate New Item")) that.GenerateItem();
        if (GUILayout.Button("Clear All Childs")) that.Clear();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (ListView)target;

        _Scroll = serializedObject.FindProperty("_Scroll");
        _List = serializedObject.FindProperty("_List");
        _Title = serializedObject.FindProperty("_Title");
        Prefab = serializedObject.FindProperty("Prefab");
        index = serializedObject.FindProperty("index");
    }

}
