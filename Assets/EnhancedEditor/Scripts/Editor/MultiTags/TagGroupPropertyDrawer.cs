// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(TagGroup), true)]
	public class TagGroupPropertyDrawer : PropertyDrawer
    {
        private float height = EnhancedEditorGUIUtility.TagHeight;

        // -----------------------

        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            return height;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // Get height only on repaint event as it doesn't has accurate values on layout one.
            float _height = EnhancedEditorGUI.TagGroupField(_position, _property, _label);

            if (Event.current.type == EventType.Repaint)
                height = _height;

            // Repaint to accurately display color changes.
            EnhancedEditorGUIUtility.Repaint(_property.serializedObject);
        }
    }
}
