using UnityEditor;
using UnityEngine;
using NavMeshPlus.Extensions;
using NavMeshPlus.Editors.Components;

//***********************************************************************************
// Contributed by author jl-randazzo github.com/jl-randazzo
//***********************************************************************************
namespace NavMeshPlus.Editors.Extensions
{
    [CustomPropertyDrawer(typeof(NavMeshAreaAttribute))]
    public class NavMeshAreaAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            NavMeshComponentsGUIUtility.AreaPopup(position, label.text, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 20;
    }
}
