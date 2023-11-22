using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Experimental.ChatGPT
{
    public class TestTalk : MonoBehaviour
    {
        public AD.UI.InputField inputField;
        public AD.UI.Text outputField;

        public AD.Utility.AIExtension.AITalkAPI AITalkAPI;

        private void Awake()
        {
            AITalkAPI = new();
        }

        private void Update()
        {
            outputField.text = AITalkAPI.Reply;
        }

        public void Click()
        {
            AITalkAPI.SendMessage(inputField.text);
        }
    }
}
