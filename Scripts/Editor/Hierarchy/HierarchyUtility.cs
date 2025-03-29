// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Contains multiple Hierarchy utility tools.
    /// </summary>
    internal static class HierarchyUtility {
        #region Utility
        /// <summary>
        /// Opens an utility window to rename all selected objects.
        /// </summary>
        [MenuItem(InternalUtility.MenuItemPath + "Rename Multiple #TAB", false, 201)]
        public static void RenameMultiple() {

            Object[] _objects = Selection.objects;
            if (_objects.Length == 0)
                return;

            Object _first = _objects[0];
            bool _isAsset = AssetDatabase.Contains(_first);

            Action<string, string, string, string, int> _delegate = _isAsset ? RenameAsset : Rename;
            RenameObjectWindow.GetWindow(_first.name, _delegate);

            if (_isAsset) {
                AssetDatabase.Refresh();
            }

            // ----- Local Methods ----- \\

            void Rename(string _name, string _toReplace, string _replaceBy, string _numberFormat, int _decimalCount) {
                Undo.RecordObjects(_objects, "Rename object(s)");

                // Multiple rename.
                for (int i = 0; i < _objects.Length; i++) {
                    _objects[i].name = GetName(i, _name, _toReplace, _replaceBy, _numberFormat, _decimalCount);
                }
            }

            void RenameAsset(string _name, string _toReplace, string _replaceBy, string _numberFormat, int _decimalCount) {
                Undo.RecordObjects(_objects, "Rename object(s)");

                // Multiple rename.
                for (int i = 0; i < _objects.Length; i++) {
                    Object _object = _objects[i];
                    string _path = AssetDatabase.GetAssetPath(_object);

                    if (string.IsNullOrEmpty(_path))
                        continue;

                    string _message = AssetDatabase.RenameAsset(_path, GetName(i, _name, _toReplace, _replaceBy, _numberFormat, _decimalCount));
                    if (!string.IsNullOrEmpty(_message)) {
                        Debug.LogError(_message);
                    }
                }
            }

            string GetName(int _index, string _name, string _toReplace, string _replaceBy, string _numberFormat, int _decimalCount) {
                if (_objects.Length == 1) {
                    return _name;
                }

                Object _object = _objects[_index];
                if (!string.IsNullOrEmpty(_toReplace) && !string.IsNullOrEmpty(_replaceBy)) {
                    _name = _object.name.Replace(_toReplace, _replaceBy);
                }

                return _object.name = $"{_name}{string.Format(_numberFormat, (_index + 1).ToStringX(_decimalCount))}";
            }
        }

        /// <summary>
        /// Removes missing script(s) from all Game Objects in the currently loaded scene(s).
        /// </summary>
        [MenuItem(InternalUtility.MenuItemPath + "Remove Missing Script(s)", false, 202)]
        public static void RemoveMissingScriptsInLoadedScenes() {

            List<GameObject> _objects = new List<GameObject>();
            int _count = 0;

            for (int i = 0; i < SceneManager.sceneCount; i++) {
                SceneManager.GetSceneAt(i).GetRootGameObjects(_objects);

                for (int j = 0; j < _objects.Count; j++) {
                    _count += RemoveScripts(_objects[j]);
                }
            }

            if (_count == 0) {
                Debug.Log("No missing script was found in the loaded scene(s)");
            }

            // ----- Local Methods ----- \\

            int RemoveScripts(GameObject _object) {

                // Root.
                int _count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(_object);
                int _removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(_object);

                if (_count != 0) {
                    Debug.Log($"Found {_count} missing script(s) on Game Object {_object.name.Bold()} - Removed {_removed}", _object);
                }

                // Children.
                Transform _transform = _object.transform;

                for (int i = 0; i < _transform.childCount; i++) {
                    _count += RemoveScripts(_transform.GetChild(i).gameObject);
                }

                return _count;
            }
        }
        #endregion

        #region Rename Window
        /// <summary>
        /// Utility window used to rename an object.
        /// </summary>
        public sealed class RenameObjectWindow : EditorWindow {
            /// <summary>
            /// Creates and shows a new <see cref="RenameObjectWindow"/> instance,
            /// used to rename an existing tag in the project.
            /// </summary>
            /// <returns><see cref="RenameObjectWindow"/> instance on screen.</returns>
            public static RenameObjectWindow GetWindow(string _name, Action<string, string, string, string, int> _callback) {
                RenameObjectWindow _window = GetWindow<RenameObjectWindow>(true, "Rename Object(s)", true);

                _window.minSize = _window.maxSize
                                = new Vector2(325f, 95f);

                _window.objectName = _name;
                _window.callback = _callback;

                _window.ShowUtility();
                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            private const char SpecialChar   = '#';

            private const float PrefixWidth  = 40f;
            private const float NameWidth    = 150f;
            private const float FormatWidth  = 100f;
            private const float ReplaceWidth = 125f;

            private const string UndoRecordTitle    = "Name change(s)";
            private const string EmptyTagMessage    = "Name cannot be null or empty";
            private const string TooltipMessage     = "Customize the count format where \"#\" is the object identifier";

            private static readonly GUIContent nameGUI    = new GUIContent("Name:");
            private static readonly GUIContent replaceGUI = new GUIContent("Replace:");
            private static readonly GUIContent renameGUI  = new GUIContent("OK", "Rename selected object(s).");

            private string objectName       = "NameMe";
            private string toReplace        = string.Empty;
            private string replaceBy        = string.Empty;
            private string separatorFormat  = $" ({SpecialChar})";

            private Action<string, string, string, string, int> callback = null;

            // -----------------------

            private void OnGUI() {
                Undo.RecordObject(this, UndoRecordTitle);

                // Button and content.
                Rect _position = new Rect(5f, 5f, PrefixWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(_position, nameGUI);

                // Name.
                _position.x    += 60f;
                _position.width = NameWidth;
                objectName      = EditorGUI.TextField(_position, objectName);

                // Format.
                _position.xMin += _position.width + 5f;
                _position.width = FormatWidth;
                separatorFormat = EditorGUI.TextField(_position, separatorFormat);

                // Replace.
                _position = new Rect() {
                    x = 5f,
                    y = _position.y + _position.height + EditorGUIUtility.standardVerticalSpacing,
                    width = position.width - 10f,
                    height = EditorGUIUtility.singleLineHeight
                };

                EditorGUI.LabelField(_position, replaceGUI);

                _position.x += 60f;
                _position.width = ReplaceWidth;
                toReplace = EditorGUI.TextField(_position, toReplace);

                // Format.
                _position.x += _position.width + 5f;
                replaceBy = EditorGUI.TextField(_position, replaceBy);

                // Empty management.
                if (string.IsNullOrEmpty(objectName.Trim()) && (string.IsNullOrEmpty(toReplace.Trim()) || string.IsNullOrEmpty(replaceBy.Trim()))) {

                    _position = new Rect() {
                        x = 5f,
                        y = _position.y + _position.height + 5f,
                        width = position.width - 10f
                    };

                    _position.height = EnhancedEditorGUIUtility.GetHelpBoxHeight(EmptyTagMessage, UnityEditor.MessageType.Error, _position.width);
                    EditorGUI.HelpBox(_position, EmptyTagMessage, UnityEditor.MessageType.Error);

                    return;
                }

                // Tooltip.
                Rect _temp = new Rect() {
                    x = 5f,
                    y = _position.y + _position.height + 5f,
                    width = position.width - 65f
                };

                _temp.height = EnhancedEditorGUIUtility.GetHelpBoxHeight(TooltipMessage, UnityEditor.MessageType.Info, _temp.width);
                EditorGUI.HelpBox(_temp, TooltipMessage, UnityEditor.MessageType.Info);

                // Validation button.
                _position = new Rect() {
                    x = position.width - 55f,
                    y = _position.y + _position.height + 10f,
                    width = 50f,
                    height = 25f
                };

                if (GUI.Button(_position, renameGUI)) {
                    int _decimalCount = 1;
                    int _index = separatorFormat.IndexOf(SpecialChar);

                    for (int i = _index + 1; i < separatorFormat.Length; i++) {
                        if (separatorFormat[i] != SpecialChar)
                            break;

                        _decimalCount++;
                    }

                    if (_decimalCount != 1) {
                        separatorFormat = separatorFormat.Remove(_index, _decimalCount - 1);
                    }

                    callback?.Invoke(objectName, toReplace, replaceBy, separatorFormat.Replace(SpecialChar.ToString(), "{0}"), _decimalCount);
                    Close();
                }
            }
        }
        #endregion
    }
}
