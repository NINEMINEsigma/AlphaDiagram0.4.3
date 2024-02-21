using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using UnityEditor;
using UnityEngine;

public class TestObject : AD.Experimental.GameEditor.ICanSerializeOnCustomEditor
{
    public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
    public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; }
    public ICanSerializeOnCustomEditor ParentTarget { get; set; }

    public int SerializeIndex { get; set; }

    public TestObject()
    {
        MatchHierarchyEditor = new TestSerializeHierarchyEditor(this);
        MatchPropertiesEditors = new()
        {
            //new TestSerializePropertiesEditor1(this),
            new TestSerializePropertiesEditor2(this),
            new PropertiesBlock<TestObject>(this,"test")
        };
        color = Random.ColorHSV();
    }
    ~TestObject()
    {
        Debug.Log(this.SerializeIndex.ToString() + " TestObject Is Destroy");
    }

    [ADSerialize(layer = "test", index = 0, message = "color")]
    public Color color;
    [ADSerialize(layer = "test", index = 2, message = "key")]
    public bool key;
    [ADSerialize(layer = "test", index = 3, message = "Test Vec4")]
    public Vector4 vec4;

    [ADActionButton(layer = "test", index = 1, message = "ClickOnLeft", methodName = "Click")]
    public void ClickOnLeft()
    {
        Debug.Log("Click");
    }

    public void ClickOnRight()
    {
        Debug.Log(SerializeIndex);
    }

    List<ICanSerializeOnCustomEditor> Childs = new();
    public List<ICanSerializeOnCustomEditor> GetChilds() => Childs;
}

public class TestSerializeHierarchyEditor : ISerializeHierarchyEditor
{
    public HierarchyItem MatchItem
    {
        get => MatchItems.Count == 0 ? null : MatchItems[0];
        set
        {
            if (MatchItems.Count == 0) MatchItems.Add(value);
            else MatchItems[0] = value;
        }
    }
    public List<HierarchyItem> MatchItems { get; private set; } = new();

    public ICanSerializeOnCustomEditor MatchTarget { get; private set; }

    public TestSerializeHierarchyEditor(TestObject target)
    {
        MatchTarget = target;
    }

    public bool IsOpenListView { get ; set ; }

    public int SerializeIndex { get => MatchTarget.SerializeIndex; set => throw new ADException(); }

    public void OnSerialize(HierarchyItem MatchItem)
    {
        MatchItem.SetTitle(MatchTarget.SerializeIndex.ToString());
    }
}

public class TestSerializePropertiesEditor1 : ISerializePropertiesEditor
{
    public PropertiesItem MatchItem { get; set; }

    public ICanSerializeOnCustomEditor MatchTarget { get; private set; }

    public int SerializeIndex { get => 0; set => throw new ADException(); }
    bool ISerializePropertiesEditor.IsDirty { get; set; } = false;

    public TestSerializePropertiesEditor1(TestObject target)
    {
        MatchTarget = target;
    }

    public void OnSerialize()
    {
        AD.Experimental.GameEditor.PropertiesLayout.SetUpPropertiesLayout(this);

        MatchItem.SetTitle("Test 1");

        PropertiesLayout.ModernUISwitch("Switch", MatchTarget.As<TestObject>().key, "Test Switch", T => MatchTarget.As<TestObject>().key = T);
        PropertiesLayout.ColorPanel("Test", MatchTarget.As<TestObject>().color, "Test Color", T => MatchTarget.As<TestObject>().color = T);

        AD.Experimental.GameEditor.PropertiesLayout.ApplyPropertiesLayout();
    }
}

public class TestSerializePropertiesEditor2 : ISerializePropertiesEditor
{
    public PropertiesItem MatchItem { get; set; }

    public ICanSerializeOnCustomEditor MatchTarget { get; private set; }

    public int SerializeIndex { get => 200; set => throw new ADException(); }
    bool ISerializePropertiesEditor.IsDirty { get; set; } = false;
    BindProperty<string> Property { get; set; } = new();

    public TestSerializePropertiesEditor2(TestObject target)
    {
        MatchTarget = target;
    }

    public void OnSerialize()
    {
        AD.Experimental.GameEditor.PropertiesLayout.SetUpPropertiesLayout(this);

        MatchItem.SetTitle("Test 2");


        AD.Experimental.GameEditor.PropertiesLayout.ApplyPropertiesLayout();
    }
}

[CustomEditor(typeof(Hierarchy))]
public class HierarchyEditor : AbstractCustomADEditor
{
    Hierarchy that;

    int currentIndex = 0;

    SerializedProperty EditorAssets;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (Hierarchy)target;

        EditorAssets = serializedObject.FindProperty("EditorAssets");
    }

    public override void OnContentGUI()
    {

    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(EditorAssets);
    }

    public static List<TestObject> TestObjects = new();

    public override void OnSettingsGUI()
    {
        if (GUILayout.Button("Generate One[Test]"))
        {
            TestObject temp = new()
            {
                SerializeIndex = currentIndex++
            };
            that.AddOnTop(temp.MatchHierarchyEditor);
            TestObjects.Add(temp);
            that.ClearAndRefresh();
        }
        if (GUILayout.Button("Delete All[Test]"))
        {
            that.Init();
            TestObjects.Clear();
        }
    }
}
