using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(AssetPreviewAttribute))]
    public class AssetPreviewPropertyDrawer : PropertyDrawer
    {
        #region Methods
        /***************************
         *******   METHODS   *******
         **************************/

        // Specify how tall the GUI for this decorator is in pixels
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            AssetPreviewAttribute _attribute = (AssetPreviewAttribute)attribute;
            return EditorGUIUtilityEnhanced.GetAssetPreviewPropertyHeight(_property, _attribute.Fodlout, _attribute.Aspect);
        }

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            AssetPreviewAttribute _attribute = (AssetPreviewAttribute)attribute;
            _attribute.Fodlout = EditorGUIEnhanced.AssetPreviewField(_position, _property, _label, _attribute.Fodlout);
        }
        #endregion
    }
}
