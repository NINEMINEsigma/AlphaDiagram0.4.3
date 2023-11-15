using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AD.UI
{
    [RequireComponent(typeof(Camera))]
    public class Projection : MonoBehaviour
    {
        [Header("Projection")]
        [SerializeField] private Shader ADOriginatedShader;
        public List<Image> Options = new List<Image>();

        private Material AD__OriginatedMaterial;

        public Material ADOriginatedMaterial
        {
            get
            {
                if (AD__OriginatedMaterial == null)
                {
                    AD__OriginatedMaterial = new Material(ADOriginatedShader);
                }
                return AD__OriginatedMaterial;
            }
        }

        private void OnEnable()
        {
            if (ADOriginatedShader == null)
            {
                this.gameObject.SetActive(false);
                throw new System.Exception("No Shader");
            }
            else
            {
                foreach (var image in Options)
                {
                    image.material = AD__OriginatedMaterial;
                }
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            ADOriginatedMaterial.mainTexture = source;
        }
    }
}