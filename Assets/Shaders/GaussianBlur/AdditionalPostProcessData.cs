using UnityEngine.Rendering;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AD.Experimental.Rendering.Universal
{
    [Serializable]
    public class AdditionalPostProcessData : ScriptableObject
    {

#if UNITY_EDITOR
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]

#pragma warning disable CS0618 // 类型或成员已过时
        [MenuItem("Assets/Create/Rendering/Universal Render Pipeline/Additional Post-process Data", priority = CoreUtils.assetCreateMenuPriority3 + 1)]
#pragma warning restore CS0618 // 类型或成员已过时
        static void CreateAdditionalPostProcessData()
        {
            //ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreatePostProcessDataAsset>(), "CustomPostProcessData.asset", null, null);
            var instance = CreateInstance<AdditionalPostProcessData>();
            AssetDatabase.CreateAsset(instance, string.Format("Assets/Settings/{0}.asset", typeof(AdditionalPostProcessData).Name));
            Selection.activeObject = instance;
        }
#endif
        [Serializable]
        public sealed class Shaders
        {
            public Shader gaussianBlur = Shader.Find("Custom/GaussianBlur");
        }
        public Shaders shaders;
    }
}
