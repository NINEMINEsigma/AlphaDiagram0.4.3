using AD.Experimental.GameEditor;
using UnityEditor;

[CustomEditor(typeof(Title))]
public class TitleEditor : AbstractCustomADEditor
{
    Title that;

    SerializedProperty Source;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (Title)target;

        Source = serializedObject.FindProperty("Source");
    }

    public override void OnContentGUI()
    {
        if (that.Source == null)
        {
            HelpBox("You Dont Set Title(UI) On Resource", MessageType.Error);
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            string str = EditorGUILayout.TextField("Title", that.Source.text);
            if (EditorGUI.EndChangeCheck()) that.Source.text = str;
        }
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(Source);
    }

    public override void OnSettingsGUI()
    {

    }
}
