using System.Collections;
using AD.Experimental.Performance;
using AD.Experimental.SceneTrans;
using AD.UI;
using AD.Utility.Object;
using UnityEngine;

namespace AD.Sample.Texter
{
    public class ChoosingPageSubManager : SubSceneLoader
    {
        public Animator animator;

        public void Setup(DataAssets assets)
        {
            CurrentLink = assets;
            CreaterName.SetText(assets.CreaterName);
            Description.SetText(assets.Description);
            DateTime.SetText(assets.DateTime);
            AssetsName.SetText(assets.AssetsName);
            SureCilck.AddListener(delegate
            {
                assets.Description = Description.text;
                assets.Save();
                ADGlobalSystem.instance.OnEnd();
                SceneTrans.instance.SceneOpenAnimation["EntryScene"] = "ZoomIn";
            });
            CameraCore.IsLockKeyBoardDetectForMove = true;
        }

        public void UnloadClick()
        {
            animator.Play("ZoomOut");
            StartCoroutine(RealUnload());
        }

        private IEnumerator RealUnload()
        {
            CameraCore.IsLockKeyBoardDetectForMove = false;
            yield return new WaitForSeconds(0.5f);
            MainLoadAssets.Unload(this.SceneName);
        }

        public DataAssets CurrentLink;
        public Button SureCilck;
        public Text CreaterName;
        public Text AssetsName;
        public ModernUIInputField Description;
        public Text DateTime;
    }
}

