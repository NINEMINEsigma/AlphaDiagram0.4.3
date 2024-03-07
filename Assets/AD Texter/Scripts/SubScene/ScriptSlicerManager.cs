using System.Collections;
using System.Collections.Generic;
using AD.UI;
using UnityEngine;

namespace AD.Sample.Texter.Scene
{
    public class ScriptSlicerManager : SubSceneManager
    {
        [Header("Assets")]
        public Button mButton;
        public override IButton BackSceneButton => mButton;
        public ListView DataListView;
        public ListViewItem ConsoleItem, ScriptSlicerListViewItemPrefab, OptionsItemPrefab;
        [Header("Data")]
        public ProjectScriptSlicerData data;

        private static ScriptSlicerManager manager;

        protected override void Start()
        {
            base.Start();
            manager = this;
        }

        protected override void SetupProjectItemData(ProjectItemData data)
        {
            this.data = ADGlobalSystem.FinalCheckWithThrow(data as ProjectScriptSlicerData);
            mainTitle.SetText(data.ProjectItemID);
            DataListView.SetPrefab(ScriptSlicerListViewItemPrefab);
            if ((data as ProjectScriptSlicerData).Items != null)
                foreach (ScriptItemEntry item in (data as ProjectScriptSlicerData).Items)
                {
                    ScriptSlicerListViewItem lv = DataListView.GenerateItem() as ScriptSlicerListViewItem;
                    lv.SetupScriptSlicerTarget(item);
                }
        }

        private void InternalPreviewOneEntry(ScriptItemEntry entry)
        {

        }
        public static void PreviewOneEntry(ScriptItemEntry entry)
        {
            manager.InternalPreviewOneEntry(entry);
        }

        private void InternalRemove(ScriptSlicerListViewItem entry)
        {
            data.Items.Remove(entry.mEntry);
            DataListView.Remove(entry.gameObject);
        }
        public static void Remove(ScriptSlicerListViewItem entry)
        {
            manager.InternalRemove(entry);
        }
        public void RemoveLast()
        {
            data.Items.RemoveAt(data.Items.Count - 1);
            DataListView.Remove(DataListView.Childs.Count - 1);
        }

        public ScriptItemEntry NewAddOne = new();
        public void SetNewOneName(string T)
        {
            NewAddOne.Name = T;
        }
        public void SetNewOneSoundAssets(string T)
        {
            NewAddOne.SoundAssets = T;
        }

        public void InternalAdd()
        {
            DataListView.SetPrefab(ScriptSlicerListViewItemPrefab);
            ScriptSlicerListViewItem lv = DataListView.GenerateItem() as ScriptSlicerListViewItem;
            var temp = new ScriptItemEntry()
            {
                SoundAssets = NewAddOne.SoundAssets,
                Name = NewAddOne.Name,
            };
            data.Items.Add(temp);
            lv.SetupScriptSlicerTarget(temp);
        }
        public static void Add()
        {
            manager.InternalAdd();
        }
        public void AddAtLast()
        {
            InternalAdd();
        }
    }
}
