using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(AssetPreviewAttribute))]
    public class AssetPreviewPropertyDrawer : PropertyDrawer
    {
        #region Methods
        /********************************
         *****   ORIGINAL METHODS   *****
         *******************************/

        /// <summary>
        /// Get the asset preview texture from the property object reference.
        /// </summary>
        /// <param name="_property">Property to get asset preview texture from.</param>
        /// <returns>Returns property object reference preview texture if exist, null if none or wrong property type.</returns>
        private Texture2D GetAssetPreview(SerializedProperty _property) => _property.objectReferenceValue ? AssetPreview.GetAssetPreview(_property.objectReferenceValue) : null;

        /// <summary>
        /// Get if the property type is object reference or not.
        /// </summary>
        /// <param name="_property">Property to check type validity.</param>
        /// <returns>Returns true if property type is object reference, false otherwise.</returns>
        private bool IsPropertyValid(SerializedProperty _property) => _property.propertyType == SerializedPropertyType.ObjectReference;


        /*****************************
         *****   UNITY METHODS   *****
         ****************************/

        // Specify how tall the GUI for this decorator is in pixels
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            AssetPreviewAttribute _attribute = (AssetPreviewAttribute)attribute;
            float _height = EditorGUIUtility.singleLineHeight;
            if (!IsPropertyValid(_property)) _height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtilityEnhanced.DefaultHelpBoxHeight;
            else if (_attribute.IsVisible && GetAssetPreview(_property)) _height += EditorGUIUtility.standardVerticalSpacing + _attribute.Height;

            return _height;
        }

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            AssetPreviewAttribute _attribute = (AssetPreviewAttribute)attribute;
            Texture2D _texture = GetAssetPreview(_property);

            // Display property field
            Rect _rect = new Rect()
            {
                x = _position.x,
                y = _position.y,
                width = _position.width - (_texture ? 25 : 0),
                height = EditorGUIUtility.singleLineHeight
            };
            EditorGUI.PropertyField(_rect, _property, _label);

            // If no texture, draw nothing
            if (!_texture)
            {
                // If property is not valid, display informative box
                if (!IsPropertyValid(_property))
                {
                    _position.y += _rect.height + EditorGUIUtility.standardVerticalSpacing;
                    _position.height = EditorGUIUtilityEnhanced.DefaultHelpBoxHeight;

                    EditorGUI.HelpBox(_position, "Asset Preview attribute can only be used with object reference type fields !", MessageType.Error);
                }
                return;
            }

            // Display foldout button next to property field
            _rect.x += _position.width - 5;
            _rect.width = _position.width - _rect.width;

            _attribute.IsVisible = EditorGUI.Foldout(_rect, _attribute.IsVisible, GUIContent.none);

            // If visible & assigned, display asset preview
            if (_attribute.IsVisible)
            {
                _position.x += _position.width - _attribute.Width - 25;
                _position.y += _rect.height + EditorGUIUtility.standardVerticalSpacing;
                _position.width = _attribute.Width;
                _position.height = _attribute.Height;

                EditorGUI.DrawPreviewTexture(_position, _texture);
            }
        }
        #endregion
    }
}
