// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer for fields with the attribute <see cref="EndFoldoutAttribute"/> (inherit from <see cref="EnhancedPropertyDrawer"/>).
    /// </summary>
    [CustomDrawer(typeof(EndFoldoutAttribute))]
	public class EndFoldoutPropertyDrawer : EnhancedPropertyDrawer
    {
        #region Drawer Content
        private static readonly List<EndFoldoutPropertyDrawer> endFoldouts = new List<EndFoldoutPropertyDrawer>();
        private static readonly Dictionary<string, int> reconnectionHelper = new Dictionary<string, int>();

        private BeginFoldoutPropertyDrawer begin = null;
        private string propertyPath = string.Empty;
        private float height = 0f;

        private bool isFirstDraw = true;

        public override bool RequireConstantRepaint => true;

        // -----------------------

        public override void OnEnable()
        {
            propertyPath = SerializedProperty.propertyPath;
            Connect(false);
        }

        public override void OnAfterGUI(Rect _position, SerializedProperty _property, GUIContent _label, out float _height)
        {
            // Ignore this foldout if it has no beginning.
            if ((begin == null) && !Connect(true))
            {
                _height = 0f;
                return;
            }

            // Get the full foldout group state and position.
            bool _foldout = begin.PopFoldout(_position.y, height, out float _beginPos, out float _fade, out bool _hasColor);

            // Again, only update the foldout position on Repaint event.
            if (Event.current.type == EventType.Repaint)
            {
                height = (_beginPos - _position.y) * (1f - _fade);

                // Calculates the new drawing position.
                _position = EditorGUI.IndentedRect(_position);

                _position.x -= EnhancedEditorGUIUtility.FoldoutWidth;
                _position.width += EnhancedEditorGUIUtility.FoldoutWidth + 2f;
                _position.height = height + (_foldout ? 0f : 2f);

                // Fill any blank in this color group during transitions.
                Color _color = BeginFoldoutPropertyDrawer.PopColor();
                if ((height != 0f) && (_fade > 0f))
                {
                    EditorGUI.DrawRect(_position, _color);
                }

                // Repaint over leaking colors.
                if (_color == EnhancedEditorGUIUtility.GUIThemeBackgroundColor)
                {
                    float _screenHeight = EnhancedEditorGUIUtility.ScreenHeight;

                    _position.y += _screenHeight;
                    _position.height -= _screenHeight - (_foldout ? 2f : 0f);

                    EditorGUI.DrawRect(_position, _color);
                }
            }
            else if (isFirstDraw)
            {
                height = (_beginPos - _position.y) * (1f - _fade);
                isFirstDraw = false;
            }

            // Add some spacing for color groups.
            _height = height;
            if (_hasColor)
            {
                _height += EditorGUIUtility.standardVerticalSpacing + 1f;
            }
        }
        #endregion

        #region Utility
        private bool Connect(bool _isRegistered)
        {
            // Reconnection helper.
            bool _needToReconnect = reconnectionHelper.ContainsKey(propertyPath);

            int _replaceIndex = -1;
            int _pathCount = 0;
            int _count = _needToReconnect
                       ? reconnectionHelper[propertyPath]
                       : 1;

            // Try to reconnect this foldout, as some properties can be recreated while
            // already existing (like the ObjectReference type properties).
            for (int _i = 0; _i < endFoldouts.Count; _i++)
            {
                EndFoldoutPropertyDrawer _foldout = endFoldouts[_i];

                // Remove null entries.
                if (_foldout == null)
                {
                    endFoldouts.RemoveAt(_i);
                    _i--;

                    continue;
                }

                // If an existing foldout is found with the same path and whose target access throws an error, replace it.
                if (_foldout.propertyPath == propertyPath)
                {
                    try
                    {
                        bool _errorTest = _foldout.SerializedProperty.serializedObject.targetObject == null;
                    }
                    catch (ArgumentNullException)
                    {
                        begin = _foldout.begin;
                        endFoldouts[_i] = this;

                        _foldout.begin = null;
                        return true;
                    }

                    // Reconnect helper update.
                    _pathCount++;
                    _count--;

                    if (_count == 0)
                    {
                        _replaceIndex = _i;
                    }
                }
            }

            // Only register this foldout if it has a beginning.
            if (!_isRegistered && BeginFoldoutPropertyDrawer.GetFoldout(out begin))
            {
                endFoldouts.Add(this);
            }
            else if (_replaceIndex > -1)
            {
                // Reconnect this foldout.
                EndFoldoutPropertyDrawer _foldout = endFoldouts[_replaceIndex];

                begin = _foldout.begin;
                endFoldouts[_replaceIndex] = this;

                _foldout.begin = null;

                // Reconnection helper update.
                if (_needToReconnect)
                {
                    if (_pathCount == reconnectionHelper[propertyPath])
                    {
                        // All foldouts of this property have been reconnected.
                        reconnectionHelper.Remove(propertyPath);
                    }
                    else
                    {
                        // Reconnect counter update.
                        reconnectionHelper[propertyPath] = reconnectionHelper[propertyPath] + 1;
                    }
                }
                else if (_pathCount > 1)
                {
                    // All foldouts of this property must now be reconnected.
                    reconnectionHelper.Add(propertyPath, 2);
                }
            }
            else
                return false;

            return true;
        }

        internal static bool ReconnectFoldout(BeginFoldoutPropertyDrawer _beginFoldout, string _id)
        {
            for (int _i = endFoldouts.Count; _i-- > 0;)
            {
                EndFoldoutPropertyDrawer _endFoldout = endFoldouts[_i];
                if (_endFoldout == null)
                {
                    endFoldouts.RemoveAt(_i);
                    continue;
                }

                if (_endFoldout.begin.id == _id)
                {
                    _endFoldout.begin = _beginFoldout;
                    return true;
                }
            }

            return false;
        }

        internal static bool IsConnected(BeginFoldoutPropertyDrawer _beginFoldout)
        {
            for (int _i = endFoldouts.Count; _i-- > 0;)
            {
                EndFoldoutPropertyDrawer _endFoldout = endFoldouts[_i];
                if (_endFoldout == null)
                {
                    endFoldouts.RemoveAt(_i);
                    continue;
                }

                if (_endFoldout.begin == _beginFoldout)
                {
                    return true;
                }
                else if (_endFoldout.begin.id == _beginFoldout.id)
                {
                    // Sometimes, a previously used drawer that was cached is reused, so let's reconnect it.
                    _endFoldout.begin = _beginFoldout;
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
