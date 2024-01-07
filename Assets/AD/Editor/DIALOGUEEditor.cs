using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AD.Experimental.LLM;
using AD.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AD.UI.DIALOGUE)), CanEditMultipleObjects]
public class DIALOGUEEditor : ListViewEditor
{
    private DIALOGUE that_DIALOGUE;

    SerializedProperty Index;

    SerializedProperty IsAutoPlayChart;
    SerializedProperty SourcePairs;

    SerializedProperty IsUsingLLM;
    SerializedProperty TargetLLM;
    SerializedProperty SenderName, LLMName;
    SerializedProperty InputWord;

    protected override void OnEnable()
    {
        base.OnEnable();
        Index = serializedObject.FindProperty(nameof(Index));
        IsAutoPlayChart = serializedObject.FindProperty(nameof(IsAutoPlayChart));
        SourcePairs = serializedObject.FindProperty(nameof(SourcePairs));
        IsUsingLLM = serializedObject.FindProperty(nameof(IsUsingLLM));
        TargetLLM = serializedObject.FindProperty(nameof(TargetLLM));
        SenderName = serializedObject.FindProperty(nameof(SenderName));
        LLMName = serializedObject.FindProperty(nameof(LLMName));
        InputWord = serializedObject.FindProperty(nameof(InputWord));
    }

    public override void OnContentGUI()
    {
        EditorGUILayout.PropertyField(SourcePairs);
        this.OnNotChangeGUI(() =>
        {
            EditorGUILayout.PropertyField(Index);
        });
        EditorGUILayout.Space(20);
        base.OnContentGUI();

        if(IsUsingLLM.boolValue)
        {
            this.HelpBox("Using LLM", MessageType.Info);
            EditorGUILayout.PropertyField(InputWord);
            EditorGUILayout.PropertyField(TargetLLM);
            EditorGUILayout.PropertyField(SenderName);
            EditorGUILayout.PropertyField(LLMName);
        }
    }

    public override void OnSettingsGUI()
    {
        this.Toggle(IsAutoPlayChart,"Auto Play By Sources");
        base.OnSettingsGUI();

        this.Toggle(IsUsingLLM, "Is Using LLM");
    }
}
