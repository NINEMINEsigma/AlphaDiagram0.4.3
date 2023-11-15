using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Experimental.AssetScript
{
    public abstract class ASStream
    {
        public string path;
        public string key;
        public ASSeting setting;

        private string loadStr = "";
        private string[] sourceStrStreamLine;
        public string DataStr
        {
            get => loadStr;
            protected set
            {
                loadStr = value;
                sourceStrStreamLine = value.Split("\n");
            }
        }
        public string[] SourceStrStreamLine
        {
            get { return sourceStrStreamLine; }
        }

        public string this[int index]=> SourceStrStreamLine[index];

        public ASStream(string path, string key, ASSeting setting)
        {
            this.path = path;
            this.key = key;
            this.setting = setting;
        }

        public ASResult Result { get; private set; }

        protected abstract void RefreshResult();

        protected void SetResult(object result, System.Exception ex = null, ASResult.ErrorType errorType = ASResult.ErrorType.None, string errorMessage = "")
        {
            Result = new()
            {
                errorException = ex,
                errorExceptionType = errorType,
                errorMessage = errorMessage,
                result = result
            };
        }
        protected void SetError(System.Exception ex, ASResult.ErrorType errorType, string errorMessage, object result)
        {
            SetResult(result, ex, errorType, errorMessage);
        }
    }
}
