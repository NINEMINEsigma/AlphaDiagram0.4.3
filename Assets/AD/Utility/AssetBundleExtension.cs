using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AD.BASE;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace AD.Utility
{
    public static class AssetBundleExtension
    {
        public static AssetBundle LoadAssetBundle(string path)
        {
            return AD.BASE.FileC.LoadAssetBundle(path);
        }

        public static AssetBundle LoadAssetBundle(string path, params string[] targetName)
        {
            return AD.BASE.FileC.LoadAssetBundle(path, targetName);
        }

        [Obsolete]
        public static AssetBundle LoadFromMemory(byte[] binary)
        {
            return AssetBundle.LoadFromMemory(binary);
        }

        [Obsolete]
        public static AssetBundle LoadFromMemory(string filePath)
        {
            if (File.Exists(filePath))
            {
                return AssetBundle.LoadFromMemory(FileC.ReadAllBytes(filePath));
            }
            return null;
        }

        public static IEnumerator DownloadHandlerAssetBundle(string uri, Action<AssetBundle> callback)
        {
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri);
            yield return request.SendWebRequest();
            if (string.IsNullOrEmpty(request.error) == false)
            {
                Debug.LogError(request.error);
                yield break;
            }
            callback(UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request));
        }

    }

    public class AssetBundleInfo
    {
        public AssetBundleInfo(AssetBundle assetBundle, bool isLoad) { this.assetBundle = assetBundle; this.isLoad = isLoad && assetBundle != null; }
        public AssetBundleInfo(string path) : this(AssetBundleExtension.LoadAssetBundle(path), true) { }
        public AssetBundleInfo(string path, params string[] targetName) : this(AssetBundleExtension.LoadAssetBundle(path, targetName), true) { }
        [Obsolete] public AssetBundleInfo(byte[] binary) : this(AssetBundleExtension.LoadFromMemory(binary), true) { }

        //public static implicit operator AssetBundle(AssetBundleInfo info) => info.assetBundle;
        public static implicit operator bool(AssetBundleInfo info) => info.isLoad && info.assetBundle != null;

        public AssetBundle assetBundle { get; private set; }
        [SerializeField] private bool isLoad;
        private Dictionary<int, AsyncOperationClosure> closures = new();

        public class AsyncOperationClosure
        {
            public Action parentInfoAction;
            public Action<UnityEngine.Object> callback;

            public AsyncOperationClosure(Action parentInfoAction, Action<UnityEngine.Object> callback)
            {
                this.parentInfoAction = parentInfoAction;
                this.callback = callback;
            }

            public void WhenCompleted(AsyncOperation asyncOperation)
            {
                Debug.Log(asyncOperation.progress);
                callback.Invoke(asyncOperation.As<AssetBundleRequest>().asset);
                parentInfoAction.Invoke();
            }
        }

        public void UnLoad()
        {
            assetBundle.Unload(true);
            isLoad = false;
        }

        public void UnLoadAndNotClearMemory()
        {
            assetBundle.Unload(false);
            isLoad = false;
        }

        public UnityEngine.Object[] LoadAllAssets()
        {
            return assetBundle.LoadAllAssets();
        }

        public T[] LoadAllAssets<T>() where T : UnityEngine.Object
        {
            return assetBundle.LoadAllAssets<T>();
        }

        public bool Contains(string name)
        {
            return assetBundle.Contains(name);
        }

        public UnityEngine.Object Load(string name)
        {
            return assetBundle.LoadAsset(name);
        }

        public T Load<T>(string name) where T : UnityEngine.Object
        {
            return assetBundle.LoadAsset<T>(name);
        }

        public GameObject LoadGameObject(string name)
        {
            return Load<GameObject>(name);
        }

        public Texture LoadPicture(string name)
        {
            return Load<Texture>(name);
        }

        public AudioClip LoadAudio(string name)
        {
            return Load<AudioClip>(name);
        }

        public Material LoadMaterial(string name)
        {
            return Load<Material>(name);
        }

        public Material LoadMaterialAsNew(string name)
        {
            var temp = Load<Material>(name);
            return temp == null ? null : new Material(temp);
        }

        public void Load(string name, Action<UnityEngine.Object> callback)
        {
            assetBundle.LoadAssetAsync(name);
            AssetBundleRequest temp = assetBundle.LoadAssetAsync(name);
            int i = closures.Count;
            while (closures.ContainsKey(i)) i++;
            AsyncOperationClosure closure = new AsyncOperationClosure(() => closures.Remove(i), callback);
            closures[i] = closure;
            temp.completed += closure.WhenCompleted;
        }

        public void LoadGameObject(string name, Action<GameObject> callback)
        {
            Load(name, T => callback.Invoke(T as GameObject));
        }

        public void LoadPicture(string name, Action<Texture> callback)
        {
            Load(name, T => callback.Invoke(T as Texture));
        }

        public void LoadAudio(string name, Action<AudioClip> callback)
        {
            Load(name, T => callback.Invoke(T as AudioClip));
        }

        public void LoadMaterial(string name, Action<Material> callback)
        {
            Load(name, T => callback.Invoke(T as Material));
        }
    }
}

