using System.Collections.Generic;
using AD.Experimental.GameEditor;
using System;
using AD.Utility.Object;
using AD.BASE;

namespace AD.Sample.Texter
{
    public interface IProjectItem : ICanSerializeOnCustomEditor
    {
        bool IsAbleDisplayedOnHierarchyAtTheSameTime(Type type);
        bool SaveData(out List<IProjectItem> badSaveItems);
        bool SaveProjectSourceData();

        void ReDrawLine();

        EditGroup MyEditGroup { get; }
    }

    public interface IProjectItemWhereNeedInitData : IProjectItem, ICanInitialize
    {
        ProjectItemData SourceData { get; set; }
        string ProjectItemBindKey { get; }
    }

    public interface IProjectItemRoot: IProjectItem
    {

    }
}
