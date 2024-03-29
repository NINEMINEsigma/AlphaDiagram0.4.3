using System.Collections;
using System.Collections.Generic;
using System.IO;
using AD.BASE;
using AD.Experimental.Performance;
using AD.Sample.Texter.Project;
using AD.UI;
using UnityEngine;

namespace AD.Sample.Texter.Scene
{
    public class ScriptSlicerManager : SubSceneManager
    {
        [Header("Assets")]
        public Button mButton;
        public override IButton BackSceneButton => mButton;
        public ListView DataListView, OptionListView;
        public ListViewItem ConsoleItem, ScriptSlicerListViewItemPrefab, OptionsItemPrefab;
        public InputField CharIF, SoundIF;
        public Button AdditionalButton;
        public InputField BackgroundIF, BackgroundAudioIF;
        public Button SelectBackground, SelectBackgroundAudio;
        public ViewController BackPreview;
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
            OptionListView.SetPrefab(OptionsItemPrefab);
            if ((data as ProjectScriptSlicerData).Options != null)
                foreach (SceneEndingSelectOption item in (data as ProjectScriptSlicerData).Options)
                {
                    ScriptSlicerOptionItem lv = OptionListView.GenerateItem() as ScriptSlicerOptionItem;
                    lv.SetupScriptSlicerTarget(item);
                }

            if (this.data.Items.Count > 0)
            {
                CharIF.SetText(this.data.Items[^1].Name);
                CharIF.AddListener(SetNewOneName);
                SoundIF.SetText(this.data.Items[^1].SoundAssets);
                SoundIF.AddListener(SetNewOneSoundAssets);
            }
            AdditionalButton.AddListener(AddAtLast);

            if (this.data.BackgroundImage != ProjectScriptSlicerData.NoBackgroundImage)
            {
                BackgroundIF.SetText(this.data.BackgroundImage);
                BackPreview.LoadOnUrl(this.data.BackgroundImage);
            }
            SelectBackground.AddListener(SetBackgroundOnWin);

            if (this.data.BackgroundAudio != ProjectScriptSlicerData.NoBackgroundAudio)
            {
                BackgroundAudioIF.SetText(this.data.BackgroundAudio);
            }
            SelectBackgroundAudio.AddListener(SetBackgroundAudioOnWin);
        }

        private void InternalPreviewOneEntry(ScriptItemEntry entry)
        {
            App.instance.CurrentProjectItemData = data;
            App.instance.GetController<MainSceneLoader>().Load<ScriptViewFieldManager>("ScriptViewField");
        }
        public static void PreviewOneEntry(ScriptItemEntry entry)
        {
            manager.InternalPreviewOneEntry(entry);
        }

        private void InternalRemove(ScriptSlicerListViewItem entry)
        {
            if (data.Items != null && data.Items.Count > 0)
            {
                data.Items.Remove(entry.mEntry);
                DataListView.Remove(entry.gameObject);
            }
        }
        public static void Remove(ScriptSlicerListViewItem entry)
        {
            manager.InternalRemove(entry);
        }
        public void RemoveLast()
        {
            if (data.Items != null && data.Items.Count > 0)
            {
                data.Items.RemoveAt(data.Items.Count - 1);
                DataListView.Remove(DataListView.Childs.Count - 1);
            }
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

        public SceneEndingSelectOption NewAddOption = new();
        public void SetNewOptionID(string T)
        {
            NewAddOption.TargetScriptSceneID = T;
        }
        public void SetNewOptionName(string T)
        {
            NewAddOption.OptionName = T;
        }

        public void AddNewOption()
        {
            var temp = new SceneEndingSelectOption()
            {
                TargetScriptSceneID = NewAddOption.TargetScriptSceneID,
                OptionName = NewAddOption.OptionName
            };
            data.Options ??= new();
            data.Options.Add(temp);
            ScriptSlicerOptionItem lv = OptionListView.GenerateItem() as ScriptSlicerOptionItem;
            lv.SetupScriptSlicerTarget(temp);
        }
        public void RemoveLastOption()
        {
            if (data.Options != null && data.Options.Count > 0)
            {
                data.Options.RemoveAt(data.Options.Count - 1);
                OptionListView.Remove(OptionListView.Childs.Count - 1);
            }
        }

        public void SetBackgroundOnWin()
        {
            FileC.SelectFileOnSystem(SetBackground, "����", "png,jpg", "png", "jpg");
        }
        public void SetBackground(string path)
        {
            data.BackgroundImage = path;
            BackgroundIF.SetText(path);
            BackPreview.LoadOnUrl(path, true);
        }

        public void SetBackgroundAudioOnWin()
        {
            FileC.SelectFileOnSystem(SetBackgroundAudio, "��������", "mp3,ogg,wav", "mp3", "ogg", "wav");
        }
        public void SetBackgroundAudio(string path)
        {
            data.BackgroundAudio = path;
            BackgroundAudioIF.SetText(path);
        }
    }
}
