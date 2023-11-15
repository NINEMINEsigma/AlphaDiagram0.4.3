using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AD.Experimental.GameEditor;

[CustomEditor(typeof(Properties))]
public class PropertiesEditor : AbstractCustomADEditor
{
    Properties that;

    SerializedProperty EditorAssets;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (Properties)target;

        EditorAssets = serializedObject.FindProperty("EditorAssets");
    }

    public override void OnContentGUI()
    {

    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(EditorAssets);
    }

    public override void OnSettingsGUI()
    {

    }
}
