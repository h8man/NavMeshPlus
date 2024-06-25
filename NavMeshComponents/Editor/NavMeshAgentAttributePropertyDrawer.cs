using NavMeshPlus.Components.Editors;
using UnityEditor;
using UnityEngine;


//***********************************************************************************
// Contributed by author jl-randazzo github.com/jl-randazzo
//***********************************************************************************
namespace NavMeshPlus.Extensions.Editors
{
    [CustomPropertyDrawer(typeof(NavMeshAgentAttribute))]
    public class NavMeshAgentAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            NavMeshComponentsGUIUtility.AgentTypePopup(position, label.text, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => NavMeshComponentsGUIUtility.IsAgentSelectionValid(property) ? 20 : 40;
    }
}
