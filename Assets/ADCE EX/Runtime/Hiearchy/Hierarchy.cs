using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.Experimental.GameEditor
{
    [Serializable]
    public class HierarchyEditorAssets
    {
        public HierarchyItem HierarchyItemPrefab;
        public ListView HierarchyListView;
        public BehaviourContext behaviourContext;
    }

    public class Hierarchy : ADController
    {
        public HierarchyEditorAssets EditorAssets;

        public List<ISerializeHierarchyEditor> TargetTopObjectEditors { get; private set; } = new();

        public void AddOnTop(ISerializeHierarchyEditor editor)
        {
            TargetTopObjectEditors.Add(editor);
            RefreshPanel();
            Architecture.GetController<Properties>().ClearAndRefresh();
        }

        public void RemoveOnTop(ISerializeHierarchyEditor editor)
        {
            if (TargetTopObjectEditors.Remove(editor))
            {
                RefreshPanel();
                Architecture.GetController<Properties>().ClearAndRefresh();
            }
        }

        private void Start()
        {
            GameEditorApp.instance.RegisterController(this);
        }

        public override void Init()
        {
            //EditorAssets.behaviourContext.OnPointerEnterEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerEnterEvent, RefreshPanel);
            //EditorAssets.behaviourContext.OnPointerExitEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerExitEvent, RefreshPanel);

            TargetTopObjectEditors = new();
            ClearAndRefresh();
        }

        public ISerializeHierarchyEditor this[int index]
        {
            get
            {
                if (index < 0 || index > TargetTopObjectEditors.Count)
                {
                    Debug.LogError("Over Bound");
                    return null;
                }
                return TargetTopObjectEditors[index];
            }
            set
            {
                if (index == -1)
                {
                    TargetTopObjectEditors.Add(value);
                }
                else if (index < 0 || index > TargetTopObjectEditors.Count)
                {
                    Debug.LogError("Over Bound");
                    return;
                }
                else TargetTopObjectEditors[index] = value;
            }
        }

        private HierarchyItem RegisterHierarchyItem(ISerializeHierarchyEditor target)
        {
            HierarchyItem hierarchyItem = EditorAssets.HierarchyListView.GenerateItem() as HierarchyItem;
            target.MatchItem = hierarchyItem;
            hierarchyItem.MatchEditor = target;
            target.BaseHierarchyItemSerialize();
            target.OnSerialize();
            hierarchyItem.name = hierarchyItem.SortIndex.ToString();
            return hierarchyItem;
        }

        public void ClearAndRefresh()
        {
            EditorAssets.HierarchyListView.Clear();
            GameEditorApp.instance.CurrentHierarchyItem = null;
            GameEditorApp.instance.SendImmediatelyCommand<CurrentItemSelectOnHierarchyPanel>();
            foreach (var item in TargetTopObjectEditors)
            {
                item.MatchItem = RegisterHierarchyItem(item);
            }
            EditorAssets.HierarchyListView.SortChilds();
        }

        public void RefreshPanel(PointerEventData axisEventData=null)
        {
            foreach (var target in TargetTopObjectEditors)
            {
                target.BaseHierarchyItemSerialize();
                target.OnSerialize();
            }
            EditorAssets.HierarchyListView.SortChilds();
        }

        private void OnApplicationQuit()
        {
            GameEditorApp.instance.SaveRecord();
        }
    }
}
