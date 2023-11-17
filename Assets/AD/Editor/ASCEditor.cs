using UnityEditor;
using UnityEngine;
using AD.UI;

[CustomEditor(typeof(AudioSourceController)), CanEditMultipleObjects]
public class ASCEditor : Editor
{
    private AudioSourceController that = null;

    private /*List<SourcePair>*/SerializedProperty SourcePairs;

    /*private SourcePair CurrentSourcePair = null;
    private AudioClip CurrentClip = null;
    private int CurrentIndex = 0;
    private bool IsPlay = false;
    private float CurrentTime = 0;*/

    private /*AudioPostMixer*/SerializedProperty _Mixer;

    private /*bool*/SerializedProperty LoopAtAll;
    private /*bool*/SerializedProperty DrawingLine;
    private /*bool*/SerializedProperty Sampling;

    private /*Gameobject*/SerializedProperty _m_LineRenderer;
    private /*Gameobject*/SerializedProperty LineRendererPrefab;

    private /*SpectrumLength*/SerializedProperty SpectrumCount;
    private /*uint*/SerializedProperty BandCount;
    private /*BufferDecreasingType*/SerializedProperty decreasingType;
    private /*float*/SerializedProperty decreasing;
    private /*float*/SerializedProperty DecreaseAcceleration;
    private /*BufferIncreasingType*/SerializedProperty increasingType;
    private /*float*/SerializedProperty increasing;

    private /*float[]*/SerializedProperty samples;
    private /*float[]*/SerializedProperty bands;
    private /*float[]*/SerializedProperty normalizedBands; 

    private void OnEnable()
    {
        that = target as AudioSourceController;

        SourcePairs = serializedObject.FindProperty("SourcePairs");

        _Mixer = serializedObject.FindProperty("_Mixer");

        _m_LineRenderer = serializedObject.FindProperty("_m_LineRenderer");
        LineRendererPrefab = serializedObject.FindProperty("LineRendererPrefab");

        LoopAtAll = serializedObject.FindProperty("LoopAtAll");
        DrawingLine = serializedObject.FindProperty("DrawingLine");
        Sampling = serializedObject.FindProperty("Sampling");

        SpectrumCount = serializedObject.FindProperty("SpectrumCount");
        BandCount = serializedObject.FindProperty("BandCount");
        decreasingType = serializedObject.FindProperty("decreasingType");
        decreasing = serializedObject.FindProperty("decreasing");
        DecreaseAcceleration = serializedObject.FindProperty("DecreaseAcceleration");
        increasingType = serializedObject.FindProperty("increasingType");
        increasing = serializedObject.FindProperty("increasing");

        samples = serializedObject.FindProperty("samples");
        bands = serializedObject.FindProperty("bands");
        normalizedBands = serializedObject.FindProperty("normalizedBands");
    }

    public override void OnInspectorGUI()
    {
        //BASE.OnInspectorGUI();

        serializedObject.Update();

        GUI.enabled = false;

        if (Application.isPlaying)
        {
            EditorGUILayout.IntSlider("SerialNumber", that.SerialNumber, 0, AD.UI.ADUI.TotalSerialNumber - 1);
            EditorGUILayout.TextField("ElementName", that.ElementName);
        }

        GUI.enabled = true;

        EditorGUILayout.PropertyField(SourcePairs);

        if (Application.isPlaying)
        {
            GUI.enabled = false;

            EditorGUILayout.IntSlider("CurrentIndex", that.CurrentIndex, 0, that.SourcePairs.Count - 1, null);
            if (that.CurrentClip != null) EditorGUILayout.Slider("CurrentTime", that.CurrentTime, 0, that.CurrentClip.length + 0.2f, null);

            GUI.enabled = true;

            if (that.CurrentDelay > 0.1f)
            {
                if (that.CurrentDelay < that.CurrentClip.length + 0.2f)
                    EditorGUILayout.Slider("Delay Clock", that.CurrentDelay, 0, that.CurrentClip.length + 0.2f, null);
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.Slider("Delay Clock", that.CurrentDelay, 0, Mathf.Infinity, null);
                }
            }

            GUI.enabled = true;

            if (GUILayout.Button("NextLine", new GUILayoutOption[] { })) that.NextPair();
            if (GUILayout.Button("Previous", new GUILayoutOption[] { })) that.PreviousPair();
            if (GUILayout.Button("Random", new GUILayoutOption[] { })) that.RandomPair();
            if (GUILayout.Button("Play", new GUILayoutOption[] { })) that.Play();
            if (GUILayout.Button("Pause", new GUILayoutOption[] { })) that.Pause();
            if (GUILayout.Button("Stop", new GUILayoutOption[] { })) that.Stop();
        }

        EditorGUILayout.PropertyField(_Mixer);

        EditorGUILayout.PropertyField(_m_LineRenderer);
        EditorGUILayout.PropertyField(LineRendererPrefab);

        EditorGUILayout.PropertyField(LoopAtAll);
        EditorGUILayout.PropertyField(Sampling);

        if (Sampling.boolValue)
        {
            EditorGUILayout.PropertyField(DrawingLine);
            EditorGUILayout.PropertyField(SpectrumCount);
            EditorGUILayout.PropertyField(BandCount);
            EditorGUILayout.PropertyField(decreasingType);
            EditorGUILayout.PropertyField(decreasing);
            EditorGUILayout.PropertyField(DecreaseAcceleration);
            EditorGUILayout.PropertyField(increasingType);
            EditorGUILayout.PropertyField(increasing);

            EditorGUILayout.Space(20);

            EditorGUILayout.PropertyField(samples);
            EditorGUILayout.PropertyField(bands);
            EditorGUILayout.PropertyField(normalizedBands);
        }


        serializedObject.ApplyModifiedProperties();
    }
}
