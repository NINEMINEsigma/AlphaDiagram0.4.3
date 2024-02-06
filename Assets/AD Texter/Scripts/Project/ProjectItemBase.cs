using System.Collections.Generic;
using AD.Experimental.GameEditor;
using System;
using AD.Utility.Object;

namespace AD.Sample.Texter
{
    public interface IProjectItem : ICanSerializeOnCustomEditor
    {
        bool IsAbleDisplayedOnHierarchyAtTheSameTime(Type type);
        bool SaveData(out List<IProjectItem> badSaveItems);
        bool SaveProjectSourceData();

        EditGroup MyEditGroup { get; }
    }

    public interface IProjectItemWhereNeedInitData: IProjectItem
    {
        ProjectItemData SourceData { get; set; }
        string ProjectItemBindKey { get; }
    }

    public interface IProjectItemRoot: IProjectItem
    {

    }
}
