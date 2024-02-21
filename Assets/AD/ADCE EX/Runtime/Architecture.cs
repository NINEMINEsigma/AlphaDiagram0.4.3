using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using AD.Utility;

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
            target.MatchItems.Add(hierarchyItem);
            hierarchyItem.MatchEditor = target;
            target.OnSerialize(hierarchyItem);
            return hierarchyItem;
        }

        public static void SetParent(this ICanSerializeOnCustomEditor self, ICanSerializeOnCustomEditor _Right)
        {
            if (self == null && _Right == null) return;
            if (self.ParentTarget == _Right) return;
            if (_Right == self) throw new ADException("Loop Serialize Editor");

            if (ADGlobalSystem.AppQuitting)
            {
                //if (self.GetChilds().Contains(_Right)) return;
                self.ParentTarget?.GetChilds().Remove(self);
                self.ParentTarget = _Right;
                _Right?.GetChilds().Add(self);
            }
            else
            {
                IUpdateOnChange onChange0 = null, onChange1 = null, onChange2 = null;
                if (self.ParentTarget != null)
                {
                    //if (self.GetChilds().Contains(_Right)) return;
                    self.ParentTarget.GetChilds().Remove(self);
                    self.ParentTarget.MatchHierarchyEditor.MatchItem.Refresh();
                    if (self.ParentTarget is IUpdateOnChange onChange)
                    {
                        onChange0 = onChange;
                    }
                }
                self.ParentTarget = _Right;
                if (self is IUpdateOnChange sonChange)
                {
                    onChange1 = sonChange;
                }
                if (_Right != null)
                {
                    _Right.GetChilds().Add(self);
                    if (_Right.MatchHierarchyEditor != null && _Right.MatchHierarchyEditor.MatchItem != null)
                        _Right.MatchHierarchyEditor.MatchItem.Refresh();
                    if (_Right is IUpdateOnChange onChange)
                    {
                        onChange2 = onChange;
                    }
                }

                onChange0?.OnChange();
                onChange1?.OnChange();
                onChange2?.OnChange();
            }
        }

        public static void BaseHierarchyItemSerialize(this ISerializeHierarchyEditor self)
        {
            if (self.IsOpenListView)
            {
                self.MatchItem.Refresh();
                foreach (var item in self.MatchTarget.GetChilds())
                {
                    item.MatchHierarchyEditor.BaseHierarchyItemSerialize();
                    foreach (var itemSingle in item.MatchHierarchyEditor.MatchItems)
                    {
                        item.MatchHierarchyEditor.OnSerialize(itemSingle);
                    }
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
            GameEditorApp.instance.GetController<Hierarchy>().RefreshPanel();
        }
    }

    public class RefreshPropertiesPanel : ADCommand
    {
        public override void OnExecute()
        {
            GameEditorApp.instance.GetController<Properties>().RefreshPanel();
        }
    }

    public interface ICanSerialize
    {
        int SerializeIndex { get; set; }
    }

    public interface ISerializeHierarchyEditor : ICanSerialize
    {
        void OnSerialize(HierarchyItem MatchItem);

        ICanSerializeOnCustomEditor MatchTarget { get; }
        bool IsOpenListView { get; set; }
        HierarchyItem MatchItem { get; set; }
        List<HierarchyItem> MatchItems { get; }
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
        void OnSerialize();

        PropertiesItem MatchItem { get; set; }
        ICanSerializeOnCustomEditor MatchTarget { get; }
        bool IsDirty { get; set; }
    }

    public interface IUpdateOnChange
    {
        void OnChange();
    }
}
