using System;
using System.Threading.Tasks;
using ChatGPT.API.Framework;
using UnityEngine;

namespace AD.Utility
{
    public class AIExtension
    {
        [Serializable]
        public sealed class AITalkAPI
        {
            public ChatGPTClient CGPTClient;
            public string Message;
            [SerializeField]
            private string reply;
            public string Reply
            {
                get
                {
                    return CGPTClient == null ? "No CGPTClient" : reply;
                }
                private set
                {
                    reply = value;
                }
            }

            public AITalkAPI()
            {
                Debug.Log("TryLoad Path : " + Application.streamingAssetsPath + @"\ChatGPTSetting.json");
                CGPTClient = new ChatGPTClient("Test"/*File.ReadAllText(Application.streamingAssetsPath + @"\ChatGPTSetting.json")*/);
            }

            public Task SendMessage(string text)
            {
                return Task.Run(() => OPENAI(string.IsNullOrEmpty(text) ? "..." : text));
            }
            public void OPENAI(string content)
            {
                if (string.IsNullOrEmpty(content)) return;
                if (CGPTClient == null)
                {
                    Message = "请先前往设置中设置 ChatGPT API";
                    return;
                }
                try
                {
                    var resp = CGPTClient.Ask("vpet", content);
                    var reply = resp.GetMessageContent();
                    if (resp.choices[0].finish_reason == "length")
                    {
                        reply += " ...";
                    }
                    Message = "当前Token使用: " + resp.usage.total_tokens;
                    Reply = reply;
                }
                catch (Exception exp)
                {
                    var e = exp.ToString();
                    string str = "请检查设置和网络连接";
                    if (e.Contains("401"))
                    {
                        str = "请检查API token设置";
                    }
                    Message = "API调用失败" + $",{str}\n{e}";//, GraphCore.Helper.SayType.Serious);
                }
            }
        }
    }
}
