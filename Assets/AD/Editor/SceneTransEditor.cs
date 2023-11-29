using AD.Experimental.SceneTrans;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneTrans))]
public class SceneTransEditor : AbstractCustomADEditor
{
    SceneTrans that;

    SerializedProperty SceneOpenEvent;
    SerializedProperty SceneCloseEvent;

    SerializedProperty SceneOpenAnimation;
    SerializedProperty SceneCloseAnimation;

    SerializedProperty animator;

    string testKey;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (SceneTrans)target;
        SceneOpenEvent = serializedObject.FindProperty(nameof(SceneOpenEvent));
        SceneCloseEvent = serializedObject.FindProperty(nameof(SceneCloseEvent));
        SceneOpenAnimation = serializedObject.FindProperty(nameof(SceneOpenAnimation));
        SceneCloseAnimation = serializedObject.FindProperty(nameof(SceneCloseAnimation));
        animator = serializedObject.FindProperty(nameof(animator));
    }

    public override void OnContentGUI()
    {
        EditorGUILayout.PropertyField(SceneOpenEvent);
        EditorGUILayout.PropertyField(SceneCloseEvent);
        EditorGUILayout.PropertyField(SceneOpenAnimation);
        EditorGUILayout.PropertyField(SceneCloseAnimation);
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(animator);
    }

    public override void OnSettingsGUI()
    {
        testKey = EditorGUILayout.TextField(testKey);
        if (GUILayout.Button("- Test String Animation -"))
        {
            that.PlayAnimation(testKey);
        }
        if (GUILayout.Button("- Test String Action -"))
        {
            that.PlayAction(testKey);
        }
    }
}
