using UnityEditor;
using NavMeshPlus.Components;
using UnityEngine;

namespace NavMeshPlus.Editors.Components
{
    [CustomPropertyDrawer(typeof(NavMeshAgentType))]
    public class NavMeshAgentTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty agentTypeID = property.FindPropertyRelative(nameof(NavMeshAgentType.agentTypeID));
            var agentTypeNames = NavMeshComponentsGUIUtility.GetAgentTypeOptionsSelection(agentTypeID, out int index);
            index = EditorGUI.Popup(position, label.text, index, agentTypeNames);
            NavMeshComponentsGUIUtility.HandleAgentTypeSelection(agentTypeID, index);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 20;
    }
}
