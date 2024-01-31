using System;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
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

        public void ReplaceTop(List<ISerializeHierarchyEditor> newList)
        {
            TargetTopObjectEditors = newList;
            //RefreshPanel();
            ClearAndRefresh();
            Architecture.GetController<Properties>().ClearAndRefresh();
        }

        public void AddOnTop(ISerializeHierarchyEditor editor)
        {
            TargetTopObjectEditors.Add(editor);
            //RefreshPanel();
            ClearAndRefresh();
            Architecture.GetController<Properties>().ClearAndRefresh();
        }

        public void RemoveOnTop(ISerializeHierarchyEditor editor)
        {
            if (TargetTopObjectEditors.Remove(editor))
            {
                //RefreshPanel();
                ClearAndRefresh();
                Architecture.GetController<Properties>().ClearAndRefresh();
            }
        }

        private bool IsRegister = false;

        private void Awake()
        {
            if (!IsRegister)
            {
                try
                {
                    GameEditorApp.instance.RegisterController(this);
                    IsRegister = true;
                }
                catch { }
            }
        }

        private void Start()
        {
            if (!IsRegister)
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
