using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AD
{
    [CreateAssetMenu(menuName = "AD/SingleFileCreator")]
    public class SingleFileCreator : ScriptableObject
    {
        [TextArea] public string LicenseCode = "ninemine";

        public string FileName;

        public MonoScript[] Scripts;

        public MonoScript[] DependencyScripts;

        [HideInInspector] public string OutputFilePath = "Assets/";
    }

    [CustomEditor(typeof(SingleFileCreator))]
    public class SingleFileCreatorInspector : Editor
    {
        private SerializedProperty mOutputFilepathProperty;

        private void OnEnable()
        {
            mOutputFilepathProperty = serializedObject.FindProperty("OutputFilePath");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            GUILayout.BeginHorizontal();

            GUILayout.Label(mOutputFilepathProperty.stringValue);

            if (GUILayout.Button("..."))
            {
                mOutputFilepathProperty.stringValue =
                    EditorUtility.OpenFolderPanel("Select Folder", mOutputFilepathProperty.stringValue, "");
            }


            GUILayout.EndHorizontal();

            if (GUILayout.Button("Open Output Folder"))
            {
                EditorUtility.RevealInFinder(mOutputFilepathProperty.stringValue);
            }

            if (GUILayout.Button("Create"))
            {
                var folderPath = mOutputFilepathProperty.stringValue;
                var singleFileCreator = target as SingleFileCreator;
                var codeFilePath = Path.Combine(folderPath, singleFileCreator.FileName);

                var namespaces = new HashSet<string>()
                {
                   $"namespace AD.SingleFile.Dependency.Internal.{Path.GetFileNameWithoutExtension(singleFileCreator.FileName)}"
                };

                var codeLines = new List<string>();

                foreach (var monoScript in singleFileCreator.Scripts)
                {
                    foreach (var codeLine in monoScript.text.Split('\n'))
                    {
                        var codeLineTrim = codeLine.Trim();

                        if (codeLineTrim.StartsWith("using "))
                        {
                            namespaces.Add(codeLineTrim);
                        }
                        else if (codeLineTrim.StartsWith("/***") || codeLineTrim.StartsWith("*") ||
                                 codeLineTrim.StartsWith("****"))
                        {
                            // continue
                        }
                        else
                        {
                            codeLines.Add(codeLine);
                        }
                    }
                }

                foreach (var monoScript in singleFileCreator.DependencyScripts)
                {
                    foreach (var codeLine in monoScript.text.Split('\n'))
                    {
                        var codeLineTrim = codeLine.Trim();

                        if (codeLineTrim.StartsWith("using "))
                        {
                            namespaces.Add(codeLineTrim);
                        }
                        else if (codeLineTrim.StartsWith("/***") || codeLineTrim.StartsWith("*") ||
                                 codeLineTrim.StartsWith("****"))
                        {
                            // continue
                        }
                        else if (codeLineTrim.StartsWith("namespace AD"))
                        {
                            codeLines.Add(
                                $"namespace AD.SingleFile.Dependency.Internal.{Path.GetFileNameWithoutExtension(singleFileCreator.FileName)}");
                        }
                        else if ((codeLineTrim.StartsWith("public class") ||
                                  codeLineTrim.StartsWith("public static class"))
                                 && !codeLineTrim.Contains("EasyEvent")
                                 && !codeLineTrim.Contains("TableIndex")
                                )
                        {
                            codeLines.Add(codeLine.Replace("public", "internal"));
                        }
                        else
                        {
                            codeLines.Add(codeLine);
                        }
                    }
                }

                if (File.Exists(codeFilePath))
                {
                    File.Delete(codeFilePath); 
                } 

                File.WriteAllLines(codeFilePath,
                    new[] { singleFileCreator.LicenseCode, "" }.Concat(namespaces).Concat(codeLines));

                AssetDatabase.Refresh();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
