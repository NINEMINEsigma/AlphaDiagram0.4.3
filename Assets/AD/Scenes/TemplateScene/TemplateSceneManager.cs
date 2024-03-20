using System;
using System.Collections.Generic;
using System.Linq;
using AD.Utility;
using UnityEngine;

[Serializable]
public class ADSceneAssets
{
    public ADSerializableDictionary<GameObject, List<GameObject>> All;

    private void LogWarning(string index)
    {
        Debug.LogWarning("Cannt Find Layer:" + index);
    }

    public void LoadAllKeyLayer()
    {
        All = new();
        foreach (GameObject rootObject in SceneExtension.GetCurrentRootGameObject())
        {
            var temp = rootObject.name.Trim().Split(" ");
            if (temp.Length < 2) continue;
            if (temp[^1] != "layer" && temp[^1] != "Layer") continue;
            var currentList = new List<GameObject>();
            foreach (Transform child in rootObject.transform)
            {
                currentList.Add(child.gameObject);
            }
            All[rootObject] = currentList;
        }
    }

    public GameObject FindLayer(string name)
    {
        return All.FirstOrDefault(T => T.Key.name == name).Key;
    }

    public List<GameObject> FindLayerChilds(string name)
    {
        var root = FindLayer(name);
        return root == null ? null : (All.TryGetValue(root, out var result) ? result : null);
    }

}

[ExecuteAlways, Serializable]
public class TemplateSceneManager : MonoBehaviour
{
    public static ADSceneAssets SceneComponents;
    [SerializeField] public ADSceneAssets MySceneComponents;
    public bool IsDebugInfo = true;

    private void OnValidate()
    {
        LoadAllKeyLayer();
    }

    private float clock = 0;
    private void Update()
    {
        if (!Application.isPlaying)
        {
            clock += Time.deltaTime;
            if (clock > 3)
            {
                clock = 0;
                LoadAllKeyLayer();
            }
        }
        else this.gameObject.SetActive(false);
    }

    private void Awake()
    {
        LoadAllKeyLayer();
    }

    public void LoadAllKeyLayer()
    {
        SceneComponents ??= new();
        SceneComponents.LoadAllKeyLayer();
        MySceneComponents = SceneComponents;
        int count = 0, layercount = 0;
        foreach (var item in MySceneComponents.All)
        {
            count++;
            layercount++;
            foreach (var chlidlist in item.Value)
            {
                count++;
            }
        }
        if (IsDebugInfo)
            Debug.Log("[" + System.DateTime.Now.ToShortDateString() +
                "] Detect On Current Scene , Find " + layercount.ToString() + (layercount > 1 ? " Layers" : "Layer")
                + " , And Total " + count.ToString() + (count > 1 ? " Items." : "Item.") +
                "\nMessage From TemplateSceneManager<" + gameObject.name + ">");
    }
}
