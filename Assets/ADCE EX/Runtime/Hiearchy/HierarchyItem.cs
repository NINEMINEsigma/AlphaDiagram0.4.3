using System.Collections.Generic;
using AD.BASE;
using AD.UI;
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

        private void OnRightClick()
        {
            if (!ListToggle.Selected) return; 
            MatchEditor?.MatchTarget.ClickOnRight();
        }

        public void OnClickAndRefresh(bool boolen)
        {
            MatchEditor.IsOpenListView = boolen;
            if (boolen == true) OpenListView(); else CloseListView();
            MatchEditor?.MatchTarget.ClickOnLeft();
            GameEditorApp.instance.CurrentHierarchyItem = this;
            GameEditorApp.instance.SendImmediatelyCommand<CurrentItemSelectOnHierarchyPanel>();
        }

        private int AddtionalLevel = 0;
        public void AddRectHightLevel(int t)
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, temp.y + DefaultHight * t);
            AddtionalLevel += t;
            MatchEditor.MatchTarget.ParentTarget?.MatchHierarchyEditor.MatchItem.AddRectHightLevel(t);
        }

        public void ClearRectHightLevel()
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, DefaultHight);
            MatchEditor?.MatchTarget.ParentTarget?.MatchHierarchyEditor.MatchItem.AddRectHightLevel(-AddtionalLevel);
            AddtionalLevel = 0;
        }

        private void OpenListView()
        {
            List<ICanSerializeOnCustomEditor> ChildItems = MatchEditor.MatchTarget.GetChilds();
            int ChildsSum = MatchEditor.MatchTarget.GetChilds().Count;

            ListSubListView.gameObject.SetActive(true);
            int OpenSingleItemSum = MaxOpenSingleItemSum + ExtensionOpenSingleItemSum;
            int t = Mathf.Clamp(ChildsSum, 1, OpenSingleItemSum);
            this.AddRectHightLevel(t);
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
                item.MatchHierarchyEditor.MatchItem = RegisterHierarchyItem(item.MatchHierarchyEditor);
            }
            ListSubListView.SortChilds();
        }

        private HierarchyItem RegisterHierarchyItem(ISerializeHierarchyEditor target)
        {
            HierarchyItem hierarchyItem = ListSubListView.GenerateItem() as HierarchyItem;
            target.MatchItem = hierarchyItem;
            hierarchyItem.MatchEditor = target;
            target.OnSerialize();
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
            CloseListView();
            if (this.MatchEditor.IsOpenListView)
                OpenListView();
        }
    }
}
