// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Special <see cref="DecoratorDrawer"/> for fields with the attribute <see cref="TitleAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(TitleAttribute), false)]
    public class TitleDecoratorDrawer : DecoratorDrawer {
        #region Decorator Content
        public override float GetHeight() {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect _position) {
            TitleAttribute _attribute = attribute as TitleAttribute;
            _position.height -= EditorGUIUtility.standardVerticalSpacing;

            EnhancedEditorGUI.UnderlinedLabel(_position, _attribute.Label, EditorStyles.boldLabel);
        }
        #endregion
    }
}
