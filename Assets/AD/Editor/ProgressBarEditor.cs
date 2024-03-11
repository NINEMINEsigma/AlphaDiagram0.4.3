using AD.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModernUIFillBar)), CanEditMultipleObjects]
public class ProgressBarEditor : ADUIEditor
{
    private ModernUIFillBar pbTarget;

    SerializedProperty minValue;
    SerializedProperty maxValue;
    SerializedProperty OnValueChange;
    SerializedProperty OnEndChange;
    SerializedProperty OnTransValueChange;
    SerializedProperty OnEndTransChange;
    SerializedProperty loadingBar;
    SerializedProperty textPercent;
    SerializedProperty textValue;
    SerializedProperty IsPercent;
    SerializedProperty IsInt;
    SerializedProperty IsLockByScript;
    SerializedProperty DragChangeSpeed;
    SerializedProperty NumericManagerName;

    protected override void OnEnable()
    {
        base.OnEnable();
        pbTarget = (ModernUIFillBar)target;

        minValue = serializedObject.FindProperty("minValue");
        maxValue = serializedObject.FindProperty("maxValue");
        OnValueChange = serializedObject.FindProperty(nameof(OnValueChange));
        OnEndChange = serializedObject.FindProperty(nameof(OnEndChange));
        OnTransValueChange = serializedObject.FindProperty(nameof(OnTransValueChange));
        OnEndTransChange = serializedObject.FindProperty(nameof(OnEndTransChange));
        loadingBar = serializedObject.FindProperty("loadingBar");
        textPercent = serializedObject.FindProperty("textPercent");
        textValue = serializedObject.FindProperty("textValue");
        IsPercent = serializedObject.FindProperty("IsPercent");
        IsInt = serializedObject.FindProperty("IsInt");
        IsLockByScript = serializedObject.FindProperty("IsLockByScript");
        DragChangeSpeed = serializedObject.FindProperty("DragChangeSpeed");
        NumericManagerName = serializedObject.FindProperty(nameof(NumericManagerName));
    }

    public override void OnContentGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Current Percent"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        pbTarget.currentPercent = EditorGUILayout.Slider(pbTarget.currentPercent, 0, 1);

        GUILayout.EndHorizontal();

        if (pbTarget.loadingBar != null && pbTarget.textPercent != null && pbTarget.textValue != null)
        {
            pbTarget.LateUpdate();
        }
        else
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Some resources are not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Min Value"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(minValue, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Max Value"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(maxValue, new GUIContent(""));

        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(OnValueChange);
        EditorGUILayout.PropertyField(OnEndChange);
        EditorGUILayout.PropertyField(OnTransValueChange);
        EditorGUILayout.PropertyField(OnEndTransChange);
    }

    public override void OnResourcesGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Loading Bar"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(loadingBar, new GUIContent(""));

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Text Indicator"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(textPercent, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Text Value"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(textValue, new GUIContent(""));

        GUILayout.EndHorizontal();
    }

    public override void OnSettingsGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        IsLockByScript.boolValue = GUILayout.Toggle(IsLockByScript.boolValue, new GUIContent("Is Lcok By Script"), customSkin.FindStyle("Toggle"));
        IsLockByScript.boolValue = GUILayout.Toggle(IsLockByScript.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        IsPercent.boolValue = GUILayout.Toggle(IsPercent.boolValue, new GUIContent("Is Percent"), customSkin.FindStyle("Toggle"));
        IsPercent.boolValue = GUILayout.Toggle(IsPercent.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        IsInt.boolValue = GUILayout.Toggle(IsInt.boolValue, new GUIContent("Is Int"), customSkin.FindStyle("Toggle"));
        IsInt.boolValue = GUILayout.Toggle(IsInt.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(DragChangeSpeed);
        EditorGUILayout.PropertyField(NumericManagerName);
    }
}
