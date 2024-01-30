using System.Collections;
using System.Collections.Generic;
using AD.UI;
using AD.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TTT
{
    public class TestController : MonoBehaviour
    {
        public string path;
        public string objName;
        public AssetBundleInfo info;
        public RawImage RawImage;

        public void TetsRun()
        {
            info ??= new(path);
            if (!info)
            {
                info.UnLoad();
                info = new(path);
            }
            info.Load(objName, T =>
            {
                Debug.Log(T.GetType().Name);
                RawImage.source.texture = T as Texture;
            });
        }

    }
}
