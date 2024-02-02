using System.Collections.Generic;
using AD.Experimental.GameEditor;
using System;

namespace AD.Sample.Texter
{
    public interface IProjectItem : ICanSerializeOnCustomEditor
    {
        bool IsAbleDisplayedOnHierarchyAtTheSameTime(Type type);
        bool SaveData(out List<IProjectItem> badSaveItems);
        bool SaveProjectSourceData();
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
