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
        [Header("Data")]
        public ProjectScriptSlicerData data;

        protected override void SetupProjectItemData(ProjectItemData data)
        {
            this.data = ADGlobalSystem.FinalCheckWithThrow(data as ProjectScriptSlicerData);
            mainTitle.SetText(data.ProjectItemID);
        }

    }
}
