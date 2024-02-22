using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AD.Experimental.GameEditor
{
    public class HierarchyItem : ListViewItem
    {
        public const float DefaultHight = 20;
        public static int MaxOpenSingleItemSum = 10;
        [SerializeField] private AD.UI.Toggle ListToggle;
        public AD.UI.ListView ListSubListView;
        public int ExtensionOpenSingleItemSum = 0;

        public ISerializeHierarchyEditor MatchEditor;

        private bool DetectStats()
        {
            return MatchEditor != null && MatchEditor.MatchTarget != null;
        }

        public override int SortIndex { get => MatchEditor.SerializeIndex; set { } }

        public override ListViewItem Init()
        {
            ListSubListView.SetPrefab(GameEditorApp.instance.GetController<Hierarchy>().EditorAssets.HierarchyItemPrefab);
            MatchEditor = null;
            InitToggle();
            ClearRectHightLevel();
            return this;
        }

        public void SetTitle(string title)
        {
            ListToggle.SetTitle(title);
        }

        private void InitToggle()
        {
            ListToggle.Init();
            ListToggle.RemoveListener(OnClickAndRefresh);
            ListToggle.AddListener(OnClickAndRefresh);
            ListToggle.SetTitle("[ N U L L ]");
        }

        private void Update()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame) OnRightClick();
        }

        public void OnDisable()
        {
            MatchEditor?.MatchItems.Remove(this);
            MatchEditor = null;
        }

        private void OnRightClick()
        {
            if (!DetectStats()) return;
            if (!ListToggle.Selected) return;
            MatchEditor.MatchTarget.ClickOnRight();
            if(MatchEditor.MatchTarget is ISetupSingleMenuOnClickRight cMenu)
            {
                GameEditorApp.instance.GetSystem<SinglePanelGenerator>().OnMenuInitWithRect(cMenu.OnMenu, this.transform as RectTransform, cMenu.OnMenuTitle);
            }
        }

        public void OnClickAndRefresh(bool boolen)
        {
            if (!DetectStats()) return;
            MatchEditor.IsOpenListView = boolen;
            if (boolen == true) OpenListView(); else CloseListView();
            MatchEditor.OnSerialize(this);
            MatchEditor.MatchTarget.ClickOnLeft();
            GameEditorApp.instance.CurrentHierarchyItem = this;
            GameEditorApp.instance.SendImmediatelyCommand<CurrentItemSelectOnHierarchyPanel>();
        }

        private int AddtionalLevel = 0;

        public void AddRectHightLevel(int t)
        {
            if (t == 0) return;
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, temp.y + DefaultHight * t);
            AddtionalLevel += t;
            if(MatchEditor.MatchTarget.ParentTarget!=null)
            {
                foreach (var item in MatchEditor.MatchTarget.ParentTarget.MatchHierarchyEditor.MatchItems)
                {
                    item.AddRectHightLevel(t);
                }
            }
        }

        public void ClearRectHightLevel()
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, DefaultHight);
            if (MatchEditor != null)
            {
                if (MatchEditor.MatchTarget.ParentTarget != null)
                {
                    foreach (var item in MatchEditor.MatchTarget.ParentTarget.MatchHierarchyEditor.MatchItems)
                    {
                        item.AddRectHightLevel(-AddtionalLevel);
                    }
                }
            }
            AddtionalLevel = 0;
        }

        private void OpenListView()
        {
            List<ICanSerializeOnCustomEditor> ChildItems = MatchEditor.MatchTarget.GetChilds();
            int ChildsSum = ChildItems.Count;
            ListSubListView.gameObject.SetActive(true);
            int OpenSingleItemSum = MaxOpenSingleItemSum + ExtensionOpenSingleItemSum;
            int RealCountOfLineAdd = Mathf.Clamp(ChildsSum, 0, OpenSingleItemSum);
            this.AddRectHightLevel(RealCountOfLineAdd);
            //this.AddRectHightLevel(OpenSingleItemSum);
            SetUpSubListView(ChildItems);
        }

        private void CloseListView()
        {
            ClearRectHightLevel();
            ClearSubListView();
        }

        private void SetUpSubListView(List<ICanSerializeOnCustomEditor> childs)
        {
            ListSubListView.Clear();
            foreach (var item in childs)
            {
                RegisterHierarchyItem(item.MatchHierarchyEditor);
            }
            ListSubListView.SortChilds();
        }

        private HierarchyItem RegisterHierarchyItem(ISerializeHierarchyEditor target)
        {
            HierarchyItem hierarchyItem = ListSubListView.GenerateItem() as HierarchyItem;
            target.MatchItems.Add(hierarchyItem);
            hierarchyItem.MatchEditor = target;
            target.OnSerialize(hierarchyItem);
            hierarchyItem.name = hierarchyItem.SortIndex.ToString();
            return hierarchyItem;
        }

        private void ClearSubListView()
        {
            ListSubListView.gameObject.SetActive(false);
            ListSubListView.Clear();
        }

        public void Refresh()
        {
            if (!DetectStats()) return;
            CloseListView();
            if (this.MatchEditor.IsOpenListView)
                OpenListView();
        }
    }
}
