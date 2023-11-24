using AD.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ViewController)), CanEditMultipleObjects]
public class VCEditor : ADUIEditor
{
    private ViewController that = null;

    SerializedProperty SourcePairs;
    SerializedProperty IsKeepCoverParent;


    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as ViewController; 

        SourcePairs = serializedObject.FindProperty("SourcePairs");
        IsKeepCoverParent = serializedObject.FindProperty("IsKeepCoverParent");

    }

    public override void OnContentGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(SourcePairs);
        if (EditorGUI.EndChangeCheck())
        {
            that.ViewImage.sprite = that.CurrentImage;
        }

        if (Application.isPlaying)
        {
            GUI.enabled = false;

            EditorGUILayout.IntSlider("CurrentIndex", that.CurrentIndex, 0, that.SourcePairs.Count - 1, null);

            GUI.enabled = true;
            if (GUILayout.Button("NextLine", new GUILayoutOption[] { })) that.NextPair();
            if (GUILayout.Button("Previous", new GUILayoutOption[] { })) that.PreviousPair();
            if (GUILayout.Button("Random", new GUILayoutOption[] { })) that.RandomPair();
        }

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Try Cover Parent")) that.SetupCoverParent();
    }

    public override void OnResourcesGUI()
    {

    }

    public override void OnSettingsGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        IsKeepCoverParent.boolValue = GUILayout.Toggle(IsKeepCoverParent.boolValue, new GUIContent("Is Keep Cover Parent"), customSkin.FindStyle("Toggle"));
        IsKeepCoverParent.boolValue = GUILayout.Toggle(IsKeepCoverParent.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
    }
}
