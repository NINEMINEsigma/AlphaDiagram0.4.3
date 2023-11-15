using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Experimental.AssetScript
{
    [Serializable]
    public class ASResult
    {
        public string errorMessage = "";
        public ErrorType errorExceptionType = ErrorType.None;
        public Exception errorException = null;

        public object result = null;

        public bool ResultValue
        {
            get
            {
                return errorExceptionType == ErrorType.None;
            }
        }

        public enum ErrorType
        {
            None = 0,
            FileNotFind = 1 << 0,
            KeyNotMatch = 1 << 1,
            ThrowError = 1 << 2,
        }

        public T GetResult<T>()
        {
            return (T)result;
        }
    }
}
