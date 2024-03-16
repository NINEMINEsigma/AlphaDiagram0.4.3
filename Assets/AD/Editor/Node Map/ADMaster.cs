using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AD;
using AD.UI;
using AD.BASE;
using AD.Utility;
using UnityEditor;

public class ADMaster : GridEditorWindow<ADMaster>
{
    internal class SubjectADUIItem : SubjectItem
    {
        public IADUI Target;

        public SubjectADUIItem(IADUI target)
        {
            this.Target = target;
        }

        public Vector2 Position { get; set; }
        public Vector2 SizeData { get; set; }

        public int ID => Target.SerialNumber;

        public GUIContent MyGUIContent => new GUIContent(GetName(), Target.GetType().Name);

        public string GetName()
        {
            return Target.As<MonoBehaviour>(out MonoBehaviour mono) ? (mono ? mono.name : "[Destory]") : Target.ElementName;
        }

        public GUILayoutOption[] MyGUILayerOutOptions => null;

        public GUI.WindowFunction OnWindowFunction { get => OWF; }

        public void OWF(int id)
        {
            if (Target.As<MonoBehaviour>(out MonoBehaviour mono)) Selection.activeObject = mono.gameObject;
        }

    }

    private Vector2 InternalGetPosition(int index, int maxX)
    {
        int xIndex = index % maxX;
        int yIndex = index / maxX;
        return new Vector2(xIndex * 200 + (xIndex - 1) * 50, yIndex * 50 + (yIndex - 1) * 20);
    }

    public override void Init()
    {
        int i = 0;
        foreach (var item in ADUI.Items)
        {
            if (item != null)
            {
                IADUIs.Add(new SubjectADUIItem(item).Share(out var temp));
                temp.Position = InternalGetPosition(i, 10);
                temp.SizeData = new Vector2(200, 50);
            }
            i++;
        }
    }

    private List<SubjectItem> IADUIs = new();

    protected override void OnInspectorUpdate()
    {
        if (Application.isPlaying)
            Init();
    }

    private float LeftScrollbarValue = 0;

    protected override void HowDrawGUI()
    {
        if (Application.isPlaying)
        {
            bool isNeedInit = false;
            foreach (var item in IADUIs)
            {
                if (item.As<SubjectADUIItem>().Target == null)
                {
                    isNeedInit = true;
                    break;
                }
            }
            if (isNeedInit)
                Init();
            foreach (var item in IADUIs)
            {
                SubBlock(item);
            }

            GUI.Box(new Rect(0, 0, 200, position.height), "List");
            LeftScrollbarValue = GUI.VerticalScrollbar(new Rect(180, 0, 20, position.height), LeftScrollbarValue, 20, IADUIs.Count, 0);
            for (int i = 0; i < IADUIs.Count; i++)
            {
                if (GUI.Button(new Rect(0, (i + LeftScrollbarValue) * 25, 180, 25), IADUIs[i].As<SubjectADUIItem>().Share(out var cur).GetName()))
                {
                    cur.OWF(0);
                    OffsetPosition = IADUIs[i].Position * BaseScale;
                }
            }
        }
        else
        {
            GUI.TextField(new Rect(0, 0, position.width, position.height), "Just Work On Play Mode");
        }
    }

    protected override void SubBlock(SubjectItem target)
    {
        GUI.Box(new Rect((target.Position + OffsetPosition) * BaseScale, target.SizeData * BaseScale), target.MyGUIContent);
        var cat = target.SizeData * BaseScale * 0.1f;
        if (GUI.Button(new Rect((target.Position + OffsetPosition) * BaseScale + cat, target.SizeData * BaseScale - cat * 2), target.As<SubjectADUIItem>().GetName()))
            target.OnWindowFunction.Invoke(target.ID);
    }
}
