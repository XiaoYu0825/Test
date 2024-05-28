using System;
using System.Collections.Generic;
using TGame.Common;
using UnityEditor;
using UnityEngine;

namespace TGame.Editor.Inspector
{
    /// <summary>
    /// 作者: Teddy
    /// 时间: 2018/03/05
    /// 功能: 
    /// </summary>
    [CustomEditor(typeof(ProcedureModule))]
    public class ProcedureModuleInspector : BaseInspector
    {
        private SerializedProperty proceduresProperty;
        private SerializedProperty defaultProcedureProperty;

        private List<string> allProcedureTypes;

        protected override void OnInspectorEnable()
        {
            base.OnInspectorEnable();
            proceduresProperty = serializedObject.FindProperty("proceduresNames");
            defaultProcedureProperty = serializedObject.FindProperty("defaultProcedureName");

            UpdateProcedures();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();
            UpdateProcedures();
        }

        private void UpdateProcedures()
        {
            allProcedureTypes = Utility.Types.GetAllSubclasses(typeof(BaseProcedure), false, Utility.Types.GAME_CSHARP_ASSEMBLY).ConvertAll((Type t) => { return t.FullName; });

            //移除不存在的procedure
            for (int i = proceduresProperty.arraySize - 1; i >= 0; i--)
            {
                string procedureTypeName = proceduresProperty.GetArrayElementAtIndex(i).stringValue;
                if (!allProcedureTypes.Contains(procedureTypeName))
                {
                    proceduresProperty.DeleteArrayElementAtIndex(i);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                if (allProcedureTypes.Count > 0)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        for (int i = 0; i < allProcedureTypes.Count; i++)
                        {
                            GUI.changed = false;
                            int? index = FindProcedureTypeIndex(allProcedureTypes[i]);
                            bool selected = EditorGUILayout.ToggleLeft(allProcedureTypes[i], index.HasValue);
                            if (GUI.changed)
                            {
                                if (selected)
                                {
                                    AddProcedure(allProcedureTypes[i]);
                                }
                                else
                                {
                                    RemoveProcedure(index.Value);
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUI.EndDisabledGroup();

            if (proceduresProperty.arraySize == 0)
            {
                if (allProcedureTypes.Count == 0)
                {
                    EditorGUILayout.HelpBox("Can't find any procedure", UnityEditor.MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Please select a procedure at least", UnityEditor.MessageType.Info);
                }
            }
            else
            {
                if (Application.isPlaying)
                {
                    //播放中显示当前状态
                    EditorGUILayout.LabelField("Current Procedure", TGameFramework.Instance.GetModule<ProcedureModule>().CurrentProcedure?.GetType().FullName);
                }
                else
                {
                    //显示默认状态
                    List<string> selectedProcedures = new List<string>();
                    for (int i = 0; i < proceduresProperty.arraySize; i++)
                    {
                        selectedProcedures.Add(proceduresProperty.GetArrayElementAtIndex(i).stringValue);
                    }
                    selectedProcedures.Sort();
                    int defaultProcedureIndex = selectedProcedures.IndexOf(defaultProcedureProperty.stringValue);
                    defaultProcedureIndex = EditorGUILayout.Popup("Default Procedure", defaultProcedureIndex, selectedProcedures.ToArray());
                    if (defaultProcedureIndex >= 0)
                    {
                        defaultProcedureProperty.stringValue = selectedProcedures[defaultProcedureIndex];
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AddProcedure(string procedureType)
        {
            proceduresProperty.InsertArrayElementAtIndex(0);
            proceduresProperty.GetArrayElementAtIndex(0).stringValue = procedureType;
        }

        private void RemoveProcedure(int index)
        {
            string procedureType = proceduresProperty.GetArrayElementAtIndex(index).stringValue;
            if (procedureType == defaultProcedureProperty.stringValue)
            {
                Debug.LogWarning("Can't remove default procedure");
                return;
            }
            proceduresProperty.DeleteArrayElementAtIndex(index);
        }

        private int? FindProcedureTypeIndex(string procedureType)
        {
            for (int i = 0; i < proceduresProperty.arraySize; i++)
            {
                SerializedProperty p = proceduresProperty.GetArrayElementAtIndex(i);
                if (p.stringValue == procedureType)
                {
                    return i;
                }
            }
            return null;
        }
    }
}