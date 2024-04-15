using UnityEngine;
using UnityEditor;

namespace NavMeshPlus.Extensions.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CollectSourcesCache2d))]
    internal class CollectSourcesCache2dEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
    
            var surf = target as CollectSourcesCache2d;

            serializedObject.ApplyModifiedProperties();
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Sources:");
                if (Application.isPlaying)
                {
                    GUILayout.Label(surf.SourcesCount.ToString());
                    GUILayout.Label("Cached:");
                    GUILayout.Label(surf.CahcheCount.ToString());
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Actions:");
                if (GUILayout.Button("Update Mesh"))
                {
                    surf.UpdateNavMesh();
                }
                GUILayout.EndHorizontal();
            }
        }
    }

}
