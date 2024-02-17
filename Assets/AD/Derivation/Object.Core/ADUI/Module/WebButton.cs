using UnityEngine;
using AD.BASE;

namespace AD.UI
{
    public class WebButton : MonoBehaviour
    {
        public string loadPath, savePath;
        public double loadedBytes = 1 << 20;
        public Animator animator;
        public AD.UI.Text text;

        public void DownLoad()
        {
            animator.SetBool("Loading", true);
            this.StartCoroutine(FileC.BreakpointResume(this, loadPath, savePath, loadedBytes, (T, P) =>
            {
                text.SetText(T >= 0 ? (T.ToString("F2") + " " + P) : P);
                animator.SetBool("Loading", false);
            }));
        }
    }
}
