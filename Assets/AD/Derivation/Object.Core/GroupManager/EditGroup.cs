using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility.Object
{
    public class EditGroup : MonoBehaviour
    {
        #region TopLayer

        public ColliderLayer ColliderLayer;
        public ViewLayer ViewLayer;

        #endregion

        #region Resources

        public bool IsHaveTitleTip = false;

        #endregion

        private void Start()
        {
            if (IsHaveTitleTip)
            {
                ViewLayer.UIs["Title"].SetActive(false);
            }
        }

        public void DoUpdate(CameraCore core)
        {
            if(IsHaveTitleTip)
            {
                if (core.Target.transform.parent.parent.gameObject == this.gameObject)
                {
                    ViewLayer.UIs["Title"].SetActive(true);
                    ViewLayer.UIs["Title"].transform.LookAt(core.Core.transform);
                }
                else ViewLayer.UIs["Title"].SetActive(false);
            }
        }
    }
}
