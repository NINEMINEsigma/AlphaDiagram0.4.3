using UnityEditor;

[CustomEditor(typeof(AD.UI.RawImage)), CanEditMultipleObjects]
public class RawImageEdior : ADUIEditor
{
    private AD.UI.RawImage that = null;

    public override void OnContentGUI()
    {

    }

    public override void OnResourcesGUI()
    {

    }

    public override void OnSettingsGUI()
    {

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.RawImage;
    }
}
