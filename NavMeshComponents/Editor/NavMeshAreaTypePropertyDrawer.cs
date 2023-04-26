using UnityEditor;
using NavMeshPlus.Extensions;

namespace NavMeshPlus.Editors.Extensions
{
    [CustomPropertyDrawer(typeof(NavMeshAreaType))]
    public class NavMeshAreaTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty areaIDProperty = property.FindPropertyRelative(nameof(NavMeshAreaType.areaID));
            var areaTypeNames = NavMeshComponentsGUIUtility.GetAgentTypeOptionsSelection(areaIDProperty, out int index);
            index = EditorGUI.Popup(position, label.text, index, areaTypeNames);
            NavMeshComponentsGUIUtility.HandleAgentTypeSelection(areaIDProperty, index);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 20;
    }
}
