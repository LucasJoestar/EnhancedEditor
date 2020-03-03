using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxDecoratorDrawer : DecoratorDrawer
    {
        #region Fields
        /*****************************
         *******   CONSTANTS   *******
         ****************************/

        /// <summary>
        /// Substractor to line width due to help box left icon size and margins.
        /// </summary>
        public const int    WidthSubtractor =   52;
        #endregion

        #region Methods
        /***************************
         *******   METHODS   *******
         **************************/

        // Specify how tall the GUI for this decorator is in pixels
        public override float GetHeight()
        {
            HelpBoxAttribute _attribute = (HelpBoxAttribute)attribute;
            return Mathf.Max(EditorGUIUtilityEnhanced.DefaultHelpBoxHeight, EditorStyles.helpBox.CalcHeight(new GUIContent(_attribute.Label), EditorGUIUtility.currentViewWidth - WidthSubtractor)) + EditorGUIUtility.standardVerticalSpacing;
        }

        // Make your own IMGUI based GUI for the property
        public override void OnGUI(Rect _position)
        {
            _position.height -= EditorGUIUtility.standardVerticalSpacing;

            HelpBoxAttribute _attribute = (HelpBoxAttribute)attribute;
            EditorGUI.HelpBox(_position, _attribute.Label, (MessageType)(int)_attribute.Type);
        }
        #endregion
    }
}
