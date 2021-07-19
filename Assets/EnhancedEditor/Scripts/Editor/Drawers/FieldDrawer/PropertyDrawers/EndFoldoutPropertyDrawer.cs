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
    /// Special drawer (inheriting from <see cref="EnhancedPropertyDrawer"/>) for classes with attribute <see cref="EndFoldoutAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(EndFoldoutAttribute))]
	public class EndFoldoutPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private const float FoldoutSpeed = .75f;
        private const float FoldoutSpeedAcceleration = .007f;

        private static EndFoldoutPropertyDrawer[] endFoldouts = new EndFoldoutPropertyDrawer[] { };

        private BeginFoldoutPropertyDrawer foldout = null;
        private string propertyName = string.Empty;

        private float height = 0f;
        private float lerpHeight = 0f;

        private float lerpSpeed = FoldoutSpeed;
        
        // -----------------------

        public override void OnEnable(SerializedProperty _property)
        {
            UnityEditor.ArrayUtility.Add(ref endFoldouts, this);
            propertyName = _property.name;

            // Try to recover foldout, for some properties can be recreated while
            // already existing (like ObjectReference field properties).
            for (int _i = endFoldouts.Length - 1; _i-- > 0;)
            {
                EndFoldoutPropertyDrawer _foldout = endFoldouts[_i];
                if (_foldout == null)
                {
                    UnityEditor.ArrayUtility.RemoveAt(ref endFoldouts, _i);
                    continue;
                }

                if (_foldout.propertyName == _property.name)
                {
                    UnityEditor.ArrayUtility.RemoveAt(ref endFoldouts, _i);
                    foldout = _foldout.foldout;
                    return;
                }
            }

            foldout = BeginFoldoutPropertyDrawer.GetFoldout();
        }

        public override void OnAfterGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            Event _event = Event.current;
            bool _foldout = foldout.PopFoldout(_position.y, lerpHeight, out float _positionHeight);
            if (!_foldout)
            {
                if (_event.type == EventType.Repaint)
                    height = _positionHeight - _position.y;
            }
            else
                height = -1f;

            // Lerp height for a cool and smooth foldout
            // opening / closing effect (repaint is used to get smooth transitions).
            if (lerpHeight != height)
            {
                lerpHeight = Mathf.MoveTowards(lerpHeight, height, lerpSpeed);
                lerpSpeed += FoldoutSpeedAcceleration;

                EnhancedEditorGUIUtility.Repaint(_property.serializedObject);
            }
            else
                lerpSpeed = FoldoutSpeed;

            if (_event.type == EventType.Repaint)
            {
                Color _color = BeginFoldoutPropertyDrawer.PopColor();

                _position = EditorGUI.IndentedRect(_position);
                EditorGUI.DrawRect(new Rect(_position.x + EnhancedEditorGUIUtility.BoxLeftOffset,
                                            _position.y,
                                            _position.width + EnhancedEditorGUIUtility.BoxRightOffset,
                                            lerpHeight + (_foldout ? 1f : 2f)),
                                   _color);

                if (_color == EnhancedEditorGUIUtility.GetGUIBackgroundColor())
                {
                    EditorGUI.DrawRect(new Rect(_position.x + EnhancedEditorGUIUtility.BoxLeftOffset,
                                            _position.y + Screen.height,
                                            _position.width + EnhancedEditorGUIUtility.BoxRightOffset,
                                            lerpHeight + (_foldout ? 1f : 2f) - Screen.height),
                                   _color);
                }
            }

            _height = lerpHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        #endregion

        #region Utility
        internal static bool RecoverFoldout(BeginFoldoutPropertyDrawer _beginFoldout, string _propertyName)
        {
            for (int _i = endFoldouts.Length; _i-- > 0;)
            {
                EndFoldoutPropertyDrawer _endFoldout = endFoldouts[_i];
                if (_endFoldout == null)
                {
                    UnityEditor.ArrayUtility.RemoveAt(ref endFoldouts, _i);
                    continue;
                }

                if (_endFoldout.foldout.PropertyName == _propertyName)
                {
                    _endFoldout.foldout = _beginFoldout;
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
