using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using AD.Utility;

namespace AD.Experimental.GameEditor
{
    public class GameEditorApp : ADArchitecture<GameEditorApp>
    {
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
        public static void SetParent(this ICanSerializeOnCustomEditor self, ICanSerializeOnCustomEditor _Right, bool JustSetParent = false)
        {
            if (self == null && _Right == null) return;
            if (self.ParentTarget == _Right) return;
            if (_Right == self) throw new ADException("Loop Serialize Editor");

            if (ADGlobalSystem.AppQuitting || JustSetParent)
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
                    foreach (var item in self.ParentTarget.MatchHierarchyEditor.MatchItems)
                    {
                        item.Refresh();
                    }
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
                    if (_Right.MatchHierarchyEditor != null)
                        foreach (var item in _Right.MatchHierarchyEditor.MatchItems)
                        {
                            item.Refresh();
                        }
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

        public static void BaseHierarchyItemSerialize(this ISerializeHierarchyEditor self, int depth)
        {
            depth++;
            if (self.IsOpenListView && depth < 4)
            {
                foreach (var item in self.MatchItems)
                {
                    item.Refresh();
                }
                foreach (var item in self.MatchTarget.GetChilds())
                {
                    item.MatchHierarchyEditor.BaseHierarchyItemSerialize(depth);
                    foreach (var itemSingle in item.MatchHierarchyEditor.MatchItems)
                    {
                        item.MatchHierarchyEditor.OnSerialize(itemSingle);
                    }
                }
            }
            else
            {
                self.IsOpenListView = false;
                foreach (var item in self.MatchItems)
                {
                    item.Refresh();
                }
            }
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
        //HierarchyItem MatchItem { get; set; }
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

    public interface ISetupSingleMenuOnClickRight
    {
        Dictionary<int, Dictionary<string, ADEvent>> OnMenu { get; }
        string OnMenuTitle { get; }
    }
}
