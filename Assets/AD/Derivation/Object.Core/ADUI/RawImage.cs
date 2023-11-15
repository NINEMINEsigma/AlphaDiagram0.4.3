using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AD.UI
{
    [Serializable, RequireComponent(typeof(UnityEngine.UI.RawImage))]
    public class RawImage : AD.UI.ADUI
    {
        public RawImage()
        {
            ElementArea = "RawImage";
        }

        protected void Start()
        {
            AD.UI.ADUI.Initialize(this);
        }

        protected void OnDestroy()  
        {
            AD.UI.ADUI.Destory(this);
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/AD/RawImage", false, 10)]
        private static void ADD(UnityEditor.MenuCommand menuCommand)
        {
            AD.UI.RawImage rawImage = GameObject.Instantiate(ADGlobalSystem.instance._RawImage);
            rawImage.name = "New RawImage";
            GameObjectUtility.SetParentAndAlign(rawImage.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(rawImage.gameObject, "Create " + rawImage.name);
            Selection.activeObject = rawImage.gameObject;
        }
#endif
        public static AD.UI.RawImage Generate(string name = "New RawImage", Transform parent = null)
        {
            AD.UI.RawImage rawImage = GameObject.Instantiate(ADGlobalSystem.instance._RawImage);
            rawImage.transform.SetParent(parent, false);
            rawImage.transform.localPosition = Vector3.zero;
            rawImage.name = name;
            return rawImage;
        }

        private UnityEngine.UI.RawImage _source;
        public UnityEngine.UI.RawImage source
        {
            get
            {
                _source ??= GetComponent<UnityEngine.UI.RawImage>();
                return _source;
            }
        }
    }
}
