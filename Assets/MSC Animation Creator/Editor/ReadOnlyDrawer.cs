using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Disable the GUI, making the field non-interactive
        GUI.enabled = false;

        // Draw the property field using the standard method
        EditorGUI.PropertyField(position, property, label, true);

        // Re-enable the GUI for subsequent fields
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Ensures correct height for properties that can be expanded (like lists or custom structs)
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
