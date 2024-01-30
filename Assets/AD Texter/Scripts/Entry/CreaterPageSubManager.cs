using System.Collections;
using AD.Experimental.Performance;
using AD.UI;
using UnityEngine;

namespace AD.Sample.Texter
{
    public class CreaterPageSubManager : SubSceneLoader
    {
        public Animator animator;
        public GameObject Prefab;
        public Transform parent;

        public void Setup()
        {
            if (!ADGlobalSystem.instance.StringValues.TryGetValue(LoadingManager.CurrentCreateNameKey, out var CurrentCreater)) CurrentCreater = "New Creater";
            CreaterName.SetText(CurrentCreater);
            DateTimeStr = System.DateTime.Now.ToString();
            DateTime.SetText(DateTimeStr);
        }

        public void SureCilckSetup()
        {
            DataAssets assets = new(CreaterName.text, Description.text, DateTimeStr, AssetsName.text);
            ADGlobalSystem.instance.StringValues[LoadingManager.CurrentCreateNameKey] = CreaterName.text;
            ADGlobalSystem.instance.SaveNumericManager();
            assets.Save().CreateInstance(Prefab, parent);
            this.gameObject.SetActive(false);
        }

        public void UnloadClick()
        {
            animator.Play("ZoomOut");
            StartCoroutine(RealUnload());
        }

        private IEnumerator RealUnload()
        {
            yield return new WaitForSeconds(0.75f);
            MainLoadAssets.Unload(this.SceneName);
        }

        public Button SureCilck;
        public InputField CreaterName;
        public InputField AssetsName;
        public ModernUIInputField Description;
        public Text DateTime;

        public string DateTimeStr;
    }
}

