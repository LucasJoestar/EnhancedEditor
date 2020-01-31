using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    [CustomPropertyDrawer(typeof(SectionAttribute))]
    public class SectionDecoratorDrawer : DecoratorDrawer
    {
        #region Fields / Properties
        /****************************
         *******   CONSTANTS   *******
         ***************************/

        /// <summary>
        /// Space on each side of the label (in pixels).
        /// </summary>
        public const float          SpaceAroundLabel =      5f;


        /****************************
         *******   PROPERTY   *******
         ***************************/

        /// <summary>
        /// GUIStyle used to draw section label.
        /// </summary>
        public static GUIStyle      LabelStyle            { get { return EditorStyles.boldLabel; } }
        #endregion

        #region Methods
        /***************************
         *******   METHODS   *******
         ***************************/

        // Specify how tall the GUI for this decorator is in pixels
        public override float GetHeight()
        {
            SectionAttribute _attribute = (SectionAttribute)attribute;
            return LabelStyle.CalcSize(new GUIContent(_attribute.Label)).y + EditorGUIUtility.singleLineHeight + (_attribute.HeightSpace * 2);
        }

        // Make your own GUI for the decorator
        public override void OnGUI(Rect _position)
        {
            SectionAttribute _attribute = (SectionAttribute)attribute;

            // Get label size, plus line and section width
            Vector2 _labelSize = LabelStyle.CalcSize(new GUIContent(_attribute.Label));
            float _sectionWidth = Mathf.Min(_position.width, _labelSize.x + (SpaceAroundLabel * 2) + (_attribute.LineWidth * 2));
            float _lineWidth = ((_sectionWidth - _labelSize.x) / 2f) - SpaceAroundLabel;

            // Set position at the middle of the area and line height
            _position.y += (EditorGUIUtility.singleLineHeight / 2f) + _attribute.HeightSpace - 1;

            // Draw horizontal line if enough space
            if (_lineWidth > 0)
            {
                Rect _lineRect = new Rect()
                {
                    x = _position.x + ((_position.width - _sectionWidth) / 2f),
                    y = _position.y + _labelSize.y / 2f,
                    height = 2f,
                    width = _lineWidth
                };

                // Set label x position
                _position.x = _lineRect.x + _lineWidth + SpaceAroundLabel;

                // Draw lines around label
                EditorGUI.DrawRect(_lineRect, LabelStyle.normal.textColor);
                _lineRect.x += _lineWidth + _labelSize.x + (SpaceAroundLabel * 2);
                EditorGUI.DrawRect(_lineRect, LabelStyle.normal.textColor);
            }
            // Set maximum available space surrounding the label
            else if (_position.width > _labelSize.x)
            {
                _position.x += (_position.width - _labelSize.x) / 2f;
            }

            // Draw label
            EditorGUI.LabelField(_position, _attribute.Label, LabelStyle);
        }
        #endregion
    }
}
