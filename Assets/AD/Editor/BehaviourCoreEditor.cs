using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AD;
using AD.Experimental.Runtime;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BehaviourCore))]
public class BehaviourCoreEditor : AbstractCustomADEditor
{
    BehaviourCore that;

    SerializedProperty MatchBehaviour;
    SerializedProperty MatchBehaviourTree;
    SerializedProperty CurrentR;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as BehaviourCore;
        MatchBehaviour = serializedObject.FindProperty(nameof(MatchBehaviour));
        MatchBehaviourTree = serializedObject.FindProperty(nameof(MatchBehaviourTree));
        CurrentR = serializedObject.FindProperty(nameof(CurrentR));
    }

    public override void OnContentGUI()
    {
        this.OnNotChangeGUI(() => EditorGUILayout.PropertyField(MatchBehaviour));
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(MatchBehaviourTree);
    }

    public override void OnSettingsGUI()
    {
        if (that.CurrentR == null) that.CurrentR = that.MatchBehaviourTree.LastOrDefault() as BehaviourClip;
        EditorGUILayout.PropertyField(CurrentR);
        this.VerticalBlockWithBox(() =>
        {
            this.HorizontalBlock(() =>
            {
                if (GUILayout.Button("截取初始状态"))
                {
                    var current = that.CurrentR.source;
                    current.Position.x.StartValue = that.transform.localPosition.x;
                    current.Position.y.StartValue = that.transform.localPosition.y;
                    current.Position.z.StartValue = that.transform.localPosition.z;
                    current.Rotation.x.StartValue = that.transform.localEulerAngles.x;
                    current.Rotation.y.StartValue = that.transform.localEulerAngles.y;
                    current.Rotation.z.StartValue = that.transform.localEulerAngles.z;
                    current.Scale.x.StartValue = that.transform.localScale.x;
                    current.Scale.y.StartValue = that.transform.localScale.y;
                    current.Scale.z.StartValue = that.transform.localScale.z;
                }
                if (GUILayout.Button("截取结束状态"))
                {
                    var current = that.CurrentR.source;
                    current.Position.x.EndValue = that.transform.localPosition.x;
                    current.Position.y.EndValue = that.transform.localPosition.y;
                    current.Position.z.EndValue = that.transform.localPosition.z;
                    current.Rotation.x.EndValue = that.transform.localEulerAngles.x;
                    current.Rotation.y.EndValue = that.transform.localEulerAngles.y;
                    current.Rotation.z.EndValue = that.transform.localEulerAngles.z;
                    current.Scale.x.EndValue = that.transform.localScale.x;
                    current.Scale.y.EndValue = that.transform.localScale.y;
                    current.Scale.z.EndValue = that.transform.localScale.z;
                }
            });
            this.HorizontalBlock(() =>
            {
                if (GUILayout.Button("调整为初始状态"))
                {
                    that.CurrentR.DoUpdate(that, that.CurrentR.source.StartTime);
                }
                if (GUILayout.Button("调整为结束状态"))
                {
                    that.CurrentR.DoUpdate(that, that.CurrentR.source.EndTime);
                }
            });
        });
    }
}
