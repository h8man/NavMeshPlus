using UnityEngine.AI;
using UnityEngine;
using UnityEditor;

namespace NavMeshComponents.Extensions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshCollectRootSources2d))]
    internal class NavMeshCollectRootSources2dEditor: Editor
    {
        SerializedProperty _rootSources;
        void OnEnable()
        {
            _rootSources = serializedObject.FindProperty("_rootSources");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
    
            var surf = target as NavMeshCollectRootSources2d;

            if (surf.NavMeshSurfaceOwner.collectObjects != CollectObjects.Children)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Root Sources are only suitable for 'CollectObjects - Children'", MessageType.Info);
                EditorGUILayout.Space();

            }
            EditorGUILayout.PropertyField(_rootSources);

            serializedObject.ApplyModifiedProperties();
        }
    }

}
