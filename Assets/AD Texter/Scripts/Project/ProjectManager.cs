using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using UnityEngine;

namespace AD.Simple.Texter
{
    public class ProjectManager : ADController
    {
        public GameEditorApp UIApp => GameEditorApp.instance;

        public ProjectData CurrentProjectData;

        private void Start()
        {
            App.instance.RegisterController(this);
        }

        public override void Init()
        {
            CurrentProjectData = new()
            {
                DataAssetsForm = Architecture.GetModel<DataAssets>()
            };
            CurrentProjectData.Load();
        }

        public void SaveProjectData()
        {
            CurrentProjectData.Save();
        }

        public void BackToEntry()
        {
            SaveProjectData();
            ADGlobalSystem.instance.OnEnd();
        }
    }
}
