using System.Collections;
using AD.Experimental.Performance;
using AD.UI;
using UnityEngine;

namespace AD.Simple.Texter
{
    public class ChoosingPageSubManager : SubSceneLoader
    {
        public Animator animator;

        public void Setup(DataAssets assets)
        {
            CreaterName.SetText(assets.CreaterName);
            Description.SetText(assets.Description);
            DateTime.SetText(assets.DateTime);
            SureCilck.AddListener(delegate
            {
                assets.CreaterName = CreaterName.text;
                assets.Description = Description.text;
                ADGlobalSystem.instance.StringValues[LoadingManager.CurrentCreateNameKey] = CreaterName.text;
                ADGlobalSystem.instance.SaveNumericManager();
            });
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
    }
}

