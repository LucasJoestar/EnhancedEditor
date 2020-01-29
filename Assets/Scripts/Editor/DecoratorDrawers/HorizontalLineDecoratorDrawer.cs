using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedAttributes.Editor
{
    [CustomPropertyDrawer(typeof(HorizontalLineAttribute))]
    public class HorizontalLineDecoratorDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            HorizontalLineAttribute _horizontalLine = (HorizontalLineAttribute)attribute;
            return EditorGUIUtility.singleLineHeight + _horizontalLine.Height;
        }

        public override void OnGUI(Rect _position)
        {
            HorizontalLineAttribute _horizontalLine = (HorizontalLineAttribute)attribute;
            _position.y += (EditorGUIUtility.singleLineHeight / 2f) - 1;
            _position.height = _horizontalLine.Height;
            EditorGUI.DrawRect(_position, Color.gray);

            
        }
    }
}
