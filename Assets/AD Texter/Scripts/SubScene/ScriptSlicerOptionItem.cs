using System.Collections;
using System.Collections.Generic;
using AD.UI;
using UnityEngine;

namespace AD.Sample.Texter
{
    public class ScriptSlicerOptionItem : ListViewItem
    {
        public InputField TargetScriptSceneID, OptionName;

        public override ListViewItem Init()
        {
            return this;
        }

        public void SetupScriptSlicerTarget(SceneEndingSelectOption option)
        {
            TargetScriptSceneID.SetText(option.TargetScriptSceneID);
            TargetScriptSceneID.AddListener(T =>
            {
                option.TargetScriptSceneID = T;
            });
            OptionName.SetText(option.OptionName);
            OptionName.AddListener(T =>
            {
                option.OptionName = T;
            });
        }

    }
}
