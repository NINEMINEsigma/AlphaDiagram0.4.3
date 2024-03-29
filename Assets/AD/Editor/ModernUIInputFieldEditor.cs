using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using AD.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModernUIInputField))]
public class ModernUIInputFieldEditor : ADUIEditor
{
    ModernUIInputField that;

    SerializedProperty _Source;
    SerializedProperty Title;
    SerializedProperty Icon;
    SerializedProperty _inputFieldAnimator; 

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (ModernUIInputField)target;

        _Source = serializedObject.FindProperty(nameof(_Source));
        Title = serializedObject.FindProperty(nameof(Title));
        Icon = serializedObject.FindProperty(nameof(Icon));
        _inputFieldAnimator = serializedObject.FindProperty(nameof(_inputFieldAnimator));
    }

    public override void OnContentGUI()
    {
        string str= EditorGUILayout.TextField(that.Source.text, GUILayout.Height(100));
        if (str != that.Source.text) that.Source.text = str;
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(Title);
        EditorGUILayout.PropertyField(Icon);
        EditorGUILayout.PropertyField(_inputFieldAnimator);
    }

    public override void OnSettingsGUI()
    {
        MakeUpNumericManager(nameof(that.NumericManagerName));
    }
}