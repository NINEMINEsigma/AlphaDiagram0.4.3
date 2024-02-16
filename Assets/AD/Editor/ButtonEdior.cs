using UnityEditor;

[CustomEditor(typeof(AD.UI.Button)), CanEditMultipleObjects]
public class ButtonEdior : ADUIEditor
{
    private AD.UI.Button that = null;

    SerializedProperty animator;
    SerializedProperty ChooseMode;
    SerializedProperty AnimatorBoolString;
    SerializedProperty AnimatorONString, AnimatorOFFString;
    SerializedProperty OnClick, OnRelease;
    SerializedProperty _IsClick;
    SerializedProperty IsKeepState;
    SerializedProperty title;
    SerializedProperty AnimationSpeed;

    protected override string TopHeader => "Button Top Header";

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.Button;

        animator = serializedObject.FindProperty(nameof(animator));
        ChooseMode = serializedObject.FindProperty(nameof(ChooseMode));
        AnimatorBoolString = serializedObject.FindProperty(nameof(AnimatorBoolString));
        AnimatorONString = serializedObject.FindProperty(nameof(AnimatorONString));
        AnimatorOFFString = serializedObject.FindProperty(nameof(AnimatorOFFString));
        OnClick = serializedObject.FindProperty(nameof(OnClick));
        OnRelease = serializedObject.FindProperty(nameof(OnRelease));
        _IsClick = serializedObject.FindProperty(nameof(_IsClick));
        IsKeepState = serializedObject.FindProperty(nameof(IsKeepState));
        title = serializedObject.FindProperty(nameof(title));
        AnimationSpeed = serializedObject.FindProperty(nameof(AnimationSpeed));
    }

    public override void OnContentGUI()
    {
        EditorGUILayout.PropertyField(OnClick);
        EditorGUILayout.PropertyField(OnRelease);
        HorizontalBlockWithBox(() => OnNotChangeGUI(() => EditorGUILayout.PropertyField(_IsClick)));
    }

    public override void OnResourcesGUI()
    {
        HorizontalBlockWithBox(() => {
            HelpBox("You Can Set Animator Null To Close Animation",MessageType.Info);
            EditorGUILayout.PropertyField(animator);
            });
        EditorGUILayout.PropertyField(title);
    }

    public override void OnSettingsGUI()
    {
        if(that.animator != null)
        {
            HorizontalBlockWithBox(() => OnNotChangeGUI(() => EditorGUILayout.TextArea("Animatior")));
            EditorGUILayout.PropertyField(ChooseMode);
            EditorGUILayout.PropertyField(AnimatorBoolString);
            EditorGUILayout.PropertyField(AnimatorONString);
            EditorGUILayout.PropertyField(AnimatorOFFString);
            EditorGUILayout.PropertyField(AnimationSpeed);
            EditorGUILayout.Space(20);
        }
        else HorizontalBlockWithBox(() => OnNotChangeGUI(() => EditorGUILayout.TextArea("No Animatior")));
        HorizontalBlockWithBox(() => {
            HelpBox("Do You Need Animation And Keep Button State", MessageType.Info);
            EditorGUILayout.PropertyField(IsKeepState);
        });
    }
} 
