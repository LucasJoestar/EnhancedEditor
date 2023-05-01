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
    internal static class HierarchyUtility  {
        #region Utility
        /// <summary>
        /// Opens an utility window to rename all selected objects.
        /// </summary>
        [MenuItem(InternalUtility.MenuItemPath + "Rename Multiple #TAB", false, 201)]
        public static void RenameMultiple() {

            Object[] _objects = Selection.objects;
            if (_objects.Length == 0) {
                return;
            }

            RenameObjectWindow.GetWindow(_objects[0].name, Rename);

            // ----- Local Method ----- \\

            void Rename(string _name, string _numberFormat) {

                Undo.RecordObjects(_objects, "Rename object(s)");

                // Single rename.
                if (_objects.Length == 1) {
                    _objects[0].name = _name;
                    return;
                }

                // Multiple rename.
                for (int i = 0; i < _objects.Length; i++) {
                    _objects[i].name = $"{_name}{string.Format(_numberFormat, i + 1)}";
                }
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
        public class RenameObjectWindow : EditorWindow {
            /// <summary>
            /// Creates and shows a new <see cref="RenameObjectWindow"/> instance,
            /// used to rename an existing tag in the project.
            /// </summary>
            /// <returns><see cref="RenameObjectWindow"/> instance on screen.</returns>
            public static RenameObjectWindow GetWindow(string _name, Action<string, string> _callback) {
                RenameObjectWindow _window = GetWindow<RenameObjectWindow>(true, "Rename Object(s)", true);

                _window.minSize = _window.maxSize
                                = new Vector2(325f, 70f);

                _window.objectName = _name;
                _window.callback = _callback;

                _window.ShowUtility();
                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            private const float PrefixWidth = 40f;
            private const float NameWidth   = 150f;
            private const float FormatWidth = 130f;

            private const string UndoRecordTitle    = "Name change(s)";
            private const string EmptyTagMessage    = "Name cannot be null or empty";
            private const string TooltipMessage     = "Customize the count format where \"#\" is the object identifier";

            private static readonly GUIContent nameGUI = new GUIContent("Name:");
            private static readonly GUIContent renameGUI = new GUIContent("OK", "Rename selected object(s).");

            private string objectName = "NameMe";
            private string separatorFormat = " (#)";

            private Action<string, string> callback = null;

            // -----------------------

            private void OnGUI() {
                Undo.RecordObject(this, UndoRecordTitle);

                // Button and content.
                Rect _position = new Rect(5f, 5f, PrefixWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(_position, nameGUI);

                // Name.
                _position.x += 50f;
                _position.width = NameWidth;
                objectName = EditorGUI.TextField(_position, objectName);

                // Color.
                _position.xMin += _position.width + 10f;
                _position.width = FormatWidth;
                separatorFormat = EditorGUI.TextField(_position, separatorFormat);

                // Empty management.
                string _value = objectName.Trim();
                if (string.IsNullOrEmpty(_value)) {

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
                    callback?.Invoke(objectName, separatorFormat.Replace("#", "{0}"));
                    Close();
                }
            }
        }
        #endregion
    }
}
