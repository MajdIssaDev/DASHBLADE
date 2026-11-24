using Dashblade.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dashblade.EditorTools
{
    /// <summary>
    /// Draws fields marked <see cref="ReadOnlyInInspectorAttribute"/> greyed out. Booleans get a bound UI Toolkit
    /// toggle (it follows the value live in Play Mode); other types fall back to a disabled IMGUI field.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyInInspectorAttribute))]
    public sealed class ReadOnlyInInspectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                return null; // Use OnGUI.
            }

            var toggle = new Toggle(property.displayName) { tooltip = property.tooltip };
            toggle.AddToClassList(BaseField<bool>.alignedFieldUssClassName);
            toggle.BindProperty(property);
            toggle.SetEnabled(false);
            return toggle;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
}
