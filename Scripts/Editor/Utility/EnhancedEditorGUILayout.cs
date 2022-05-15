// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Auto laid-out version of <see cref="EnhancedEditorGUI"/>, containing multiple editor-related GUI methods.
    /// </summary>
    public static class EnhancedEditorGUILayout
    {
        // --- Decorator Drawers --- \\

        #region Horizontal Line
        /// <summary>
        /// Draws a horizontal line on screen.
        /// </summary>
        /// <inheritdoc cref="HorizontalLine(Color, float, GUILayoutOption[])"/>
        public static void HorizontalLine(Color _color, params GUILayoutOption[] _options)
        {
            float _margins = 0f;
            HorizontalLine(_color, _margins, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.HorizontalLine(Rect, Color, float)"/>
        public static void HorizontalLine(Color _color, float _margins, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(false, 2f, _options);
            EnhancedEditorGUI.HorizontalLine(_position, _color, _margins);
        }
        #endregion

        #region Section
        /// <inheritdoc cref="Section(GUIContent, float, GUILayoutOption[])"/>
        public static void Section(string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            Section(_labelGUI, _options);
        }

        /// <inheritdoc cref="Section(GUIContent, float, GUILayoutOption[])"/>
        public static void Section(GUIContent _label, params GUILayoutOption[] _options)
        {
            float _lineWidth = EnhancedEditorGUIUtility.SectionDefaultLineWidth;
            Section(_label, _lineWidth, _options);
        }

        /// <inheritdoc cref="Section(GUIContent, float, GUILayoutOption[])"/>
        public static void Section(string _label, float _lineWidth = EnhancedEditorGUIUtility.SectionDefaultLineWidth, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            Section(_labelGUI, _lineWidth, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.Section(Rect, GUIContent, float)"/>
        public static void Section(GUIContent _label, float _lineWidth = EnhancedEditorGUIUtility.SectionDefaultLineWidth, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.Section(_position, _label, _lineWidth);
        }
        #endregion

        // --- Property Drawers --- \\

        #region Asset Preview
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="AssetPreviewField(SerializedProperty, GUIContent, float, GUILayoutOption[])"/>
        public static void AssetPreviewField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            AssetPreviewField(_property, _label, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(SerializedProperty, GUIContent, float, GUILayoutOption[])"/>
        public static void AssetPreviewField(SerializedProperty _property,
                                             float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            AssetPreviewField(_property, _label, _previewSize, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(SerializedProperty, GUIContent, float, GUILayoutOption[])"/>
        public static void AssetPreviewField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            AssetPreviewField(_property, _labelGUI, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(SerializedProperty, GUIContent, float, GUILayoutOption[])"/>
        public static void AssetPreviewField(SerializedProperty _property, string _label,
                                             float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            AssetPreviewField(_property, _labelGUI, _previewSize, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(SerializedProperty, GUIContent, float, GUILayoutOption[])"/>
        public static void AssetPreviewField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize;
            AssetPreviewField(_property, _label, _previewSize, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.AssetPreviewField(Rect, SerializedProperty, GUIContent, out float, float)"/>
        public static void AssetPreviewField(SerializedProperty _property, GUIContent _label,
                                             float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.AssetPreviewField(_position, _property, _label, out float _extraHeight, _previewSize);

            IncrementPosition(_extraHeight);
        }

        // ===== Object Value ===== \\

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(Object _object, Type _objectType, ref bool _foldout, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return AssetPreviewField(_object, _objectType, _allowSceneObjects, ref _foldout, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(Object _object, Type _objectType, bool _allowSceneObjects, ref bool _foldout, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return AssetPreviewField(_label, _object, _objectType, _allowSceneObjects, ref _foldout, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(Object _object, Type _objectType, ref bool _foldout,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return AssetPreviewField(_object, _objectType, _allowSceneObjects, ref _foldout, _previewSize, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(Object _object, Type _objectType, bool _allowSceneObjects, ref bool _foldout,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return AssetPreviewField(_label, _object, _objectType, _allowSceneObjects, ref _foldout, _previewSize, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(string _label, Object _object, Type _objectType, ref bool _foldout, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return AssetPreviewField(_label, _object, _objectType, _allowSceneObjects, ref _foldout, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(string _label, Object _object, Type _objectType, bool _allowSceneObjects, ref bool _foldout, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return AssetPreviewField(_labelGUI, _object, _objectType, _allowSceneObjects, ref _foldout, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(string _label, Object _object, Type _objectType, ref bool _foldout,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return AssetPreviewField(_label, _object, _objectType, _allowSceneObjects, ref _foldout, _previewSize, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(string _label, Object _object, Type _objectType, bool _allowSceneObjects, ref bool _foldout,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return AssetPreviewField(_labelGUI, _object, _objectType, _allowSceneObjects, ref _foldout, _previewSize, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(GUIContent _label, Object _object, Type _objectType, ref bool _foldout, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return AssetPreviewField(_label, _object, _objectType, _allowSceneObjects, ref _foldout, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(GUIContent _label, Object _object, Type _objectType, bool _allowSceneObjects, ref bool _foldout, params GUILayoutOption[] _options)
        {
            float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize;
            return AssetPreviewField(_label, _object, _objectType, _allowSceneObjects, ref _foldout, _previewSize, _options);
        }

        /// <inheritdoc cref="AssetPreviewField(GUIContent, Object, Type, bool, ref bool, float, GUILayoutOption[])"/>
        public static Object AssetPreviewField(GUIContent _label, Object _object, Type _objectType, ref bool _foldout,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return AssetPreviewField(_label, _object, _objectType, _allowSceneObjects, ref _foldout, _previewSize, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.AssetPreviewField(Rect, GUIContent, Object, Type, bool, ref bool, out float, float)"/>
        public static Object AssetPreviewField(GUIContent _label, Object _object, Type _objectType, bool _allowSceneObjects, ref bool _foldout,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            _object = EnhancedEditorGUI.AssetPreviewField(_position, _label, _object, _objectType, _allowSceneObjects, ref _foldout, out float _extraHeight, _previewSize);

            IncrementPosition(_extraHeight);
            return _object;
        }
        #endregion

        #region Block
        /// <inheritdoc cref="BlockField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void BlockField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            BlockField(_property, _label, _options);
        }

        /// <inheritdoc cref="BlockField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void BlockField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            BlockField(_property, _labelGUI, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.BlockField(Rect, SerializedProperty, GUIContent)"/>
        public static void BlockField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(true, EditorGUI.GetPropertyHeight(_property, true), _options);
            EnhancedEditorGUI.BlockField(_position, _property, _label);
        }
        #endregion

        #region Color Palette
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="ColorPaletteField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void ColorPaletteField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ColorPaletteField(_property, _label, _options);
        }

        /// <inheritdoc cref="ColorPaletteField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void ColorPaletteField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ColorPaletteField(_property, _labelGUI, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ColorPaletteField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void ColorPaletteField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.ColorPaletteField(_position, _property, _label, out float _extraHeight);

            IncrementPosition(_extraHeight);
        }

        // ===== Color Value ===== \\

        /// <inheritdoc cref="ColorPaletteField(GUIContent, Color, GUILayoutOption[])"/>
        public static Color ColorPaletteField(Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return ColorPaletteField(_label, _color, _options);
        }

        /// <inheritdoc cref="ColorPaletteField(GUIContent, Color, GUILayoutOption[])"/>
        public static Color ColorPaletteField(string _label, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return ColorPaletteField(_labelGUI, _color, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ColorPaletteField(Rect, GUIContent, Color, out float)"/>
        public static Color ColorPaletteField(GUIContent _label, Color _color, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            _color = EnhancedEditorGUI.ColorPaletteField(_position, _label, _color, out float _extraHeight);

            IncrementPosition(_extraHeight);
            return _color;
        }
        #endregion

        #region Folder
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="FolderField(SerializedProperty, GUIContent, bool, string, GUILayoutOption[])"/>
        public static void FolderField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            FolderField(_property, _label, _options);
        }

        /// <inheritdoc cref="FolderField(SerializedProperty, GUIContent, bool, string, GUILayoutOption[])"/>
        public static void FolderField(SerializedProperty _property, bool _allowOutsideProjectFolder = false, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            FolderField(_property, _label, _allowOutsideProjectFolder, _options);
        }

        /// <inheritdoc cref="FolderField(SerializedProperty, GUIContent, bool, string, GUILayoutOption[])"/>
        public static void FolderField(SerializedProperty _property, bool _allowOutsideProjectFolder, string _folderPanelTitle, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            FolderField(_property, _label, _allowOutsideProjectFolder, _folderPanelTitle, _options);
        }

        /// <inheritdoc cref="FolderField(SerializedProperty, GUIContent, bool, string, GUILayoutOption[])"/>
        public static void FolderField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            FolderField(_property, _labelGUI, _options);
        }

        /// <inheritdoc cref="FolderField(SerializedProperty, GUIContent, bool, string, GUILayoutOption[])"/>
        public static void FolderField(SerializedProperty _property, string _label, bool _allowOutsideProjectFolder = false, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            FolderField(_property, _labelGUI, _allowOutsideProjectFolder, _options);
        }

        /// <inheritdoc cref="FolderField(SerializedProperty, GUIContent, bool, string, GUILayoutOption[])"/>
        public static void FolderField(SerializedProperty _property, string _label, bool _allowOutsideProjectFolder, string _folderPanelTitle, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            FolderField(_property, _labelGUI, _allowOutsideProjectFolder, _folderPanelTitle, _options);
        }

        /// <inheritdoc cref="FolderField(SerializedProperty, GUIContent, bool, string, GUILayoutOption[])"/>
        public static void FolderField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            bool _allowOutsideProjectFolder = false;
            FolderField(_property, _label, _allowOutsideProjectFolder, _options);
        }

        /// <inheritdoc cref="FolderField(SerializedProperty, GUIContent, bool, string, GUILayoutOption[])"/>
        public static void FolderField(SerializedProperty _property, GUIContent _label, bool _allowOutsideProjectFolder = false, params GUILayoutOption[] _options)
        {
            string _folderPanelTitle = string.Empty;
            FolderField(_property, _label, _allowOutsideProjectFolder, _folderPanelTitle, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.FolderField(Rect, SerializedProperty, GUIContent, bool, string)"/>
        public static void FolderField(SerializedProperty _property, GUIContent _label, bool _allowOutsideProjectFolder, string _folderPanelTitle, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.FolderField(_position, _property, _label, _allowOutsideProjectFolder, _folderPanelTitle);
        }

        // ===== String Value ===== \\

        /// <inheritdoc cref="FolderField(GUIContent, string, bool, string, GUILayoutOption[])"/>
        public static string FolderField(string _folderPath, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return FolderField(_label, _folderPath, _options);
        }

        /// <inheritdoc cref="FolderField(GUIContent, string, bool, string, GUILayoutOption[])"/>
        public static string FolderField(string _folderPath, bool _allowOutsideProjectFolder = false, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return FolderField(_label, _folderPath, _allowOutsideProjectFolder, _options);
        }

        /// <inheritdoc cref="FolderField(GUIContent, string, bool, string, GUILayoutOption[])"/>
        public static string FolderField(string _folderPath, bool _allowOutsideProjectFolder, string _folderPanelTitle, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return FolderField(_label, _folderPath, _allowOutsideProjectFolder, _folderPanelTitle, _options);
        }

        /// <inheritdoc cref="FolderField(GUIContent, string, bool, string, GUILayoutOption[])"/>
        public static string FolderField(string _label, string _folderPath, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return FolderField(_labelGUI, _folderPath, _options);
        }

        /// <inheritdoc cref="FolderField(GUIContent, string, bool, string, GUILayoutOption[])"/>
        public static string FolderField(string _label, string _folderPath, bool _allowOutsideProjectFolder = false, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return FolderField(_labelGUI, _folderPath, _allowOutsideProjectFolder, _options);
        }

        /// <inheritdoc cref="FolderField(GUIContent, string, bool, string, GUILayoutOption[])"/>
        public static string FolderField(string _label, string _folderPath, bool _allowOutsideProjectFolder, string _folderPanelTitle, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return FolderField(_labelGUI, _folderPath, _allowOutsideProjectFolder, _folderPanelTitle, _options);
        }

        /// <inheritdoc cref="FolderField(GUIContent, string, bool, string, GUILayoutOption[])"/>
        public static string FolderField(GUIContent _label, string _folderPath, params GUILayoutOption[] _options)
        {
            bool _allowOutsideProjectFolder = false;
            return FolderField(_label, _folderPath, _allowOutsideProjectFolder, _options);
        }

        /// <inheritdoc cref="FolderField(GUIContent, string, bool, string, GUILayoutOption[])"/>
        public static string FolderField(GUIContent _label, string _folderPath, bool _allowOutsideProjectFolder = false, params GUILayoutOption[] _options)
        {
            string _folderPanelTitle = string.Empty;
            return FolderField(_label, _folderPath, _allowOutsideProjectFolder, _folderPanelTitle, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.FolderField(Rect, GUIContent, string, bool, string)"/>
        public static string FolderField(GUIContent _label, string _folderPath, bool _allowOutsideProjectFolder, string _folderPanelTitle, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.FolderField(_position, _label, _folderPath, _allowOutsideProjectFolder, _folderPanelTitle);
        }

        // ===== Editor Folder - Serialized Property ===== \\

        /// <inheritdoc cref="EditorFolderField(SerializedProperty, GUIContent, string, GUILayoutOption[])"/>
        public static void EditorFolderField(SerializedProperty _property, string _folderPanelTitle = EnhancedEditorGUI.DefaultEditorPanelTitle, params GUILayoutOption[] _options) {
            GUIContent _label = GUIContent.none;
            EditorFolderField(_property, _label, _folderPanelTitle);
        }

        /// <inheritdoc cref="EditorFolderField(SerializedProperty, GUIContent, string, GUILayoutOption[])"/>
        public static void EditorFolderField(SerializedProperty _property, string _label, string _folderPanelTitle = EnhancedEditorGUI.DefaultEditorPanelTitle,
                                             params GUILayoutOption[] _options) {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            EditorFolderField(_property, _labelGUI, _folderPanelTitle);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.EditorFolderField(Rect, SerializedProperty, GUIContent, string)"/>
        public static void EditorFolderField(SerializedProperty _property, GUIContent _label, string _folderPanelTitle = EnhancedEditorGUI.DefaultEditorPanelTitle,
                                             params GUILayoutOption[] _options) {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.EditorFolderField(_position, _property, _label, _folderPanelTitle);
        }

        // ===== Editor Folder - String Value ===== \\

        /// <inheritdoc cref="EditorFolderField(GUIContent, string, string, GUILayoutOption[])"/>
        public static string EditorFolderField(string _folderPath, string _folderPanelTitle = EnhancedEditorGUI.DefaultEditorPanelTitle, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return EditorFolderField(_label, _folderPath, _folderPanelTitle, _options);
        }

        /// <inheritdoc cref="EditorFolderField(GUIContent, string, string, GUILayoutOption[])"/>
        public static string EditorFolderField(string _label, string _folderPath, string _folderPanelTitle = EnhancedEditorGUI.DefaultEditorPanelTitle, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return EditorFolderField(_labelGUI, _folderPath, _folderPanelTitle, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.EditorFolderField(Rect, GUIContent, string, string)"/>
        public static string EditorFolderField(GUIContent _label, string _folderPath, string _folderPanelTitle = EnhancedEditorGUI.DefaultEditorPanelTitle, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.EditorFolderField(_position, _label, _folderPath, _folderPanelTitle);
        }
        #endregion

        #region Inline
        /// <inheritdoc cref="InlineField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void InlineField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            InlineField(_property, _label, _options);
        }

        /// <inheritdoc cref="InlineField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void InlineField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            InlineField(_property, _labelGUI, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.InlineField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void InlineField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.InlineField(_position, _property, _label, out float _extraHeight);
            
            IncrementPosition(_extraHeight);
        }
        #endregion

        #region Max
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="MaxField(SerializedProperty, GUIContent, MemberValue{float}, GUILayoutOption[])"/>
        public static void MaxField(SerializedProperty _property, MemberValue<float> _maxMember, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MaxField(_property, _label, _maxMember, _options);
        }

        /// <inheritdoc cref="MaxField(SerializedProperty, GUIContent, MemberValue{float}, GUILayoutOption[])"/>
        public static void MaxField(SerializedProperty _property, string _label, MemberValue<float> _maxMember, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MaxField(_property, _labelGUI, _maxMember, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MaxField(Rect, SerializedProperty, GUIContent, MemberValue{float})"/>
        public static void MaxField(SerializedProperty _property, GUIContent _label, MemberValue<float> _maxMember, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.MaxField(_position, _property, _label, _maxMember);
        }

        /// <inheritdoc cref="MaxField(SerializedProperty, GUIContent, float, GUILayoutOption[])"/>
        public static void MaxField(SerializedProperty _property, float _maxValue, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MaxField(_property, _label, _maxValue, _options);
        }

        /// <inheritdoc cref="MaxField(SerializedProperty, GUIContent, float, GUILayoutOption[])"/>
        public static void MaxField(SerializedProperty _property, string _label, float _maxValue, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MaxField(_property, _labelGUI, _maxValue, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MaxField(Rect, SerializedProperty, GUIContent, float)"/>
        public static void MaxField(SerializedProperty _property, GUIContent _label, float _maxValue, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.MaxField(_position, _property, _label, _maxValue);
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="MaxField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MaxField(float _value, float _maxValue, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MaxField(_label, _value, _maxValue, _options);
        }

        /// <inheritdoc cref="MaxField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MaxField(float _value, float _maxValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MaxField(_label, _value, _maxValue, _style, _options);
        }

        /// <inheritdoc cref="MaxField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MaxField(string _label, float _value, float _maxValue, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MaxField(_labelGUI, _value, _maxValue, _options);
        }

        /// <inheritdoc cref="MaxField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MaxField(string _label, float _value, float _maxValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MaxField(_labelGUI, _value, _maxValue, _style, _options);
        }

        /// <inheritdoc cref="MaxField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MaxField(GUIContent _label, float _value, float _maxValue, params GUILayoutOption[] _options)
        {
            GUIStyle _style = EditorStyles.numberField;
            return MaxField(_label, _value, _maxValue, _style, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MaxField(GUIContent _label, float _value, float _maxValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.MaxField(_position, _label, _value, _maxValue, _style);
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="MaxField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MaxField(int _value, int _maxValue, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MaxField(_label, _value, _maxValue, _options);
        }

        /// <inheritdoc cref="MaxField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MaxField(int _value, int _maxValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MaxField(_label, _value, _maxValue, _style, _options);
        }

        /// <inheritdoc cref="MaxField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MaxField(string _label, int _value, int _maxValue, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MaxField(_labelGUI, _value, _maxValue, _options);
        }

        /// <inheritdoc cref="MaxField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MaxField(string _label, int _value, int _maxValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MaxField(_labelGUI, _value, _maxValue, _style, _options);
        }

        /// <inheritdoc cref="MaxField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MaxField(GUIContent _label, int _value, int _maxValue, params GUILayoutOption[] _options)
        {
            GUIStyle _style = EditorStyles.numberField;
            return MaxField(_label, _value, _maxValue, _style, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MaxField(Rect, GUIContent, int, int, GUIStyle)"/>
        public static int MaxField(GUIContent _label, int _value, int _maxValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.MaxField(_position, _label, _value, _maxValue, _style);
        }
        #endregion

        #region Min
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="MinField(SerializedProperty, GUIContent, MemberValue{float}, GUILayoutOption[])"/>
        public static void MinField(SerializedProperty _property, MemberValue<float> _minMember, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinField(_property, _label, _minMember, _options);
        }

        /// <inheritdoc cref="MinField(SerializedProperty, GUIContent, MemberValue{float}, GUILayoutOption[])"/>
        public static void MinField(SerializedProperty _property, string _label, MemberValue<float> _minMember, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MinField(_property, _labelGUI, _minMember, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinField(Rect, SerializedProperty, GUIContent, MemberValue{float})"/>
        public static void MinField(SerializedProperty _property, GUIContent _label, MemberValue<float> _minMember, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.MinField(_position, _property, _label, _minMember);
        }

        /// <inheritdoc cref="MinField(SerializedProperty, GUIContent, float, GUILayoutOption[])"/>
        public static void MinField(SerializedProperty _property, float _minValue, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinField(_property, _label, _minValue, _options);
        }

        /// <inheritdoc cref="MinField(SerializedProperty, GUIContent, float, GUILayoutOption[])"/>
        public static void MinField(SerializedProperty _property, string _label, float _minValue, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MinField(_property, _labelGUI, _minValue, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinField(Rect, SerializedProperty, GUIContent, float)"/>
        public static void MinField(SerializedProperty _property, GUIContent _label, float _minValue, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.MinField(_position, _property, _label, _minValue);
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="MinField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MinField(float _value, float _minValue, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MinField(_label, _value, _minValue, _options);
        }

        /// <inheritdoc cref="MinField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MinField(float _value, float _minValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MinField(_label, _value, _minValue, _style, _options);
        }

        /// <inheritdoc cref="MinField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MinField(string _label, float _value, float _minValue, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinField(_labelGUI, _value, _minValue, _options);
        }

        /// <inheritdoc cref="MinField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MinField(string _label, float _value, float _minValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinField(_labelGUI, _value, _minValue, _style, _options);
        }

        /// <inheritdoc cref="MinField(GUIContent, float, float, GUIStyle, GUILayoutOption[])"/>
        public static float MinField(GUIContent _label, float _value, float _minValue, params GUILayoutOption[] _options)
        {
            GUIStyle _style = EditorStyles.numberField;
            return MinField(_label, _value, _minValue, _style, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MinField(GUIContent _label, float _value, float _minValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.MinField(_position, _label, _value, _minValue, _style);
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="MinField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MinField(int _value, int _minValue, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MinField(_label, _value, _minValue, _options);
        }

        /// <inheritdoc cref="MinField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MinField(int _value, int _minValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MinField(_label, _value, _minValue, _style, _options);
        }

        /// <inheritdoc cref="MinField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MinField(string _label, int _value, int _minValue, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinField(_labelGUI, _value, _minValue, _options);
        }

        /// <inheritdoc cref="MinField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MinField(string _label, int _value, int _minValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinField(_labelGUI, _value, _minValue, _style, _options);
        }

        /// <inheritdoc cref="MinField(GUIContent, int, int, GUIStyle, GUILayoutOption[])"/>
        public static int MinField(GUIContent _label, int _value, int _minValue, params GUILayoutOption[] _options)
        {
            GUIStyle _style = EditorStyles.numberField;
            return MinField(_label, _value, _minValue, _style, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinField(Rect, GUIContent, int, int, GUIStyle)"/>
        public static int MinField(GUIContent _label, int _value, int _minValue, GUIStyle _style, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.MinField(_position, _label, _value, _minValue, _style);
        }
        #endregion

        #region Min Max
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="MinMaxField(SerializedProperty, GUIContent, MemberValue{Vector2}, GUILayoutOption[])"/>
        public static void MinMaxField(SerializedProperty _property, MemberValue<Vector2> _minMaxMember, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinMaxField(_property, _label, _minMaxMember, _options);
        }

        /// <inheritdoc cref="MinMaxField(SerializedProperty, GUIContent, MemberValue{Vector2}, GUILayoutOption[])"/>
        public static void MinMaxField(SerializedProperty _property, string _label, MemberValue<Vector2> _minMaxMember, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MinMaxField(_property, _labelGUI, _minMaxMember, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinMaxField(Rect, SerializedProperty, GUIContent, MemberValue{Vector2})"/>
        public static void MinMaxField(SerializedProperty _property, GUIContent _label, MemberValue<Vector2> _minMaxMember, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.MinMaxField(_position, _property, _label, _minMaxMember);
        }

        /// <inheritdoc cref="MinMaxField(SerializedProperty, GUIContent, float, float, GUILayoutOption[])"/>
        public static void MinMaxField(SerializedProperty _property, float _minLimit, float _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinMaxField(_property, _label, _minLimit, _maxLimit, _options);
        }

        /// <inheritdoc cref="MinMaxField(SerializedProperty, GUIContent, float, float, GUILayoutOption[])"/>
        public static void MinMaxField(SerializedProperty _property, string _label, float _minLimit, float _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MinMaxField(_property, _labelGUI, _minLimit, _maxLimit, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinMaxField(Rect, SerializedProperty, GUIContent, float, float)"/>
        public static void MinMaxField(SerializedProperty _property, GUIContent _label, float _minLimit, float _maxLimit, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.MinMaxField(_position, _property, _label, _minLimit, _maxLimit);
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="MinMaxField(GUIContent, Vector2, float, float, GUILayoutOption[])"/>
        public static Vector2 MinMaxField(Vector2 _value, float _minLimit, float _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MinMaxField(_label, _value, _minLimit, _maxLimit, _options);
        }

        /// <inheritdoc cref="MinMaxField(GUIContent, Vector2, float, float, GUILayoutOption[])"/>
        public static Vector2 MinMaxField(string _label, Vector2 _value, float _minLimit, float _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinMaxField(_labelGUI, _value, _minLimit, _maxLimit, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinMaxField(Rect, GUIContent, Vector2, float, float)"/>
        public static Vector2 MinMaxField(GUIContent _label, Vector2 _value, float _minLimit, float _maxLimit, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.MinMaxField(_position, _label, _value, _minLimit, _maxLimit);
        }

        /// <inheritdoc cref="MinMaxField(GUIContent, float, float, float, float, GUILayoutOption[])"/>
        public static Vector2 MinMaxField(float _minValue, float _maxValue, float _minLimit, float _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MinMaxField(_label, _minValue, _maxValue, _minLimit, _maxLimit, _options);
        }

        /// <inheritdoc cref="MinMaxField(GUIContent, float, float, float, float, GUILayoutOption[])"/>
        public static Vector2 MinMaxField(string _label, float _minValue, float _maxValue, float _minLimit, float _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinMaxField(_labelGUI, _minValue, _maxValue, _minLimit, _maxLimit, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinMaxField(Rect, GUIContent, float, float, float, float)"/>
        public static Vector2 MinMaxField(GUIContent _label, float _minValue, float _maxValue, float _minLimit, float _maxLimit, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.MinMaxField(_position, _label, _minValue, _maxValue, _minLimit, _maxLimit);
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="MinMaxField(GUIContent, Vector2Int, int, int, GUILayoutOption[])"/>
        public static Vector2Int MinMaxField(Vector2Int _value, int _minLimit, int _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MinMaxField(_label, _value, _minLimit, _maxLimit, _options);
        }

        /// <inheritdoc cref="MinMaxField(GUIContent, Vector2Int, int, int, GUILayoutOption[])"/>
        public static Vector2Int MinMaxField(string _label, Vector2Int _value, int _minLimit, int _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinMaxField(_labelGUI, _value, _minLimit, _maxLimit, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinMaxField(Rect, GUIContent, Vector2Int, int, int)"/>
        public static Vector2Int MinMaxField(GUIContent _label, Vector2Int _value, int _minLimit, int _maxLimit, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.MinMaxField(_position, _label, _value, _minLimit, _maxLimit);
        }

        /// <inheritdoc cref="MinMaxField(GUIContent, int, int, int, int, GUILayoutOption[])"/>
        public static Vector2Int MinMaxField(int _minValue, int _maxValue, int _minLimit, int _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return MinMaxField(_label, _minValue, _maxValue, _minLimit, _maxLimit, _options);
        }

        /// <inheritdoc cref="MinMaxField(GUIContent, int, int, int, int, GUILayoutOption[])"/>
        public static Vector2Int MinMaxField(string _label, int _minValue, int _maxValue, int _minLimit, int _maxLimit, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinMaxField(_labelGUI, _minValue, _maxValue, _minLimit, _maxLimit, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.MinMaxField(Rect, GUIContent, int, int, int, int)"/>
        public static Vector2Int MinMaxField(GUIContent _label, int _minValue, int _maxValue, int _minLimit, int _maxLimit, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.MinMaxField(_position, _label, _minValue, _maxValue, _minLimit, _maxLimit);
        }
        #endregion

        #region Picker
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="PickerField(SerializedProperty, GUIContent, Type, GUILayoutOption[])"/>
        public static void PickerField(SerializedProperty _property, Type _requiredType, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            PickerField(_property, _label, _requiredType, _options);
        }

        /// <inheritdoc cref="PickerField(SerializedProperty, GUIContent, Type, GUILayoutOption[])"/>
        public static void PickerField(SerializedProperty _property, string _label, Type _requiredType, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            PickerField(_property, _labelGUI, _requiredType, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.PickerField(Rect, SerializedProperty, GUIContent, Type)"/>
        public static void PickerField(SerializedProperty _property, GUIContent _label, Type _requiredType, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.PickerField(_position, _property, _label, _requiredType);
        }

        /// <inheritdoc cref="PickerField(SerializedProperty, GUIContent, Type[], GUILayoutOption[])"/>
        public static void PickerField(SerializedProperty _property, Type[] _requiredTypes, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            PickerField(_property, _label, _requiredTypes, _options);
        }

        /// <inheritdoc cref="PickerField(SerializedProperty, GUIContent, Type[], GUILayoutOption[])"/>
        public static void PickerField(SerializedProperty _property, string _label, Type[] _requiredTypes, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            PickerField(_property, _labelGUI, _requiredTypes, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.PickerField(Rect, SerializedProperty, GUIContent, Type[])"/>
        public static void PickerField(SerializedProperty _property, GUIContent _label, Type[] _requiredTypes, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.PickerField(_position, _property, _label, _requiredTypes);
        }

        // ===== Object Value ===== \\

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type, bool, GUILayoutOption[])"/>
        public static Object PickerField(Object _object, Type _objectType, Type _requiredType, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return PickerField(_object, _objectType, _requiredType, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type, bool, GUILayoutOption[])"/>
        public static Object PickerField(Object _object, Type _objectType, Type _requiredType, bool _allowSceneObjects, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return PickerField(_label, _object, _objectType, _requiredType, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type, bool, GUILayoutOption[])"/>
        public static Object PickerField(string _label, Object _object, Type _objectType, Type _requiredType, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return PickerField(_label, _object, _objectType, _requiredType, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type, bool, GUILayoutOption[])"/>
        public static Object PickerField(string _label, Object _object, Type _objectType, Type _requiredType, bool _allowSceneObjects, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return PickerField(_labelGUI, _object, _objectType, _requiredType, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type, bool, GUILayoutOption[])"/>
        public static Object PickerField(GUIContent _label, Object _object, Type _objectType, Type _requiredType, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return PickerField(_label, _object, _objectType, _requiredType, _allowSceneObjects, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.PickerField(Rect, GUIContent, Object, Type, Type, bool)"/>
        public static Object PickerField(GUIContent _label, Object _object, Type _objectType, Type _requiredType, bool _allowSceneObjects, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.PickerField(_position, _label, _object, _objectType, _requiredType, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type[], bool, GUILayoutOption[])"/>
        public static Object PickerField(Object _object, Type _objectType, Type[] _requiredTypes, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return PickerField(_object, _objectType, _requiredTypes, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type[], bool, GUILayoutOption[])"/>
        public static Object PickerField(Object _object, Type _objectType, Type[] _requiredTypes, bool _allowSceneObjects, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return PickerField(_label, _object, _objectType, _requiredTypes, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type[], bool, GUILayoutOption[])"/>
        public static Object PickerField(string _label, Object _object, Type _objectType, Type[] _requiredTypes, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return PickerField(_label, _object, _objectType, _requiredTypes, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type[], bool, GUILayoutOption[])"/>
        public static Object PickerField(string _label, Object _object, Type _objectType, Type[] _requiredTypes, bool _allowSceneObjects, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return PickerField(_labelGUI, _object, _objectType, _requiredTypes, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="PickerField(GUIContent, Object, Type, Type[], bool, GUILayoutOption[])"/>
        public static Object PickerField(GUIContent _label, Object _object, Type _objectType, Type[] _requiredTypes, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return PickerField(_label, _object, _objectType, _requiredTypes, _allowSceneObjects, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.PickerField(Rect, GUIContent, Object, Type, Type[], bool)"/>
        public static Object PickerField(GUIContent _label, Object _object, Type _objectType, Type[] _requiredTypes, bool _allowSceneObjects, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.PickerField(_position, _label, _object, _objectType, _requiredTypes, _allowSceneObjects);
        }
        #endregion

        #region Precision Slider
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="PrecisionSliderField(SerializedProperty, GUIContent, float, float, float, GUILayoutOption[])"/>
        public static void PrecisionSliderField(SerializedProperty _property, float _minValue, float _maxValue, float _precision, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            PrecisionSliderField(_property, _label, _minValue, _maxValue, _precision, _options);
        }

        /// <inheritdoc cref="PrecisionSliderField(SerializedProperty, GUIContent, float, float, float, GUILayoutOption[])"/>
        public static void PrecisionSliderField(SerializedProperty _property, string _label, float _minValue, float _maxValue, float _precision, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            PrecisionSliderField(_property, _labelGUI, _minValue, _maxValue, _precision, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.PrecisionSliderField(Rect, SerializedProperty, GUIContent, float, float, float, out float)"/>
        public static void PrecisionSliderField(SerializedProperty _property, GUIContent _label, float _minValue, float _maxValue, float _precision, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.PrecisionSliderField(_position, _property, _label, _minValue, _maxValue, _precision, out float _extraHeight);

            IncrementPosition(_extraHeight);
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="PrecisionSliderField(GUIContent, float, float, float, float, ref bool, GUILayoutOption[])"/>
        public static float PrecisionSliderField(float _value, float _minValue, float _maxValue, float _precision, ref bool _foldout, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return PrecisionSliderField(_label, _value, _minValue, _maxValue, _precision, ref _foldout, _options);
        }

        /// <inheritdoc cref="PrecisionSliderField(GUIContent, float, float, float, float, ref bool, GUILayoutOption[])"/>
        public static float PrecisionSliderField(string _label, float _value, float _minValue, float _maxValue, float _precision, ref bool _foldout, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return PrecisionSliderField(_labelGUI, _value, _minValue, _maxValue, _precision, ref _foldout, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.PrecisionSliderField(Rect, GUIContent, float, float, float, float, ref bool, out float)"/>
        public static float PrecisionSliderField(GUIContent _label, float _value, float _minValue, float _maxValue, float _precision, ref bool _foldout, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            _value = EnhancedEditorGUI.PrecisionSliderField(_position, _label, _value, _minValue, _maxValue, _precision, ref _foldout, out float _extraHeight);

            IncrementPosition(_extraHeight);
            return _value;
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="PrecisionSliderField(GUIContent, int, int, int, int, ref bool, GUILayoutOption[])"/>
        public static int PrecisionSliderField(int _value, int _minValue, int _maxValue, int _precision, ref bool _foldout, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return PrecisionSliderField(_label, _value, _minValue, _maxValue, _precision, ref _foldout, _options);
        }

        /// <inheritdoc cref="PrecisionSliderField(GUIContent, int, int, int, int, ref bool, GUILayoutOption[])"/>
        public static int PrecisionSliderField(string _label, int _value, int _minValue, int _maxValue, int _precision, ref bool _foldout, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return PrecisionSliderField(_labelGUI, _value, _minValue, _maxValue, _precision, ref _foldout, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.PrecisionSliderField(Rect, GUIContent, int, int, int, int, ref bool, out float)"/>
        public static int PrecisionSliderField(GUIContent _label, int _value, int _minValue, int _maxValue, int _precision, ref bool _foldout, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            _value = EnhancedEditorGUI.PrecisionSliderField(_position, _label, _value, _minValue, _maxValue, _precision, ref _foldout, out float _extraHeight);

            IncrementPosition(_extraHeight);
            return _value;
        }
        #endregion

        #region Progress Bar
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, MemberValue{float}, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, MemberValue<float> _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_property, _label, _maxValue, _color, _options);
        }

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, MemberValue{float}, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, MemberValue<float> _maxValue, Color _color,
                                       bool _isEditable = false, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_property, _label, _maxValue, _color, _isEditable, _options);
        }

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, MemberValue{float}, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, string _label, MemberValue<float> _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ProgressBar(_property, _labelGUI, _maxValue, _color, _options);
        }

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, MemberValue{float}, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, string _label, MemberValue<float> _maxValue, Color _color,
                                       bool _isEditable = false, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ProgressBar(_property, _labelGUI, _maxValue, _color, _isEditable, _options);
        }

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, MemberValue{float}, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, GUIContent _label, MemberValue<float> _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            bool _isEditable = false;
            ProgressBar(_property, _label, _maxValue, _color, _isEditable, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ProgressBar(Rect, SerializedProperty, GUIContent, MemberValue{float}, Color, bool)"/>
        public static void ProgressBar(SerializedProperty _property, GUIContent _label, MemberValue<float> _maxValue, Color _color,
                                       bool _isEditable = false, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.ProgressBar(_position, _property, _label, _maxValue, _color, _isEditable);
        }

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, float, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, float _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_property, _label, _maxValue, _color, _options);
        }

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, float, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, float _maxValue, Color _color,
                                       bool _isEditable = false, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_property, _label, _maxValue, _color, _isEditable, _options);
        }

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, float, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, string _label, float _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ProgressBar(_property, _labelGUI, _maxValue, _color, _options);
        }

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, float, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, string _label, float _maxValue, Color _color,
                                       bool _isEditable = false, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ProgressBar(_property, _labelGUI, _maxValue, _color, _isEditable, _options);
        }

        /// <inheritdoc cref="ProgressBar(SerializedProperty, GUIContent, float, Color, bool, GUILayoutOption[])"/>
        public static void ProgressBar(SerializedProperty _property, GUIContent _label, float _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            bool _isEditable = false;
            ProgressBar(_property, _label, _maxValue, _color, _isEditable, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ProgressBar(Rect, SerializedProperty, GUIContent, float, Color, bool)"/>
        public static void ProgressBar(SerializedProperty _property, GUIContent _label, float _maxValue, Color _color,
                                       bool _isEditable = false, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.ProgressBar(_position, _property, _label, _maxValue, _color, _isEditable);
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="ProgressBar(GUIContent, float, float, Color, bool, GUILayoutOption[])"/>
        public static float ProgressBar(float _value, float _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return ProgressBar(_label, _value, _maxValue, _color, _options);
        }

        /// <inheritdoc cref="ProgressBar(GUIContent, float, float, Color, bool, GUILayoutOption[])"/>
        public static float ProgressBar(float _value, float _maxValue, Color _color, bool _isEditable = false, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return ProgressBar(_label, _value, _maxValue, _color, _isEditable, _options);
        }

        /// <inheritdoc cref="ProgressBar(GUIContent, float, float, Color, bool, GUILayoutOption[])"/>
        public static float ProgressBar(string _label, float _value, float _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return ProgressBar(_labelGUI, _value, _maxValue, _color, _options);
        }

        /// <inheritdoc cref="ProgressBar(GUIContent, float, float, Color, bool, GUILayoutOption[])"/>
        public static float ProgressBar(string _label, float _value, float _maxValue, Color _color, bool _isEditable = false, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return ProgressBar(_labelGUI, _value, _maxValue, _color, _isEditable, _options);
        }

        /// <inheritdoc cref="ProgressBar(GUIContent, float, float, Color, bool, GUILayoutOption[])"/>
        public static float ProgressBar(GUIContent _label, float _value, float _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            bool _isEditable = false;
            return ProgressBar(_label, _value, _maxValue, _color, _isEditable, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ProgressBar(Rect, GUIContent, float, float, Color, bool)"/>
        public static float ProgressBar(GUIContent _label, float _value, float _maxValue, Color _color, bool _isEditable = false, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.ProgressBar(_position, _label, _value, _maxValue, _color, _isEditable);
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="ProgressBar(GUIContent, int, int, Color, bool, GUILayoutOption[])"/>
        public static int ProgressBar(int _value, int _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return ProgressBar(_label, _value, _maxValue, _color, _options);
        }

        /// <inheritdoc cref="ProgressBar(GUIContent, int, int, Color, bool, GUILayoutOption[])"/>
        public static int ProgressBar(int _value, int _maxValue, Color _color, bool _isEditable = false, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return ProgressBar(_label, _value, _maxValue, _color, _isEditable, _options);
        }

        /// <inheritdoc cref="ProgressBar(GUIContent, int, int, Color, bool, GUILayoutOption[])"/>
        public static int ProgressBar(string _label, int _value, int _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return ProgressBar(_labelGUI, _value, _maxValue, _color, _options);
        }

        /// <inheritdoc cref="ProgressBar(GUIContent, int, int, Color, bool, GUILayoutOption[])"/>
        public static int ProgressBar(string _label, int _value, int _maxValue, Color _color, bool _isEditable = false, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return ProgressBar(_labelGUI, _value, _maxValue, _color, _isEditable, _options);
        }

        /// <inheritdoc cref="ProgressBar(GUIContent, int, int, Color, bool, GUILayoutOption[])"/>
        public static int ProgressBar(GUIContent _label, int _value, int _maxValue, Color _color, params GUILayoutOption[] _options)
        {
            bool _isEditable = false;
            return ProgressBar(_label, _value, _maxValue, _color, _isEditable, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ProgressBar(Rect, GUIContent, int, int, Color, bool)"/>
        public static int ProgressBar(GUIContent _label, int _value, int _maxValue, Color _color, bool _isEditable = false, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.ProgressBar(_position, _label, _value, _maxValue, _color, _isEditable);
        }
        #endregion

        #region Readonly
        /// <inheritdoc cref="ReadonlyField(SerializedProperty, GUIContent, bool, bool, GUILayoutOption[])"/>
        public static void ReadonlyField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ReadonlyField(_property, _label, _options);
        }

        /// <inheritdoc cref="ReadonlyField(SerializedProperty, GUIContent, bool, bool, GUILayoutOption[])"/>
        public static void ReadonlyField(SerializedProperty _property, bool _includeChildren, params GUILayoutOption[] _options)
        {
            bool _useRadioToggle = false;
            ReadonlyField(_property, _includeChildren, _useRadioToggle, _options);
        }

        /// <inheritdoc cref="ReadonlyField(SerializedProperty, GUIContent, bool, bool, GUILayoutOption[])"/>
        public static void ReadonlyField(SerializedProperty _property, bool _includeChildren, bool _useRadioToggle = false, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ReadonlyField(_property, _label, _includeChildren, _useRadioToggle, _options);
        }

        /// <inheritdoc cref="ReadonlyField(SerializedProperty, GUIContent, bool, bool, GUILayoutOption[])"/>
        public static void ReadonlyField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ReadonlyField(_property, _labelGUI, _options);
        }

        /// <inheritdoc cref="ReadonlyField(SerializedProperty, GUIContent, bool, bool, GUILayoutOption[])"/>
        public static void ReadonlyField(SerializedProperty _property, string _label, bool _includeChildren, params GUILayoutOption[] _options)
        {
            bool _useRadioToggle = false;
            ReadonlyField(_property, _label, _includeChildren, _useRadioToggle, _options);
        }

        /// <inheritdoc cref="ReadonlyField(SerializedProperty, GUIContent, bool, bool, GUILayoutOption[])"/>
        public static void ReadonlyField(SerializedProperty _property, string _label, bool _includeChildren, bool _useRadioToggle = false, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ReadonlyField(_property, _labelGUI, _includeChildren, _useRadioToggle, _options);
        }

        /// <inheritdoc cref="ReadonlyField(SerializedProperty, GUIContent, bool, bool, GUILayoutOption[])"/>
        public static void ReadonlyField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            bool _includeChildren = false;
            ReadonlyField(_property, _label, _includeChildren, _options);
        }

        /// <inheritdoc cref="ReadonlyField(SerializedProperty, GUIContent, bool, bool, GUILayoutOption[])"/>
        public static void ReadonlyField(SerializedProperty _property, GUIContent _label, bool _includeChildren, params GUILayoutOption[] _options)
        {
            bool _useRadioToggle = false;
            ReadonlyField(_property, _label, _includeChildren, _useRadioToggle, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ReadonlyField(Rect, SerializedProperty, GUIContent, bool, bool)"/>
        public static void ReadonlyField(SerializedProperty _property, GUIContent _label, bool _includeChildren, bool _useRadioToggle = false, params GUILayoutOption[] _options)
        {
            Rect _position = _includeChildren
                            ? GetPosition(true, EditorGUI.GetPropertyHeight(_property, true), _options)
                            : GetPosition(_options);

            EnhancedEditorGUI.ReadonlyField(_position, _property, _label, _includeChildren, _useRadioToggle);
        }
        #endregion

        #region Required
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="RequiredField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void RequiredField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            RequiredField(_property, _label, _options);
        }

        /// <inheritdoc cref="RequiredField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void RequiredField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            RequiredField(_property, _labelGUI, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.RequiredField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void RequiredField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.RequiredField(_position, _property, _label, out float _extraHeight);

            IncrementPosition(_extraHeight);
        }

        // ===== Object Value ===== \\

        /// <inheritdoc cref="RequiredField(GUIContent, Object, Type, bool, GUILayoutOption[])"/>
        public static Object RequiredField(Object _object, Type _objectType, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return RequiredField(_object, _objectType, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="RequiredField(GUIContent, Object, Type, bool, GUILayoutOption[])"/>
        public static Object RequiredField(Object _object, Type _objectType, bool _allowSceneObjects, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return RequiredField(_label, _object, _objectType, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="RequiredField(GUIContent, Object, Type, bool, GUILayoutOption[])"/>
        public static Object RequiredField(string _label, Object _object, Type _objectType, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return RequiredField(_label, _object, _objectType, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="RequiredField(GUIContent, Object, Type, bool, GUILayoutOption[])"/>
        public static Object RequiredField(string _label, Object _object, Type _objectType, bool _allowSceneObjects, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return RequiredField(_labelGUI, _object, _objectType, _allowSceneObjects, _options);
        }

        /// <inheritdoc cref="RequiredField(GUIContent, Object, Type, bool, GUILayoutOption[])"/>
        public static Object RequiredField(GUIContent _label, Object _object, Type _objectType, params GUILayoutOption[] _options)
        {
            bool _allowSceneObjects = true;
            return RequiredField(_label, _object, _objectType, _allowSceneObjects, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.RequiredField(Rect, GUIContent, Object, Type, bool, out float)"/>
        public static Object RequiredField(GUIContent _label, Object _object, Type _objectType, bool _allowSceneObjects, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            _object = EnhancedEditorGUI.RequiredField(_position, _label, _object, _objectType, _allowSceneObjects, out float _extraHeight);

            IncrementPosition(_extraHeight);
            return _object;
        }
        #endregion

        #region Scene Asset
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="SceneAssetField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void SceneAssetField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            SceneAssetField(_property, _label, _options);
        }

        /// <inheritdoc cref="SceneAssetField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void SceneAssetField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            SceneAssetField(_property, _labelGUI, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.SceneAssetField(Rect, SerializedProperty, GUIContent)"/>
        public static void SceneAssetField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.SceneAssetField(_position, _property, _label);
        }

        // ===== Scene Asset Value ===== \\

        /// <inheritdoc cref="SceneAssetField(GUIContent, SceneAsset, GUILayoutOption[])"/>
        public static void SceneAssetField(SceneAsset _sceneAsset, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            SceneAssetField(_label, _sceneAsset, _options);
        }

        /// <inheritdoc cref="SceneAssetField(GUIContent, SceneAsset, GUILayoutOption[])"/>
        public static void SceneAssetField(string _label, SceneAsset _sceneAsset, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            SceneAssetField(_labelGUI, _sceneAsset, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.SceneAssetField(Rect, GUIContent, SceneAsset)"/>
        public static void SceneAssetField(GUIContent _label, SceneAsset _sceneAsset, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.SceneAssetField(_position, _label, _sceneAsset);
        }
        #endregion

        #region Text Area
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="TextArea(SerializedProperty, GUIContent, bool, GUILayoutOption[])"/>
        public static void TextArea(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            TextArea(_property, _label, _options);
        }

        /// <inheritdoc cref="TextArea(SerializedProperty, GUIContent, bool, GUILayoutOption[])"/>
        public static void TextArea(SerializedProperty _property, bool _isWide, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            TextArea(_property, _label, _isWide, _options);
        }

        /// <inheritdoc cref="TextArea(SerializedProperty, GUIContent, bool, GUILayoutOption[])"/>
        public static void TextArea(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TextArea(_property, _labelGUI, _options);
        }

        /// <inheritdoc cref="TextArea(SerializedProperty, GUIContent, bool, GUILayoutOption[])"/>
        public static void TextArea(SerializedProperty _property, string _label, bool _isWide, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TextArea(_property, _labelGUI, _isWide, _options);
        }

        /// <inheritdoc cref="TextArea(SerializedProperty, GUIContent, bool, GUILayoutOption[])"/>
        public static void TextArea(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            bool _isWide = false;
            TextArea(_property, _label, _isWide, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.TextArea(Rect, SerializedProperty, GUIContent, bool, out float)"/>
        public static void TextArea(SerializedProperty _property, GUIContent _label, bool _isWide, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(true, 0f, _options);
            EnhancedEditorGUI.TextArea(_position, _property, _label, _isWide, out float _totalHeight);

            IncrementPosition(_totalHeight);
        }

        // ===== String Value ===== \\

        /// <inheritdoc cref="TextArea(GUIContent, string, bool, GUILayoutOption[])"/>
        public static string TextArea(string _text, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return TextArea(_label, _text, _options);
        }

        /// <inheritdoc cref="TextArea(GUIContent, string, bool, GUILayoutOption[])"/>
        public static string TextArea(string _text, bool _isWide, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return TextArea(_label, _text, _isWide, _options);
        }

        /// <inheritdoc cref="TextArea(GUIContent, string, bool, GUILayoutOption[])"/>
        public static string TextArea(string _label, string _text, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return TextArea(_labelGUI, _text, _options);
        }

        /// <inheritdoc cref="TextArea(GUIContent, string, bool, GUILayoutOption[])"/>
        public static string TextArea(string _label, string _text, bool _isWide, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return TextArea(_labelGUI, _text, _isWide, _options);
        }

        /// <inheritdoc cref="TextArea(GUIContent, string, bool, GUILayoutOption[])"/>
        public static string TextArea(GUIContent _label, string _text, params GUILayoutOption[] _options)
        {
            bool _isWide = false;
            return TextArea(_label, _text, _isWide, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.TextArea(Rect, GUIContent, string, bool, out float)"/>
        public static string TextArea(GUIContent _label, string _text, bool _isWide, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(true, 0f, _options);
            _text = EnhancedEditorGUI.TextArea(_position, _label, _text, _isWide, out float _totalHeight);

            IncrementPosition(_totalHeight);
            return _text;
        }
        #endregion

        #region Validation Property
        /// <inheritdoc cref="ValidationMemberField(SerializedProperty, GUIContent, MemberValue{object}, GUILayoutOption[])"/>
        public static void ValidationMemberField(SerializedProperty _property, MemberValue<object> _validationMember, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ValidationMemberField(_property, _label, _validationMember, _options);
        }

        /// <inheritdoc cref="ValidationMemberField(SerializedProperty, GUIContent, MemberValue{object}, GUILayoutOption[])"/>
        public static void ValidationMemberField(SerializedProperty _property, string _label, MemberValue<object> _validationMember, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ValidationMemberField(_property, _labelGUI, _validationMember, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ValidationMemberField(Rect, SerializedProperty, GUIContent, MemberValue{object})"/>
        public static void ValidationMemberField(SerializedProperty _property, GUIContent _label, MemberValue<object> _validationMember, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.ValidationMemberField(_position, _property, _label, _validationMember);
        }
        #endregion

        // --- Multi-Tags --- \\

        #region Tag
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="TagField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void TagField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            TagField(_property, _label, _options);
        }

        /// <inheritdoc cref="TagField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void TagField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TagField(_property, _labelGUI, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.TagField(Rect, SerializedProperty, GUIContent)"/>
        public static void TagField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.TagField(_position, _property, _label);
        }

        // ===== Tag ===== \\

        /// <inheritdoc cref="TagField(GUIContent, Tag, GUILayoutOption[])"/>
        public static Tag TagField(Tag _tag, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            return TagField(_label, _tag, _options);
        }

        /// <inheritdoc cref="TagField(GUIContent, Tag, GUILayoutOption[])"/>
        public static Tag TagField(string _label, Tag _tag, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return TagField(_labelGUI, _tag, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.TagField(Rect, GUIContent, Tag)"/>
        public static Tag TagField(GUIContent _label, Tag _tag, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            return EnhancedEditorGUI.TagField(_position, _label, _tag);
        }
        #endregion

        #region Tag Group
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="TagGroupField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void TagGroupField(SerializedProperty _property, params GUILayoutOption[] _options)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            TagGroupField(_property, _label, _options);
        }

        /// <inheritdoc cref="TagGroupField(SerializedProperty, GUIContent, GUILayoutOption[])"/>
        public static void TagGroupField(SerializedProperty _property, string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TagGroupField(_property, _labelGUI, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.TagGroupField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void TagGroupField(SerializedProperty _property, GUIContent _label, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.TagGroupField(_position, _property, _label, out float _extraHeight);

            IncrementPosition(_extraHeight);
        }

        // ===== Tag Group ===== \\

        /// <inheritdoc cref="TagGroupField(GUIContent, TagGroup, GUILayoutOption[])"/>
        public static void TagGroupField(TagGroup _group, params GUILayoutOption[] _options)
        {
            GUIContent _label = GUIContent.none;
            TagGroupField(_label, _group, _options);
        }

        /// <inheritdoc cref="TagGroupField(GUIContent, TagGroup, GUILayoutOption[])"/>
        public static void TagGroupField(string _label, TagGroup _group, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TagGroupField(_labelGUI, _group, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.TagGroupField(Rect, GUIContent, TagGroup, out float)"/>
        public static void TagGroupField(GUIContent _label, TagGroup _group, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(_options);
            EnhancedEditorGUI.TagGroupField(_position, _label, _group, out float _extraHeight);

            IncrementPosition(_extraHeight);
        }
        #endregion

        // --- Various GUI Controls --- \\

        #region Link Label
        /// <inheritdoc cref="LinkLabel(GUIContent, string, GUILayoutOption[])"/>
        public static void LinkLabel(string _label, string _url, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            LinkLabel(_labelGUI, _url, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.LinkLabel(Rect, GUIContent, string)"/>
        public static void LinkLabel(GUIContent _label, string _url, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(true, EnhancedEditorStyles.LinkLabel, _options);
            EnhancedEditorGUI.LinkLabel(_position, _label, _url);
        }
        #endregion

        #region Underlined Label
        /// <inheritdoc cref="UnderlinedLabel(GUIContent, Color, GUIStyle, GUILayoutOption[])"/>
        public static void UnderlinedLabel(string _label, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            UnderlinedLabel(_labelGUI, _options);
        }

        /// <inheritdoc cref="UnderlinedLabel(GUIContent, Color, GUIStyle, GUILayoutOption[])"/>
        public static void UnderlinedLabel(GUIContent _label, params GUILayoutOption[] _options)
        {
            GUIStyle _style = EditorStyles.label;
            UnderlinedLabel(_label, _style, _options);
        }

        /// <inheritdoc cref="UnderlinedLabel(GUIContent, Color, GUIStyle, GUILayoutOption[])"/>
        public static void UnderlinedLabel(string _label, Color _color, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            UnderlinedLabel(_labelGUI, _color, _options);
        }

        /// <inheritdoc cref="UnderlinedLabel(GUIContent, Color, GUIStyle, GUILayoutOption[])"/>
        public static void UnderlinedLabel(GUIContent _label, Color _color, params GUILayoutOption[] _options)
        {
            GUIStyle _style = EditorStyles.label;
            UnderlinedLabel(_label, _color, _style, _options);
        }

        /// <inheritdoc cref="UnderlinedLabel(GUIContent, Color, GUIStyle, GUILayoutOption[])"/>
        public static void UnderlinedLabel(string _label, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            UnderlinedLabel(_labelGUI, _style, _options);
        }

        /// <inheritdoc cref="UnderlinedLabel(GUIContent, Color, GUIStyle, GUILayoutOption[])"/>
        public static void UnderlinedLabel(GUIContent _label, GUIStyle _style, params GUILayoutOption[] _options)
        {
            Color _color = Color.white;
            UnderlinedLabel(_label, _color, _style, _options);
        }

        /// <inheritdoc cref="UnderlinedLabel(GUIContent, Color, GUIStyle, GUILayoutOption[])"/>
        public static void UnderlinedLabel(string _label, Color _color, GUIStyle _style, params GUILayoutOption[] _options)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            UnderlinedLabel(_labelGUI, _color, _style, _options);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.UnderlinedLabel(Rect, GUIContent, Color, GUIStyle)"/>
        public static void UnderlinedLabel(GUIContent _label, Color _color, GUIStyle _style, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(true, _style, _options);
            EnhancedEditorGUI.UnderlinedLabel(_position, _label, _color, _style);
        }
        #endregion

        #region Texture
        /// <summary>
        /// Draws a texture, with its height automatically adjusted to the width available on screen.
        /// </summary>
        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <returns></returns>
        /// <inheritdoc cref="EnhancedEditorGUI.Texture(Rect, Texture2D)"/>
        public static void Texture(Texture2D _texture, params GUILayoutOption[] _options)
        {
            Rect _position = GetPosition(false, 0f, _options);
            float _height = EnhancedEditorGUI.Texture(_position, _texture);

            if (_height == 0f)
            {
                _height = -EditorGUIUtility.standardVerticalSpacing;
            }

            IncrementPosition(_height);
        }
        #endregion

        #region Toolbar Search Field
        /// <inheritdoc cref="ToolbarSearchField(string, string, GUILayoutOption[])"/>
        public static string ToolbarSearchField(string _searchFilter, params GUILayoutOption[] _options)
        {
            Rect _position = EditorGUILayout.GetControlRect(_options);
            return EnhancedEditorGUI.ToolbarSearchField(_position, _searchFilter);
        }

        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ToolbarSearchField(string, Rect, string)"/>
        public static string ToolbarSearchField(string _controlName, string _searchFilter, params GUILayoutOption[] _options)
        {
            Rect _position = EditorGUILayout.GetControlRect(_options);
            return EnhancedEditorGUI.ToolbarSearchField(_controlName, _position, _searchFilter);
        }
        #endregion

        #region Toolbar Sort Options
        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <inheritdoc cref="EnhancedEditorGUI.ToolbarSortOptions(Rect, ref int, ref bool, GUIContent[])"/>
        public static void ToolbarSortOptions(ref int _selectedOption, ref bool _doSortAscending, GUIContent[] _sortingOptions, params GUILayoutOption[] _options)
        {
            Rect _position = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbar, _options);
            EnhancedEditorGUI.ToolbarSortOptions(_position, ref _selectedOption, ref _doSortAscending, _sortingOptions);
        }
        #endregion

        // --- Internal GUI Controls --- \\

        #region Centered Controls
        /// <summary>
        /// Makes a centered generic popup selection field.
        /// </summary>
        /// <param name="_selectedIndex">The index of the selected option.</param>
        /// <param name="_displayedOptions">An array of text, image and tooltips for the displayed options.</param>
        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <returns>The index of the option that has been selected by the user</returns>
        internal static int CenteredPopup(int _selectedIndex, GUIContent[] _displayedOptions, params GUILayoutOption[] _options)
        {
            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _displayedOptions, EnhancedEditorStyles.CenteredPopup, _options);
                GUILayout.FlexibleSpace();
            }

            return _selectedIndex;
        }

        /// <summary>
        /// Makes a centered toolbar.
        /// </summary>
        /// <param name="_selectedIndex">The index of the selected button.</param>
        /// <param name="_buttons">An array of text, image and tooltips for the buttons.</param>
        /// <param name="_options"><inheritdoc cref="DocumentationMethod(GUILayoutOption[])" path="/param[@name='_options']"/></param>
        /// <returns>The index of the button that has been selected by the user</returns>
        internal static int CenteredToolbar(int _selectedIndex, GUIContent[] _buttons, params GUILayoutOption[] _options)
        {
            using (var _scope = new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                _selectedIndex = GUILayout.Toolbar(_selectedIndex, _buttons, EnhancedEditorStyles.Button, GUI.ToolbarButtonSize.FitToContents, _options);
                GUILayout.FlexibleSpace();
            }

            return _selectedIndex;
        }
        #endregion

        // --- Utility --- \\

        #region Internal Utility
        internal static void ManageDynamicGUIControlHeight(GUIContent _label, float _height)
        {
            // Get saved control height and increment position.
            _height = EnhancedEditorGUI.ManageDynamicControlHeight(_label, _height);
            IncrementPosition(_height);
        }

        private static void IncrementPosition(float _extraHeight)
        {
            GetPosition(false, _extraHeight - EditorGUIUtility.standardVerticalSpacing);
        }

        private static Rect GetPosition(params GUILayoutOption[] _options)
        {
            bool _hasLabel = true;
            return GetPosition(_hasLabel, _options);
        }

        private static Rect GetPosition(bool _hasLabel, params GUILayoutOption[] _options)
        {
            float _height = EditorGUIUtility.singleLineHeight;
            return GetPosition(_hasLabel, _height, _options);
        }

        private static Rect GetPosition(bool _hasLabel, GUIStyle _style, params GUILayoutOption[] _options)
        {
            float _height = _style.lineHeight;
            return GetPosition(_hasLabel, _height, _options);
        }

        private static Rect GetPosition(bool _hasLabel, float _height, params GUILayoutOption[] _options)
        {
            Rect _position = EditorGUILayout.GetControlRect(_hasLabel, _height, _options);
            return _position;
        }
        #endregion

        #region Documentation
        /// <summary>
        /// This method is for documentation only, used by inheriting its parameters documentation to centralize it in one place.
        /// </summary>
        /// <param name="_options">An optional list of layout options that specify extra layout properties.
        /// Any value passed in here will override settings defined by the style.</param>
        internal static void DocumentationMethod(params GUILayoutOption[] _options) { }
        #endregion
    }
}
