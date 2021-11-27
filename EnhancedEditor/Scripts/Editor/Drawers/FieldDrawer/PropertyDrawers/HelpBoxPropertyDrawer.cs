// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="HelpBoxAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        public override bool OnBeforeGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            _height = DrawHelpBox(_position, true);
            return false;
        }

        public override void OnAfterGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            _height = DrawHelpBox(_position, false);
        }

        // -----------------------

        private float DrawHelpBox(Rect _position, bool _isAbove)
        {
            HelpBoxAttribute _attribute = Attribute as HelpBoxAttribute;
            if (_isAbove != _attribute.IsAbove)
                return 0f;

            string _message = _attribute.Label.text;
            UnityEditor.MessageType _messageType = (UnityEditor.MessageType)_attribute.MessageType;
            _position = EditorGUI.IndentedRect(_position);

            float _height = EnhancedEditorGUIUtility.GetHelpBoxHeight(_message, _messageType, _position.width);
            _position.height = _height
                             = EnhancedEditorGUI.ManageDynamicControlHeight(_attribute.Label, _height);
            
            EditorGUI.HelpBox(_position, _message, _messageType);
            return _height;
        }
        #endregion
    }
}
