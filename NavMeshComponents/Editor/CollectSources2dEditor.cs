using UnityEngine;
using UnityEditor;

namespace NavMeshPlus.Extensions.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CollectSources2d))]
    internal class CollectSources2dEditor: Editor
    {
        SerializedProperty m_OverrideByGrid;
        SerializedProperty m_UseMeshPrefab;
        SerializedProperty m_CompressBounds;
        SerializedProperty m_OverrideVector;
        void OnEnable()
        {
            m_OverrideByGrid = serializedObject.FindProperty("m_OverrideByGrid");
            m_UseMeshPrefab = serializedObject.FindProperty("m_UseMeshPrefab");
            m_CompressBounds = serializedObject.FindProperty("m_CompressBounds");
            m_OverrideVector = serializedObject.FindProperty("m_OverrideVector");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
    
            var surf = target as CollectSources2d;

            EditorGUILayout.PropertyField(m_OverrideByGrid);
            using (new EditorGUI.DisabledScope(!m_OverrideByGrid.boolValue))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_UseMeshPrefab);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(m_CompressBounds);
            EditorGUILayout.PropertyField(m_OverrideVector);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Rotate Surface to XY", "Rotates Surface along XY plane to face toward standard 2d camera.")))
                {
                    foreach (CollectSources2d item in targets)
                    {
                        item.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                    }
                }
                if (GUILayout.Button(new GUIContent("Tilt Surface", "If your agent get stuck on vertical movement it may help to solve the issue. This will tilt Surface to -89.98. It may impact baking and navigation.")))
                {
                    foreach (CollectSources2d item in targets)
                    {
                        item.transform.rotation = Quaternion.Euler(-89.98f, 0f, 0f);
                    }
                }
                GUILayout.EndHorizontal();
                foreach (CollectSources2d navSurface in targets)
                {
                    if (!Mathf.Approximately(navSurface.transform.eulerAngles.x, 270.0198f) && !Mathf.Approximately(navSurface.transform.eulerAngles.x, 270f))
                    {
                        EditorGUILayout.HelpBox("NavMeshSurface is not rotated respectively to (x-90;y0;z0). Apply rotation unless intended.", MessageType.Warning);
                    }
                }
            }
        }
    }

}
