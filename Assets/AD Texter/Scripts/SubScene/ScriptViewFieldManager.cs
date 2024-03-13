using AD.Experimental.Performance;
using AD.Sample.Texter.Project;
using AD.UI;
using AD.Utility;
using AD.BASE;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace AD.Sample.Texter.Scene
{
    public class DefaultCharacterMoving : AlgorithmBase
    {

        public override Type[] ArgsTypes => new Type[] { typeof(ScriptItemEntry), typeof(int), typeof(GameObject) };

        public override Type ReturnType => typeof(GameObject);

        public override object Invoke(params object[] args)
        {
            ScriptItemEntry entry = args[0] as ScriptItemEntry;
            int index = (int)args[1];
            GameObject target = args[2] as GameObject;
            infos[target].Stop();
            infos.Remove(target);
            infos.Add(target, ADGlobalSystem.OpenCoroutine(Moving(target, entry.CharacterXPositionList[index])));
            return target;
        }

        Dictionary<GameObject, ADGlobalSystem.CoroutineInfo> infos = new();

        private IEnumerator Moving(GameObject target,float end)
        {
            float start = target.transform.localPosition.x;
            float counter = 1;
            while (counter>0)
            {
                target.transform.localPosition = new Vector3(Mathf.Lerp(end, start, counter), target.transform.localPosition.y, target.transform.localPosition.z);
                yield return null;
                counter -= Time.deltaTime;
            }
            target.transform.localPosition = new Vector3(end, target.transform.localPosition.y, target.transform.localPosition.z);
            infos.Remove(target);
        }
    }

    public class ScriptViewFieldManager : SubSceneManager
    {
        public ProjectScriptSlicer TargetSSItem;
        public ProjectScriptSlicerData TargetData;
        [Header("Prefab")]
        public GameObject CharacterPrefab;
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
        public ADSerializableDictionary<string, GameObject> CharacterInstances = new();

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
            AlgorithmExtension.RegisterAlgorithm("ScriptCharacterMoving", null, new DefaultCharacterMoving());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AlgorithmExtension.UnRegisterAlgorithm<DefaultCharacterMoving>();
        }

        protected override void SetupProjectItemData(ProjectItemData data)
        {
            if (data.MatchProjectItem is ProjectScriptSlicer projectScriptSlicer)
                SetupScript(projectScriptSlicer, 0);
            else
                throw new ADException("data.MatchProjectItem isn't any support type");
        }

        public void SetupScript(ProjectScriptSlicer target, int index)
        {
            TargetSSItem = target;
            CurrentIndex = index;
            TargetData = target.ProjectScriptSlicingSourceData;
            if (TargetData.BackgroundImage != ProjectScriptSlicerData.NoBackgroundImage)
            {
                //string imagePath = Path.Combine(LoadingManager.FilePath, App.instance.GetModel<DataAssets>().AssetsName + LoadingManager.PointExtension, TargetData.BackgroundImage);
                string imagePath = TargetData.BackgroundImage;
                m_Background.SyncLoadOnUrl(imagePath, true);
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
            List<GameObject> characterGameObject = TargetData.ObtainAllCharacter(CharacterPrefab);
            foreach (var item in characterGameObject)
            {
                CharacterInstances.Add(item.name, item);
            }
        }

        private void DoNext()
        {
            CurrentIndex++;
            DoMakeup();
        }

        private void DoMakeup()
        {
            m_CurrentProcess.SetPerecent(Mathf.Clamp01(CurrentIndex / (float)TargetData.Items.Count));
            if (CurrentIndex >= TargetData.Items.Count)
            {
                m_DialogBox.IsStop = true;
                return;
            }
            else
            {
                CurrentIndex = Mathf.Clamp(CurrentIndex, 0, TargetData.Items.Count);
            }
            if (CurrentIndex < TargetData.Items.Count)
            {
                var currentData = TargetData.Items[CurrentIndex];
                DoMakeupCurrent(currentData);
            }
            else
            {
                m_DialogBox.IsStop = true;
            }
        }

        private void DoMakeupCurrent(ScriptItemEntry currentData)
        {
            m_DialogBox.SetText(currentData.Words);
            if (currentData.SoundAssets == ScriptItemEntry.NoVoice)
            {
                //string audioPath = Path.Combine(LoadingManager.FilePath, App.instance.GetModel<DataAssets>().AssetsName + LoadingManager.PointExtension, currentData.SoundAssets);
                string audioPath = currentData.SoundAssets;
                m_AudioS.Play(audioPath);
            }
            for (int i = 0,e=currentData.CharacterXPositionList.Count; i <e ; i++)
            {
                if(CharacterInstances.TryGetValue(currentData.CharacterNameList[i],out var ins))
                {
                    //data , index , GameObject
                    AlgorithmExtension.GetAlgorithm("ScriptCharacterMoving").Invoke(currentData,i,ins);
                }
                else
                {
                    App.instance.AddMessage($"Character {currentData.CharacterNameList[i]} not find");
                    ADGlobalSystem.AddWarning("Character not find");
                }
            }
        }

        public void CreateOptions()
        {
            //TODO
        }
    }
}
