using UnityEditor;

[CustomEditor(typeof(AD.UI.InputField)), CanEditMultipleObjects]
public class InputFieldEdior : ADUIEditor
{
    private AD.UI.InputField that = null;

    SerializedProperty Placeholder;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.InputField;

        Placeholder = serializedObject.FindProperty("Placeholder");
    }

    public override void OnContentGUI()
    {
        EditorGUI.BeginChangeCheck();
        string str1 = EditorGUILayout.TextField("InputText", that.text);
        if (EditorGUI.EndChangeCheck()) that.text = str1;

        EditorGUI.BeginChangeCheck();
        string str2 = EditorGUILayout.TextField("PlaceholderText", that.Placeholder.text);
        if (EditorGUI.EndChangeCheck()) that.SetPlaceholderText(str2);
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(Placeholder);
    }

    public override void OnSettingsGUI()
    {

    }
}
