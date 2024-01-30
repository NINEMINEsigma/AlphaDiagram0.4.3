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
    }
}
