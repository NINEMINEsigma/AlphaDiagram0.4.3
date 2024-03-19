using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AD.Experimental.LLM
{
    public class BaiduSettings
    {
        #region 参数定义
        /// <summary>
        /// API Key
        /// </summary>
        [Header("填写应用的API Key")] public string m_API_key = string.Empty;
        /// <summary>
        /// Secret Key
        /// </summary>
        [Header("填写应用的Secret Key")] public string m_Client_secret = string.Empty;
        /// <summary>
        /// 是否从服务器获取token
        /// </summary>
        public bool GetTokenFromServer = true;
        /// <summary>
        /// token值
        /// </summary>
        public string m_Token = string.Empty;
        /// <summary>
        /// 获取Token的地址
        /// </summary>
        public string m_AuthorizeURL = "https://aip.baidubce.com/oauth/2.0/token";
        #endregion

        public void GetTokenAction(string _token)
        {
            m_Token = _token;
        }

        public IEnumerator GetToken(System.Action<string> _callback)
        {
            //获取token的api地址
            string _token_url = string.Format(m_AuthorizeURL + "?client_id={0}&client_secret={1}&grant_type=client_credentials"
                , m_API_key, m_Client_secret);

            using (UnityWebRequest request = new UnityWebRequest(_token_url, "GET"))
            {
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                yield return request.SendWebRequest();
                if (request.isDone)
                {
                    string _msg = request.downloadHandler.text;
                    TokenInfo _textback = JsonUtility.FromJson<TokenInfo>(_msg);
                    string _token = _textback.access_token;
                    _callback(_token);

                }
            }
        }

        [System.Serializable]
        public class TokenInfo
        {
            public string access_token = string.Empty;
        }
    }

    public class ChatBaidu : LLM
    {
        public override VariantSetting GetSetting()
        {
            var setting = base.GetSetting();
            setting.Settings = new()
            {
                { "APIKey", m_Settings.m_API_key },
                { "GetTokenFromServer", m_Settings.GetTokenFromServer.ToString() },
                { "Secret", m_Settings.m_Client_secret },
                { "Token", m_Settings.m_Token },
                { "AuthorizeURL", m_Settings.m_AuthorizeURL },
                { "ModelType", m_ModelType.ToString() },
                { "History", ADGlobalSystem.Serialize<List<message>>(m_History,out string str)?str:"" }
            };
            return setting;
        }

        public override void InitVariant(VariantSetting setting)
        {
            base.InitVariant(setting);
            m_Settings.m_API_key = setting.Settings["APIKey"];
            m_Settings.GetTokenFromServer = bool.Parse(setting.Settings["GetTokenFromServer"]);
            m_Settings.m_Client_secret = setting.Settings["Secret"];
            m_Settings.m_Token = setting.Settings["Token"];
            m_Settings.m_AuthorizeURL = setting.Settings["AuthorizeURL"];
            m_ModelType = Enum.Parse<ModelType>(setting.Settings["ModelType"]);
            m_History = ADGlobalSystem.Deserialize<List<message>>(setting.Settings["History"], out var obj) ? obj as List<message> : new();
        }

        public ChatBaidu()
        {
            url = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/eb-instant";
        }

        public override void Init()
        {
            base.Init();
            if (m_Settings.GetTokenFromServer)
            {
                StartCoroutine(m_Settings.GetToken(m_Settings.GetTokenAction));
            }
            GetEndPointUrl();
        }

        /// <summary>
        /// token脚本
        /// </summary>
        [SerializeField] private BaiduSettings m_Settings = new();
        /// <summary>
        /// 历史对话
        /// </summary>
        private List<message> m_History = new List<message>();
        /// <summary>
        /// 选择的模型类型
        /// </summary>
        [Header("设置模型名称")]
        public ModelType m_ModelType = ModelType.ERNIE_Bot_turbo;

        public override void PostMessage(string _msg, Action<string> _callback)
        {
            base.PostMessage(_msg, _callback);
        }

        public override IEnumerator Request(string _postWord, System.Action<string> _callback)
        {
            stopwatch.Restart();

            string _postUrl = url + "?access_token=" + m_Settings.m_Token;
            m_History.Add(new message("user", _postWord));
            RequestData _postData = new RequestData
            {
                messages = m_History
            };

            using (UnityWebRequest request = new UnityWebRequest(_postUrl, "POST"))
            {
                string _jsonData = JsonUtility.ToJson(_postData);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonData);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.responseCode == 200)
                {
                    string _msg = request.downloadHandler.text;
                    ResponseData response = JsonConvert.DeserializeObject<ResponseData>(_msg);

                    //历史记录
                    string _responseText = response.result;
                    m_History.Add(new message("assistant", response.result));

                    //添加记录
                    m_DataList.Add(new SendData("assistant", response.result));
                    //回调
                    _callback(response.result);

                }

            }


            stopwatch.Stop();
            Debug.Log("chat百度-耗时：" + stopwatch.Elapsed.TotalSeconds);
        }

        /// <summary>
        /// 获取资源路径
        /// </summary>
        private void GetEndPointUrl()
        {
            url = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/" + CheckModelType(m_ModelType);
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        private string CheckModelType(ModelType _type)
        {
            if (_type == ModelType.ERNIE_Bot)
            {
                return "completions";
            }
            if (_type == ModelType.ERNIE_Bot_turbo)
            {
                return "eb-instant";
            }
            if (_type == ModelType.BLOOMZ_7B)
            {
                return "bloomz_7b1";
            }
            if (_type == ModelType.Qianfan_BLOOMZ_7B_compressed)
            {
                return "qianfan_bloomz_7b_compressed";
            }
            if (_type == ModelType.ChatGLM2_6B_32K)
            {
                return "chatglm2_6b_32k";
            }
            if (_type == ModelType.Llama_2_7B_Chat)
            {
                return "llama_2_7b";
            }
            if (_type == ModelType.Llama_2_13B_Chat)
            {
                return "llama_2_13b";
            }
            if (_type == ModelType.Llama_2_70B_Chat)
            {
                return "llama_2_70b";
            }
            if (_type == ModelType.Qianfan_Chinese_Llama_2_7B)
            {
                return "qianfan_chinese_llama_2_7b";
            }
            if (_type == ModelType.AquilaChat_7B)
            {
                return "aquilachat_7b";
            }
            return "";
        }

        #region 数据定义
        //发送的数据
        [Serializable]
        private class RequestData
        {
            public List<message> messages = new List<message>();//发送的消息
            public bool stream = false;//是否流式输出
            public string user_id = string.Empty;
        }
        [Serializable]
        private class message
        {
            public string role = string.Empty;//角色
            public string content = string.Empty;//对话内容
            public message() { }
            public message(string _role, string _content)
            {
                role = _role;
                content = _content;
            }
        }

        //接收的数据
        [Serializable]
        private class ResponseData
        {
            public string id = string.Empty;//本轮对话的id
            public int created;
            public int sentence_id;//表示当前子句的序号。只有在流式接口模式下会返回该字段
            public bool is_end;//表示当前子句是否是最后一句。只有在流式接口模式下会返回该字段
            public bool is_truncated;//表示当前子句是否是最后一句。只有在流式接口模式下会返回该字段
            public string result = string.Empty;//返回的文本
            public bool need_clear_history;//表示用户输入是否存在安全
            public int ban_round;//当need_clear_history为true时，此字段会告知第几轮对话有敏感信息，如果是当前问题，ban_round=-1
            public Usage usage = new Usage();//token统计信息，token数 = 汉字数+单词数*1.3 
        }
        [Serializable]
        private class Usage
        {
            public int prompt_tokens;//问题tokens数
            public int completion_tokens;//回答tokens数
            public int total_tokens;//tokens总数
        }

        #endregion

        /// <summary>
        /// 模型名称
        /// </summary>
        public enum ModelType
        {
            ERNIE_Bot,
            ERNIE_Bot_turbo,
            BLOOMZ_7B,
            Qianfan_BLOOMZ_7B_compressed,
            ChatGLM2_6B_32K,
            Llama_2_7B_Chat,
            Llama_2_13B_Chat,
            Llama_2_70B_Chat,
            Qianfan_Chinese_Llama_2_7B,
            AquilaChat_7B,
        }
    }
}
