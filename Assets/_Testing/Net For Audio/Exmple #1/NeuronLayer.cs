using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using AD.UI;
using AD.Utility;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AD.Experimental.Neuron.AudioSampling
{
    public class NeuronLayer : MonoBehaviour
    {
        [SerializeField] AudioSourceController audioSourceController;
        [SerializeField] LinearNeuron root;

        [SerializeField] LinearNeuron prefab;

        public float Value;

        private void Start()
        {
            StartCoroutine(DoStart());
        }

        private IEnumerator DoStart()
        {
            yield return new WaitForSeconds(1);
            var task = GameEditorApp.instance.GetModel<TaskList>().RegisterTask("Generate", 0, 0, new Vector2(0, 1), false);

            root = prefab.PrefabInstantiate();
            yield return new WaitForSeconds(1);
            GameEditorApp.instance.GetController<Hierarchy>().AddOnTop(root.MatchHierarchyEditor);
            root.IsMainTop = true;
            Init(root, null, 0, 0);
            for (int i = 0; i < 8; i++)
            {
                var current = prefab.PrefabInstantiate();
                Init(current, root, 1, i);
                for (int j = 0; j < 8; j++)
                {
                    var sub = prefab.PrefabInstantiate();
                    Init(sub, current, 2, j);
                    sub.StartCoroutine(DoUpdateSubN(sub, audioSourceController, (i * 8 + j)));
                    yield return null;
                    task.TaskPercent = (i * 8 + j) / (8.0f * 8.0f);
                }
            }
            end = true;
            yield return new WaitForSeconds(1);
            task.TaskPercent = 1.1f;
            GameEditorApp.instance.GetModel<TaskList>().RemoveTask(task);
            task = GameEditorApp.instance.GetModel<TaskList>().RegisterTask("Song Time", 0, 0, new Vector2(0, audioSourceController.CurrentClip.length), false);
            while (true)
            {
                task.TaskValue = audioSourceController.CurrentTime;
                yield return null;
            }
        }

        private IEnumerator DoUpdateSubN(LinearNeuron targeet, AudioSourceController audio, int index)
        {
            while (true)
            {
                yield return null;
                targeet.NeuronValue = audio.normalizedBands[index] * 0.5f;//(audio.bands[index * 4] + audio.bands[index * 4 + 1] + audio.bands[index * 4 + 2] + audio.bands[index * 4 + 3]) * 0.25f * 0.25f;
            }
        }

        private void Init(LinearNeuron target, LinearNeuron parent, int level, int index)
        {
            if (level == 0)
            {
                target.transform.position = new(5, 0, 5);
                target.transform.SetParent(transform);
            }
            else if (level == 1)
            {
                target.transform.SetParent(parent.transform);
                target.transform.localPosition = new Vector3(-5, index - 3.5f, 5);
                parent.Childs.Add(target);
            }
            else if (level == 2)
            {
                target.transform.SetParent(parent.transform);
                target.transform.localPosition = new Vector3(index - 3.5f - 10, 0, 5);
                parent.Childs.Add(target);
            }
        }

        public bool end = false;

        void LateUpdate()
        {
            if(end) Value = root.NeuronValue;
        }
    }
}
