using System;
using AD.Experimental.GameEditor;
using AD.UI;
using UnityEngine;
using UnityEngine.Events;

namespace AD.Sample.Texter.Project
{
    public class LLMCorePExWindow : CustomWindowElement
    {
        protected override bool isSubPageUsingOtherSetting => true;

        public override CustomWindowElement Init()
        {
            base.Init();
            this.isCanDrag = false;

            UnityAction<CustomWindowElement> action = window => GameObject.Destroy(window.gameObject);
            HowBackPool = action;
            MainListView.SetPrefab(WordItemPrefab);
            MainListView.Clear();

            SendMessageButton.RemoveAllListeners();
            if (GameEditorApp.instance.GetController<Properties>().MatchTarget is LLMCore core)
            {
                SendMessageButton.AddListener(() =>
                {
                    LockMask.gameObject.SetActive(true);
                    try
                    {
                        core.MatchLLM.PostMessage(Message.text, T =>
                        {
                            if (T.StartsWith("ERROR"))
                            {
                                GameEditorApp.instance.GetSystem<GameEditorWindowGenerator>().ObtainElement(new Vector2(200, 50)).SetTitle("Error").GenerateText(T);
                                GameEditorApp.instance.GetController<Information>().Error(T);
                            }
                            else
                            {
                                SetupNewWord(MainListView.GenerateItem(), core.MatchLLM.m_DataList[^2].content);
                                SetupNewWord(MainListView.GenerateItem(), T);
                            }
                            LockMask.gameObject.SetActive(false);
                        });
                        StatusText.SetText("正常");
                    }
                    catch (Exception ex)
                    {
                        StatusText.SetText("错误 : " + ex.Message);
                    }
                });

                foreach (var item in core.MatchLLM.m_DataList)
                {
                    SetupNewWord(MainListView.GenerateItem(), item.content);
                }
            }
            else ADGlobalSystem.Error("Unknown Error");
            return this;
        }

        private void SetupNewWord(ListViewItem item,string request)
        {
            var inputField = item.GetComponentInChildren<ModernUIInputField>();
            inputField.SetText(request);
        }

        public ListView MainListView;
        public ListViewItem WordItemPrefab;

        [Header("Console")]
        public ModernUIInputField Message;
        public Button SendMessageButton;
        public Text StatusText;
        public UnityEngine.UI.Image LockMask;
    }
}
