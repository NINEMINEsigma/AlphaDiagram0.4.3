using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AD.UI
{
    public class InkGradient : ADController, IADUI
    {
        #region ADUI

        public bool Selected = false;
        private BehaviourContext _Context;
        public bool IsNeedContext => true;
        public BehaviourContext Context
        {
            get
            {
                if (!IsNeedContext) return null;
                _Context ??= this.GetOrAddComponent<BehaviourContext>();
                return _Context;
            }
        }
        public string ElementName { get; set; } = "null";
        public int SerialNumber { get; set; } = 0;
        public string ElementArea = "null";
        public IADUI Obtain(int serialNumber)
        {
            return ADUI.Items.Find((P) => P.SerialNumber == serialNumber);
        }
        public IADUI Obtain(string elementName)
        {
            return ADUI.Items.Find((P) => P.ElementName == elementName);
        }
        public void InitializeContext()
        {
            Context.OnPointerEnterEvent = ADUI.InitializeContextSingleEvent(Context.OnPointerEnterEvent, OnPointerEnter);
            Context.OnPointerExitEvent = ADUI.InitializeContextSingleEvent(Context.OnPointerExitEvent, OnPointerExit);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            Selected = true;
            ADUI.CurrentSelect = null;
            ADUI.UIArea = ElementArea;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            Selected = false;
            ADUI.UIArea = "null";
        }

        #endregion

        public InkGradient()
        {
            this.ElementArea = "InkGradient";
        }

        public override void Init()
        {

        }

        #region Assets

        [Header("Assets")]
        public InkGradientAssets mAssets;
        public Material mGradientMaterial;
        [Header("Setting")]
        public bool IsGenerateNewInstanceMaterial = true;
        public Image View;
        public float WaitTime = 1;

        private void Start()
        {
            AD.UI.ADUI.Initialize(this);
            if(IsGenerateNewInstanceMaterial)
            {
                mGradientMaterial = View.material = new(mGradientMaterial);
            }
            mGradientMaterial.SetTexture("_StartTex", mAssets.Normal.texture);
            mGradientMaterial.SetTexture("_TargetTex", mAssets.Normal.texture);
            mGradientMaterial.SetFloat("_Process", 1);
            View.sprite = mAssets.Normal;
            coroutine = null;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ADUI.DestroyADUI(this);
        }

        public void StartInkGrandient(string name)
        {
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(GrandinentChanging(View.sprite, mAssets.Obtain(name), WaitTime));
        }

        private Coroutine coroutine;

        public IEnumerator GrandinentChanging(Sprite startSprite, Sprite targetSprite,float waitTime)
        {
            mGradientMaterial.SetTexture("_StartTex", startSprite.texture);
            mGradientMaterial.SetTexture("_TargetTex", targetSprite.texture);
            mGradientMaterial.SetFloat("_Process", 0);
            while (waitTime>-1)
            {
                mGradientMaterial.SetFloat("_Process", 1 - waitTime / WaitTime);
                mGradientMaterial.SetFloat("_Process2", 1 - waitTime / WaitTime + 0.5f);
                yield return null;
                waitTime -= Time.deltaTime * 2;
            }
            mGradientMaterial.SetTexture("_StartTex", targetSprite.texture);
            mGradientMaterial.SetTexture("_TargetTex", targetSprite.texture);
            mGradientMaterial.SetFloat("_Process", 1);
            View.sprite = targetSprite;
            coroutine = null;
        }

        #endregion
    }
}
