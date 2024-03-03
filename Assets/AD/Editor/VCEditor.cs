using AD.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ViewController)), CanEditMultipleObjects]
public class VCEditor : IADUIEditor
{
    private ViewController that = null;

    SerializedProperty SourcePairs;
    SerializedProperty IsKeepCoverParent;
    SerializedProperty IsSetUpCoverParentAtStart;

    SerializedProperty _ViewImage;

    SerializedProperty _effectGradient;
    SerializedProperty _gradientType;
    SerializedProperty _offset;
    SerializedProperty _zoom;
    SerializedProperty _modifyVertices;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as ViewController; 

        SourcePairs = serializedObject.FindProperty("SourcePairs");
        IsKeepCoverParent = serializedObject.FindProperty("IsKeepCoverParent");
        IsSetUpCoverParentAtStart = serializedObject.FindProperty(nameof(IsSetUpCoverParentAtStart));

        _ViewImage = serializedObject.FindProperty("_ViewImage");

        _effectGradient = serializedObject.FindProperty("_effectGradient");
        _gradientType = serializedObject.FindProperty("_gradientType");
        _offset = serializedObject.FindProperty("_offset");
        _zoom = serializedObject.FindProperty("_zoom");
        _modifyVertices = serializedObject.FindProperty("_modifyVertices");

    }

    public override void OnContentGUI()
    {
        if (that.ViewImage != null)
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
                if (GUILayout.Button("NextLine")) that.NextPair();
                if (GUILayout.Button("Previous")) that.PreviousPair();
                if (GUILayout.Button("Random")) that.RandomPair();
            }
        }
        else this.HelpBox("This GameObject is without Image , some functionality is limited", MessageType.Warning);

        if (GUILayout.Button("Try Cover Parent")) that.SetupCoverParent();
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(_ViewImage);
    }

    public override void OnSettingsGUI()
    {
        Toggle(IsSetUpCoverParentAtStart, "Is Cover Parent When Start");
        Toggle(IsKeepCoverParent, "Is Keep Cover Parent");
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Gradient"), customSkin.FindStyle("Text"), GUILayout.Width(100));
        EditorGUILayout.PropertyField(_effectGradient, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Type"), customSkin.FindStyle("Text"), GUILayout.Width(100));
        EditorGUILayout.PropertyField(_gradientType, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Offset"), customSkin.FindStyle("Text"), GUILayout.Width(100));
        EditorGUILayout.PropertyField(_offset, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Zoom"), customSkin.FindStyle("Text"), GUILayout.Width(100));
        EditorGUILayout.PropertyField(_zoom, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        _modifyVertices.boolValue = GUILayout.Toggle(_modifyVertices.boolValue, new GUIContent("Complex Gradient"), customSkin.FindStyle("Toggle"));
        _modifyVertices.boolValue = GUILayout.Toggle(_modifyVertices.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
    }
}
