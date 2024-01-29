using System.Collections;
using System.Collections.Generic;
using AD.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TTT
{
    public class TestController : MonoBehaviour
    {
        public string path;
        public string objName;
        public Texture texture;
        public AssetBundleInfo info;

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
                texture = T as Texture;
            });
        }

    }
}
