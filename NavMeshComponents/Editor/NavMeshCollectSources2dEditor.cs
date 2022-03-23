using UnityEngine.AI;
using UnityEngine;
using UnityEditor;

namespace NavMeshComponents.Extensions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshCollectSources2d))]
    internal class NavMeshCollectSources2dEditor: Editor
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
    
            var surf = target as NavMeshCollectSources2d;

            if (surf?.NavMeshSurfaceOwner.collectObjects != CollectObjects.Children
                && GameObject.FindObjectOfType<Grid>() == null)
            {
                EditorGUILayout.HelpBox($"{CollectObjects.All} or {CollectObjects.Volume} is not intended to be used without root Grid object in scene. Use {CollectObjects.Children} instead.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_OverrideByGrid);
            EditorGUILayout.PropertyField(m_UseMeshPrefab);
            EditorGUILayout.PropertyField(m_CompressBounds);
            EditorGUILayout.PropertyField(m_OverrideVector);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Rotate Surface to XY"))
                {
                    foreach (NavMeshCollectSources2d item in targets)
                    {
                        item.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                    }
                }
                GUILayout.EndHorizontal();
                foreach (NavMeshCollectSources2d navSurface in targets)
                {
                    if (!Mathf.Approximately(navSurface.transform.eulerAngles.x, 270f))
                    {
                        EditorGUILayout.HelpBox("NavMeshSurface is not rotated respectively to (x-90;y0;z0). Apply rotation unless intended.", MessageType.Warning);
                    }
                }
            }
        }
    }

}
