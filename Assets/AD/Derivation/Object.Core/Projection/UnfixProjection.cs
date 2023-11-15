using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AD.UI
{
    [Obsolete]
    [RequireComponent(typeof(Camera))]
    public class UnfixProjection : MonoBehaviour
    {
        [SerializeField] private GameObject TargetPrefab;
        public List<GameObject> childs = new();
        public bool Created;
        [SerializeField] private Material testMaterial;
        private Material material_;
        private RenderTexture render_;

        private RenderTexture render
        {
            get
            {
                if (render_ == null) render_ = new RenderTexture(256, 256, 0);
                return render_;
            }
        }

        public Material material
        {
            get
            {
                if (material_ == null)
                {
                    material_ = new Material(testMaterial);
                    material_.SetTexture("_MainTex", render);
                }

                return material_;
            }
        }

        private void Start()
        {
            var cam = GetComponent<Camera>();
            cam.targetTexture = render;
        }

        private void Update()
        {
            if (Created)
            {
                CreateNew();
                Created = false;
            }
        }

        public GameObject CreateNew()
        {
            var cat = Instantiate(TargetPrefab);
            //UnfixProjection_Child catc = cat.transform.GetChild(0).gameObject.AddComponent<UnfixProjection_Child>();
            cat.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<Image>().material = material;
            childs.Add(cat);
            return cat;
        }
    }

    public class UnfixProjection_Child : MonoBehaviour
    {
    }
}