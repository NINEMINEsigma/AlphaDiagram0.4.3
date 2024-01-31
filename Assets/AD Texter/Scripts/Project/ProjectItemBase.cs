using System.Collections;
using System.Collections.Generic;
using AD.Experimental.GameEditor;
using UnityEngine;
using AD.Utility;
using System;

namespace AD.Sample.Texter
{
    namespace Internal
    {
        internal static class ProjectUtility
        {

        }

        public class ProjectItemBlock : ISerializePropertiesEditor
        {
            public PropertiesItem MatchItem { get; set; }

            public IProjectItem _that;

            public ICanSerializeOnCustomEditor MatchTarget => _that;

            public bool IsDirty { get; set; }
            public int SerializeIndex { get; set; }

            public void OnSerialize()
            {
                PropertiesLayout.SetUpPropertiesLayout(this);
                HowSerialize();
                PropertiesLayout.ApplyPropertiesLayout();
            }

            protected virtual void HowSerialize()
            {
            }
        }

    }

    public interface IProjectItem : ICanSerializeOnCustomEditor
    {
        bool IsAbleDisplayedOnHierarchyAtTheSameTime(Type type);
        bool SaveData(out List<IProjectItem> badSaveItems);
    }

    public interface IProjectItemWhereNeedInitData: IProjectItem
    {
        ProjectItemData SourceData { get; set; }
    }

    public interface IProjectItemRoot
    {

    }
}
