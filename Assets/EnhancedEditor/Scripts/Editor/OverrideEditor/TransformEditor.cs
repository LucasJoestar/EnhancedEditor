// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="Transform"/> editor, adding multiple additional utilities in inspector.
    /// </summary>
    [CustomEditor(typeof(Transform), true), CanEditMultipleObjects]
    public class TransformEditor : UnityObjectEditor
    {
        #region Editor Content
        private const float Spacing = 5f;
        private const float ResetButtonWidth = 25f;

        private static readonly GUIContent resetGUI = new GUIContent("R", "Reset");

        private static readonly GUIContent resetPositionGUI = new GUIContent("Reset Position", "Reset the local position of the object(s).");
        private static readonly GUIContent resetRotationGUI = new GUIContent("Reset Rotation", "Reset the local rotation of the object(s).");
        private static readonly GUIContent resetScaleGUI = new GUIContent("Reset Scale", "Reset the local scale of the object(s).");

        private static readonly GUIContent copyPositionGUI = new GUIContent("Copy Position", "Copy the local position of the object(s).");
        private static readonly GUIContent copyRotationGUI = new GUIContent("Copy Rotation", "Copy the local rotation of the object(s).");
        private static readonly GUIContent copyScaleGUI = new GUIContent("Copy Scale", "Copy the local scale of the object(s).");

        private static readonly GUIContent pastePositionGUI = new GUIContent("Paste Position", "Overwrite the local position of the object(s) with the one in buffer.");
        private static readonly GUIContent pasteRotationGUI = new GUIContent("Paste Rotation", "Overwrite the local rotation of the object(s) with the one in buffer.");
        private static readonly GUIContent pasteScaleGUI = new GUIContent("Paste Scale", "Overwrite the local scale of the object(s) with the one in buffer.");

        private static float width = 500f;

        /// <summary>
        /// Current position value in buffer (used to copy / paste).
        /// </summary>
        public static Vector3 PositionBuffer = Vector3.zero;

        /// <summary>
        /// Current rotation value in buffer (used to copy / paste).
        /// </summary>
        public static Quaternion RotationBuffer = Quaternion.identity;

        /// <summary>
        /// Current scale value in buffer (used to copy / paste).
        /// </summary>
        public static Vector3 ScaleBuffer = Vector3.one;

        private UnityEditor.Editor transformEditor = null;
        private SerializedProperty localPosition = null;
        private SerializedProperty localRotation = null;
        private SerializedProperty localScale = null;

        private Transform[] transforms = new Transform[] { };

        // -----------------------

        public override void OnInspectorGUI()
        {
            Rect _rect = EditorGUILayout.GetControlRect(false, 0f);
            if (Event.current.type == EventType.Repaint)
                width = _rect.width - ResetButtonWidth - Spacing;

            // Draw original inspector.
            EditorGUILayout.BeginVertical(GUILayout.Width(width));
            transformEditor.OnInspectorGUI();
            EditorGUILayout.EndVertical();

            // Reset buttons.
            _rect.Set(_rect.x + width + Spacing, _rect.y + EditorGUIUtility.standardVerticalSpacing - 1f, ResetButtonWidth, EditorGUIUtility.singleLineHeight + 1f);
            if (GUI.Button(_rect, resetGUI))
                ResetPosition();

            _rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + .2f;
            if (GUI.Button(_rect, resetGUI))
                ResetRotation();

            _rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + .2f;
            if (GUI.Button(_rect, resetGUI))
                ResetScale();

            // Copy buttons.
            EditorGUILayout.BeginHorizontal();
            if (localPosition.hasMultipleDifferentValues)
            {
                GUI.enabled = false;
                GUILayout.Button(copyPositionGUI);
                GUI.enabled = true;
            }
            else if (GUILayout.Button(copyPositionGUI))
                CopyPosition();

            if (localRotation.hasMultipleDifferentValues)
            {
                GUI.enabled = false;
                GUILayout.Button(copyRotationGUI);
                GUI.enabled = true;
            }
            else if (GUILayout.Button(copyRotationGUI))
                CopyRotation();

            if (localScale.hasMultipleDifferentValues)
            {
                GUI.enabled = false;
                GUILayout.Button(copyScaleGUI);
                GUI.enabled = true;
            }
            else if (GUILayout.Button(copyScaleGUI))
                CopyScale();

            EditorGUILayout.EndHorizontal();

            // Paste buttons.
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(pastePositionGUI))
                PastePosition();

            if (GUILayout.Button(pasteRotationGUI))
                PasteRotation();

            if (GUILayout.Button(pasteScaleGUI))
                PasteScale();

            EditorGUILayout.EndHorizontal();

            // Context menu.
            Event _event = Event.current;
            if (_event.type == EventType.ContextClick)
            {
                GenericMenu _menu = new GenericMenu();

                // Reset options.
                _menu.AddItem(resetPositionGUI, false, ResetPosition);
                _menu.AddItem(resetRotationGUI, false, ResetRotation);
                _menu.AddItem(resetScaleGUI, false, ResetScale);
                _menu.AddSeparator(string.Empty);

                // Copy options.
                if (localPosition.hasMultipleDifferentValues)
                {
                    _menu.AddDisabledItem(copyPositionGUI, false);
                }
                else
                    _menu.AddItem(copyPositionGUI, false, CopyPosition);

                if (localRotation.hasMultipleDifferentValues)
                {
                    _menu.AddDisabledItem(copyRotationGUI, false);
                }
                else
                    _menu.AddItem(copyRotationGUI, false, CopyRotation);

                if (localScale.hasMultipleDifferentValues)
                {
                    _menu.AddDisabledItem(copyScaleGUI, false);
                }
                else
                    _menu.AddItem(copyScaleGUI, false, CopyScale);

                // Paste options.
                _menu.AddSeparator(string.Empty);
                _menu.AddItem(pastePositionGUI, false, PastePosition);
                _menu.AddItem(pasteRotationGUI, false, PasteRotation);
                _menu.AddItem(pasteScaleGUI, false, PasteScale);

                _menu.ShowAsContext();
                _event.Use();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            transformEditor = CreateEditor(serializedObject.targetObjects, typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.TransformInspector"));
            localPosition = serializedObject.FindProperty("m_LocalPosition");
            localRotation = serializedObject.FindProperty("m_LocalRotation");
            localScale = serializedObject.FindProperty("m_LocalScale");

            transforms = Array.ConvertAll(serializedObject.targetObjects, (t) => t as Transform);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Avoid memory leak.
            DestroyImmediate(transformEditor);
        }
        #endregion

        #region Reset
        private void ResetPosition()
        {
            Undo.RecordObjects(transforms, "reset object(s) position");
            for (int _i = 0; _i < transforms.Length; _i++)
            {
                Transform _transform = transforms[_i];
                _transform.localPosition = Vector3.zero;
            }
        }

        private void ResetRotation()
        {
            Undo.RecordObjects(transforms, "reset object(s) rotation");
            for (int _i = 0; _i < transforms.Length; _i++)
            {
                Transform _transform = transforms[_i];
                _transform.localRotation = Quaternion.identity;
            }
        }

        private void ResetScale()
        {
            Undo.RecordObjects(transforms, "reset object(s) scale");
            for (int _i = 0; _i < transforms.Length; _i++)
            {
                Transform _transform = transforms[_i];
                _transform.localScale = Vector3.one;
            }
        }
        #endregion

        #region Copy
        private void CopyPosition()
        {
            Transform _transform = transforms[0];
            PositionBuffer = _transform.localPosition;
        }

        private void CopyRotation()
        {
            Transform _transform = transforms[0];
            RotationBuffer = _transform.localRotation;
        }

        private void CopyScale()
        {
            Transform _transform = transforms[0];
            ScaleBuffer = _transform.localScale;
        }
        #endregion

        #region Paste
        private void PastePosition()
        {
            Undo.RecordObjects(transforms, "paste object(s) position");
            for (int _i = 0; _i < transforms.Length; _i++)
            {
                Transform _transform = transforms[_i];
                _transform.localPosition = PositionBuffer;
            }
        }

        private void PasteRotation()
        {
            Undo.RecordObjects(transforms, "paste object(s) rotation");
            for (int _i = 0; _i < transforms.Length; _i++)
            {
                Transform _transform = transforms[_i];
                _transform.localRotation = RotationBuffer;
            }
        }

        private void PasteScale()
        {
            Undo.RecordObjects(transforms, "paste object(s) scale");
            for (int _i = 0; _i < transforms.Length; _i++)
            {
                Transform _transform = transforms[_i];
                _transform.localScale = ScaleBuffer;
            }
        }
        #endregion
    }
}
