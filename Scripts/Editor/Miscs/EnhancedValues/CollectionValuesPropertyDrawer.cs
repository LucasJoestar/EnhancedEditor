// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="CollectionValues{T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(CollectionValues<>), true)]
    public class CollectionValuesPropertyDrawer : EnhancedPropertyEditor {
        #region Drawer Content
        private const string NotFoundMemberMessage = "The member \'{0}\' could not be found in the script \'{1}\'";

        // -----------------------

        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            MemberValue<IList> _collection = _property.FindPropertyRelative("collectionMember").stringValue;

            // Not found member management.
            if (!_collection.GetValue(_property, out IList _list)) {
                string _message = string.Format(NotFoundMemberMessage, _collection.Name, _property.serializedObject.targetObject.GetType().Name);
                _position.height = EnhancedEditorGUIUtility.GetHelpBoxHeight(_message, UnityEditor.MessageType.Error, _position.width);

                EditorGUI.HelpBox(_position, _message, UnityEditor.MessageType.Error);
                return _position.height;
            }

            SerializedProperty _valuesProperty = _property.FindPropertyRelative("Values");
            SerializedObject _serializedObject = _property.serializedObject;

            while (_valuesProperty.arraySize < _list.Count) {
                _valuesProperty.InsertArrayElementAtIndex(_valuesProperty.arraySize);

                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Update();
            }

            while (_valuesProperty.arraySize > _list.Count) {
                _valuesProperty.DeleteArrayElementAtIndex(_valuesProperty.arraySize - 1);

                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Update();
            }

            float _height = 0f;

            // Button.
            _property.isExpanded = EditorGUI.Foldout(_position, _property.isExpanded, _label, true, EditorStyles.foldoutHeader);
            IncrementPosition();

            if (_property.isExpanded) {
                // Draw each collection value.
                using (var _scope = new EditorGUI.IndentLevelScope(1)) {
                    for (int i = 0; i < _valuesProperty.arraySize; i++) {
                        SerializedProperty _valueProperty = _valuesProperty.GetArrayElementAtIndex(i);

                        _position.height = EditorGUI.GetPropertyHeight(_valueProperty, true);
                        EditorGUI.PropertyField(_position, _valueProperty, EnhancedEditorGUIUtility.GetLabelGUI(_list[i].ToString()), true);

                        // Position increment.
                        IncrementPosition();
                    }
                }
            }

            // ----- Local Method ----- \\

            void IncrementPosition() {
                float _spacing = _position.height + EditorGUIUtility.standardVerticalSpacing;

                _height += _spacing;
                _position.y += _spacing;
            }

            return _height;
        }
        #endregion
    }
}
