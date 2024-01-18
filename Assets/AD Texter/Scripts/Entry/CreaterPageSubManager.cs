using System.Collections;
using AD.Experimental.Performance;
using AD.UI;
using UnityEngine;

namespace AD.Simple.Texter
{
    public class CreaterPageSubManager : SubSceneLoader
    {
        public Animator animator;
        public GameObject Prefab;
        public Canvas Canvas;

        public void Setup()
        {
            if (!ADGlobalSystem.instance.StringValues.TryGetValue(LoadingManager.CurrentCreateNameKey, out var CurrentCreater)) CurrentCreater = "New Creater";
            CreaterName.SetText(CurrentCreater);
            Description.SetText("");
            DateTimeStr = System.DateTime.Now.ToShortDateString();
            DateTime.SetText(DateTimeStr);
        }

        public void SureCilckSetup()
        {
            DataAssets assets = new(CreaterName.text, Description.text, DateTimeStr);
            ADGlobalSystem.instance.StringValues[LoadingManager.CurrentCreateNameKey] = CreaterName.text;
            ADGlobalSystem.instance.SaveNumericManager();
            assets.CreateInstance(Prefab, Canvas);
            this.gameObject.SetActive(false);
        }

        public void UnloadClick()
        {
            animator.Play("ZoomOut");
            StartCoroutine(RealUnload());
        }

        private IEnumerator RealUnload()
        {
            yield return new WaitForSeconds(1f);
            MainLoadAssets.Unload(this.SceneName);
        }

        public Button SureCilck;
        public InputField CreaterName;
        public ModernUIInputField Description;
        public Text DateTime;

        public string DateTimeStr;
    }
}

