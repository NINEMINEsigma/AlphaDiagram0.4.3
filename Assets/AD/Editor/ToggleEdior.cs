using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AD.UI.Toggle)), CanEditMultipleObjects]
public class ToggleEdior : ADUIEditor
{
    private AD.UI.Toggle that = null;

    private SerializedProperty background = null;
    private SerializedProperty tab = null;
    private SerializedProperty mark = null;
    private SerializedProperty title = null; 
    private SerializedProperty actions = null;

    private SerializedProperty _IsCheck = null; 

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.Toggle;

        background = serializedObject.FindProperty("background");
        tab = serializedObject.FindProperty("tab");
        mark = serializedObject.FindProperty("mark");
        title = serializedObject.FindProperty("title");
        _IsCheck = serializedObject.FindProperty("BoolProperty");
        actions = serializedObject.FindProperty("actions");
    }

    public override void OnContentGUI()
    {
        OnNotChangeGUI(() => HorizontalBlockWithBox(() => EditorGUILayout.Toggle("isOn", that.isOn, customSkin.toggle)));

        EditorGUILayout.PropertyField(actions);

        Sprite sprite = null;
        UnityEngine.Object @object = null;
        string str = "";

        EditorGUI.BeginChangeCheck();
        GUIContent gUIContent = new GUIContent("Background");
        sprite = EditorGUILayout.ObjectField(gUIContent, that.background.sprite as Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.background.sprite = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent tUIContent = new GUIContent("Tab");
        sprite = EditorGUILayout.ObjectField(tUIContent, that.tab.sprite as Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.tab.sprite = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent mUIContent = new GUIContent("Mark");
        sprite = EditorGUILayout.ObjectField(mUIContent, that.mark.sprite as Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.mark.sprite = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent tiUIContent = new GUIContent("Title");
        str = EditorGUILayout.TextField(tiUIContent, that.title.text);
        if (EditorGUI.EndChangeCheck()) that.title.text = str;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(background);
        EditorGUILayout.PropertyField(tab);
        EditorGUILayout.PropertyField(mark);
        EditorGUILayout.PropertyField(title);
    }

    public override void OnSettingsGUI()
    {
        if (GUILayout.Button("Init")) that.Init();
    }
}
