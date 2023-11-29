using System.Collections;
using System.Collections.Generic;
using AD.Experimental.GameEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EditorBar))]
public class EditorBarEditor : AbstractCustomADEditor
{
    EditorBar that;

    SerializedProperty StaticItem;
    SerializedProperty SubPage;
    SerializedProperty ButtonPerfab;

    SerializedProperty CustomWindowElementPerfab;

    protected override void OnEnable()
    {
        base.OnEnable(); 
        that = (EditorBar)target;

        StaticItem=serializedObject.FindProperty("StaticItem");
        SubPage = serializedObject.FindProperty("SubPage");
        ButtonPerfab = serializedObject.FindProperty("ButtonPerfab");
        CustomWindowElementPerfab = serializedObject.FindProperty("CustomWindowElementPerfab");
    }

    public override void OnContentGUI()
    {
        EditorGUILayout.PropertyField(StaticItem);
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(SubPage);
        EditorGUILayout.PropertyField(ButtonPerfab);
        EditorGUILayout.PropertyField(CustomWindowElementPerfab);
    }

    public override void OnSettingsGUI()
    {

    }
}
