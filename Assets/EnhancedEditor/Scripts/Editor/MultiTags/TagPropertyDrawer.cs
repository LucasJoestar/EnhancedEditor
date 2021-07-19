// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(Tag), true)]
	public class TagPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            return EnhancedEditorGUIUtility.TagHeight;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EnhancedEditorGUI.TagField(_position, _property, _label);

            // Repaint to accurately display color changes.
            EnhancedEditorGUIUtility.Repaint(_property.serializedObject);
        }
    }
}
