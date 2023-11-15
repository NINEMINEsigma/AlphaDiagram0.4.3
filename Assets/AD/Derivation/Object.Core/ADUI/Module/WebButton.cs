using UnityEngine;
using AD.BASE;

namespace AD.UI
{
    public class WebButton : MonoBehaviour, ICanBreakpointResume
    {
        public string loadPath, savePath;
        public double loadedBytes = 1000000000;
        public AD.UI.Text text;

        public void DownLoad()
        {
            this.BreakpointResume(loadPath, savePath, loadedBytes, T => text.SetText(T));
        }
    }
}
