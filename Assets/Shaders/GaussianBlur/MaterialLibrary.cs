using UnityEngine.Rendering;
using UnityEngine;

namespace AD.Experimental.Rendering.Universal
{
    public class MaterialLibrary
    {
        public readonly Material gaussianBlur;

        public MaterialLibrary(AdditionalPostProcessData data)
        {
            gaussianBlur = Load(data.shaders.gaussianBlur);
        }

        Material Load(Shader shader)
        {
            if (shader == null)
            {
                Debug.LogErrorFormat($"Missing shader. {GetType().DeclaringType.Name} render pass will not execute. Check for missing reference in the renderer resources.");
                return null;
            }
            else if (!shader.isSupported)
            {
                return null;
            }

            return CoreUtils.CreateEngineMaterial(shader);

        }
        internal void Cleanup()
        {
            CoreUtils.Destroy(gaussianBlur);
        }
    }
}
