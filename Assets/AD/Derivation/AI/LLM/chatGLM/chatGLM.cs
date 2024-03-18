using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace AD.Experimental.LLM
{
    public class chatGLM : LLM
    {
        public override VariantSetting GetSetting()
        {
            var setting = base.GetSetting();
            setting.Settings = new();
            if (ADGlobalSystem.Serialize<List<List<string>>>(m_History, out string str))
                setting.Settings.Add("History", str);

            return setting;
        }

        public override void InitVariant(VariantSetting setting)
        {
            base.InitVariant(setting);
            m_History = ADGlobalSystem.Deserialize<List<List<string>>>(setting.Settings["History"], out object obj) ? obj as List<List<string>> : new();
        }

        public chatGLM()
        {
            url = "http://localhost:8000";
        }

        [SerializeField] private List<List<string>> m_History = new();

        public override void PostMessage(string _msg, Action<string> _callback)
        {
            base.PostMessage(_msg, _callback);
        }

        public override IEnumerator Request(string _postWord, System.Action<string> _callback)
        {
            stopwatch.Restart();
            string jsonPayload = JsonConvert.SerializeObject(new RequestData
            {
                prompt = _postWord,
                history = m_History
            });

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.responseCode == 200)
                {
                    string _msg = request.downloadHandler.text;
                    ResponseData response = JsonConvert.DeserializeObject<ResponseData>(_msg);

                    //记录历史对话
                    m_History = response.history;
                    //添加记录
                    m_DataList.Add(new SendData("assistant", _msg));
                    //回调
                    _callback(response.response);

                }

            }

            stopwatch.Stop();
            Debug.Log("chatGLM耗时：" + stopwatch.Elapsed.TotalSeconds);
        }

        #region 报文定义

        [Serializable]
        private class RequestData
        {
            public string prompt;
            public List<List<string>> history;
        }

        [Serializable]
        private class ResponseData
        {
            public string response;
            public List<List<string>> history;
            public int status;
            public string time;
        }
        #endregion

    }
}
