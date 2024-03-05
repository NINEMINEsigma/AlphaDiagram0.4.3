using AD.UI;
using UnityEngine;

namespace AD.Sample.Texter.Scene
{
    public class EasyTextManager : SubSceneManager
    {
        [Header("Assets")]
        public Button mButton;
        public override IButton BackSceneButton => mButton;
        public ModernUIInputField mainInputField;
        [Header("Data")]
        public ProjectTextData data;
        public Mode CurrentMode = Mode.Text;

        public enum Mode
        {
            Text, Description
        }

        protected override void SetupProjectItemData(ProjectItemData data)
        {
            this.data = ADGlobalSystem.FinalCheckWithThrow(data as ProjectTextData);
            mainTitle.SetText(data.ProjectItemID);
            SetupText();
        }

        public void InputFieldOnChange(string T)
        {
            switch (CurrentMode)
            {
                case Mode.Text:
                    {
                        data.text = T;
                    }
                    break;
                case Mode.Description:
                    {
                        data.description = T;
                    }
                    break;
            }
        }

        public void SetupText()
        {
            mainInputField.SetText(data.text);
        }

        public void SetupDescription()
        {
            mainInputField.SetText(data.description);
        }

        public void SwitchToText()
        {
            CurrentMode = Mode.Text;
            SetupText();
        }

        public void SwitchToDescription()
        {
            CurrentMode= Mode.Description;
            SetupDescription();
        }
    }
}
