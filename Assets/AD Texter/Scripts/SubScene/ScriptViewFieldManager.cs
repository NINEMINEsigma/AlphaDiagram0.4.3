using System.IO;
using AD.Experimental.Performance;
using AD.Sample.Texter.Project;
using AD.UI;
using AD.Utility;
using UnityEngine;

namespace AD.Sample.Texter.Scene
{
    public class ScriptViewFieldManager : SubSceneManager
    {
        public ProjectScriptSlicer TargetItem;
        public ProjectScriptSlicerData TargetData => TargetItem.ProjectScriptSlicingSourceData;
        [Header("ScriptViewFieldManager Assets")]
        public DialogBox m_DialogBox;
        public AudioSourceController m_AudioS;
        public ViewController m_Background;
        public ModernUIFillBar m_CurrentProcess;
        public Button m_Back;
        public override IButton BackSceneButton => m_Back;
        public Button m_Setting;
        public int CurrentIndex = 0;
        public ADSerializableDictionary<string, AudioClip> AudioAssets = new();

        protected override void Start()
        {
            base.Start();
            m_DialogBox.OnNext.AddListener(DoNext);
            m_Setting.AddListener(() =>
            {
                ADGlobalSystem.instance.TargetSceneName = "SettingScene";
                ADGlobalSystem.instance.OnEnd();
                App.instance.GetController<MainSceneLoader>().UnloadAll();
            });
        }

        protected override void SetupProjectItemData(ProjectItemData data)
        {
            SetupScript(ADGlobalSystem.FinalCheckWithThrow(data.MatchProjectItem as ProjectScriptSlicer, "data.MatchProjectItem isn't ProjectScriptSlicer"), 0);
        }

        public void SetupScript(ProjectScriptSlicer target, int index)
        {
            TargetItem = target;
            CurrentIndex = index;
            if (TargetData.BackgroundImage != ProjectScriptSlicerData.NoBackgroundImage)
            {
                //string imagePath = Path.Combine(LoadingManager.FilePath, App.instance.GetModel<DataAssets>().AssetsName + LoadingManager.PointExtension, TargetData.BackgroundImage);
                string imagePath = TargetData.BackgroundImage;
                m_Background.LoadOnUrl(imagePath, true);
            }
            foreach (var item in TargetData.Items)
            {
                if (item.SoundAssets == ScriptItemEntry.NoVoice) continue;
                //string audioPath = Path.Combine(LoadingManager.FilePath, App.instance.GetModel<DataAssets>().AssetsName + LoadingManager.PointExtension, item.SoundAssets);
                string audioPath = item.SoundAssets;
                m_AudioS.LoadOnUrl(audioPath, false);
            }
            mainTitle.SetText(target.ProjectScriptSlicingSourceData.ProjectItemID);
            m_CurrentProcess.Set(0, TargetData.Items.Count);
            DoMakeup();
        }

        private void DoNext()
        {
            CurrentIndex++;
            DoMakeup();
        }

        private void DoMakeup()
        {
            if (CurrentIndex >= TargetData.Items.Count) return;
            else
            {
                CurrentIndex = Mathf.Clamp(CurrentIndex, 0, TargetData.Items.Count);
            }
            if (CurrentIndex < TargetData.Items.Count)
            {
                var currentData = TargetData.Items[CurrentIndex];
                m_DialogBox.SetText(currentData.Words);
                if (currentData.SoundAssets == ScriptItemEntry.NoVoice)
                {
                    //string audioPath = Path.Combine(LoadingManager.FilePath, App.instance.GetModel<DataAssets>().AssetsName + LoadingManager.PointExtension, currentData.SoundAssets);
                    string audioPath = currentData.SoundAssets;
                    m_AudioS.Play(audioPath);
                }
            }
            else
            {
                m_DialogBox.IsStop = true;
            }
            m_CurrentProcess.SetPerecent(CurrentIndex / (float)TargetData.Items.Count);
        }

        public void CreateOptions()
        {
            //TODO
        }
    }
}
