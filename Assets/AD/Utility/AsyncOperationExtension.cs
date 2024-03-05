using System;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility
{
    public static class AsyncOperationExtension
    {
        public static bool IsDone(this AsyncOperation operation, float targetProgress = 0.9f)
        {
            Debug.Log("now progress : " + operation.progress);
            if (operation.progress >= targetProgress)
            {
                Debug.Log("The operation is complete");
                return true;
            }
            else return false;
        }

        public static void MarkCompleted(this AsyncOperation operation,Action action)
        {
            operation.completed += DoneCompleteHelper.Register(action).InternalCompleted;
        }

        public class DoneCompleteHelper
        {
            private static readonly List<DoneCompleteHelper> helpers = new();

            readonly Action action;

            public static DoneCompleteHelper Register(Action action)
            {
                var result = new DoneCompleteHelper(action);
                helpers.Add(result);
                return result;
            }

            public void UnRegister()
            {
                helpers.Remove(this);
            }

            public DoneCompleteHelper(Action action)
            {
                this.action = action;
            }

            public void InternalCompleted(AsyncOperation obj)
            {
                if (obj.IsDone(0.99f))
                {
                    action.Invoke();
                    this.UnRegister();
                }
            }

        }
    }
}
