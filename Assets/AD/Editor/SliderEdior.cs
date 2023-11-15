using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AD.UI.Slider)), CanEditMultipleObjects]
public class SliderEdior : ADUIEditor
{
    private AD.UI.Slider that = null;

    private SerializedProperty background = null;
     private SerializedProperty handle = null;
    private SerializedProperty fill = null;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.Slider;

        background = serializedObject.FindProperty("background");
        handle = serializedObject.FindProperty("handle");
        fill = serializedObject.FindProperty("fill");
    }

    public override void OnContentGUI()
    {
        UnityEngine.Object @object = null;
        Sprite sprite = null;

        EditorGUI.BeginChangeCheck();
        GUIContent gUIContent = new GUIContent("Background");
        sprite = EditorGUILayout.ObjectField(gUIContent, that.backgroundView as UnityEngine.Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.backgroundView = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent fUIContent = new GUIContent("Fill");
        sprite = EditorGUILayout.ObjectField(fUIContent, that.fillView as UnityEngine.Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.fillView = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent hUIContent = new GUIContent("Handle");
        sprite = EditorGUILayout.ObjectField(hUIContent, that.handleView as UnityEngine.Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.handleView = sprite;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(background);
        EditorGUILayout.PropertyField(handle);
        EditorGUILayout.PropertyField(fill);
    }

    public override void OnSettingsGUI()
    {

    }
}
