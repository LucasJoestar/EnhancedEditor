﻿// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="AnimationCurve"/> drawer, with some additional context click utilities.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimationCurve), true)]
    public class AnimationCurvePropertyDrawer : EnhancedPropertyEditor
    {
        #region Drawer Content
        private static readonly GUIContent copyGUI = new GUIContent("Copy", "Copy this animation curve value.");
        private static readonly GUIContent pasteGUI = new GUIContent("Paste", "Overwrite this animation curve value with the one in buffer.");

        /// <summary>
        /// Current in-buffer curve value (used for copy / paste operations).
        /// </summary>
        public static AnimationCurve CurveBuffer = null;

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            float _height = _position.height
                          = EditorGUIUtility.singleLineHeight;

            // Add new copy / paste options on context menu.
            // Do not use the context click event to prevent from opening the curve editor.
            Rect _temp = EnhancedEditorGUI.InvisiblePrefixLabel(_position, _label);

            if ((_temp.Event(out Event _event) == EventType.MouseDown) && (_event.button == 1))
            {
                GenericMenu _menu = new GenericMenu();
                OnContextMenu(_menu, _property);

                _menu.ShowAsContext();
                _event.Use();
            }

            // Draw base property field.
            EditorGUI.PropertyField(_position, _property, _label);
            return _height;
        }

        internal static void OnContextMenu(GenericMenu _menu, SerializedProperty _property)
        {
            // Copy value.
            if (!_property.hasMultipleDifferentValues)
            {
                _menu.AddItem(copyGUI, false, () =>
                {
                    CurveBuffer = _property.animationCurveValue;
                });
            }
            else
                _menu.AddDisabledItem(copyGUI);

            // Paste value.
            if (CurveBuffer != null)
            {
                _menu.AddItem(pasteGUI, false, () =>
                {
                    if (CurveBuffer != null)
                    {
                        _property.animationCurveValue = CurveBuffer;
                        _property.serializedObject.ApplyModifiedProperties();
                    }
                });
            }
            else
                _menu.AddDisabledItem(pasteGUI);
        }
        #endregion
    }
}
