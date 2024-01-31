using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.Sample.Texter.Internal;
using AD.UI;
using AD.Utility;
using AD.Utility.Object;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.Sample.Texter
{
    public class ProjectRoot : MonoBehaviour, IProjectItemRoot, IProjectItem
    {
        public class ProjectRootBlock : ProjectItemBlock
        {
            public ProjectRootBlock(ProjectRoot target)
            {
                that = target;
            }

            public ProjectRoot that;

            protected override void HowSerialize()
            {
                var data = that.CurrentDataAssets;
                this.MatchItem.SetTitle("Project Assets");
                PropertiesLayout.Label(data.CreaterName, "Project Creater");
                PropertiesLayout.Label(data.DateTime, "Project Created Time");
                PropertiesLayout.InputField(data.Description, "Project Description");
                PropertiesLayout.Label(data.AssetsName, "Project Assets Time");
            }
        }

        public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; }
        public ICanSerializeOnCustomEditor ParentTarget { get; set; }

        public int SerializeIndex => 0;

        private void Start()
        {
            MatchHierarchyEditor = new HierarchyBlock<ProjectRoot>(this, nameof(ProjectRoot));
            MatchPropertiesEditors = new List<ISerializePropertiesEditor>()
            {
                new ProjectRootBlock(this)
            };
            GetComponent<EditGroup>().OnEnter.AddListener(EnterMe);
        }

        public void ClickOnLeft()
        {

        }

        public void ClickOnRight()
        {

        }

        public List<IProjectItem> Childs = new();
        public List<ICanSerializeOnCustomEditor> GetChilds() => Childs.GetSubList<IProjectItem, ICanSerializeOnCustomEditor>();

        public bool IsAbleDisplayedOnHierarchyAtTheSameTime(Type type)
        {
            return false;
        }

        public void SaveProjectSourceData_ReturnVoid()
        {
            SaveProjectSourceData();
        }

        public bool SaveProjectSourceData()
        {
            try
            {
                CurrentDataAssets.Save();
                ADGlobalSystem.AddMessage(nameof(SaveProjectSourceData), "Save");
                App.instance.AddMessage("Save Project Source Data Successful");
                return true;
            }
            catch(Exception ex)
            {
                ADGlobalSystem.AddError(nameof(SaveProjectSourceData), ex);
                App.instance.AddMessage("Save Project Source Data Failed");
                return false;
            }
        }

        public DataAssets CurrentDataAssets => App.instance.GetModel<DataAssets>();

        public List<GameObject> badSaveItemsObjects;

        public void SaveDataForStaticEditorBar()
        {
            if (!SaveData(out var badSaveItems)) 
                badSaveItemsObjects = badSaveItems.GetSubList<GameObject, IProjectItem>(T => T is MonoBehaviour, T => T.As<MonoBehaviour>().gameObject);
        }

        public bool SaveData(out List<IProjectItem> badSaveItems)
        {
            badSaveItems = null;
            if (SaveProjectSourceData())
            {
                bool result = true;
                foreach (var child in Childs)
                {
                    result = child.SaveData(out List<IProjectItem> temp) && result;
                    if (temp != null)
                    {
                        if (badSaveItems == null) badSaveItems = temp;
                        else badSaveItems.AddRange(temp);
                    }
                }
                return result;
            }
            else
            {
                badSaveItems = new() { this };
                return false;
            }
        }

        //EditGroup

        private void EnterMe()
        {
            if (GameEditorApp.instance.GetController<Hierarchy>().TargetTopObjectEditors.Contains(this.MatchHierarchyEditor)) return;

            List<ISerializeHierarchyEditor> newList = new() { this.MatchHierarchyEditor };
            foreach (ISerializeHierarchyEditor sEditor in GameEditorApp.instance.GetController<Hierarchy>().TargetTopObjectEditors)
            {
                if(IsAbleDisplayedOnHierarchyAtTheSameTime(sEditor.MatchTarget.GetType()))
                    newList.Add(sEditor);
            }
            GameEditorApp.instance.GetController<Hierarchy>().ReplaceTop(newList);
        }
    }
}
