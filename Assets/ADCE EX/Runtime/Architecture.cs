using System.Collections.Generic;
using System.Runtime.InteropServices;
using AD.BASE;
using AD.UI;

namespace AD.Experimental.GameEditor
{
    public class GameEditorApp : ADArchitecture<GameEditorApp>
    {
        public override bool FromMap(IBaseMap from)
        {
            throw new System.NotImplementedException();
        }

        public override IBaseMap ToMap()
        {
            throw new System.NotImplementedException();
        }

        public override void Init()
        {
            this.RegisterModel<TaskList>();
        }

        public HierarchyItem CurrentHierarchyItem;

    }

    public class GameEditorWindowGenerator : CustomWindowGenerator<GameEditorWindowGenerator>
    {

    }

    public static class CustomEditorUtility
    {
        public static HierarchyItem RegisterHierarchyItem(this ISerializeHierarchyEditor self, ISerializeHierarchyEditor target)
        {
            HierarchyItem hierarchyItem = self.MatchItem.ListSubListView.GenerateItem() as HierarchyItem;
            target.MatchItem = hierarchyItem;
            hierarchyItem.MatchEditor = target;
            target.OnSerialize();
            return hierarchyItem;
        }

        public static void SetParent(this ICanSerializeOnCustomEditor self, ICanSerializeOnCustomEditor _Right)
        {
            if (self.ParentTarget != null)
            {
                self.ParentTarget.GetChilds().Remove(self);
                self.ParentTarget.MatchHierarchyEditor.MatchItem.Refresh();
            }
            self.ParentTarget = _Right;
            if (_Right != null)
            {
                _Right.GetChilds().Add(self);
                _Right.MatchHierarchyEditor.MatchItem.Refresh();
            }
        }

        //需要在自定义的ISerializeHierarchyEditor.OnSerialize的最前面使用
        public static void BaseHierarchyItemSerialize(this ISerializeHierarchyEditor self)
        {
            if (self.IsOpenListView)
            {
                self.MatchItem.Refresh();
                foreach (var item in self.MatchTarget.GetChilds())
                {
                    item.MatchHierarchyEditor.BaseHierarchyItemSerialize();
                    item.MatchHierarchyEditor.OnSerialize();
                }
            }
        }

        public static void SetTitle(ISerializeHierarchyEditor editor, string title)
        {
            editor.MatchItem.SetTitle(title);
        }

        public static void SetMaxSubItemCount(ISerializeHierarchyEditor editor, int max)
        {
            editor.MatchItem.ExtensionOpenSingleItemSum = (max - HierarchyItem.MaxOpenSingleItemSum) > 0 ? max - HierarchyItem.MaxOpenSingleItemSum : 1;
        }
    }

    public class CurrentItemSelectOnHierarchyPanel : ADCommand
    {
        public override void OnExecute()
        {
            var cat = Architecture.As<GameEditorApp>().CurrentHierarchyItem;
            if (cat != null)
            {
                Architecture.GetController<Properties>().MatchTarget = cat.MatchEditor.MatchTarget;
                Architecture.GetController<Properties>().ClearAndRefresh();

            }
        }
    }

    public class RefreshHierarchyPanel : ADCommand
    {
        public override void OnExecute()
        {
            Architecture.GetController<Hierarchy>().RefreshPanel();
        }
    }

    public interface ICanSerialize
    {
        void OnSerialize();
        int SerializeIndex { get; set; }
    }

    public interface ISerializeHierarchyEditor : ICanSerialize
    {
        HierarchyItem MatchItem { get; set; }
        ICanSerializeOnCustomEditor MatchTarget { get; }
        bool IsOpenListView { get; set; }
    }

    public interface ICanSerializeOnCustomEditor
    {
        ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; }
        ICanSerializeOnCustomEditor ParentTarget { get; set; }
        List<ICanSerializeOnCustomEditor> GetChilds();
        void ClickOnLeft();
        void ClickOnRight();
        int SerializeIndex { get; }
    }

    public interface ISerializePropertiesEditor : ICanSerialize
    {
        PropertiesItem MatchItem { get; set; }
        ICanSerializeOnCustomEditor MatchTarget { get; }
        bool IsDirty { get; set; }
    }
}
