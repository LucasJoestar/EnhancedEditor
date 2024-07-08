// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedEditor.Editor {
    /// <summary>
    /// Custom <see cref="TagHolder"/> editor.
    /// </summary>
    [CustomEditor(typeof(TagHolder), true)]
    public class TagHolderEditor : UnityObjectEditor {
        #region Editor GUI
        private const string UndoRecordTitle = "Tag Holder Change";
        private const string TooltipMessage  = "You can edit any tag name and color using the context menu.";
        private const string NoTagMessage    = "No tag could be found in this holder! Create a new one using the button below.";

        private TagHolder tagHolder = null;
        private Vector2 scroll      = new Vector2();

        // -----------------------

        protected override void OnEnable() {
            base.OnEnable();
            tagHolder = target as TagHolder;

            MultiTagsWindow.UpdateDatabase();

            // Repaint on undo / redo.
            Undo.undoRedoPerformed -= Repaint;
            Undo.undoRedoPerformed += Repaint;
        }

        public override void OnInspectorGUI() {
            Undo.RecordObject(this, UndoRecordTitle);

            // Holder tags.
            using (var _scrollScrop = new GUILayout.ScrollViewScope(scroll)) {
                scroll = _scrollScrop.scrollPosition;

                // Standard properties.
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"));
                serializedObject.ApplyModifiedProperties();

                GUILayout.Space(10f);

                if (tagHolder.Count == 0) {
                    // No tag.
                    EditorGUILayout.HelpBox(NoTagMessage, UnityEditor.MessageType.Warning, true);
                } else {
                    // Displayed tags.
                    Rect _position = EditorGUILayout.GetControlRect();
                    Rect _temp = new Rect(_position);

                    for (int i = 0; i < tagHolder.Count; i++) {
                        TagData _tag = tagHolder.Tags[i];

                        // Draw this tag and remove it from the project on associated button click.
                        if (EnhancedEditorGUI.DrawTagGroupElement(_position, ref _temp, _tag)
                         && EditorUtility.DisplayDialog("Delete this Tag?",
                                                        "You are about to completely erase this tag from your project.\n\n" +
                                                        "Are you sure you want to do this? All uses of this tag will become obsolete and will be ignored.", "Yes", "Cancel")) {
                            tagHolder.DeleteTag(_tag);
                            InternalEditorUtility.RepaintAllViews();
                        }
                    }

                    // Layout update.
                    float _extraHeight = _temp.yMax - _position.yMax;
                    EnhancedEditorGUILayout.ManageDynamicGUIControlHeight(GUIContent.none, _extraHeight);

                    if (_temp.position != _position.position) {
                        GUILayout.Space(20f);
                        EditorGUILayout.HelpBox(TooltipMessage, UnityEditor.MessageType.Info, true);
                    }
                }

                GUILayout.Space(5f);
                if (GUILayout.Button("Create Tag")) {
                    MultiTagsWindow.CreateTagWindow.GetWindow(tagHolder);
                }
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            Undo.undoRedoPerformed -= Repaint;
        }
        #endregion
    }
}
