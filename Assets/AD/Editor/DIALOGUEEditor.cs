using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AD.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AD.UI.DIALOGUE)), CanEditMultipleObjects]
public class DIALOGUEEditor : ListViewEditor
{
    private DIALOGUE that_DIALOGUE;

    SerializedProperty IsAutoPlayChart;
    SerializedProperty SourcePairs;

    protected override void OnEnable()
    {
        base.OnEnable();
        IsAutoPlayChart = serializedObject.FindProperty(nameof(IsAutoPlayChart));
        SourcePairs = serializedObject.FindProperty(nameof(SourcePairs));
    }

    public override void OnContentGUI()
    {
        EditorGUILayout.PropertyField(SourcePairs);
        EditorGUILayout.Space(20);
        base.OnContentGUI();
    }

    public override void OnSettingsGUI()
    {
        this.Toggle(IsAutoPlayChart,"Auto Play By Sources");
        base.OnSettingsGUI();
    }
}
