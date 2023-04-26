using UnityEditor;
using NavMeshPlus.Extensions;
using UnityEngine;
using NavMeshPlus.Extensions;
using NavMeshPlus.Editors.Components;

namespace NavMeshPlus.Editors.Extensions
{
    [CustomPropertyDrawer(typeof(NavMeshAreaType))]
    public class NavMeshAreaTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var overrideAreaProperty = property.FindPropertyRelative(nameof(NavMeshAreaType.overrideArea));
            Rect overrideRect = position;
            overrideRect.height = 20;
            GUIContent overrideLabel = new GUIContent(label);
            overrideLabel.text = "Override Area?";
            EditorGUI.PropertyField(overrideRect, overrideAreaProperty, overrideLabel);

            Rect areaRect = overrideRect;
            areaRect.y += 20;
            var areaIDProperty = property.FindPropertyRelative(nameof(NavMeshAreaType.areaID));
            var areaNames = NavMeshComponentsGUIUtility.GetAreaTypeOptionsSelection(areaIDProperty, out int areaIndex);
            areaIndex = EditorGUI.Popup(areaRect, label.text, areaIndex, areaNames);
            NavMeshComponentsGUIUtility.HandleAreaTypeSelection(areaIDProperty, areaIndex);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 50;
    }
}
