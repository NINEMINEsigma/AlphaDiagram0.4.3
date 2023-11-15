using System;
using System.Collections.Generic;
using AD.BASE;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AD.Experimental.Malody
{
#if UNITY_EDITOR

    public class MalodyBeatMap : MonoBehaviour
    {
        public MalodyBeatMapBM bm;
        public MalodyBeatMapBMTimeMode bmT;
        public string path;
    }

#endif

    [Serializable]
    public class MalodyBeatMapBM
    {
        public Meta meta = new();
        public List<TimeC> time = new();
        public List<Note> note = new();
        public Extra extra = new();

        public MalodyBeatMapBMTimeMode ToTimeMode()
        {
            MalodyBeatMapBMTimeMode result = new();
            result.meta = meta;
            result.time.Add(new TimeCTimeMode() { keyTime = 0, bpm = time[0].bpm });
            float bpm = time[0].bpm;
            float beattime = 60 / bpm;
            Note info = note[^1];
            int offset = info.offset;
            for (int i = 0; i < note.Count - 1; i++)
            {
                Note singleNote = note[i];
                result.note.Add(
                    new NoteTimeMode()
                    {
                        column = singleNote.column,
                        endTime = ((singleNote.endbeat != null) ? (TurnTime(singleNote.endbeat, beattime, offset)) : (0)),
                        keyTime = TurnTime(singleNote.beat, beattime, offset)
                    });
            }
            result.someInfo = info;
            result.extra = extra;
            return result;
        }

        public float TurnTime(List<int> beats, float beattime, int offtime)
        {
            if (beats.Count != 3) throw new ADException("this map is error");
            return beats[0] * beattime + beats[1] * beattime / beats[2] - offtime * 0.001f;
        }
    }

    [Serializable]
    public class MalodyBeatMapBMTimeMode
    {
        public Meta meta = new();
        public List<TimeCTimeMode> time = new();
        public Note someInfo = new();
        public List<NoteTimeMode> note = new();
        public Extra extra = new();
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MalodyBeatMap))]
    public class MalodyBeatMapEditor:Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            if(GUILayout.Button("Input")&&AD.ADGlobalSystem.Input<MalodyBeatMapBM>(target.As<MalodyBeatMap>().path, out object obj))
            {
                target.As<MalodyBeatMap>().bm = obj as MalodyBeatMapBM;
            }

            if (GUILayout.Button("Turn"))
            {
                target.As<MalodyBeatMap>().bmT = target.As<MalodyBeatMap>().bm.ToTimeMode();
            }

            serializedObject.ApplyModifiedProperties();

        }
    }

#endif

    [Serializable]
    public class Meta
    {
        public string creator;
        public string background;
        public string version;
        public int id;
        public int mode;
        public int time;
        public Song song;
        public ModeExt mode_ext;

        [Serializable]
        public class Song
        {
            public string title;
            public string artist;
            public int id;
        }

        [Serializable]
        public class ModeExt
        {
            public int column;
        }
    }

    [Serializable]
    public class TimeC
    {
        public List<int> beat;
        public float bpm; 
    }

    [Serializable]
    public class TimeCTimeMode
    {
        public float keyTime;
        public float bpm;
    }

    [Serializable]
    public class Note
    {
        public List<int> beat;
        public List<int> endbeat;
        public string sound;
        public int vol;
        public int offset;
        public int type; 
        public int column;
    }

    [Serializable]
    public class NoteTimeMode
    {
        public float keyTime;
        public float endTime;
        public int column;
    } 

    [Serializable]
    public class Extra
    {
        public Test test;

        [Serializable]
        public class Test
        {
            public int divide;
            public int speed;
            public int save;
            public int @lock;
            public int edit_mode;
        }
    }
}
