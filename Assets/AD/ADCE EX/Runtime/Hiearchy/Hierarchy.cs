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
            ADGlobalSystem.FinalCheck(editor);
            if (!TargetTopObjectEditors.Contains(editor))
            {
                TargetTopObjectEditors.Add(editor);
                ClearAndRefresh();
            }
        }

        public void RemoveOnTop(ISerializeHierarchyEditor editor)
        {
            if (editor == null) return;
            if (TargetTopObjectEditors.Remove(editor))
            {
                //RefreshPanel();
                ClearAndRefresh();
                if (Architecture.GetController<Properties>().MatchTarget != null && Architecture.GetController<Properties>().MatchTarget.MatchHierarchyEditor == editor)
                {
                    Architecture.GetController<Properties>().MatchTarget = null;
                }
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
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private void Start()
        {
            if (!IsRegister)
                GameEditorApp.instance.RegisterController(this);
        }

        public override void Init()
        {
            TargetTopObjectEditors = new();
            ClearAndRefresh();
        }

        public ISerializeHierarchyEditor this[int index]
        {
            get
            {
                return TargetTopObjectEditors[index];
            }
            set
            {
                if (index == -1)
                {
                    TargetTopObjectEditors.Add(value);
                }
                else TargetTopObjectEditors[index] = value;
            }
        }

        private HierarchyItem RegisterHierarchyItem(ISerializeHierarchyEditor target)
        {
            HierarchyItem hierarchyItem = EditorAssets.HierarchyListView.GenerateItem() as HierarchyItem;
            hierarchyItem.MatchEditor = target;
            target.MatchItems.Add(hierarchyItem);
            target.BaseHierarchyItemSerialize(0);
            target.OnSerialize(hierarchyItem);
            hierarchyItem.name = hierarchyItem.SortIndex.ToString();
            return hierarchyItem;
        }

        public void ClearAndRefresh()
        {
            EditorAssets.HierarchyListView.Clear();
            GameEditorApp.instance.CurrentHierarchyItem = null;
            //GameEditorApp.instance.SendImmediatelyCommand<CurrentItemSelectOnHierarchyPanel>();
            foreach (var item in TargetTopObjectEditors)
            {
                RegisterHierarchyItem(item);
            }
            EditorAssets.HierarchyListView.SortChilds();
        }

        public void RefreshPanel(PointerEventData axisEventData = null)
        {
            foreach (var target in TargetTopObjectEditors)
            {
                target.BaseHierarchyItemSerialize(0);
                foreach (var item in target.MatchItems)
                {
                    target.OnSerialize(item);
                }
            }
            EditorAssets.HierarchyListView.SortChilds();
        }

        private void OnApplicationQuit()
        {
            GameEditorApp.instance.SaveRecord();
        }
    }
}
