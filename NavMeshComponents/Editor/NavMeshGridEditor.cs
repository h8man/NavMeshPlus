using UnityEditor;

namespace UnityEngine.AI {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshGrid))]
    public class NavMeshGridEditor : Editor {

        SerializedProperty m_registerOnEnable;
        SerializedProperty m_unregisterOnDisable;

        private void OnEnable() {
            m_registerOnEnable = serializedObject.FindProperty("m_registerOnEnable");
            m_unregisterOnDisable = serializedObject.FindProperty("m_unregisterOnDisable");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_registerOnEnable.displayName, GUILayout.Width(140));
            EditorGUILayout.PropertyField(m_registerOnEnable, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_unregisterOnDisable.displayName, GUILayout.Width(140));
            EditorGUILayout.PropertyField(m_unregisterOnDisable, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

    }
}