using System.Collections;
using System.Collections.Generic;
using AD.UI;
using UnityEngine;

namespace AD.Sample.Texter.Scene
{
    public class ScriptSlicerListViewItem : ListViewItem
    {
        public InputField CharacterName, AudioAssets;
        public ModernUIInputField InputText;
        public ModernUIButton PreviewButton;

        public ScriptItemEntry mEntry;

        public override ListViewItem Init()
        {
            return this;
        }

        public void SetupScriptSlicerTarget(ScriptItemEntry entry)
        {
            CharacterName.SetText(entry.Name);
            CharacterName.AddListener(T =>
            {
                entry.Name = T;
            });
            AudioAssets.SetText(entry.SoundAssets);
            AudioAssets.AddListener(T =>
            {
                entry.SoundAssets = T;
            });
            InputText.SetText(entry.Words);
            InputText.AddListener(T =>
            {
                entry.Words = T;
            });
            PreviewButton.AddListener(()=>ScriptSlicerManager.PreviewOneEntry(entry));
        }


    }
}
