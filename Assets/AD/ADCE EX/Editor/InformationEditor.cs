using UnityEditor;
using UnityEngine;
using AD.Experimental.GameEditor;

[CustomEditor(typeof(Information))]
public class InformationEditor : AbstractCustomADEditor
{
    Information that;
    SerializedProperty LeftText;
    SerializedProperty RightText;
    SerializedProperty TaskPanelAnimator;
    SerializedProperty TaskPanelTitle;
    SerializedProperty TaskPanelListView;
    SerializedProperty TaskPanelPercentBar;
    SerializedProperty TaskViewItemPerfab;
    SerializedProperty SinglePanelPerfab;
    SerializedProperty SinglePanelLinePerfab;
    SerializedProperty EnterMessagePanelTigger;
    SerializedProperty MessageInputField;
    SerializedProperty ExitMessagePanelTigger;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (Information)target;

        LeftText = serializedObject.FindProperty("LeftText");
        RightText = serializedObject.FindProperty("RightText");
        TaskPanelAnimator = serializedObject.FindProperty("TaskPanelAnimator");
        TaskPanelTitle = serializedObject.FindProperty("TaskPanelTitle");
        TaskPanelListView = serializedObject.FindProperty("TaskPanelListView");
        TaskPanelPercentBar = serializedObject.FindProperty("TaskPanelPercentBar");
        TaskViewItemPerfab = serializedObject.FindProperty("TaskViewItemPerfab");
        SinglePanelPerfab = serializedObject.FindProperty("SinglePanelPerfab");
        SinglePanelLinePerfab = serializedObject.FindProperty("SinglePanelLinePerfab");
        EnterMessagePanelTigger = serializedObject.FindProperty("EnterMessagePanelTigger");
        MessageInputField = serializedObject.FindProperty("MessageInputField");
        ExitMessagePanelTigger = serializedObject.FindProperty("ExitMessagePanelTigger");
    }

    public override void OnContentGUI()
    {

    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(LeftText);
        EditorGUILayout.PropertyField(RightText);
        EditorGUILayout.PropertyField(TaskPanelAnimator);
        EditorGUILayout.PropertyField(TaskPanelTitle);
        EditorGUILayout.PropertyField(TaskPanelListView);
        EditorGUILayout.PropertyField(TaskPanelPercentBar);
        EditorGUILayout.PropertyField(TaskViewItemPerfab);
        EditorGUILayout.PropertyField(SinglePanelPerfab);
        EditorGUILayout.PropertyField(SinglePanelLinePerfab);
        EditorGUILayout.PropertyField(EnterMessagePanelTigger);
        EditorGUILayout.PropertyField(MessageInputField);
        EditorGUILayout.PropertyField(ExitMessagePanelTigger);
    }

    public override void OnSettingsGUI()
    {
        if(GUILayout.Button("Generate One Single Panel"))
        {
            that.Architecture.GetSystem<SinglePanelGenerator>().OnMenuInit(new()).isCanBackPool = true;
        }
    }
}
