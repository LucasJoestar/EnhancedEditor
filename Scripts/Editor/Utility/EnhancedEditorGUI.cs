// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Contains multiple editor-related GUI methods and variables.
    /// </summary>
    [InitializeOnLoad]
    public static class EnhancedEditorGUI
    {
        #region GUI Buffers
        /// <summary>
        /// <see cref="Handles.color"/> buffer system. Use this to dynamically push / pop Handles colors.
        /// </summary>
        public static readonly GUIBuffer<Color> HandlesColor = new GUIBuffer<Color>(() => Handles.color,
                                                                                    (c) => Handles.color = c, "Handles Color");

        /// <summary>
        /// <see cref="EditorGUIUtility.labelWidth"/> buffer system. Use this to dynamically push / pop width for GUI labels.
        /// </summary>
        public static readonly GUIBuffer<float> GUILabelWidth = new GUIBuffer<float>(() => EditorGUIUtility.labelWidth,
                                                                                     (w) => EditorGUIUtility.labelWidth = w, "GUI Label Width");
        #endregion

        #region Initialization
        static EnhancedEditorGUI()
        {
            EditorApplication.contextualPropertyMenu -= OnContextualPropertyMenu;
            EditorApplication.contextualPropertyMenu += OnContextualPropertyMenu;
        }
        #endregion

        #region Context Menu
        private static readonly string SerializedInterfaceTypeName = typeof(SerializedInterface).Name;

        private static readonly GUIContent SortTagsGUI = new GUIContent("Sort Tags by their Name");

        // -----------------------

        private static void OnContextualPropertyMenu(GenericMenu _menu, SerializedProperty _property)
        {
            // TagGroup option: sort tags by their name.
            if ((_property.type == TagGroupTypeName) && !_property.hasMultipleDifferentValues)
            {
                _menu.AddItem(SortTagsGUI, false, () =>
                {
                    if (EnhancedEditorUtility.FindSerializedObjectField(_property.serializedObject, _property.propertyPath, out FieldInfo _field))
                    {
                        // Get the direct tag group reference and sort its tags.
                        TagGroup _group = _field.GetValue(_property.serializedObject.targetObject) as TagGroup;
                        _group.SortTagsByName();

                        // In order for the modifications to be properly registered, update all tags id through their respective serialized property.
                        // Otherwise, the property state is confused and its data may be disrupted when modifying or saving it.
                        SerializedProperty _groupProperty = _property.Copy();
                        _groupProperty.Next(true);

                        for (int _i = 0; _i < _groupProperty.arraySize; _i++)
                        {
                            SerializedProperty _tagProperty = _groupProperty.GetArrayElementAtIndex(_i);
                            _tagProperty.Next(true);

                            _tagProperty.longValue = _group.Tags[_i].ID;
                        }

                        _groupProperty.serializedObject.ApplyModifiedProperties();
                    }
                });
            }

            // SerializedInterface option: get interface reference from object.
            if (!_property.serializedObject.isEditingMultipleObjects && _property.type.StartsWith(SerializedInterfaceTypeName)
                && (_property.serializedObject.targetObject is Component _component))
            {
                _menu.AddItem(requiredGetReferenceGUI, false, () =>
                {
                    if (EnhancedEditorUtility.FindSerializedObjectField(_property.serializedObject, _property.propertyPath, out FieldInfo _field))
                    {
                        Type _type = EnhancedEditorUtility.GetFieldInfoType(_field);

                        if (_component.TryGetComponent(_type, out Component _interface))
                        {
                            SerializedProperty _gameObjectProperty = _property.FindPropertyRelative("gameObject");

                            _gameObjectProperty.objectReferenceValue = _interface.gameObject;
                            _gameObjectProperty.serializedObject.ApplyModifiedProperties();
                        }
                    }
                });
            }

            // Enhanced property drawers context menu.
            if (EnhancedPropertyEditor.propertyInfos.ContainsKey(_property.propertyPath))
            {
                var _infos = EnhancedPropertyEditor.propertyInfos[_property.propertyPath];
                _infos.OnContextMenu(_menu);
            }
        }
        #endregion

        // --- Decorator Drawers --- \\

        #region Horizontal Line
        /// <summary>
        /// Draws a horizontal line on screen, with specific margins.
        /// </summary>
        /// <param name="_margins">Margins on both sides of the line (in pixels).</param>
        /// <inheritdoc cref="HorizontalLine(Rect, Color)"/>
        public static void HorizontalLine(Rect _position, Color _color, float _margins)
        {
            _position.xMin += _margins;
            _position.xMax -= _margins;

            HorizontalLine(_position, _color);
        }

        /// <summary>
        /// Draws a horizontal line on screen. This is pretty much equivalent to <see cref="EditorGUI.DrawRect(Rect, Color)"/>.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_color">Line color.</param>
        public static void HorizontalLine(Rect _position, Color _color)
        {
            _position = EditorGUI.IndentedRect(_position);
            EditorGUI.DrawRect(_position, _color);
        }
        #endregion

        #region Section
        /// <inheritdoc cref="Section(Rect, GUIContent, float)"/>
        public static void Section(Rect _position, string _label, float _lineWidth = EnhancedEditorGUIUtility.SectionDefaultLineWidth)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            Section(_position, _labelGUI, _lineWidth);
        }

        /// <summary>
        /// Draws a section, a header-like label surrounded by horizontal lines. Use this to decorate your GUI.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_label">Header label.</param>
        /// <param name="_lineWidth">Width of the lines surrounding the label (in pixels).</param>
        public static void Section(Rect _position, GUIContent _label, float _lineWidth = EnhancedEditorGUIUtility.SectionDefaultLineWidth)
        {
            GUIStyle _style = EnhancedEditorStyles.BoldCenteredLabel;
            Vector2 _labelSize = _style.CalcSize(_label);
            float _totalWidth = Mathf.Min(_position.width, _labelSize.x + (_lineWidth * 2f) + (EnhancedEditorGUIUtility.SectionLabelMargins * 2f));

            // Draws the horizontal lines surrounding the label (if there is enough space).
            _lineWidth = ((_totalWidth - _labelSize.x) / 2f) - EnhancedEditorGUIUtility.SectionLabelMargins;

            if (_lineWidth > 0f)
            {
                float _verticalSpacing = Mathf.Max(0f, (_position.height - _labelSize.y) / 2f);
                Color _color = _style.normal.textColor;
                Rect _temp = new Rect()
                {
                    x = _position.x + ((_position.width - _totalWidth) / 2f),
                    y = _position.y + _verticalSpacing + (_labelSize.y / 2f),
                    width = _lineWidth,
                    height = 2f
                };

                EditorGUI.DrawRect(_temp, _color);

                _temp.x += _totalWidth - _lineWidth;
                EditorGUI.DrawRect(_temp, _color);
            }

            // Label.
            using (var _scope = ZeroIndentScope())
            {
                EditorGUI.LabelField(_position, _label, _style);
            }
        }
        #endregion

        // --- Property Drawers --- \\

        #region Asset Preview
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="AssetPreviewField(Rect, SerializedProperty, GUIContent, out float, float)"/>
        public static void AssetPreviewField(Rect _position, SerializedProperty _property, out float _extraHeight,
                                             float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            AssetPreviewField(_position, _property, _label, out _extraHeight, _previewSize);
        }

        /// <inheritdoc cref="AssetPreviewField(Rect, SerializedProperty, GUIContent, out float, float)"/>
        public static void AssetPreviewField(Rect _position, SerializedProperty _property, string _label, out float _extraHeight,
                                             float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            AssetPreviewField(_position, _property, _labelGUI, out _extraHeight, _previewSize);
        }

        /// <summary>
        /// Makes a property field with an unfoldable preview of its object reference below.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to draw field and display reference object preview.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_extraHeight"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_extraHeight']"/></param>
        /// <param name="_previewSize">Size of the asset preview (in pixels).</param>
        public static void AssetPreviewField(Rect _position, SerializedProperty _property, GUIContent _label, out float _extraHeight,
                                             float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize)
        {
            // Multiple different values and incompatible property management.
            if (_property.hasMultipleDifferentValues || (_property.propertyType != SerializedPropertyType.ObjectReference))
            {
                EditorGUI.PropertyField(_position, _property, _label);
                _extraHeight = 0f;

                return;
            }

            // Rect height calculs.
            Rect _temp = new Rect(_position);
            bool _foldout = _property.isExpanded;

            _extraHeight = GetAssetPreviewExtraHeight(_foldout, _previewSize);

            _position.y += _position.height;
            _position.height = _extraHeight;

            // Asset preview field.
            using (var _scope = new EditorGUI.PropertyScope(_position, GUIContent.none, _property))
            {
                _temp = DoAssetPreviewField(_temp, _property.objectReferenceValue, ref _foldout, _previewSize);
                EditorGUI.PropertyField(_temp, _property, _label);

                // Save foldout state.
                if (_foldout != _property.isExpanded)
                {
                    _property.isExpanded = _foldout;
                }
            }
        }

        // ===== Object Value ===== \\

        /// <inheritdoc cref="AssetPreviewField(Rect, GUIContent, Object, Type, bool, ref bool, out float, float)"/>
        public static Object AssetPreviewField(Rect _position, Object _object, Type _objectType, ref bool _foldout, out float _extraHeight,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize)
        {
            bool _allowSceneObjects = true;
            return AssetPreviewField(_position, _object, _objectType, _allowSceneObjects, ref _foldout, out _extraHeight, _previewSize);
        }

        /// <inheritdoc cref="AssetPreviewField(Rect, GUIContent, Object, Type, bool, ref bool, out float, float)"/>
        public static Object AssetPreviewField(Rect _position, Object _object, Type _objectType, bool _allowSceneObjects, ref bool _foldout, out float _extraHeight,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize)
        {
            GUIContent _label = GUIContent.none;
            return AssetPreviewField(_position, _label, _object, _objectType, _allowSceneObjects, ref _foldout, out _extraHeight, _previewSize);
        }

        /// <inheritdoc cref="AssetPreviewField(Rect, GUIContent, Object, Type, bool, ref bool, out float, float)"/>
        public static Object AssetPreviewField(Rect _position, string _label, Object _object, Type _objectType, ref bool _foldout, out float _extraHeight,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize)
        {
            bool _allowSceneObjects = true;
            return AssetPreviewField(_position, _label, _object, _objectType, _allowSceneObjects, ref _foldout, out _extraHeight, _previewSize);
        }

        /// <inheritdoc cref="AssetPreviewField(Rect, GUIContent, Object, Type, bool, ref bool, out float, float)"/>
        public static Object AssetPreviewField(Rect _position, string _label, Object _object, Type _objectType, bool _allowSceneObjects, ref bool _foldout, out float _extraHeight,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return AssetPreviewField(_position, _labelGUI, _object, _objectType, _allowSceneObjects, ref _foldout, out _extraHeight, _previewSize);
        }

        /// <inheritdoc cref="AssetPreviewField(Rect, GUIContent, Object, Type, bool, ref bool, out float, float)"/>
        public static Object AssetPreviewField(Rect _position, GUIContent _label, Object _object, Type _objectType, ref bool _foldout, out float _extraHeight,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize)
        {
            bool _allowSceneObjects = true;
            return AssetPreviewField(_position, _label, _object, _objectType, _allowSceneObjects, ref _foldout, out _extraHeight, _previewSize);
        }

        /// <summary>
        /// Makes an object field with an unfoldable preview of its object reference below.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/param[@name='_object']"/></param>
        /// <param name="_objectType"><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/param[@name='_objectType']"/></param>
        /// <param name="_allowSceneObjects"><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/param[@name='_allowSceneObjects']"/></param>
        /// <param name="_foldout"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_foldout']"/></param>
        /// <returns><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/returns"/></returns>
        /// <inheritdoc cref="AssetPreviewField(Rect, SerializedProperty, GUIContent, out float, float)"/>
        public static Object AssetPreviewField(Rect _position, GUIContent _label, Object _object, Type _objectType, bool _allowSceneObjects, ref bool _foldout, out float _extraHeight,
                                               float _previewSize = EnhancedEditorGUIUtility.AssetPreviewDefaultSize)
        {
            _position = DoAssetPreviewField(_position, _object, ref _foldout, _previewSize);
            _object = EditorGUI.ObjectField(_position, _label, _object, _objectType, _allowSceneObjects);

            _extraHeight = GetAssetPreviewExtraHeight(_foldout, _previewSize);
            return _object;
        }

        // -----------------------

        private static Rect DoAssetPreviewField(Rect _position, Object _object, ref bool _foldout, float _previewSize)
        {
            // Foldout.
            Rect _fieldPosition;
            using (var _scope = ZeroIndentScope())
            {
                _fieldPosition = DrawFoldout(_position, ref _foldout);
            }

            // Asset preview.
            if (_foldout)
            {
                _position.Set
                (
                    _fieldPosition.xMax - _previewSize,
                    _position.y + _position.height + EditorGUIUtility.standardVerticalSpacing + 2f,
                    _previewSize,
                    _previewSize
                );

                // Catch preview null ref exception.
                try
                {
                    Texture2D _preview = AssetPreview.GetAssetPreview(_object);
                    if (_preview == null)
                        _preview = Texture2D.blackTexture;

                    EditorGUI.DrawPreviewTexture(_position, _preview);
                }
                catch (NullReferenceException) { }
            }

            return _fieldPosition;
        }

        internal static float GetAssetPreviewExtraHeight(bool _foldout, float _previewSize)
        {
            float _extraHeight = _foldout
                               ? (_previewSize + EditorGUIUtility.standardVerticalSpacing + 4f)
                               : 0f;

            return _extraHeight;
        }
        #endregion

        #region Block
        /// <inheritdoc cref="BlockField(Rect, SerializedProperty, GUIContent)"/>
        public static void BlockField(Rect _position, SerializedProperty _property)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            BlockField(_position, _property, _label);
        }

        /// <inheritdoc cref="BlockField(Rect, SerializedProperty, GUIContent)"/>
        public static void BlockField(Rect _position, SerializedProperty _property, string _label)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            BlockField(_position, _property, _labelGUI);
        }

        /// <summary>
        /// Makes a block field for a property, displaying a struct or a serializable class within a single block and without any foldout.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make a block field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        public static void BlockField(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            // If the property has no children, simply draw it.
            if (!_property.hasVisibleChildren)
            {
                GUIContent _blockLabel = EnhancedEditorGUIUtility.GetLabelGUI(_label.text + "   ");
                _blockLabel.tooltip = _label.tooltip;

                EditorGUI.PropertyField(_position, _property, _blockLabel);
                return;
            }

            // We only want to draw the children fields of this property (fields within the class / struct),
            // so store the next object property to break the chain when getting to it.
            SerializedProperty _current = _property.Copy();
            SerializedProperty _next = _property.Copy();
            _next.NextVisible(false);

            // Property label header.
            using (var _labelScope = EnhancedGUI.GUIStyleAlignment.Scope(EditorStyles.label, TextAnchor.MiddleLeft))
            {
                _position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(_position, _label);
            }

            // Expand the property, as it needs to be to properly get its height.
            if (!_property.isExpanded)
            {
                _property.isExpanded = true;
                EnhancedEditorGUIUtility.Repaint(_property.serializedObject);
            }

            _current.NextVisible(true);

            // Draw each property prefix label left-sided to their value.
            using (var _labelScope = EnhancedGUI.GUIStyleAlignment.Scope(EditorStyles.label, TextAnchor.MiddleRight))
            using (var _indentScope = new EditorGUI.IndentLevelScope(1))
            {
                do
                {
                    // Break when getting outside of this property class / struct.
                    if (SerializedProperty.EqualContents(_current, _next))
                        break;

                    _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
                    _position.height = EditorGUI.GetPropertyHeight(_current, true);

                    _label = EnhancedEditorGUIUtility.GetPropertyLabel(_current);
                    BlockField(_position, _current, _label);
                } while (_current.NextVisible(false));
            }
        }
        #endregion

        #region Color Palette
        private const float PaletteMargins = 3f;
        private const float PaletteColorSize = 20f;
        private const float PaletteMenuButtonWidth = 23f;

        private static readonly GUIContent paletteMenuGUI = new GUIContent(string.Empty, "Show additional options.");
        private static readonly GUIContent paletteNewGUI = new GUIContent("Create New Palette", "Creates a new color palette.");

        private static readonly GUIContent paletteEditColorGUI = new GUIContent("Edit", "Modify this color.");
        private static readonly GUIContent paletteSelectColorGUI = new GUIContent("Select", "Set this field value to this color.");
        private static readonly GUIContent paletteRemoveColorGUI = new GUIContent("Remove from Palette", "Remove this color from the palette.");

        private static readonly GUIContent paletteAddColorGUI = new GUIContent("Add New Color", "Add the current color as a new color to the palette.");
        private static readonly GUIContent paletteSortColorsGUI = new GUIContent("Sort Colors", "Sort all colors in the palette.");
        private static readonly GUIContent paletteRenameGUI = new GUIContent("Rename", "Rename this color palette.");
        private static readonly GUIContent paletteDeleteGUI = new GUIContent("Delete", "Delete this color palette.");
        private static readonly GUIContent paletteSaveGUI = new GUIContent("Save as...", "Save this color palette in the project.");

        private static readonly AutoManagedResource<ColorPaletteDatabase> colorPaletteResource = new AutoManagedResource<ColorPaletteDatabase>(true);
        private static readonly Dictionary<int, Vector2> palettesScroll = new Dictionary<int, Vector2>();

        /// <summary>
        /// Database containing all <see cref="ColorPalette"/> in the project.
        /// </summary>
        public static ColorPaletteDatabase ColorPaletteDatabase => colorPaletteResource.GetResource();

        private static int selectedPaletteColorControlID = -1;
        private static Color selectedPaletteColor = default;

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="ColorPaletteField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void ColorPaletteField(Rect _position, SerializedProperty _property, out float _extraHeight)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ColorPaletteField(_position, _property, _label, out _extraHeight);
        }

        /// <inheritdoc cref="ColorPaletteField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void ColorPaletteField(Rect _position, SerializedProperty _property, string _label, out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ColorPaletteField(_position, _property, _labelGUI, out _extraHeight);
        }

        /// <summary>
        /// Makes a color field with a color palette selection system below,
        /// allowing users to browse and select colors from all registered palettes in the project.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make a color palette field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_extraHeight"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_extraHeight']"/></param>
        public static void ColorPaletteField(Rect _position, SerializedProperty _property, GUIContent _label, out float _extraHeight)
        {
            // Incompatible property management.
            if (_property.propertyType != SerializedPropertyType.Color)
            {
                EditorGUI.PropertyField(_position, _property, _label);
                _extraHeight = 0f;

                return;
            }

            // Get the total height used for this field.
            int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
            Color _color = _property.hasMultipleDifferentValues
                         ? Color.white
                         : _property.colorValue;

            // Position adjustments.
            Rect _temp = new Rect(_position);
            _extraHeight = GetColorPaletteExtraHeight(_id, _position, _label, _property.colorValue);

            _position.y += _temp.height;
            _position.height = _extraHeight;

            // Color palette field.
            using (var _scope = new EditorGUI.PropertyScope(_position, GUIContent.none, _property))
            {
                EditorGUI.PropertyField(_temp, _property, _label);

                _temp.y += _temp.height + EditorGUIUtility.standardVerticalSpacing;
                _temp.height = _extraHeight - EditorGUIUtility.standardVerticalSpacing;

                Color _newColor = DoColorPaletteField(_id, _temp, _label, _color);

                // Save new value.
                if (_newColor != _color)
                {
                    _property.colorValue = _newColor;
                }
            }
        }

        // ===== Color Value ===== \\

        /// <inheritdoc cref="ColorPaletteField(Rect, GUIContent, Color, out float)"/>
        public static Color ColorPaletteField(Rect _position, Color _color, out float _extraHeight)
        {
            GUIContent _label = GUIContent.none;
            return ColorPaletteField(_position, _label, _color, out _extraHeight);
        }

        /// <inheritdoc cref="ColorPaletteField(Rect, GUIContent, Color, out float)"/>
        public static Color ColorPaletteField(Rect _position, string _label, Color _color, out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return ColorPaletteField(_position, _labelGUI, _color, out _extraHeight);
        }

        /// <param name="_color">The <see cref="Color"/> the field shows.</param>
        /// <returns>The <see cref="Color"/> that has been set by the user.</returns>
        /// <inheritdoc cref="ColorPaletteField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static Color ColorPaletteField(Rect _position, GUIContent _label, Color _color, out float _extraHeight)
        {
            int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
            _color = EditorGUI.ColorField(_position, _label, _color);

            _extraHeight = GetColorPaletteExtraHeight(_id, _position, _label, _color);
            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            _position.height = _extraHeight - EditorGUIUtility.standardVerticalSpacing;

            return DoColorPaletteField(_id, _position, _label, _color);
        }

        // -----------------------

        internal static Color DoColorPaletteField(int _id, Rect _position, GUIContent _label, Color _color)
        {
            ColorPalette _palette = ColorPaletteDatabase.GetControlPalette(_id, _color);
            Rect _palettePos;

            if (string.IsNullOrEmpty(_label.text))
            {
                _palettePos = EditorGUI.IndentedRect(_position);
                _palettePos.xMin += PaletteMenuButtonWidth;
            }
            else
            {
                _palettePos = EditorGUI.PrefixLabel(_position, blankLabelGUI);
            }

            // Draw an outline all over this palette, and add margins to the drawing position.
            GUI.Label(_palettePos, GUIContent.none, EditorStyles.helpBox);

            _palettePos.x += PaletteMargins;
            _palettePos.y += PaletteMargins;
            _palettePos.width -= PaletteMargins * 2f;
            _palettePos.height = EditorGUIUtility.singleLineHeight;

            // Palette selection.
            if (GUI.Button(_palettePos, _palette.Name, EditorStyles.popup))
            {
                GenericMenu _menu = GetColorPaletteSelectionMenu(_id, _palette);
                _menu.DropDown(_palettePos);
            }

            // Make sure there are always at least one color in the palette.
            if (_palette.Count == 0)
            {
                ArrayUtility.Add(ref _palette.Colors, Color.white);
                ColorPaletteDatabase.SaveChanges();
            }

            Rect _temp = new Rect(_palettePos)
            {
                x = _palettePos.x - PaletteMenuButtonWidth,
                width = PaletteMenuButtonWidth - 3f
            };

            // Get selected color.
            if (GetSelectedColorFromPalette(_id, out Color _newColor))
            {
                _color = _newColor;
            }

            // Button to show a menu with additional options.
            if (EditorGUI.DropdownButton(_temp, paletteMenuGUI, FocusType.Keyboard, EnhancedEditorStyles.PaneOptions))
            {
                GenericMenu _menu = GetColorPaletteOptionMenu(_palette, _color);
                _menu.DropDown(_temp);
            }

            // Position adjustments.
            _palettePos.x += 1f;
            _palettePos.y += _temp.height + EditorGUIUtility.standardVerticalSpacing;
            _palettePos.width -= 2f;
            _palettePos.yMax = _position.yMax - 1f;

            float _totalWidth = Mathf.Max(PaletteColorSize * _palette.Count, _palettePos.width);

            _temp.Set(_palettePos.x,
                      _palettePos.y,
                      (_totalWidth / _palette.Count) + 1f,
                      PaletteColorSize);

            // Get control scroll.
            if (!palettesScroll.ContainsKey(_id))
            {
                palettesScroll.Add(_id, new Vector2());
            }

            Vector2 _scroll = palettesScroll[_id];
            Rect _scrollPos = new Rect(_palettePos.x, _palettePos.y, _totalWidth, _temp.height);

            // Draw all colors within a scroll view.
            using (var _scope = new GUI.ScrollViewScope(_palettePos, _scroll, _scrollPos))
            {
                palettesScroll[_id] = _scope.scrollPosition;

                for (int _i = 0; _i < _palette.Count; _i++)
                {
                    Color _paletteColor = _palette.Colors[_i];
                    if (_i == _palette.Count - 1)
                        _temp.width -= 1f;

                    // Main click actions.
                    if ((_temp.Event(out Event _event) == EventType.MouseDown) && (_event.button == 0))
                    {
                        switch (_event.clickCount)
                        {
                            // One click: set color.
                            case 1:
                                _color = _paletteColor;
                                break;

                            // Two clicks: open color picker.
                            case 2:
                                int _index = _i;
                                OpenColorPalettePicker(_palette, _index, (c) =>
                                {
                                    selectedPaletteColorControlID = _id;
                                    selectedPaletteColor = c;
                                });
                                break;

                            default:
                                break;
                        }

                        _event.Use();

                        GUI.changed = true;
                    }
                    else if (EnhancedEditorGUIUtility.ContextClick(_temp))
                    {
                        // Context menu.
                        GenericMenu _menu = GetPaletteColorContextMenu(_id, _palette, _i);
                        _menu.DropDown(_temp);
                    }

                    // Palette color.
                    EditorGUI.DrawRect(_temp, _paletteColor);
                    
                    _temp.x += _temp.width - 1f;
                }
            }

            return _color;
        }

        internal static float GetColorPaletteExtraHeight(int _id, Rect _position, GUIContent _label, Color _color)
        {
            // Get this control associated palette and calculates its height.
            ColorPalette _palette = ColorPaletteDatabase.GetControlPalette(_id, _color);
            Rect _palettePos;

            if (string.IsNullOrEmpty(_label.text))
            {
                _palettePos = EditorGUI.IndentedRect(_position);
                _palettePos.xMin += PaletteMenuButtonWidth;
            }
            else
            {
                _palettePos = EditorGUI.PrefixLabel(_position, blankLabelGUI);
            }

            float _height = EditorGUIUtility.singleLineHeight + PaletteColorSize + (EditorGUIUtility.standardVerticalSpacing * 2f) + (PaletteMargins * 2f);
            float _scrollWidth = _palettePos.width - ((PaletteMargins + 1f) * 2f);

            if ((PaletteColorSize * _palette.Count) > _scrollWidth)
            {
                _height += EnhancedEditorGUIUtility.ScrollSize;
            }

            return ManageDynamicControlHeight(_label, _height);
        }

        private static void OpenColorPalettePicker(ColorPalette _palette, int _colorIndex, Action<Color> _callback = null)
        {
            Color _color = _palette.Colors[_colorIndex];
            EnhancedEditorUtility.ColorPicker(_color, (c) =>
            {
                Undo.RecordObject(ColorPaletteDatabase, ColorPaletteDatabase.UndoRecordTitle);
                _palette.Colors[_colorIndex] = c;

                _callback?.Invoke(c);

                ColorPaletteDatabase.SaveChanges();
                InternalEditorUtility.RepaintAllViews();
            });
        }

        // -----------------------

        /// <summary>
        /// Get a new <see cref="GenericMenu"/> used to edit or perform additional operations on a <see cref="ColorPalette"/>.
        /// </summary>
        /// <param name="_palette">The <see cref="ColorPalette"/> to edit.</param>
        /// <param name="_defaultColor">Color used when adding a new color to the palette.</param>
        /// <returns><see cref="GenericMenu"/> to be displayed.</returns>
        public static GenericMenu GetColorPaletteOptionMenu(ColorPalette _palette, Color _defaultColor)
        {
            GenericMenu _menu = new GenericMenu();

            // Palette modifications.
            _menu.AddItem(paletteAddColorGUI, false, () =>
            {
                Undo.RecordObject(ColorPaletteDatabase, ColorPaletteDatabase.UndoRecordTitle);

                ArrayUtility.Add(ref _palette.Colors, _defaultColor);
                ColorPaletteDatabase.SaveChanges();
            });

            _menu.AddItem(paletteSortColorsGUI, false, () =>
            {
                Undo.RecordObject(ColorPaletteDatabase, ColorPaletteDatabase.UndoRecordTitle);

                Array.Sort(_palette.Colors, ColorComparer.Comparer);
                ColorPaletteDatabase.SaveChanges();
            });

            // Additional utilities.
            _menu.AddSeparator(string.Empty);

            if (_palette.isPersistent)
            {
                _menu.AddItem(paletteRenameGUI, false, () =>
                {
                    SaveRenameColorPaletteWindow.GetWindow(_palette, true);
                });

                _menu.AddItem(paletteDeleteGUI, false, () =>
                {
                    if (EditorUtility.DisplayDialog("Delete Color Palette",
                                                    "You are about to delete this color palette.\nAre you sure you want to do this?",
                                                    "Yes",
                                                    "Cancel"))
                    {
                        ColorPaletteDatabase.DeletePalette(_palette);
                    }
                });
            }

            _menu.AddItem(paletteSaveGUI, false, () =>
            {
                SaveRenameColorPaletteWindow.GetWindow(_palette);
            });

            return _menu;
        }

        /// <summary>
        /// Get a new <see cref="GenericMenu"/> used to select a <see cref="ColorPalette"/>.
        /// </summary>
        /// <param name="_controlID">ID of the associated control (you can get one with <see cref="EnhancedEditorGUIUtility.GetControlID(GUIContent, FocusType, Rect)"/>.</param>
        /// <param name="_palette">The <see cref="ColorPalette"/> currently selected.</param>
        /// <returns><see cref="GenericMenu"/> to display for selecting a new color palette.</returns>
        public static GenericMenu GetColorPaletteSelectionMenu(int _controlID, ColorPalette _palette)
        {
            GenericMenu _menu = new GenericMenu();

            // New palette.
            _menu.AddItem(paletteNewGUI, false, () =>
            {
                ColorPalette _newPalette = ColorPaletteDatabase.CreatePalette();
                SwitchPalette(_newPalette);
            });

            _menu.AddSeparator(string.Empty);

            // Palette selection.
            for (int _i = 0; _i < ColorPaletteDatabase.Count; _i++)
            {
                ColorPalette _otherPalette = ColorPaletteDatabase.Palettes[_i];
                GUIContent _label = new GUIContent(_otherPalette.Name.Replace('_', '/'));
                bool _isSelected = _otherPalette == _palette;

                _menu.AddItem(_label, _isSelected, () =>
                {
                    SwitchPalette(_otherPalette);
                });
            }

            return _menu;

            // ----- Local Method ----- \\

            void SwitchPalette(ColorPalette _newPalette)
            {
                if (!_palette.isPersistent && ((_palette.Count != 2) || (_palette.Colors[0] != Color.black) || (_palette.Colors[1] != Color.white)))
                {
                    int _result = EditorUtility.DisplayDialogComplex("Unsaved Color Palette",
                                                                     "Your selected color palette has not been saved in the database.\n\n" +
                                                                     "Do you want to save it before switching?\nAll your changes will be lost otherwise.",
                                                                     "Save",
                                                                     "Don't Save",
                                                                     "Cancel");

                    switch (_result)
                    {
                        // Save.
                        case 0:
                            SaveRenameColorPaletteWindow.GetWindow(_palette);
                            return;

                        // Don't save and switch.
                        case 1:
                            break;

                        // Cancel.
                        default:
                            return;
                    }
                }

                ColorPaletteDatabase.SetControlPalette(_controlID, _newPalette);
            }
        }

        /// <summary>
        /// Get the <see cref="GenericMenu"/>  to be displayed on a <see cref="ColorPalette"/> color context click.
        /// </summary>
        /// <param name="_controlID">ID of the associated control (you can get one with <see cref="EnhancedEditorGUIUtility.GetControlID(GUIContent, FocusType, Rect)"/>.</param>
        /// <param name="_palette">The <see cref="ColorPalette"/> this color is from.</param>
        /// <param name="_colorIndex">Index of this color from its associated <see cref="ColorPalette"/> (<see cref="ColorPalette.Colors"/>).</param>
        /// <returns><see cref="GenericMenu"/> to be displayed.</returns>
        public static GenericMenu GetPaletteColorContextMenu(int _controlID, ColorPalette _palette, int _colorIndex)
        {
            GenericMenu _menu = new GenericMenu();

            // Edit.
            _menu.AddItem(paletteEditColorGUI, false, () =>
            {
                OpenColorPalettePicker(_palette, _colorIndex);
            });

            // Select.
            _menu.AddItem(paletteSelectColorGUI, false, () =>
            {
                Color _color = _palette.Colors[_colorIndex];

                selectedPaletteColorControlID = _controlID;
                selectedPaletteColor = _color;
            });

            // Remove.
            _menu.AddItem(paletteRemoveColorGUI, false, () =>
            {
                if (EditorUtility.DisplayDialog("Remove Palette Color",
                                                "You are about to remove this color from the palette.\nAre you sure you want to do this?",
                                                "Yes", "Cancel"))
                {
                    Undo.RecordObject(ColorPaletteDatabase, ColorPaletteDatabase.UndoRecordTitle);

                    ArrayUtility.RemoveAt(ref _palette.Colors, _colorIndex);
                    ColorPaletteDatabase.SaveChanges();
                }
            });

            return _menu;
        }

        /// <summary>
        /// Get the color selected by the user from a <see cref="ColorPalette"/> for a specific control.
        /// </summary>
        /// <param name="_controlID">ID of the associated control (same as used for <see cref="GetPaletteColorContextMenu(int, ColorPalette, int)"/>).</param>
        /// <param name="_color">New color selected by the user.</param>
        /// <returns>True if the user selected a new color for this control, false otherwise.</returns>
        public static bool GetSelectedColorFromPalette(int _controlID, out Color _color)
        {
            if ((selectedPaletteColorControlID != -1) && (selectedPaletteColorControlID == _controlID))
            {
                _color = selectedPaletteColor;

                selectedPaletteColorControlID = -1;
                selectedPaletteColor = default;

                GUI.changed = true;
                return true;
            }

            _color = default;
            return false;
        }
        #endregion

        #region Folder
        internal const string DefaultEditorPanelTitle = "Select an Editor Folder";
        private const string EditorFolder = "Editor";

        private static readonly GUIContent folderButtonGUI = new GUIContent(string.Empty, "Opens the panel to select a folder.");
        private static readonly string dataPath = Application.dataPath;

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="FolderField(Rect, SerializedProperty, GUIContent, bool, string)"/>
        public static void FolderField(Rect _position, SerializedProperty _property, bool _allowOutsideProjectFolder = false)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            FolderField(_position, _property, _label, _allowOutsideProjectFolder);
        }

        /// <inheritdoc cref="FolderField(Rect, SerializedProperty, GUIContent, bool, string)"/>
        public static void FolderField(Rect _position, SerializedProperty _property, bool _allowOutsideProjectFolder, string _folderPanelTitle)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            FolderField(_position, _property, _label, _allowOutsideProjectFolder, _folderPanelTitle);
        }

        /// <inheritdoc cref="FolderField(Rect, SerializedProperty, GUIContent, bool, string)"/>
        public static void FolderField(Rect _position, SerializedProperty _property, string _label, bool _allowOutsideProjectFolder = false)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            FolderField(_position, _property, _labelGUI, _allowOutsideProjectFolder);
        }

        /// <inheritdoc cref="FolderField(Rect, SerializedProperty, GUIContent, bool, string)"/>
        public static void FolderField(Rect _position, SerializedProperty _property, string _label, bool _allowOutsideProjectFolder, string _folderPanelTitle)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            FolderField(_position, _property, _labelGUI, _allowOutsideProjectFolder, _folderPanelTitle);
        }

        /// <inheritdoc cref="FolderField(Rect, SerializedProperty, GUIContent, bool, string)"/>
        public static void FolderField(Rect _position, SerializedProperty _property, GUIContent _label, bool _allowOutsideProjectFolder = false)
        {
            string _folderPanelTitle = string.Empty;
            FolderField(_position, _property, _label, _allowOutsideProjectFolder, _folderPanelTitle);
        }

        /// <summary>
        /// Makes a field for selecting a folder.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make a folder field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_allowOutsideProjectFolder">Allow or not to select a folder located outside the project.</param>
        /// <param name="_folderPanelTitle">Title of the folder selection panel.</param>
        public static void FolderField(Rect _position, SerializedProperty _property, GUIContent _label, bool _allowOutsideProjectFolder, string _folderPanelTitle)
        {
            // Incompatible property management.
            if (_property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            // Folder field.
            EditorGUI.BeginProperty(_position, _label, _property);
            EditorGUI.BeginChangeCheck();
            {
                string _folderPath = _property.stringValue;
                _folderPath = FolderField(_position, _label, _folderPath, _allowOutsideProjectFolder, _folderPanelTitle);

                // Save new value.
                if (EditorGUI.EndChangeCheck())
                {
                    _property.stringValue = _folderPath;

                    EditorGUI.BeginProperty(_position, _label, _property);
                }
            }

            EditorGUI.EndProperty();
        }

        // ===== String Value ===== \\

        /// <inheritdoc cref="FolderField(Rect, GUIContent, string, bool, string)"/>
        public static string FolderField(Rect _position, string _folderPath, bool _allowOutsideProjectFolder = false)
        {
            GUIContent _label = GUIContent.none;
            return FolderField(_position, _label, _folderPath, _allowOutsideProjectFolder);
        }

        /// <inheritdoc cref="FolderField(Rect, GUIContent, string, bool, string)"/>
        public static string FolderField(Rect _position, string _folderPath, bool _allowOutsideProjectFolder, string _folderPanelTitle)
        {
            GUIContent _label = GUIContent.none;
            return FolderField(_position, _label, _folderPath, _allowOutsideProjectFolder, _folderPanelTitle);
        }

        /// <inheritdoc cref="FolderField(Rect, GUIContent, string, bool, string)"/>
        public static string FolderField(Rect _position, string _label, string _folderPath, bool _allowOutsideProjectFolder = false)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return FolderField(_position, _labelGUI, _folderPath, _allowOutsideProjectFolder);
        }

        /// <inheritdoc cref="FolderField(Rect, GUIContent, string, bool, string)"/>
        public static string FolderField(Rect _position, string _label, string _folderPath, bool _allowOutsideProjectFolder, string _folderPanelTitle)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return FolderField(_position, _labelGUI, _folderPath, _allowOutsideProjectFolder, _folderPanelTitle);
        }

        /// <inheritdoc cref="FolderField(Rect, GUIContent, string, bool, string)"/>
        public static string FolderField(Rect _position, GUIContent _label, string _folderPath, bool _allowOutsideProjectFolder = false)
        {
            string _folderPanelTitle = string.Empty;
            return FolderField(_position, _label, _folderPath, _allowOutsideProjectFolder, _folderPanelTitle);
        }

        /// <param name="_folderPath">The folder path the field shows.</param>
        /// <returns>The folder path that has been set by the user.</returns>
        /// <inheritdoc cref="FolderField(Rect, SerializedProperty, GUIContent, bool, string)"/>
        public static string FolderField(Rect _position, GUIContent _label, string _folderPath, bool _allowOutsideProjectFolder, string _folderPanelTitle)
        {
            // Labels.
            _position = EditorGUI.PrefixLabel(_position, _label);
            _position.width -= EnhancedEditorGUIUtility.IconWidth + 2f;

            using (var _scope = ZeroIndentScope())
            {
                EditorGUI.SelectableLabel(_position, _folderPath, EditorStyles.textField);
            }

            // Folder icon.
            if (folderButtonGUI.image == null)
                folderButtonGUI.image = EditorGUIUtility.FindTexture("FolderOpened Icon");

            // Folder panel.
            _position.x += _position.width + 2f;
            _position.width = EnhancedEditorGUIUtility.IconWidth;

            if (IconButton(_position, folderButtonGUI))
            {
                string _fullFolderPath = _allowOutsideProjectFolder
                                        ? _folderPath
                                        : Path.Combine(dataPath, _folderPath);

                string _newPath = EditorUtility.OpenFolderPanel(_folderPanelTitle, _fullFolderPath, string.Empty);
               
                if (!string.IsNullOrEmpty(_newPath))
                {
                    // If the selected path is not inside the project, display an error dialog.
                    if (!_allowOutsideProjectFolder)
                    {
                        if ((_newPath.Length > dataPath.Length) && _newPath.StartsWith(dataPath) && 
                            ((_newPath[dataPath.Length] == Path.DirectorySeparatorChar) || (_newPath[dataPath.Length] == Path.AltDirectorySeparatorChar)))
                        {
                            _newPath = _newPath.Remove(0, dataPath.Length + 1);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Wrong Folder Location", "The selected folder cannot be assigned.\n\n" +
                                                        "The specified directory must be located inside the project. " +
                                                        "Please make sure the desired folder is located inside a valid directory and try again.", "Ok");
                            return _folderPath;
                        }
                    }

                    // Save folder path.
                    _folderPath = _newPath;

                    // Unfocus current text as this field selectable label won't update while in focus.
                    GUI.FocusControl(string.Empty);
                    GUI.changed = true;
                }
            }

            return _folderPath;
        }

        // ===== Editor Folder - Serialized Property ===== \\

        /// <inheritdoc cref="EditorFolderField(Rect, SerializedProperty, GUIContent, string)"/>
        public static void EditorFolderField(Rect _position, SerializedProperty _property, string _folderPanelTitle = DefaultEditorPanelTitle) {
            GUIContent _label = GUIContent.none;
            EditorFolderField(_position, _property, _label, _folderPanelTitle);
        }

        /// <inheritdoc cref="EditorFolderField(Rect, SerializedProperty, GUIContent, string)"/>
        public static void EditorFolderField(Rect _position, SerializedProperty _property, string _label, string _folderPanelTitle = DefaultEditorPanelTitle) {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            EditorFolderField(_position, _property, _labelGUI, _folderPanelTitle);
        }

        /// <summary>
        /// Makes a field for selecting a folder located in an Editor folder.
        /// </summary>
        /// <inheritdoc cref="FolderField(Rect, SerializedProperty, GUIContent, bool, string)"/>
        public static void EditorFolderField(Rect _position, SerializedProperty _property, GUIContent _label, string _folderPanelTitle = DefaultEditorPanelTitle) {
            // Incompatible property management.
            if (_property.propertyType != SerializedPropertyType.String) {
                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            // Folder field.
            EditorGUI.BeginProperty(_position, _label, _property);
            EditorGUI.BeginChangeCheck();
            {
                string _folderPath = _property.stringValue;
                _folderPath = EditorFolderField(_position, _label, _folderPath, _folderPanelTitle);

                // Save new value.
                if (EditorGUI.EndChangeCheck()) {
                    _property.stringValue = _folderPath;

                    EditorGUI.BeginProperty(_position, _label, _property);
                }
            }

            EditorGUI.EndProperty();
        }

        // ===== Editor Folder - String Value ===== \\

        /// <inheritdoc cref="EditorFolderField(Rect, GUIContent, string, string)"/>
        public static string EditorFolderField(Rect _position, string _folderPath, string _folderPanelTitle = DefaultEditorPanelTitle)
        {
            GUIContent _label = GUIContent.none;
            return EditorFolderField(_position, _label, _folderPath, _folderPanelTitle);
        }

        /// <inheritdoc cref="EditorFolderField(Rect, GUIContent, string, string)"/>
        public static string EditorFolderField(Rect _position, string _label, string _folderPath, string _folderPanelTitle = DefaultEditorPanelTitle)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return EditorFolderField(_position, _labelGUI, _folderPath, _folderPanelTitle);
        }

        /// <summary>
        /// <inheritdoc cref="EditorFolderField(Rect, SerializedProperty, GUIContent, string)" path="/summary"/>
        /// </summary>
        /// <inheritdoc cref="FolderField(Rect, GUIContent, string, bool, string)"/>
        public static string EditorFolderField(Rect _position, GUIContent _label, string _folderPath, string _folderPanelTitle = DefaultEditorPanelTitle)
        {
            string _newFolderPath = FolderField(_position, _label, _folderPath, false, _folderPanelTitle);
            if (_newFolderPath != _folderPath)
            {
                // Unfortunately, the directory separator returned by the Unity API may be different from the one used by the system,
                // so let's check for both standard and alternative separators.
                if (_newFolderPath == EditorFolder ||
                    _newFolderPath.StartsWith(EditorFolder + Path.DirectorySeparatorChar) || _newFolderPath.StartsWith(EditorFolder + Path.AltDirectorySeparatorChar) ||
                    _newFolderPath.EndsWith(Path.DirectorySeparatorChar + EditorFolder) || _newFolderPath.EndsWith(Path.AltDirectorySeparatorChar + EditorFolder) ||
                    _newFolderPath.Contains(Path.DirectorySeparatorChar + EditorFolder + Path.DirectorySeparatorChar) ||
                    _newFolderPath.Contains(Path.AltDirectorySeparatorChar + EditorFolder + Path.AltDirectorySeparatorChar))
                {
                    _folderPath = _newFolderPath;
                }
                else
                {
                    EditorUtility.DisplayDialog("Wrong Folder Location", "The selected folder cannot be assigned.\n\n" +
                                                "The specified directory must be located in an Editor folder. " +
                                                "Please make sure the desired folder is located inside a valid directory and try again.", "Ok");

                    GUI.changed = false;
                }
            }

            return _folderPath;
        }
        #endregion

        #region Inline
        /// <inheritdoc cref="InlineField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void InlineField(Rect _position, SerializedProperty _property, out float _extraHeight)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            InlineField(_position, _property, _label, out _extraHeight);
        }

        /// <inheritdoc cref="InlineField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void InlineField(Rect _position, SerializedProperty _property, string _label, out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            InlineField(_position, _property, _labelGUI, out _extraHeight);
        }

        /// <summary>
        /// Makes an inline field for a property, displaying a struct or a serializable class within a single line.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make an inline field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_extraHeight"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_extraHeight']"/></param>
        public static void InlineField(Rect _position, SerializedProperty _property, GUIContent _label, out float _extraHeight)
        {
            _extraHeight = 0f;

            // If the property has no children, simply draw it.
            if (!_property.hasVisibleChildren)
            {
                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            SerializedProperty _current = _property.Copy();
            SerializedProperty _next = _property.Copy();
            _next.NextVisible(false);
            _current.NextVisible(true);

            // Get the count of all children to draw, and their required extra size.
            int _count = 0;
            while (!SerializedProperty.EqualContents(_current, _next))
            {
                if (!_current.hasVisibleChildren)
                {
                    _extraHeight = Mathf.Max(_extraHeight, EditorGUI.GetPropertyHeight(_current, false));
                    _count++;
                }

                _current.NextVisible(false);
            }

            Rect _temp = new Rect(_position);

            _extraHeight = ManageDynamicControlHeight(_label, _extraHeight - _position.height);
            _position.height += _extraHeight;

            // Property field.
            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
            {
                _temp = EditorGUI.PrefixLabel(_temp, _label);
                _temp.width = ((_temp.width + 2f) / _count) - 2f;

                _current = _property.Copy();
                _current.NextVisible(true);

                // Draw all children of this property on one line.
                using (var _indentScope = ZeroIndentScope())
                {
                    for (int _i = 0; _i < _count; _i++)
                    {
                        EditorGUI.PropertyField(_temp, _current, GUIContent.none, false);

                        _temp.x += _temp.width + 2f;
                        _current.NextVisible(false);
                    }
                }
            }
        }
        #endregion

        #region Max
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="MaxField(Rect, SerializedProperty, GUIContent, MemberValue{float})"/>
        public static void MaxField(Rect _position, SerializedProperty _property, MemberValue<float> _maxMember)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MaxField(_position, _property, _label, _maxMember);
        }

        /// <inheritdoc cref="MaxField(Rect, SerializedProperty, GUIContent, MemberValue{float})"/>
        public static void MaxField(Rect _position, SerializedProperty _property, string _label, MemberValue<float> _maxMember)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MaxField(_position, _property, _labelGUI, _maxMember);
        }

        /// <param name="_maxMember">Class member to get value from, acting as this field maximum allowed value.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="float"/>.</param>
        /// <inheritdoc cref="MaxField(Rect, SerializedProperty, GUIContent, float)"/>
        public static void MaxField(Rect _position, SerializedProperty _property, GUIContent _label, MemberValue<float> _maxMember)
        {
            // Incompatible max value management.
            if (!_maxMember.GetValue(_property.serializedObject, out float _maxFloatValue))
            {
                // Debug message.
                Object _target = _property.serializedObject.targetObject;
                _target.LogWarning($"Could not get the value of the class member \"{_maxMember.Name}\" in the script \"{_target.GetType()}\".");

                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            MaxField(_position, _property, _label, _maxFloatValue);
        }

        /// <inheritdoc cref="MaxField(Rect, SerializedProperty, GUIContent, float)"/>
        public static void MaxField(Rect _position, SerializedProperty _property, float _maxValue)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MaxField(_position, _property, _label, _maxValue);
        }

        /// <inheritdoc cref="MaxField(Rect, SerializedProperty, GUIContent, float)"/>
        public static void MaxField(Rect _position, SerializedProperty _property, string _label, float _maxValue)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MaxField(_position, _property, _labelGUI, _maxValue);
        }

        /// <summary>
        /// Restrains a property value so that it does not exceed a specific maximum.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to ceil value.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_maxValue">Maximum allowed value.</param>
        public static void MaxField(Rect _position, SerializedProperty _property, GUIContent _label, float _maxValue)
        {
            using (var _scope = new EditorGUI.PropertyScope(Rect.zero, GUIContent.none, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(_position, _property, _label);

                // Restrains value when changed.
                if (_changeCheck.changed)
                {
                    EnhancedEditorUtility.CeilSerializedPropertyValue(_property, _maxValue);
                }
            }
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MaxField(Rect _position, float _value, float _maxValue)
        {
            GUIContent _label = GUIContent.none;
            return MaxField(_position, _label, _value, _maxValue);
        }

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MaxField(Rect _position, string _label, float _value, float _maxValue)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MaxField(_position, _labelGUI, _value, _maxValue);
        }

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MaxField(Rect _position, GUIContent _label, float _value, float _maxValue)
        {
            GUIStyle _style = EditorStyles.numberField;
            return MaxField(_position, _label, _value, _maxValue, _style);
        }

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MaxField(Rect _position, float _value, float _maxValue, GUIStyle _style)
        {
            GUIContent _label = GUIContent.none;
            return MaxField(_position, _label, _value, _maxValue, _style);
        }

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MaxField(Rect _position, string _label, float _value, float _maxValue, GUIStyle _style)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MaxField(_position, _labelGUI, _value, _maxValue, _style);
        }

        /// <summary>
        /// Restrains a specific value so that it does not exceed a specific maximum.
        /// </summary>
        /// <param name="_value">The value to edit and restrain.</param>
        /// <param name="_style"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_style']"/></param>
        /// <returns>The restrained value entered by the user.</returns>
        /// <inheritdoc cref="MaxField(Rect, SerializedProperty, GUIContent, float)"/>
        public static float MaxField(Rect _position, GUIContent _label, float _value, float _maxValue, GUIStyle _style)
        {
            _value = EditorGUI.FloatField(_position, _label, _value, _style);
            _value = Mathf.Min(_value, _maxValue);

            return _value;
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MaxField(Rect _position, int _value, int _maxValue)
        {
            GUIContent _label = GUIContent.none;
            return MaxField(_position, _label, _value, _maxValue);
        }

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MaxField(Rect _position, string _label, int _value, int _maxValue)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MaxField(_position, _labelGUI, _value, _maxValue);
        }

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MaxField(Rect _position, GUIContent _label, int _value, int _maxValue)
        {
            GUIStyle _style = EditorStyles.numberField;
            return MaxField(_position, _label, _value, _maxValue, _style);
        }

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MaxField(Rect _position, int _value, int _maxValue, GUIStyle _style)
        {
            GUIContent _label = GUIContent.none;
            return MaxField(_position, _label, _value, _maxValue, _style);
        }

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MaxField(Rect _position, string _label, int _value, int _maxValue, GUIStyle _style)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MaxField(_position, _labelGUI, _value, _maxValue, _style);
        }

        /// <inheritdoc cref="MaxField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MaxField(Rect _position, GUIContent _label, int _value, int _maxValue, GUIStyle _style)
        {
            _value = EditorGUI.IntField(_position, _label, _value, _style);
            _value = Mathf.Min(_value, _maxValue);

            return _value;
        }
        #endregion

        #region Min
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="MinField(Rect, SerializedProperty, GUIContent, MemberValue{float})"/>
        public static void MinField(Rect _position, SerializedProperty _property, MemberValue<float> _minMember)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinField(_position, _property, _label, _minMember);
        }

        /// <inheritdoc cref="MinField(Rect, SerializedProperty, GUIContent, MemberValue{float})"/>
        public static void MinField(Rect _position, SerializedProperty _property, string _label, MemberValue<float> _minMember)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MinField(_position, _property, _labelGUI, _minMember);
        }

        /// <param name="_minMember">Class member to get value from, acting as this field minimum allowed value.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="float"/>.</param>
        /// <inheritdoc cref="MinField(Rect, SerializedProperty, GUIContent, float)"/>
        public static void MinField(Rect _position, SerializedProperty _property, GUIContent _label, MemberValue<float> _minMember)
        {
            // Incompatible min value management.
            if (!_minMember.GetValue(_property.serializedObject, out float _minFloatValue))
            {
                // Debug message.
                Object _target = _property.serializedObject.targetObject;
                _target.LogWarning($"Could not get the value of the class member \"{_minMember.Name}\" in the script \"{_target.GetType()}\".");

                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            MinField(_position, _property, _label, _minFloatValue);
        }

        /// <inheritdoc cref="MinField(Rect, SerializedProperty, GUIContent, float)"/>
        public static void MinField(Rect _position, SerializedProperty _property, float _minValue)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinField(_position, _property, _label, _minValue);
        }

        /// <inheritdoc cref="MinField(Rect, SerializedProperty, GUIContent, float)"/>
        public static void MinField(Rect _position, SerializedProperty _property, string _label, float _minValue)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MinField(_position, _property, _labelGUI, _minValue);
        }

        /// <summary>
        /// Restrains a property value so that it does not go under a specific minimum.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to floor value.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_minValue">Minimum allowed value.</param>
        public static void MinField(Rect _position, SerializedProperty _property, GUIContent _label, float _minValue)
        {
            using (var _scope = new EditorGUI.PropertyScope(Rect.zero, GUIContent.none, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(_position, _property, _label);

                // Restrains value when changed.
                if (_changeCheck.changed)
                {
                    EnhancedEditorUtility.FloorSerializedPropertyValue(_property, _minValue);
                }
            }
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MinField(Rect _position, float _value, float _minValue)
        {
            GUIContent _label = GUIContent.none;
            return MinField(_position, _label, _value, _minValue);
        }

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MinField(Rect _position, string _label, float _value, float _minValue)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinField(_position, _labelGUI, _value, _minValue);
        }

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MinField(Rect _position, GUIContent _label, float _value, float _minValue)
        {
            GUIStyle _style = EditorStyles.numberField;
            return MinField(_position, _label, _value, _minValue, _style);
        }

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MinField(Rect _position, float _value, float _minValue, GUIStyle _style)
        {
            GUIContent _label = GUIContent.none;
            return MinField(_position, _label, _value, _minValue, _style);
        }

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static float MinField(Rect _position, string _label, float _value, float _minValue, GUIStyle _style)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinField(_position, _labelGUI, _value, _minValue, _style);
        }

        /// <summary>
        /// Restrains a specific value so that it does not go under a specific minimum.
        /// </summary>
        /// <param name="_value">The value to edit and restrain.</param>
        /// <param name="_style"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_style']"/></param>
        /// <returns>The restrained value entered by the user.</returns>
        /// <inheritdoc cref="MinField(Rect, SerializedProperty, GUIContent, float)"/>
        public static float MinField(Rect _position, GUIContent _label, float _value, float _minValue, GUIStyle _style)
        {
            _value = EditorGUI.FloatField(_position, _label, _value, _style);
            _value = Mathf.Max(_value, _minValue);

            return _value;
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MinField(Rect _position, int _value, int _minValue)
        {
            GUIContent _label = GUIContent.none;
            return MinField(_position, _label, _value, _minValue);
        }

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MinField(Rect _position, string _label, int _value, int _minValue)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinField(_position, _labelGUI, _value, _minValue);
        }

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MinField(Rect _position, GUIContent _label, int _value, int _minValue)
        {
            GUIStyle _style = EditorStyles.numberField;
            return MinField(_position, _label, _value, _minValue, _style);
        }

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MinField(Rect _position, int _value, int _minValue, GUIStyle _style)
        {
            GUIContent _label = GUIContent.none;
            return MinField(_position, _label, _value, _minValue, _style);
        }

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MinField(Rect _position, string _label, int _value, int _minValue, GUIStyle _style)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinField(_position, _labelGUI, _value, _minValue, _style);
        }

        /// <inheritdoc cref="MinField(Rect, GUIContent, float, float, GUIStyle)"/>
        public static int MinField(Rect _position, GUIContent _label, int _value, int _minValue, GUIStyle _style)
        {
            _value = EditorGUI.IntField(_position, _label, _value, _style);
            _value = Mathf.Max(_value, _minValue);

            return _value;
        }
        #endregion

        #region Min Max
        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="MinMaxField(Rect, SerializedProperty, GUIContent, MemberValue{Vector2})"/>
        public static void MinMaxField(Rect _position, SerializedProperty _property, MemberValue<Vector2> _minMaxMember)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinMaxField(_position, _property, _label, _minMaxMember);
        }

        /// <inheritdoc cref="MinMaxField(Rect, SerializedProperty, GUIContent, MemberValue{Vector2})"/>
        public static void MinMaxField(Rect _position, SerializedProperty _property, string _label, MemberValue<Vector2> _minMaxMember)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MinMaxField(_position, _property, _labelGUI, _minMaxMember);
        }

        /// <param name="_minMaxMember">Class member to get value from, used to determine both the minimum and maximum allowed value of the slider.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="Vector2"/>.</param>
        /// <inheritdoc cref="MinMaxField(Rect, SerializedProperty, GUIContent, float, float)"/>
        public static void MinMaxField(Rect _position, SerializedProperty _property, GUIContent _label, MemberValue<Vector2> _minMaxMember)
        {
            // Incompatible min max value management.
            if (!_minMaxMember.GetValue(_property.serializedObject, out Vector2 _minMaxVectorValue))
            {
                // Debug message.
                Object _target = _property.serializedObject.targetObject;
                _target.LogWarning($"Could not get the value of the class member \"{_minMaxMember.Name}\" in the script \"{_target.GetType()}\".");

                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            MinMaxField(_position, _property, _label, _minMaxVectorValue.x, _minMaxVectorValue.y);
        }

        /// <inheritdoc cref="MinMaxField(Rect, SerializedProperty, GUIContent, float, float)"/>
        public static void MinMaxField(Rect _position, SerializedProperty _property, float _minLimit, float _maxLimit)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            MinMaxField(_position, _property, _label, _minLimit, _maxLimit);
        }

        /// <inheritdoc cref="MinMaxField(Rect, SerializedProperty, GUIContent, float, float)"/>
        public static void MinMaxField(Rect _position, SerializedProperty _property, string _label, float _minLimit, float _maxLimit)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            MinMaxField(_position, _property, _labelGUI, _minLimit, _maxLimit);
        }

        /// <summary>
        /// Makes a slider for both a minimum and a maximum draggable value.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to use as min max value (should either be of <see cref="Vector2"/> or <see cref="Vector2Int"/> type).</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_minLimit">Slider minimum allowed value.</param>
        /// <param name="_maxLimit">Slider maximum allowed value.</param>
        public static void MinMaxField(Rect _position, SerializedProperty _property, GUIContent _label, float _minLimit, float _maxLimit)
        {
            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
            {
                switch (_property.propertyType)
                {
                    // Vector2.
                    case SerializedPropertyType.Vector2:
                    {
                        Vector2 _value = _property.vector2Value;
                        _property.vector2Value = MinMaxField(_position, _label, _value, _minLimit, _maxLimit);
                        break;
                    }

                    // Vector2Int.
                    case SerializedPropertyType.Vector2Int:
                    {
                        Vector2Int _value = _property.vector2IntValue;
                        _property.vector2IntValue = MinMaxField(_position, _label, _value, (int)_minLimit, (int)_maxLimit);
                        break;
                    }

                    // Draw default property field.
                    default:
                        EditorGUI.PropertyField(_position, _property, _label);
                        break;
                }
            }
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, Vector2, float, float)"/>
        public static Vector2 MinMaxField(Rect _position, Vector2 _value, float _minLimit, float _maxLimit)
        {
            GUIContent _label = GUIContent.none;
            return MinMaxField(_position, _label, _value, _minLimit, _maxLimit);
        }

        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, Vector2, float, float)"/>
        public static Vector2 MinMaxField(Rect _position, string _label, Vector2 _value, float _minLimit, float _maxLimit)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinMaxField(_position, _labelGUI, _value, _minLimit, _maxLimit);
        }

        /// <param name="_value">Current slider value (minimum as x, maximum as y).</param>
        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, float, float, float, float)"/>
        public static Vector2 MinMaxField(Rect _position, GUIContent _label, Vector2 _value, float _minLimit, float _maxLimit)
        {
            _value = MinMaxField(_position, _label, _value.x, _value.y, _minLimit, _maxLimit);
            return _value;
        }

        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, float, float, float, float)"/>
        public static Vector2 MinMaxField(Rect _position, float _minValue, float _maxValue, float _minLimit, float _maxLimit)
        {
            GUIContent _label = GUIContent.none;
            return MinMaxField(_position, _label, _minValue, _maxValue, _minLimit, _maxLimit);
        }

        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, float, float, float, float)"/>
        public static Vector2 MinMaxField(Rect _position, string _label, float _minValue, float _maxValue, float _minLimit, float _maxLimit)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinMaxField(_position, _labelGUI, _minValue, _maxValue, _minLimit, _maxLimit);
        }

        /// <param name="_minValue">Current slider minimum value.</param>
        /// <param name="_maxValue">Current slider maximum value.</param>
        /// <returns><see cref="Vector2"/> with min and max value respectively as x and y.</returns>
        /// <inheritdoc cref="MinMaxField(Rect, SerializedProperty, GUIContent, float, float)"/>
        public static Vector2 MinMaxField(Rect _position, GUIContent _label, float _minValue, float _maxValue, float _minLimit, float _maxLimit)
        {
            // Label.
            _position = EditorGUI.PrefixLabel(_position, _label);

            // Min value float field.
            Rect _temp = new Rect(_position)
            {
                width = EditorGUIUtility.fieldWidth
            };

            using (var _scope = ZeroIndentScope())
            {
                float _newMinValue = EditorGUI.FloatField(_temp, _minValue);
                if (_newMinValue != _minValue)
                {
                    _minValue = Mathf.Clamp(_newMinValue, _minLimit, _maxValue);
                }

                // Max value float field.
                _temp.x = _position.xMax - _temp.width;

                float _newMaxValue = EditorGUI.FloatField(_temp, _maxValue);
                if (_newMaxValue != _maxValue)
                {
                    _maxValue = Mathf.Clamp(_newMaxValue, _minValue, _maxLimit);
                }

                // Min-Max slider.
                _position.xMin += _temp.width + 5f;
                _position.xMax -= _temp.width + 5f;

                EditorGUI.MinMaxSlider(_position, ref _minValue, ref _maxValue, _minLimit, _maxLimit);
            }
                
            return new Vector2(_minValue, _maxValue);
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, Vector2Int, int, int)"/>
        public static Vector2Int MinMaxField(Rect _position, Vector2Int _value, int _minLimit, int _maxLimit)
        {
            GUIContent _label = GUIContent.none;
            return MinMaxField(_position, _label, _value, _minLimit, _maxLimit);
        }

        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, Vector2Int, int, int)"/>
        public static Vector2Int MinMaxField(Rect _position, string _label, Vector2Int _value, int _minLimit, int _maxLimit)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinMaxField(_position, _labelGUI, _value, _minLimit, _maxLimit);
        }

        /// <param name="_value"><inheritdoc cref="MinMaxField(Rect, GUIContent, Vector2, float, float)" path="/param[@name='_value']"/></param>
        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, int, int, int, int)"/>
        public static Vector2Int MinMaxField(Rect _position, GUIContent _label, Vector2Int _value, int _minLimit, int _maxLimit)
        {
            _value = MinMaxField(_position, _label, _value.x, _value.y, _minLimit, _maxLimit);
            return _value;
        }

        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, int, int, int, int)"/>
        public static Vector2Int MinMaxField(Rect _position, int _minValue, int _maxValue, int _minLimit, int _maxLimit)
        {
            GUIContent _label = GUIContent.none;
            return MinMaxField(_position, _label, _minValue, _maxValue, _minLimit, _maxLimit);
        }

        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, int, int, int, int)"/>
        public static Vector2Int MinMaxField(Rect _position, string _label, int _minValue, int _maxValue, int _minLimit, int _maxLimit)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return MinMaxField(_position, _labelGUI, _minValue, _maxValue, _minLimit, _maxLimit);
        }

        /// <returns><see cref="Vector2Int"/> with min and max value respectively as x and y.</returns>
        /// <inheritdoc cref="MinMaxField(Rect, GUIContent, float, float, float, float)"/>
        public static Vector2Int MinMaxField(Rect _position, GUIContent _label, int _minValue, int _maxValue, int _minLimit, int _maxLimit)
        {
            Vector2Int _value = new Vector2Int(_minValue, _maxValue);
            using (var _scope = new EditorGUI.ChangeCheckScope())
            {
                Vector2 _newValue = MinMaxField(_position, _label, (float)_value.x, _value.y, _minLimit, _maxLimit);

                // Only proceed to casts when the value changed.
                if (_scope.changed)
                {
                    _value.Set((int)_newValue.x, (int)_newValue.y);
                }
            }

            return _value;
        }
        #endregion

        #region Picker
        private static readonly GUIContent PickerButtonGUI = new GUIContent(string.Empty, "Opens the picker to select an object.");
        private static readonly Type[] pickerRequiredType = new Type[] { null };

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="PickerField(Rect, SerializedProperty, GUIContent, Type)"/>
        public static void PickerField(Rect _position, SerializedProperty _property, Type _requiredType)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            PickerField(_position, _property, _label, _requiredType);
        }

        /// <inheritdoc cref="PickerField(Rect, SerializedProperty, GUIContent, Type)"/>
        public static void PickerField(Rect _position, SerializedProperty _property, string _label, Type _requiredType)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            PickerField(_position, _property, _labelGUI, _requiredType);
        }

        /// <param name="_requiredType">Only the objects possessing this component will be assignable (must either be a component or an interface).</param>
        /// <inheritdoc cref="PickerField(Rect, SerializedProperty, GUIContent, Type[])"/>
        public static void PickerField(Rect _position, SerializedProperty _property, GUIContent _label, Type _requiredType)
        {
            pickerRequiredType[0] = _requiredType;
            PickerField(_position, _property, _label, pickerRequiredType);
        }

        /// <inheritdoc cref="PickerField(Rect, SerializedProperty, GUIContent, Type[])"/>
        public static void PickerField(Rect _position, SerializedProperty _property, Type[] _requiredTypes)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            PickerField(_position, _property, _label, _requiredTypes);
        }

        /// <inheritdoc cref="PickerField(Rect, SerializedProperty, GUIContent, Type[])"/>
        public static void PickerField(Rect _position, SerializedProperty _property, string _label, Type[] _requiredTypes)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            PickerField(_position, _property, _labelGUI, _requiredTypes);
        }

        /// <summary>
        /// Makes a <see cref="GameObject"/> or <see cref="Component"/> picker field,
        /// constraining its value to objects with specific components and interfaces.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to draw a picker for (must be of <see cref="GameObject"/> or <see cref="Component"/> object type).</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_requiredTypes">Only the objects possessing all of these required components will be assignable
        /// (must either be a component or an interface).</param>
        public static void PickerField(Rect _position, SerializedProperty _property, GUIContent _label, Type[] _requiredTypes)
        {
            Type _objectType;

            // In order for the picker to work, the property must be of object reference type and the target object type either a GameObject or a Component.
            if ((_property.propertyType != SerializedPropertyType.ObjectReference)
               || !EnhancedEditorUtility.IsSceneObject(_objectType = EnhancedEditorUtility.GetSerializedPropertyType(_property)))
            {
                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            // Picker window button.
            int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
            bool _allowSceneObjects = !EditorUtility.IsPersistent(_property.serializedObject.targetObject);

            _position = DoPickerField(_position, _id, _property.objectReferenceValue, _objectType, _requiredTypes, _allowSceneObjects);
            
            // Property field.
            using (var _scope = new EditorGUI.PropertyScope(Rect.zero, GUIContent.none, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(_position, _property, _label);

                if (_changeCheck.changed && ResetPickerObjectIfDontMatch(_property.objectReferenceValue, _requiredTypes))
                {
                    // Reset object value when changed if it has not all required components.
                    _property.objectReferenceValue = null;
                }
                else if (GetPickerObject(_id, _objectType, out Object _object))
                {
                    // Get newly selected object from picker if one.
                    _property.objectReferenceValue = _object;
                }
            }
        }

        // ===== Object Value ===== \\

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type, bool)"/>
        public static Object PickerField(Rect _position, Object _object, Type _objectType, Type _requiredType)
        {
            bool _allowSceneObjects = true;
            return PickerField(_position, _object, _objectType, _requiredType, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type, bool)"/>
        public static Object PickerField(Rect _position, Object _object, Type _objectType, Type _requiredType, bool _allowSceneObjects)
        {
            GUIContent _label = GUIContent.none;
            return PickerField(_position, _label, _object, _objectType, _requiredType, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type, bool)"/>
        public static Object PickerField(Rect _position, string _label, Object _object, Type _objectType, Type _requiredType)
        {
            bool _allowSceneObjects = true;
            return PickerField(_position, _label, _object, _objectType, _requiredType, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type, bool)"/>
        public static Object PickerField(Rect _position, string _label, Object _object, Type _objectType, Type _requiredType, bool _allowSceneObjects)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return PickerField(_position, _labelGUI, _object, _objectType, _requiredType, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type, bool)"/>
        public static Object PickerField(Rect _position, GUIContent _label, Object _object, Type _objectType, Type _requiredType)
        {
            bool _allowSceneObjects = true;
            return PickerField(_position, _label, _object, _objectType, _requiredType, _allowSceneObjects);
        }

        /// <param name="_requiredType"><inheritdoc cref="PickerField(Rect, SerializedProperty, GUIContent, Type)" path="/param[@name='_requiredType']"/></param>
        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type[], bool)"/>
        public static Object PickerField(Rect _position, GUIContent _label, Object _object, Type _objectType, Type _requiredType, bool _allowSceneObjects)
        {
            pickerRequiredType[0] = _requiredType;
            return PickerField(_position, _label, _object, _objectType, pickerRequiredType, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type[], bool)"/>
        public static Object PickerField(Rect _position, Object _object, Type _objectType, Type[] _requiredTypes)
        {
            bool _allowSceneObjects = true;
            return PickerField(_position, _object, _objectType, _requiredTypes, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type[], bool)"/>
        public static Object PickerField(Rect _position, Object _object, Type _objectType, Type[] _requiredTypes, bool _allowSceneObjects)
        {
            GUIContent _label = GUIContent.none;
            return PickerField(_position, _label, _object, _objectType, _requiredTypes, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type[], bool)"/>
        public static Object PickerField(Rect _position, string _label, Object _object, Type _objectType, Type[] _requiredTypes)
        {
            bool _allowSceneObjects = true;
            return PickerField(_position, _label, _object, _objectType, _requiredTypes, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type[], bool)"/>
        public static Object PickerField(Rect _position, string _label, Object _object, Type _objectType, Type[] _requiredTypes, bool _allowSceneObjects)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return PickerField(_position, _labelGUI, _object, _objectType, _requiredTypes, _allowSceneObjects);
        }

        /// <inheritdoc cref="PickerField(Rect, GUIContent, Object, Type, Type[], bool)"/>
        public static Object PickerField(Rect _position, GUIContent _label, Object _object, Type _objectType, Type[] _requiredTypes)
        {
            bool _allowSceneObjects = true;
            return PickerField(_position, _label, _object, _objectType, _requiredTypes, _allowSceneObjects);
        }

        /// <param name="_object"><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/param[@name='_object']"/></param>
        /// <param name="_objectType"><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/param[@name='_objectType']"/></param>
        /// <param name="_allowSceneObjects"><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/param[@name='_allowSceneObjects']"/></param>
        /// <returns><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/returns"/></returns>
        /// <inheritdoc cref="PickerField(Rect, SerializedProperty, GUIContent, Type[])"/>
        public static Object PickerField(Rect _position, GUIContent _label, Object _object, Type _objectType, Type[] _requiredTypes, bool _allowSceneObjects)
        {
            // In order for the picker to work, the target object must either be a GameObject or a Component.
            if (!EnhancedEditorUtility.IsSceneObject(_objectType))
            {
                _object = EditorGUI.ObjectField(_position, _label, _object, _objectType, _allowSceneObjects);
                return _object;
            }

            // Picker field.
            int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
            _position = DoPickerField(_position, _id, _object, _objectType, _requiredTypes, _allowSceneObjects);

            using (var _scope = new EditorGUI.ChangeCheckScope())
            {
                _object = EditorGUI.ObjectField(_position, _label, _object, _objectType, _allowSceneObjects);

                if (_scope.changed && ResetPickerObjectIfDontMatch(_object, _requiredTypes))
                {
                    // Reset object value when changed if it has not all required components.
                    _object = null;
                }
                else if (GetPickerObject(_id, _objectType, out Object _value))
                {
                    // Get newly selected object from picker if one.
                    _object = _value;
                }
            }

            return _object;
        }

        // -----------------------

        private static Rect DoPickerField(Rect _position, int _id, Object _object, Type _objectType, Type[] _requiredTypes, bool _allowSceneObjects)
        {
            // Get adjusted field rectangle on screen.
            Rect _fieldPosition = new Rect(_position)
            {
                width = _position.width - (EnhancedEditorGUIUtility.IconWidth + 2f)
            };

            // Reject any drag and drop operation with non eligible object.
            if (_fieldPosition.Event(out Event _event) == EventType.DragUpdated)
            {
                Object[] _drop = DragAndDrop.objectReferences;
                if ((_drop.Length != 1) || ResetPickerObjectIfDontMatch(_drop[0], _requiredTypes))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    _event.Use();
                }
            }

            // Picker button.
            _position.xMin = _fieldPosition.xMax + 2f;
            _position.width = EnhancedEditorGUIUtility.IconWidth;

            if (PickerButtonGUI.image == null)
                PickerButtonGUI.image = EditorGUIUtility.FindTexture("Search Icon");

            if (IconButton(_position, PickerButtonGUI))
            {
                if (_objectType == typeof(GameObject))
                {
                    GameObject _gameObject = _object as GameObject;
                    ObjectPickerWindow.GetWindow(_id, _gameObject, _requiredTypes, _allowSceneObjects, null);
                }
                else
                {
                    Component _component = _object as Component;
                    ObjectPickerWindow.GetWindow(_id, _component, _objectType, _requiredTypes, _allowSceneObjects, null);
                }
            }

            return _fieldPosition;
        }

        internal static bool ResetPickerObjectIfDontMatch(Object _object, Type[] _requiredTypes)
        {
            if (_object == null)
                return false;

            GameObject _gameObject = (_object is Component _component)
                                   ? _component.gameObject
                                   : (_object as GameObject);

            bool _doReset = _requiredTypes.Any(t => EnhancedEditorUtility.IsComponentOrInterface(t) && !_gameObject.TryGetComponent(t, out _));
            return _doReset;
        }

        private static bool GetPickerObject(int _id, Type _type, out Object _object)
        {
            if (_type == typeof(GameObject))
            {
                if (ObjectPickerWindow.GetSelectedObject(_id, out GameObject _gameObject))
                {
                    _object = _gameObject;
                    return true;
                }
            }
            else if (ObjectPickerWindow.GetSelectedObject(_id, out Component _component))
            {
                _object = _component;
                return true;
            }

            _object = null;
            return false;
        }
        #endregion

        #region Precision Slider
        /// <summary>
        /// Control id as key, precision slider based value as Vector x component, current value as Vector y component.
        /// </summary>
        private static readonly Dictionary<int, Vector2> precisionSliders = new Dictionary<int, Vector2>();
        private static readonly SerializedPropertyType[] PrecisionSliderCompatibleTypes = new SerializedPropertyType[]
                                                                                                {
                                                                                                    SerializedPropertyType.Integer,
                                                                                                    SerializedPropertyType.Float
                                                                                                };

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="PrecisionSliderField(Rect, SerializedProperty, GUIContent, float, float, float, out float)"/>
        public static void PrecisionSliderField(Rect _position, SerializedProperty _property, float _minValue, float _maxValue, float _precision, out float _extraHeight)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            PrecisionSliderField(_position, _property, _label, _minValue, _maxValue, _precision, out _extraHeight);
        }

        /// <inheritdoc cref="PrecisionSliderField(Rect, SerializedProperty, GUIContent, float, float, float, out float)"/>
        public static void PrecisionSliderField(Rect _position, SerializedProperty _property, string _label, float _minValue, float _maxValue, float _precision,
                                                out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            PrecisionSliderField(_position, _property, _labelGUI, _minValue, _maxValue, _precision, out _extraHeight);
        }

        /// <summary>
        /// Makes a slider coupled with an extra secondary slider, used to adjust its value more precisely.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make a precision slider field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_minValue">Slider minimum allowed value.</param>
        /// <param name="_maxValue">Slider maximum allowed value.</param>
        /// <param name="_precision">Extra slider precision. This represents half of the difference between its minimum and maximum value.</param>
        /// <param name="_extraHeight"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_extraHeight']"/></param>
        public static void PrecisionSliderField(Rect _position, SerializedProperty _property, GUIContent _label, float _minValue, float _maxValue, float _precision,
                                                out float _extraHeight)
        {
            // Incompatible property management.
            if (!ArrayUtility.Contains(PrecisionSliderCompatibleTypes, _property.propertyType))
            {
                EditorGUI.PropertyField(_position, _property, _label);
                _extraHeight = 0f;

                return;
            }

            // Rect height calculs.
            Rect _temp = new Rect(_position);
            bool _foldout = _property.isExpanded;

            _extraHeight = GetPrecisionSliderExtraHeight(_foldout);
            _position.height += _extraHeight;

            // Precision slider.
            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                switch (_property.propertyType)
                {
                    // Int.
                    case SerializedPropertyType.Integer:
                        int _int = _property.intValue;
                        _int = PrecisionSliderField(_temp, _label, _int, (int)_minValue, (int)_maxValue, (int)_precision, ref _foldout, out _);

                        if (_changeCheck.changed)
                        {
                            _property.intValue = _int;
                        }
                        break;

                    // Float.
                    case SerializedPropertyType.Float:
                        float _float = _property.floatValue;
                        _float = PrecisionSliderField(_temp, _label, _float, _minValue, _maxValue, _precision, ref _foldout, out _);

                        if (_changeCheck.changed)
                        {
                            _property.floatValue = _float;
                        }
                        break;

                    default:
                        break;
                }

                // Save foldout state.
                if (_foldout != _property.isExpanded)
                {
                    _property.isExpanded = _foldout;
                }
            }
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="PrecisionSliderField(Rect, GUIContent, float, float, float, float, ref bool, out float)"/>
        public static float PrecisionSliderField(Rect _position, float _value, float _minValue, float _maxValue, float _precision, ref bool _foldout, out float _extraHeight)
        {
            GUIContent _label = GUIContent.none;
            return PrecisionSliderField(_position, _label, _value, _minValue, _maxValue, _precision, ref _foldout, out _extraHeight);
        }

        /// <inheritdoc cref="PrecisionSliderField(Rect, GUIContent, float, float, float, float, ref bool, out float)"/>
        public static float PrecisionSliderField(Rect _position, string _label, float _value, float _minValue, float _maxValue, float _precision, ref bool _foldout,
                                                 out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return PrecisionSliderField(_position, _labelGUI, _value, _minValue, _maxValue, _precision, ref _foldout, out _extraHeight);
        }

        /// <param name="_value">The value the slider shows.</param>
        /// <param name="_foldout"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_foldout']"/></param>
        /// <returns>New slider value set by the user.</returns>
        /// <inheritdoc cref="PrecisionSliderField(Rect, SerializedProperty, GUIContent, float, float, float, out float)"/>
        public static float PrecisionSliderField(Rect _position, GUIContent _label, float _value, float _minValue, float _maxValue, float _precision, ref bool _foldout,
                                                 out float _extraHeight)
        {
            _value = DoPrecisionSliderField(_position, _label, _value, _minValue, _maxValue, _precision, ref _foldout, false);
            _extraHeight = GetPrecisionSliderExtraHeight(_foldout);

            return _value;
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="PrecisionSliderField(Rect, GUIContent, int, int, int, int, ref bool, out float)"/>
        public static int PrecisionSliderField(Rect _position, int _value, int _minValue, int _maxValue, int _precision, ref bool _foldout, out float _extraHeight)
        {
            GUIContent _label = GUIContent.none;
            return PrecisionSliderField(_position, _label, _value, _minValue, _maxValue, _precision, ref _foldout, out _extraHeight);
        }

        /// <inheritdoc cref="PrecisionSliderField(Rect, GUIContent, int, int, int, int, ref bool, out float)"/>
        public static int PrecisionSliderField(Rect _position, string _label, int _value, int _minValue, int _maxValue, int _precision, ref bool _foldout,
                                               out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return PrecisionSliderField(_position, _labelGUI, _value, _minValue, _maxValue, _precision, ref _foldout, out _extraHeight);
        }

        /// <inheritdoc cref="PrecisionSliderField(Rect, GUIContent, float, float, float, float, ref bool, out float)"/>
        public static int PrecisionSliderField(Rect _position, GUIContent _label, int _value, int _minValue, int _maxValue, int _precision, ref bool _foldout,
                                               out float _extraHeight)
        {
            float _newValue = DoPrecisionSliderField(_position, _label, _value, _minValue, _maxValue, _precision, ref _foldout, true);
            _extraHeight = GetPrecisionSliderExtraHeight(_foldout);

            return (int)_newValue;
        }

        // -----------------------

        private static float DoPrecisionSliderField(Rect _position, GUIContent _label, float _value, float _minValue, float _maxValue, float _precision, ref bool _foldout,
                                                    bool _roundValue)
        {
            // Get a unique id for this control, used to track its precision slider reference value.
            int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
            if (!precisionSliders.ContainsKey(_id))
            {
                precisionSliders.Add(_id, new Vector2(_value, _value));
            }

            // Label.
            _position = EditorGUI.PrefixLabel(_position, _label);

            // Foldout & main slider.
            using (var _scope = ZeroIndentScope())
            {
                _position = DrawFoldout(_position, ref _foldout);
                _value = EditorGUI.Slider(_position, _value, _minValue, _maxValue);

                if (_roundValue)
                {
                    _value = Mathf.Round(_value);
                }
            }

            // Precision slider.
            if (_foldout)
            {
                _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
                _position.height = EditorGUIUtility.singleLineHeight;

                Rect _temp = new Rect(_position)
                {
                    width = EditorGUIUtility.fieldWidth
                };

                // Precision slider min and max values.
                float _min = Mathf.Max(_minValue, precisionSliders[_id].x - _precision);
                float _max = Mathf.Min(_maxValue, precisionSliders[_id].x + _precision);

                using (var _scope = ZeroIndentScope())
                {
                    EditorGUI.SelectableLabel(_temp, _min.ToString(), EditorStyles.numberField);

                    _temp.x = _position.xMax - _temp.width;
                    EditorGUI.SelectableLabel(_temp, _max.ToString(), EditorStyles.numberField);

                    // If the value changed outside of the precision slider, reset its reference value.
                    if (precisionSliders[_id].y != _value)
                    {
                        precisionSliders[_id] = new Vector2(_value, _value);
                    }

                    // Precision Slider.
                    _position.xMin += _temp.width + 5f;
                    _position.xMax -= _temp.width + 5f;

                    _value = GUI.HorizontalSlider(_position, _value, _min, _max);
                    if (_roundValue)
                    {
                        _value = Mathf.Round(_value);
                    }
                }
                    
                // Update this precision slider reference value.
                precisionSliders[_id] = new Vector2(precisionSliders[_id].x, _value);
            }

            return _value;
        }

        internal static float GetPrecisionSliderExtraHeight(bool _foldout)
        {
            float _extraHeight = _foldout
                               ? (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing)
                               : 0f;

            return _extraHeight;
        }
        #endregion

        #region Progress Bar
        private static bool isDraggingProgressBar = false;
        private static int draggingProgressBarControlID = 0;

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="ProgressBar(Rect, SerializedProperty, GUIContent, MemberValue{float}, Color, bool)"/>
        public static void ProgressBar(Rect _position, SerializedProperty _property, MemberValue<float> _maxMember, Color _color, bool _isEditable = false)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_position, _property, _label, _maxMember, _color, _isEditable);
        }

        /// <inheritdoc cref="ProgressBar(Rect, SerializedProperty, GUIContent, MemberValue{float}, Color, bool)"/>
        public static void ProgressBar(Rect _position, SerializedProperty _property, string _label, MemberValue<float> _maxMember, Color _color, bool _isEditable = false)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ProgressBar(_position, _property, _labelGUI, _maxMember, _color, _isEditable);
        }

        /// <param name="_maxMember">Class member to get value from,
        /// acting as this bar maximum value and used to determine its filled amount. Minimum value is always 0.
        /// <para/>
        /// Can either be a field, a property or a method, but its value must be convertible to <see cref="float"/>.</param>
        /// <inheritdoc cref="ProgressBar(Rect, SerializedProperty, GUIContent, float, Color, bool)"/>
        public static void ProgressBar(Rect _position, SerializedProperty _property, GUIContent _label, MemberValue<float> _maxMember, Color _color, bool _isEditable = false)
        {
            // Incompatible max value management.
            if (!_maxMember.GetValue(_property.serializedObject, out float _maxFloatValue))
            {
                // Debug message.
                Object _target = _property.serializedObject.targetObject;
                _target.LogWarning($"Could not get the value of the class member \"{_maxMember.Name}\" in the script \"{_target.GetType()}\".");

                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            ProgressBar(_position, _property, _label, _maxFloatValue, _color, _isEditable);
        }

        /// <inheritdoc cref="ProgressBar(Rect, SerializedProperty, GUIContent, float, Color, bool)"/>
        public static void ProgressBar(Rect _position, SerializedProperty _property, float _maxValue, Color _color, bool _isEditable = false)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ProgressBar(_position, _property, _label, _maxValue, _color, _isEditable);
        }

        /// <inheritdoc cref="ProgressBar(Rect, SerializedProperty, GUIContent, float, Color, bool)"/>
        public static void ProgressBar(Rect _position, SerializedProperty _property, string _label, float _maxValue, Color _color, bool _isEditable = false)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ProgressBar(_position, _property, _labelGUI, _maxValue, _color, _isEditable);
        }

        /// <summary>
        /// Makes a progress bar using a <see cref="SerializedProperty"/> value as filled amount.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to use value as progress bar filled amount.</param>
        /// <param name="_label">Label displayed in the middle of the progress bar.</param>
        /// <param name="_maxValue">Maximum bar value, used to determine its filled amount. Minimum value is always 0.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_isEditable">Is this progress bar value editable (draggable) by users?</param>
        public static void ProgressBar(Rect _position, SerializedProperty _property, GUIContent _label, float _maxValue, Color _color, bool _isEditable = false)
        {
            // Incompatible property management.
            if (!EnhancedEditorUtility.GetSerializedPropertyValueAsSingle(_property, out float _value))
            {
                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            // Progress bar.
            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
            {
                float _newValue = DoProgressBar(_position, _label, _value, _maxValue, _color, _isEditable, _property.hasMultipleDifferentValues);
                if (_newValue != _value)
                {
                    EnhancedEditorUtility.SetSerializedPropertyValueAsSingle(_property, _newValue);
                }
            }
        }

        // ===== Float Value ===== \\

        /// <inheritdoc cref="ProgressBar(Rect, GUIContent, float, float, Color, bool)"/>
        public static float ProgressBar(Rect _position, float _value, float _maxValue, Color _color, bool _isEditable = false)
        {
            GUIContent _label = GUIContent.none;
            return ProgressBar (_position, _label, _value, _maxValue, _color, _isEditable);
        }

        /// <inheritdoc cref="ProgressBar(Rect, GUIContent, float, float, Color, bool)"/>
        public static float ProgressBar(Rect _position, string _label, float _value, float _maxValue, Color _color, bool _isEditable = false)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return ProgressBar (_position, _labelGUI, _value, _maxValue, _color, _isEditable);
        }

        /// <summary>
        /// Makes a progress bar with a specified filled amount.
        /// </summary>
        /// <param name="_value">Progress bar filled amount.</param>
        /// <returns>New bar value (filled amount) if editable, or the same value otherwise.</returns>
        /// <inheritdoc cref="ProgressBar(Rect, SerializedProperty, GUIContent, float, Color, bool)"/>
        public static float ProgressBar(Rect _position, GUIContent _label, float _value, float _maxValue, Color _color, bool _isEditable = false)
        {
            _value = DoProgressBar(_position, _label, _value, _maxValue, _color, _isEditable, false);
            return _value;
        }

        // ===== Int Value ===== \\

        /// <inheritdoc cref="ProgressBar(Rect, GUIContent, int, int, Color, bool)"/>
        public static int ProgressBar(Rect _position, int _value, int _maxValue, Color _color, bool _isEditable = false)
        {
            GUIContent _label = GUIContent.none;
            return ProgressBar(_position, _label, _value, _maxValue, _color, _isEditable);
        }

        /// <inheritdoc cref="ProgressBar(Rect, GUIContent, int, int, Color, bool)"/>
        public static int ProgressBar(Rect _position, string _label, int _value, int _maxValue, Color _color, bool _isEditable = false)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return ProgressBar(_position, _labelGUI, _value, _maxValue, _color, _isEditable);
        }

        /// <inheritdoc cref="ProgressBar(Rect, GUIContent, float, float, Color, bool)"/>
        public static int ProgressBar(Rect _position, GUIContent _label, int _value, int _maxValue, Color _color, bool _isEditable = false)
        {
            float _newValue = DoProgressBar(_position, _label, _value, _maxValue, _color, _isEditable, false);
            return Mathf.RoundToInt(_newValue);
        }

        // -----------------------

        private static float DoProgressBar(Rect _position, GUIContent _label, float _value, float _maxValue, Color _color, bool _isEditable, bool _hasDifferentValues)
        {
            _position = EditorGUI.IndentedRect(_position);

            // First, draw filled bar portion.
            Rect _temp = new Rect(_position)
            {
                width = _hasDifferentValues
                      ? 0f
                      : (_position.width * Mathf.Clamp(_value / _maxValue, 0f, 1f))
            };

            EditorGUI.DrawRect(_temp, _color);

            // Then, draw empty portion (if not fully filled).
            if (_temp.width < _position.width)
            {
                _temp.x += _temp.width;
                _temp.width = _position.width - _temp.width;

                EditorGUI.DrawRect(_temp, SuperColor.SmokyBlack.Get());
            }

            string _text = _label.text;
            _label.text = string.IsNullOrEmpty(_text)
                        ? string.Empty
                        : ($"[{_label.text}]" +
                           $"{((_position.height > (EditorGUIUtility.singleLineHeight * 2f)) ? "\n" : "     ")}");

            if (!_hasDifferentValues)
            {
                string _valueString = (_value == (int)_value)
                                    ? _value.ToString()
                                    : $"{_value:n2}";

                string _maxValueString = (_maxValue == (int)_maxValue)
                                       ? _maxValue.ToString()
                                       : $"{_maxValue:n2}";

                _label.text += $"{_valueString} / {_maxValueString}";
            }
            else
                _label.text += "-";

            // Draw a middle-centered label in shadow style, for better readability.
            EditorGUI.DropShadowLabel(_position, _label, EnhancedEditorStyles.DropShadowCenteredLabel);

            _label.text = _text;

            if (!_isEditable)
                return _value;

            // Editable progress bar.
            Event _event = Event.current;
            int _controlID = EnhancedEditorGUIUtility.GetControlID(FocusType.Passive, _position);

            // Allow the users to drag the progress bar actual value.
            if (!isDraggingProgressBar)
            {
                _temp.x -= 5f;
                _temp.width = 10f;

                // Change cursor when at the edge of filled bar.
                EditorGUIUtility.AddCursorRect(_temp, MouseCursor.ResizeHorizontal);
                if ((_event.GetTypeForControl(_controlID) == EventType.MouseDown) && _temp.Contains(_event.mousePosition))
                {
                    GUIUtility.hotControl = _controlID;
                    draggingProgressBarControlID = _controlID;
                    isDraggingProgressBar = true;

                    _event.Use();
                }
            }
            else if (_controlID == draggingProgressBarControlID)
            {
                EditorGUIUtility.AddCursorRect(_position, MouseCursor.ResizeHorizontal);
                if (_event.GetTypeForControl(_controlID) == EventType.MouseDrag)
                {
                    // Update progress bar value on drag.
                    GUIUtility.hotControl = _controlID;
                    GUI.changed = true;

                    _value = (float)Math.Round(((_event.mousePosition.x - _position.x) / _position.width) * _maxValue, 2);
                    _value = Mathf.Clamp(_value, 0f, _maxValue);

                    _event.Use();
                }
                else if (_event.GetTypeForControl(_controlID) == EventType.MouseUp)
                {
                    // Stop dragging on mouse button release.
                    GUIUtility.hotControl = 0;
                    isDraggingProgressBar = false;

                    _event.Use();
                }
            }

            return _value;
        }
        #endregion

        #region Readonly
        /// <inheritdoc cref="ReadonlyField(Rect, SerializedProperty, GUIContent, bool, bool)"/>
        public static void ReadonlyField(Rect _position, SerializedProperty _property)
        {
            bool _includeChildren = false;
            ReadonlyField(_position, _property, _includeChildren);
        }

        /// <inheritdoc cref="ReadonlyField(Rect, SerializedProperty, GUIContent, bool, bool)"/>
        public static void ReadonlyField(Rect _position, SerializedProperty _property, bool _includeChildren, bool _useRadioToggle = false)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ReadonlyField(_position, _property, _label, _includeChildren, _useRadioToggle);
        }

        /// <inheritdoc cref="ReadonlyField(Rect, SerializedProperty, GUIContent, bool, bool)"/>
        public static void ReadonlyField(Rect _position, SerializedProperty _property, string _label)
        {
            bool _includeChildren = false;
            ReadonlyField(_position, _property, _label, _includeChildren);
        }

        /// <inheritdoc cref="ReadonlyField(Rect, SerializedProperty, GUIContent, bool, bool)"/>
        public static void ReadonlyField(Rect _position, SerializedProperty _property, string _label, bool _includeChildren, bool _useRadioToggle = false)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ReadonlyField(_position, _property, _labelGUI, _includeChildren, _useRadioToggle);
        }

        /// <inheritdoc cref="ReadonlyField(Rect, SerializedProperty, GUIContent, bool, bool)"/>
        public static void ReadonlyField(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            bool _includeChildren = false;
            ReadonlyField(_position, _property, _label, _includeChildren);
        }

        /// <summary>
        /// Makes a readonly field for a property (can't be edited by users).
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make a readonly field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_includeChildren">If true the property including children is drawn; otherwise only the control itself (such as only a foldout but nothing below it).</param>
        /// <param name="_useRadioToggle">Determines if using a radio-style toggle with boolean properties or not.</param>
        public static void ReadonlyField(Rect _position, SerializedProperty _property, GUIContent _label, bool _includeChildren, bool _useRadioToggle = false)
        {
            using (var _scope = EnhancedGUI.GUIEnabled.Scope(false))
            {
                if ((_property.propertyType == SerializedPropertyType.Boolean) && _useRadioToggle)
                {
                    EditorGUI.Toggle(_position, _label, _property.boolValue, EditorStyles.radioButton);
                }
                else
                {
                    EditorGUI.PropertyField(_position, _property, _label, _includeChildren);
                }
            }
        }
        #endregion

        #region Required
        private const string RequiredMessage = "Keep in mind to set a reference to this field!";

        private static readonly GUIContent requiredGetReferenceGUI = new GUIContent("Get Reference", "Get an object reference from this GameObject.");

        private static int selectedRequiredID = -1;
        private static bool selectRequiredObject = false;

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="RequiredField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void RequiredField(Rect _position, SerializedProperty _property, out float _extraHeight)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            RequiredField(_position, _property, _label, out _extraHeight);
        }

        /// <inheritdoc cref="RequiredField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void RequiredField(Rect _position, SerializedProperty _property, string _label, out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            RequiredField(_position, _property, _labelGUI, out _extraHeight);
        }

        /// <summary>
        /// Makes a required property field, showing an error help box when the property object reference value is set to null.
        /// <br/> Useful as a reminder for users to set this property value.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to draw a required field for (should be of object reference type).</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_extraHeight"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_extraHeight']"/></param>
        public static void RequiredField(Rect _position, SerializedProperty _property, GUIContent _label, out float _extraHeight)
        {
            // Multiple different values and incompatible property management.
            if (_property.hasMultipleDifferentValues || (_property.propertyType != SerializedPropertyType.ObjectReference))
            {
                EditorGUI.PropertyField(_position, _property, _label);
                _extraHeight = 0f;

                return;
            }

            int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);

            // Required help box and property field.
            Object _object = _property.objectReferenceValue;

            Rect _temp = new Rect(_position);
            _position.height = _extraHeight
                             = RequiredHelpBox(_position, _label, _object);

            // Context click menu (only on the help box).
            Rect _helpBox = EditorGUI.IndentedRect(_position);

            if (EnhancedEditorGUIUtility.ContextClick(_helpBox))
            {
                GenericMenu _menu = new GenericMenu();
                AddRequiredUtilityToMenu(_id, _property, _menu);

                _menu.ShowAsContext();
            }

            // Required field.
            using (var _scope = new EditorGUI.PropertyScope(_position, GUIContent.none, _property))
            {
                _temp.y += _extraHeight;
                EditorGUI.PropertyField(_temp, _property, _label);

                // Set new object value.
                if (GetRequiredObject(_id, _property, out _object))
                {
                    _property.objectReferenceValue = _object;
                }
            }
        }

        // ===== Object Value ===== \\

        /// <inheritdoc cref="RequiredField(Rect, GUIContent, Object, Type, bool, out float)"/>
        public static Object RequiredField(Rect _position, Object _object, Type _objectType, out float _extraHeight)
        {
            bool _allowSceneObjects = true;
            return RequiredField(_position, _object, _objectType, _allowSceneObjects, out _extraHeight);
        }

        /// <inheritdoc cref="RequiredField(Rect, GUIContent, Object, Type, bool, out float)"/>
        public static Object RequiredField(Rect _position, Object _object, Type _objectType, bool _allowSceneObjects, out float _extraHeight)
        {
            GUIContent _label = GUIContent.none;
            return RequiredField(_position, _label, _object, _objectType, _allowSceneObjects, out _extraHeight);
        }

        /// <inheritdoc cref="RequiredField(Rect, GUIContent, Object, Type, bool, out float)"/>
        public static Object RequiredField(Rect _position, string _label, Object _object, Type _objectType, out float _extraHeight)
        {
            bool _allowSceneObjects = true;
            return RequiredField(_position, _label, _object, _objectType, _allowSceneObjects, out _extraHeight);
        }

        /// <inheritdoc cref="RequiredField(Rect, GUIContent, Object, Type, bool, out float)"/>
        public static Object RequiredField(Rect _position, string _label, Object _object, Type _objectType, bool _allowSceneObjects, out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return RequiredField(_position, _labelGUI, _object, _objectType, _allowSceneObjects, out _extraHeight);
        }

        /// <inheritdoc cref="RequiredField(Rect, GUIContent, Object, Type, bool, out float)"/>
        public static Object RequiredField(Rect _position, GUIContent _label, Object _object, Type _objectType, out float _extraHeight)
        {
            bool _allowSceneObjects = true;
            return RequiredField(_position, _label, _object, _objectType, _allowSceneObjects, out _extraHeight);
        }

        /// <summary>
        /// Makes a required object field, showing an error help box when the object reference value is set to null.
        /// <br/> Useful as a reminder for users to set this object value.
        /// </summary>
        /// <param name="_object"><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/param[@name='_object']"/></param>
        /// <param name="_objectType"><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/param[@name='_objectType']"/>.</param>
        /// <param name="_allowSceneObjects"><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/param[@name='_allowSceneObjects']"/></param>
        /// <returns><inheritdoc cref="DocumentationMethodObject(Object, Type, bool)" path="/returns"/></returns>
        /// <inheritdoc cref="RequiredField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static Object RequiredField(Rect _position, GUIContent _label, Object _object, Type _objectType, bool _allowSceneObjects, out float _extraHeight)
        {
            _extraHeight = RequiredHelpBox(_position, _label, _object);
            _position.y += _extraHeight;

            _object = EditorGUI.ObjectField(_position, _label, _object, _objectType, _allowSceneObjects);
            return _object;
        }

        // -----------------------

        /// <summary>
        /// Draws a required help box if a specific object reference value is set to null.
        /// </summary>
        /// <param name="_position">Position on the screen where to draw (height is automatically adjusted to match help box size).</param>
        /// <param name="_label">Label displayed in front of the field (used in the help box to indicate which field is concerned).</param>
        /// <param name="_object">If null, a required help box will be drawn.</param>
        /// <returns>The total height used to draw additional GUI controls. Use this to increment your GUI position.</returns>
        public static float RequiredHelpBox(Rect _position, GUIContent _label, Object _object)
        {
            if (!PrepareRequiredHelpBox(ref _position, _label, _object, out string _message))
                return 0f;

            float _height = _position.height;
            _position.height -= EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.HelpBox(_position, _message, UnityEditor.MessageType.Error);
            return _height;
        }

        internal static float GetRequiredExtraHeight(Rect _position, GUIContent _label, Object _object)
        {
            float _height = PrepareRequiredHelpBox(ref _position, _label, _object, out _)
                          ? _position.height
                          : 0f;

            return _height;
        }

        private static bool PrepareRequiredHelpBox(ref Rect _position, GUIContent _label, Object _object, out string _message)
        {
            if (_object != null)
            {
                _position.height = ManageDynamicControlHeight(_label, 0f);
                _message = string.Empty;

                return false;
            }

            _message = string.IsNullOrEmpty(_label.text)
                     ? RequiredMessage
                     : $"{_label.text.Trim()}: {RequiredMessage}";

            _position = EditorGUI.IndentedRect(_position);

            float _height = EnhancedEditorGUIUtility.GetHelpBoxHeight(_message, UnityEditor.MessageType.Error, _position.width) + EditorGUIUtility.standardVerticalSpacing;
            _position.height = ManageDynamicControlHeight(_label, _height);

            return true;
        }

        internal static void AddRequiredUtilityToMenu(int _id, SerializedProperty _property, GenericMenu _menu)
        {
            // This menu item can only be used on a GameObject component.
            if (!_property.serializedObject.isEditingMultipleObjects && (_property.serializedObject.targetObject is Component))
            {
                selectedRequiredID = _id;
                _menu.AddItem(requiredGetReferenceGUI, false, () =>
                {
                    selectRequiredObject = true;
                });
            }
        }

        internal static bool GetRequiredObject(int _id, SerializedProperty _property, out Object _object)
        {
            if (selectRequiredObject && (selectedRequiredID == _id) && EnhancedEditorUtility.FindSerializedObjectField(_property.serializedObject, _property.propertyPath,
                                                                                                                       out FieldInfo _field))
            {
                Component _component = _property.serializedObject.targetObject as Component;
                Type _type = _field.FieldType;

                // GetComponent requires the target type to either be a component or an interface.
                if (EnhancedEditorUtility.IsComponentOrInterface(_type) && _component.TryGetComponent(_type, out Component _objectComponent))
                {
                    _object = _objectComponent;
                }
                else if (_type == typeof(GameObject))
                {
                    _object = (_component.transform.childCount > 0)
                            ? _component.transform.GetChild(0).gameObject
                            : _component.gameObject;
                }
                else
                {
                    _object = null;
                }

                selectRequiredObject = false;
                GUI.changed = true;

                return true;
            }

            _object = null;
            return false;
        }
        #endregion

        #region Scene Asset
        private static readonly string SceneAssetTypeName = typeof(SceneAsset).Name;

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="SceneAssetField(Rect, SerializedProperty, GUIContent)"/>
        public static void SceneAssetField(Rect _position, SerializedProperty _property)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            SceneAssetField(_position, _property, _label);
        }

        /// <inheritdoc cref="SceneAssetField(Rect, SerializedProperty, GUIContent)"/>
        public static void SceneAssetField(Rect _position, SerializedProperty _property, string _label)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            SceneAssetField(_position, _property, _labelGUI);
        }

        /// <summary>
        /// Makes a scene asset selection field, allowing to select and assign a scene to a <see cref="SceneAsset"/> in the project.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to draw a scene asset field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        public static void SceneAssetField(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            SerializedProperty _guidProperty = _property.Copy();

            // Incompatible property management.
            if ((_guidProperty.type != SceneAssetTypeName) || !_guidProperty.Next(true))
            {
                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            string _guid = _guidProperty.stringValue;
            if (_property.propertyPath.Contains("Array."))
            {
                string _path = AssetDatabase.GUIDToAssetPath(_guid);
                string _name = string.IsNullOrEmpty(_path)
                             ? "None"
                             : Path.GetFileNameWithoutExtension(_path);

                _label = EnhancedEditorGUIUtility.GetLabelGUI(_name);
            }

            // Property field.
            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
            {
                _guidProperty.stringValue = DoSceneAssetField(_position, _label, _guid);
            }
        }

        // ===== Scene Asset Value ===== \\

        /// <inheritdoc cref="SceneAssetField(Rect, GUIContent, SceneAsset)"/>
        public static void SceneAssetField(Rect _position, SceneAsset _sceneAsset)
        {
            GUIContent _label = GUIContent.none;
            SceneAssetField(_position, _label, _sceneAsset);
        }

        /// <inheritdoc cref="SceneAssetField(Rect, GUIContent, SceneAsset)"/>
        public static void SceneAssetField(Rect _position, string _label, SceneAsset _sceneAsset)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            SceneAssetField(_position, _labelGUI, _sceneAsset);
        }

        /// <param name="_sceneAsset">The <see cref="SceneAsset"/> the field shows.</param>
        /// <inheritdoc cref="SceneAssetField(Rect, SerializedProperty, GUIContent)"/>
        public static void SceneAssetField(Rect _position, GUIContent _label, SceneAsset _sceneAsset)
        {
            string _guid = _sceneAsset.guid;
            _sceneAsset.guid = DoSceneAssetField(_position, _label, _guid);
        }

        // -----------------------

        private static string DoSceneAssetField(Rect _position, GUIContent _label, string _sceneGUID)
        {
            // Loads the database to ensure that one exist in the project.
            var _database = BuildSceneDatabase.Database;

            // Get the scene asset from registered guid.
            string _path = AssetDatabase.GUIDToAssetPath(_sceneGUID);
            UnityEditor.SceneAsset _scene;
            
            if (string.IsNullOrEmpty(_path))
            {
                _scene = null;

                if (!string.IsNullOrEmpty(_sceneGUID))
                {
                    _sceneGUID = string.Empty;
                    GUI.changed = true;
                }
            }
            else
                _scene = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(_path);

            // Scene asset field.
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                _scene = EditorGUI.ObjectField(_position, _label, _scene, typeof(UnityEditor.SceneAsset), false) as UnityEditor.SceneAsset;

                if (_changeCheck.changed)
                {
                    _sceneGUID = (_scene != null)
                                 ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_scene))
                                 : string.Empty;
                }
            }

            return _sceneGUID;
        }
        #endregion

        #region Text Area
        private static readonly float textAreaMinHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2f;

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="TextArea(Rect, SerializedProperty, GUIContent, bool, out float)"/>
        public static void TextArea(Rect _position, SerializedProperty _property, out float _totalHeight)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            TextArea(_position, _property, _label, out _totalHeight);
        }

        /// <inheritdoc cref="TextArea(Rect, SerializedProperty, GUIContent, bool, out float)"/>
        public static void TextArea(Rect _position, SerializedProperty _property, bool _isWide, out float _totalHeight)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            TextArea(_position, _property, _label, _isWide, out _totalHeight);
        }

        /// <inheritdoc cref="TextArea(Rect, SerializedProperty, GUIContent, bool, out float)"/>
        public static void TextArea(Rect _position, SerializedProperty _property, string _label, out float _totalHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TextArea(_position, _property, _labelGUI, out _totalHeight);
        }

        /// <inheritdoc cref="TextArea(Rect, SerializedProperty, GUIContent, bool, out float)"/>
        public static void TextArea(Rect _position, SerializedProperty _property, string _label, bool _isWide, out float _totalHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TextArea(_position, _property, _labelGUI, _isWide, out _totalHeight);
        }

        /// <inheritdoc cref="TextArea(Rect, SerializedProperty, GUIContent, bool, out float)"/>
        public static void TextArea(Rect _position, SerializedProperty _property, GUIContent _label, out float _totalHeight)
        {
            bool _isWide = false;
            TextArea(_position, _property, _label, _isWide, out _totalHeight);
        }

        /// <summary>
        /// Makes an enhanced text area field, with its height automatically adapting to the size of its content.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethodTotal(Rect, out float)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make an enhanced text area for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_isWide">If true, the text area will take the full width available on the given position.</param>
        /// <param name="_totalHeight"><inheritdoc cref="DocumentationMethodTotal(Rect, out float)" path="/param[@name='_totalHeight']"/></param>
        public static void TextArea(Rect _position, SerializedProperty _property, GUIContent _label, bool _isWide, out float _totalHeight)
        {
            // Incompatible property management.
            if (_property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(_position, _property, _label);
                _totalHeight = _position.height;

                return;
            }

            string _text = _property.stringValue;

            _position.height = _totalHeight
                             = GetTextAreaTotalHeight(_position, _label, _text, _isWide);

            // Text area.
            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                _text = DoTextArea(_position, _label, _text, _isWide);

                // Save new value.
                if (_changeCheck.changed)
                {
                    _property.stringValue = _text;
                }
            }
        }

        // ===== String Value ===== \\

        /// <inheritdoc cref="TextArea(Rect, GUIContent, string, bool, out float)"/>
        public static string TextArea(Rect _position, string _text, out float _totalHeight)
        {
            GUIContent _label = GUIContent.none;
            return TextArea(_position, _label, _text, out _totalHeight);
        }

        /// <inheritdoc cref="TextArea(Rect, GUIContent, string, bool, out float)"/>
        public static string TextArea(Rect _position, string _text, bool _isWide, out float _totalHeight)
        {
            GUIContent _label = GUIContent.none;
            return TextArea(_position, _label, _text, _isWide, out _totalHeight);
        }

        /// <inheritdoc cref="TextArea(Rect, GUIContent, string, bool, out float)"/>
        public static string TextArea(Rect _position, string _label, string _text, out float _totalHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return TextArea(_position, _labelGUI, _text, out _totalHeight);
        }

        /// <inheritdoc cref="TextArea(Rect, GUIContent, string, bool, out float)"/>
        public static string TextArea(Rect _position, string _label, string _text, bool _isWide, out float _totalHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return TextArea(_position, _labelGUI, _text, _isWide, out _totalHeight);
        }

        /// <inheritdoc cref="TextArea(Rect, GUIContent, string, bool, out float)"/>
        public static string TextArea(Rect _position, GUIContent _label, string _text, out float _totalHeight)
        {
            bool _isWide = false;
            return TextArea(_position, _label, _text, _isWide, out _totalHeight);
        }

        /// <param name="_text">The text to edit.</param>
        /// <returns>The text entered by the user.</returns>
        /// <inheritdoc cref="TextArea(Rect, SerializedProperty, GUIContent, bool, out float)"/>
        public static string TextArea(Rect _position, GUIContent _label, string _text, bool _isWide, out float _totalHeight)
        {
            _position.height = _totalHeight
                             = GetTextAreaTotalHeight(_position, _label, _text, _isWide);

            _text = DoTextArea(_position, _label, _text, _isWide);
            return _text;
        }

        // -----------------------

        private static string DoTextArea(Rect _position, GUIContent _label, string _text, bool _isWide)
        {
            if (_isWide)
            {
                Rect _temp = new Rect(_position)
                {
                    height = EditorGUIUtility.singleLineHeight
                };
                
                EditorGUI.LabelField(_temp, _label);

                _position.yMin += _temp.height + EditorGUIUtility.standardVerticalSpacing;
                _text = EditorGUI.TextArea(_position, _text, EnhancedEditorStyles.TextArea);
            }
            else
            {
                _position = EditorGUI.PrefixLabel(_position, _label);
                using (var _scope = ZeroIndentScope())
                {
                    _text = EditorGUI.TextArea(_position, _text, EnhancedEditorStyles.TextArea);
                }
            }
            
            return _text;
        }

        internal static float GetTextAreaTotalHeight(Rect _position, GUIContent _label, string _text, bool _isWide)
        {
            GUIContent _labelGUI = _label;
            float _height = 0f;

            if (_isWide)
            {
                _labelGUI = GUIContent.none;
                _height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            _position = InvisiblePrefixLabel(_position, _labelGUI);
            _height += Mathf.Max(textAreaMinHeight, EnhancedEditorStyles.TextArea.CalcHeight(new GUIContent(_text), _position.width) + 5f);

            return ManageDynamicControlHeight(_label, _height);
        }
        #endregion

        #region Validation Member
        /// <inheritdoc cref="ValidationMemberField(Rect, SerializedProperty, GUIContent, MemberValue{object})"/>
        public static void ValidationMemberField(Rect _position, SerializedProperty _property, MemberValue<object> _validationMember)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            ValidationMemberField(_position, _property, _label, _validationMember);
        }

        /// <inheritdoc cref="ValidationMemberField(Rect, SerializedProperty, GUIContent, MemberValue{object})"/>
        public static void ValidationMemberField(Rect _position, SerializedProperty _property, string _label, MemberValue<object> _validationMember)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            ValidationMemberField(_position, _property, _labelGUI, _validationMember);
        }

        /// <summary>
        /// Makes a property field associated with a validation member,
        /// which value is set to this property value whenever it changes.
        /// <para/>
        /// Use this to perform additional operations when this field value is changed in the inspector.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make a validation member field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_validationMember">Class member to set whenever this field value is changed.
        /// <para/>
        /// Can either be a field, a property or a one argument method, but it must be of the same type as this field.</param>
        public static void ValidationMemberField(Rect _position, SerializedProperty _property, GUIContent _label, MemberValue<object> _validationMember)
        {
            using (var _scope = new EditorGUI.PropertyScope(Rect.zero, GUIContent.none, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(_position, _property, _label);

                if (_changeCheck.changed)
                {
                    // First, apply modifications to update the target object(s) value.
                    _property.serializedObject.ApplyModifiedProperties();

                    // Then, set the validation member value to it.
                    SetValidationMemberValue(_property, _validationMember);
                }
            }
        }

        // -----------------------

        internal static void SetValidationMemberValue(SerializedProperty _property, MemberValue<object> _validationMember)
        {
            SerializedObject _object = _property.serializedObject;
            MemberValue<object> _propertyMember = _property.name;

            if (!_propertyMember.GetValue(_object, out object _value) || !_validationMember.SetValue(_object, _value))
            {
                // Debug message.
                Object _target = _object.targetObject;
                _target.LogWarning($"Could not assign the value \"{_value}\" to the class member \"{_validationMember.Name}\" in the script \"{_target.GetType()}\".");
            }
        }
        #endregion

        // --- Multi-Tags --- \\

        #region Tag
        private static readonly string TagTypeName = typeof(Tag).Name;

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="TagField(Rect, SerializedProperty, GUIContent)"/>
        public static void TagField(Rect _position, SerializedProperty _property)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            TagField(_position, _property, _label);
        }

        /// <inheritdoc cref="TagField(Rect, SerializedProperty, GUIContent)"/>
        public static void TagField(Rect _position, SerializedProperty _property, string _label)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TagField(_position, _property, _labelGUI);
        }

        /// <summary>
        /// Makes a field for selecting a <see cref="Tag"/>.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make a tag field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        public static void TagField(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            SerializedProperty _tagProperty = _property.Copy();

            // Incompatible property management.
            if ((_tagProperty.type != TagTypeName) || !_tagProperty.Next(true))
            {
                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }
            
            // Tag field.
            using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
            using (var _changeCheck = new EditorGUI.ChangeCheckScope())
            {
                // When the property as multiple different values, display the default unknown tag.
                long _id = _tagProperty.hasMultipleDifferentValues
                         ? -1
                         : _tagProperty.longValue;

                Tag _tag = new Tag(_id);
                _tag = TagField(_position, _label, _tag);

                // Save new value.
                if (_changeCheck.changed)
                {
                    _tagProperty.longValue = _tag.ID;
                }
            }
        }

        // ===== Tag ===== \\

        /// <inheritdoc cref="TagField(Rect, GUIContent, Tag)"/>
        public static Tag TagField(Rect _position, Tag _tag)
        {
            GUIContent _label = GUIContent.none;
            return TagField(_position, _label, _tag);
        }

        /// <inheritdoc cref="TagField(Rect, GUIContent, Tag)"/>
        public static Tag TagField(Rect _position, string _label, Tag _tag)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            return TagField(_position, _labelGUI, _tag);
        }

        /// <param name="_tag">The tag the field shows.</param>
        /// <returns>The tag that has been set by the user.</returns>
        /// <inheritdoc cref="TagField(Rect, SerializedProperty, GUIContent)"/>
        public static Tag TagField(Rect _position, GUIContent _label, Tag _tag)
        {
            // Label.
            _position = EditorGUI.PrefixLabel(_position, _label);

            int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
            MultiTags.GetTag(_tag, out TagData _data);

            // Selected tag.
            _label = EnhancedEditorGUIUtility.GetLabelGUI(_data.Name);
            _position.width = EnhancedEditorStyles.CNCountBadge.CalcSize(_label).x;

            using (var _scope = EnhancedGUI.GUIBackgroundColor.Scope(_data.Color))
            {
                using (var _indentScope = ZeroIndentScope())
                {
                    EditorGUI.LabelField(_position, _label, EnhancedEditorStyles.CNCountBadge);
                }
            }

            // Select a new tag using a custom generic menu instead of a classic popup.
            if (EnhancedEditorGUIUtility.MainMouseUp(_position))
            {
                GenericMenu _menu = GetTagSelectionMenu(_id, _data);
                _menu.DropDown(_position);
            }

            // Context menu.
            if (EnhancedEditorGUIUtility.ContextClick(_position))
            {
                GenericMenu _menu = GetTagContextMenu(_data);
                _menu.ShowAsContext();
            }

            // Get newly selected tag from menu.
            if (GetSelectedTag(_id, out Tag _newTag))
            {
                _tag = _newTag;
            }
            
            return _tag;
        }
        #endregion

        #region Tag Group
        private static readonly string TagGroupTypeName = typeof(TagGroup).Name;
        private static readonly List<TagData> tagGroupContent = new List<TagData>();

        // ===== Serialized Property ===== \\

        /// <inheritdoc cref="TagGroupField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void TagGroupField(Rect _position, SerializedProperty _property, out float _extraHeight)
        {
            GUIContent _label = EnhancedEditorGUIUtility.GetPropertyLabel(_property);
            TagGroupField(_position, _property, _label, out _extraHeight);
        }

        /// <inheritdoc cref="TagGroupField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void TagGroupField(Rect _position, SerializedProperty _property, string _label, out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TagGroupField(_position, _property, _labelGUI, out _extraHeight);
        }

        /// <summary>
        /// Makes a field for editing a <see cref="TagGroup"/>.
        /// </summary>
        /// <param name="_position">Rectangle on the screen to draw within (for one line only, the height will be automatically adjusted if needed).</param>
        /// <param name="_property"><see cref="SerializedProperty"/> to make a tag group field for.</param>
        /// <param name="_label"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_label']"/></param>
        /// <param name="_extraHeight"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_extraHeight']"/></param>
        public static void TagGroupField(Rect _position, SerializedProperty _property, GUIContent _label, out float _extraHeight)
        {
            SerializedProperty _tagGroup = _property.Copy();

            // Incompatible property management.
            if ((_tagGroup.type != TagGroupTypeName) || !_tagGroup.Next(true) || !_tagGroup.isArray)
            {
                EditorGUI.PropertyField(_position, _property, _label);
                _extraHeight = 0f;

                return;
            }

            // On property multiple different values, draw the unknown non-editable tag. 
            if (_tagGroup.hasMultipleDifferentValues)
            {
                using (var _scope = new EditorGUI.PropertyScope(_position, _label, _property))
                {
                    // Label.
                    _position = EditorGUI.PrefixLabel(_position, _label);

                    // Tag.
                    TagData _data = TagData.UnknownTag;

                    _label = EnhancedEditorGUIUtility.GetLabelGUI(_data.Name);
                    _position.width = EnhancedEditorStyles.CNCountBadge.CalcSize(_label).x;

                    using (var _indentScope = ZeroIndentScope())
                    {
                        EditorGUI.LabelField(_position, _label, EnhancedEditorStyles.CNCountBadge);
                    }
                }
                
                _extraHeight = 0f;
                return;
            }

            // Resize the content array if it is too small. Do not reallocate a new array for each call.
            if (tagGroupContent.Count < _tagGroup.arraySize)
            {
                tagGroupContent.Resize(_tagGroup.arraySize);
            }

            // Store the selection tag menu identifiers to display it later.
            int _changedTagControlID = -1;
            long _changedTagID = -2;
            Rect _changedTagPos = default;

            // As total position height is not defined yet, do not specify it.
            using (var _scope = new EditorGUI.PropertyScope(Rect.zero, _label, _property))
            {
                Rect _fieldPosition = EditorGUI.PrefixLabel(_position, _label);
                Rect _temp = new Rect(_fieldPosition);

                // Draw each tag in the group.
                for (int _i = 0; _i < _tagGroup.arraySize; _i++)
                {
                    SerializedProperty _tagProperty = _tagGroup.GetArrayElementAtIndex(_i);
                    _tagProperty.Next(true);

                    Tag _tag = new Tag(_tagProperty.longValue);

                    // Remove the tag if it is not referencing a valid data.
                    if (!_tag.GetData(out TagData _data))
                    {
                        _tagGroup.DeleteArrayElementAtIndex(_i);
                        _i--;

                        continue;
                    }

                    // Tag modifications.
                    using (var _changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
                        if (DrawTagGroupElement(_fieldPosition, ref _temp, ref _tag))
                        {
                            // Open the tag selection menu.
                            _changedTagControlID = _id;
                            _changedTagPos = _temp;
                            _changedTagID = _tag.ID;
                        }
                        else if (_changeCheck.changed || GetSelectedTag(_id, out _tag))
                        {
                            // New tag value.
                            _tagProperty.longValue = _tag.ID;
                        }

                        tagGroupContent[_i] = _data;
                        _temp.x += _temp.width + 5f;
                    }
                }

                // Remove undesired content.
                for (int _i = _tagGroup.arraySize; _i < tagGroupContent.Count; _i++)
                {
                    tagGroupContent[_i] = null;
                }

                // Only display the tag selection menu after all the group content has been registered.
                if (_changedTagControlID != -1)
                {
                    GenericMenu _menu = GetTagSelectionMenu(_changedTagControlID, MultiTags.GetTag(_changedTagID), tagGroupContent);
                    _menu.DropDown(_changedTagPos);

                    _changedTagID = -2;
                }

                // Add tag button.
                if (DrawTagGroupAddButton(_fieldPosition, ref _temp, tagGroupContent, out long _tagID))
                {
                    int _index = _tagGroup.arraySize;
                    _tagGroup.InsertArrayElementAtIndex(_index);

                    SerializedProperty _tagProperty = _tagGroup.GetArrayElementAtIndex(_index);
                    _tagProperty.Next(true);

                    _tagProperty.longValue = _tagID;
                }

                // Finally, register the total property position.
                _extraHeight = ManageDynamicControlHeight(_label, _temp.yMax - _position.yMax);
                _position.yMax = _temp.yMax;

                using (new EditorGUI.PropertyScope(_position, GUIContent.none, _property)) { }
            }
        }

        // ===== Tag Group ===== \\

        /// <inheritdoc cref="TagGroupField(Rect, GUIContent, TagGroup, out float)"/>
        public static void TagGroupField(Rect _position, TagGroup _group, out float _extraHeight)
        {
            GUIContent _label = GUIContent.none;
            TagGroupField(_position, _label, _group, out _extraHeight);
        }

        /// <inheritdoc cref="TagGroupField(Rect, GUIContent, TagGroup, out float)"/>
        public static void TagGroupField(Rect _position, string _label, TagGroup _group, out float _extraHeight)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            TagGroupField(_position, _labelGUI, _group, out _extraHeight);
        }

        /// <param name="_group"><see cref="TagGroup"/> to edit.</param>
        /// <inheritdoc cref="TagGroupField(Rect, SerializedProperty, GUIContent, out float)"/>
        public static void TagGroupField(Rect _position, GUIContent _label, TagGroup _group, out float _extraHeight)
        {
            // Label.
            Rect _fieldPosition = EditorGUI.PrefixLabel(_position, _label);
            Rect _temp = new Rect(_fieldPosition);

            // Draw each tag in the group.
            for (int _i = 0; _i < _group.Length; _i++)
            {
                Tag _tag = _group[_i];

                // Remove the tag if it is not referencing a valid data.
                if (!_tag.GetData(out TagData _data))
                {
                    _group.RemoveTag(_tag);
                    continue;
                }

                // Tag modifications.
                using (var _changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
                    if (DrawTagGroupElement(_fieldPosition, ref _temp, ref _tag))
                    {
                        // Open the tag selection menu.
                        GenericMenu _menu = GetTagSelectionMenu(_id, _data, _group.GetData());
                        _menu.DropDown(_temp);
                    }
                    else if (_changeCheck.changed || GetSelectedTag(_id, out _tag))
                    {
                        // New tag value.
                        _group[_i] = _tag;
                    }

                    _temp.x += _temp.width + 5f;
                }
            }

            // Add tag button.
            if (DrawTagGroupAddButton(_fieldPosition, ref _temp, _group.GetData(), out long _tagID))
            {
                Tag _tag = new Tag(_tagID);
                _group.AddTag(_tag);
            }

            _extraHeight = ManageDynamicControlHeight(_label, _temp.yMax - _position.yMax);
            _position.yMax = _temp.yMax;

            // Group context menu.
            if (EnhancedEditorGUIUtility.ContextClick(_position))
            {
                GenericMenu _menu = new GenericMenu();
                if (_group.Length > 1)
                {
                    _menu.AddItem(SortTagsGUI, false, _group.SortTagsByName);
                }
                else
                {
                    _menu.AddDisabledItem(SortTagsGUI);
                }

                _menu.ShowAsContext();
            }
        }

        // -----------------------

        private static bool DrawTagGroupElement(Rect _totalPosition, ref Rect _temp, ref Tag _tag)
        {
            // Tag.
            TagData _data = _tag.GetData();
            if (DrawTagGroupElement(_totalPosition, ref _temp, _data))
            {
                _tag.ID = -1;
            }

            // Opens the menu to select a new tag on tag click.
            return EnhancedEditorGUIUtility.MainMouseUp(_temp);
        }

        internal static bool DrawTagGroupElement(Rect _totalPosition, ref Rect _temp, TagData _tag)
        {
            // Label and position calculs.
            GUIContent _label = EnhancedEditorGUIUtility.GetLabelGUI(_tag.name);
            _temp.width = EnhancedEditorStyles.CNCountBadge.CalcSize(_label).x + EnhancedEditorGUIUtility.OlStyleSize;
            _temp = GetGUIPosition(_totalPosition, _temp);

            // Tag.
            Rect _tagTemp = new Rect(_temp);
            using (var _scope = EnhancedGUI.GUIBackgroundColor.Scope(_tag.Color))
            {
                using (var _indentScope = ZeroIndentScope())
                {
                    EditorGUI.LabelField(_temp, GUIContent.none, EnhancedEditorStyles.CNCountBadge);

                    _tagTemp.width -= EnhancedEditorGUIUtility.OlStyleSize;
                    EditorGUI.LabelField(_tagTemp, _label, EnhancedEditorStyles.CNCountBadge);
                }
            }

            // Remove button.
            _tagTemp.x = _tagTemp.xMax - 3f;
            _tagTemp.y -= 1f;
            _tagTemp.width = EnhancedEditorGUIUtility.OlStyleSize;

            bool _remove = false;
            if (GUI.Button(_tagTemp, GUIContent.none, EnhancedEditorStyles.OlMinus))
            {
                _remove = true;
                GUI.changed = true;
            }

            // Context menu.
            if (EnhancedEditorGUIUtility.ContextClick(_temp))
            {
                GenericMenu _menu = GetTagContextMenu(_tag);
                _menu.ShowAsContext();
            }

            return _remove;
        }

        private static bool DrawTagGroupAddButton(Rect _totalPosition, ref Rect _temp, List<TagData> _groupContent, out long _tagID)
        {
            _temp.y -= 1f;
            _temp.width = EnhancedEditorGUIUtility.OlStyleSize;
            _temp = GetGUIPosition(_totalPosition, _temp);

            int _id = EnhancedEditorGUIUtility.GetControlID(FocusType.Keyboard, _temp);

            // Add button.
            if (GUI.Button(_temp, GUIContent.none, EnhancedEditorStyles.OlPlus))
            {
                GenericMenu _menu = GetTagSelectionMenu(_id, null, _groupContent);
                _menu.DropDown(_temp);
            }

            // Get selected tag from menu.
            if (GetSelectedTag(_id, out Tag _tag))
            {
                _tagID = _tag.ID;
                return true;
            }

            _tagID = -1;
            return false;
        }

        internal static float GetTagGroupExtraHeight(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            SerializedProperty _tagGroup = _property.Copy();

            // Incompatible property management.
            if (_tagGroup.hasMultipleDifferentValues || (_tagGroup.type != TagGroupTypeName) || !_tagGroup.Next(true) || !_tagGroup.isArray)
            {
                return 0f;
            }

            _position = EditorGUI.PrefixLabel(_position, _label);
            Rect _temp = new Rect(_position);

            // Get total group position.
            for (int _i = 0; _i < _tagGroup.arraySize; _i++)
            {
                SerializedProperty _tagProperty = _tagGroup.GetArrayElementAtIndex(_i);
                _tagProperty.Next(true);

                Tag _tag = new Tag(_tagProperty.longValue);
                _label = EnhancedEditorGUIUtility.GetLabelGUI(_tag.Name);

                _temp.width = EnhancedEditorStyles.CNCountBadge.CalcSize(_label).x + EnhancedEditorGUIUtility.OlStyleSize;
                _temp = GetGUIPosition(_position, _temp);
                _temp.x += _temp.width + 5f;
            }

            // Increment with the add button position.
            _temp.width = EnhancedEditorGUIUtility.OlStyleSize;
            _temp = GetGUIPosition(_position, _temp);

            float _extraHeight = _temp.yMax - _position.yMax;
            return _extraHeight;
        }
        #endregion

        #region Tag Utility
        private static readonly GUIContent renameTagGUI = new GUIContent("Rename");
        private static readonly GUIContent setTagColorGUI = new GUIContent("Edit Color");
        private static readonly GUIContent createTagGUI = new GUIContent("Create a New Tag");
        private static readonly GUIContent openMultiTagsWindowGUI = new GUIContent("Open the Multi-Tags Window");

        private static int selectedTagControlID = -1;
        private static TagData selectedTag = null;

        // -----------------------

        /// <inheritdoc cref="GetTagSelectionMenu(int, TagData, List{TagData})"/>
        public static GenericMenu GetTagSelectionMenu(int _id, TagData _selectedTag)
        {
            return GetTagSelectionMenu(_id, _selectedTag, null);
        }

        /// <summary>
        /// Get a new <see cref="GenericMenu"/> used to select a tag.
        /// <br/> You can get the tag selected by the user with <see cref="GetSelectedTag(int, out Tag)"/>.
        /// </summary>
        /// <param name="_id">ID of the associated control (you can get it with <see cref="EnhancedEditorGUIUtility.GetControlID(GUIContent, FocusType, Rect)"/>.
        /// <br/> The same id will be needed to get the tag selected by the user with <see cref="GetSelectedTag(int, out Tag)"/>.</param>
        /// <param name="_selectedTag">The tag currently selected (to highlight in menu). Null if none.</param>
        /// <param name="_unselectableTags">All tags that can not be selected by the user. Null if none.</param>
        /// <returns><see cref="GenericMenu"/> to display for selecting a new tag.</returns>
        public static GenericMenu GetTagSelectionMenu(int _id, TagData _selectedTag, List<TagData> _unselectableTags)
        {
            GenericMenu _menu = new GenericMenu();
            selectedTagControlID = _id;

            foreach (TagData _tag in MultiTags.Database.tags)
            {
                GUIContent _label = new GUIContent(_tag.Name.Replace('_', '/'));
                if (_tag == _selectedTag)
                {
                    // Selected tag.
                    _menu.AddItem(_label, true, () => { });
                }
                else if ((_unselectableTags != null) && _unselectableTags.Contains(_tag))
                {
                    // Unselectable tag.
                    _menu.AddDisabledItem(_label, false);
                }
                else
                {
                    // Selectable tag.
                    _menu.AddItem(_label, false, () =>
                    {
                        selectedTag = _tag;
                    });
                }
            }

            // Additional menu utilities.
            _menu.AddSeparator(string.Empty);
            _menu.AddItem(createTagGUI, false, () =>
            {
                MultiTagsWindow.CreateTagWindow.GetWindow();
            });

            _menu.AddItem(openMultiTagsWindowGUI, false, () =>
            {
                MultiTagsWindow.GetWindow();
            });

            return _menu;
        }

        /// <summary>
        /// Get the <see cref="GenericMenu"/>  to be displayed on a tag context click.
        /// </summary>
        /// <param name="_tag">Selected tag.</param>
        /// <returns><see cref="GenericMenu"/> to be displayed.</returns>
        public static GenericMenu GetTagContextMenu(TagData _tag)
        {
            GenericMenu _menu = new GenericMenu();
            _menu.AddItem(renameTagGUI, false, () =>
            {
                MultiTagsWindow.RenameTagWindow.GetWindow(_tag);
            });

            _menu.AddItem(setTagColorGUI, false, () =>
            {
                EnhancedEditorUtility.ColorPicker(_tag.Color, (Color _color) =>
                {
                    MultiTags.SetTagColor(_tag.ID, _color);
                    InternalEditorUtility.RepaintAllViews();
                });
            });

            return _menu;
        }

        // -----------------------

        /// <summary>
        /// Get the tag selected by the user from a selection menu.
        /// </summary>
        /// <param name="_id">ID of the associated control (same as used for the selection menu).</param>
        /// <param name="_tag">Tag selected by the user.</param>
        /// <returns>True if the user selected a new tag from the selection menu, false otherwise.</returns>
        public static bool GetSelectedTag(int _id, out Tag _tag)
        {
            if ((selectedTagControlID == _id) && (selectedTag != null))
            {
                _tag = new Tag(selectedTag.ID);
                selectedTag = null;

                GUI.changed = true;
                return true;
            }

            _tag = default;
            return false;
        }
        #endregion

        // --- Various GUI Controls --- \\

        #region Background Line
        /// <inheritdoc cref="BackgroundLine(Rect, bool, int, Color, Color)"/>
        public static void BackgroundLine(Rect _position, bool _isSelected, int _index)
        {
            Color _selectedColor = EnhancedEditorGUIUtility.GUISelectedColor;
            Color _peerColor = EnhancedEditorGUIUtility.GUIPeerLineColor;

            BackgroundLine(_position, _isSelected, _index, _selectedColor, _peerColor);
        }

        /// <summary>
        /// Draws a background line of a certain color according to some specific conditions:
        /// <list type="bullet">
        /// <item>When this line is in a "selected" state</item>
        /// <item>If this line index is peer.</item>
        /// </list>
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_isSelected">Is this line selected?</param>
        /// <param name="_index">Index of this line.</param>
        /// <param name="_selectedColor">Color used to draw selected lines.</param>
        /// <param name="_peerColor">Color used to draw peer lines.</param>
        public static void BackgroundLine(Rect _position, bool _isSelected, int _index, Color _selectedColor, Color _peerColor)
        {
            if (_isSelected)
            {
                EditorGUI.DrawRect(_position, _selectedColor);
            }
            else if ((_index % 2) == 0)
            {
                EditorGUI.DrawRect(_position, _peerColor);
            }
        }
        #endregion

        #region Link Label
        /// <inheritdoc cref="LinkLabel(Rect, GUIContent, string)"/>
        public static void LinkLabel(Rect _position, string _label, string _url)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            LinkLabel(_position, _labelGUI, _url);
        }

        /// <summary>
        /// Draws a link label, redirecting to a specific url by clicking on it.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_label">Label to display.</param>
        /// <param name="_url">Redirection url.</param>
        public static void LinkLabel(Rect _position, GUIContent _label, string _url)
        {
            GUIStyle _style = EnhancedEditorStyles.LinkLabel;
            _position.width = _style.CalcSize(_label).x;

            Color _color = _position.Contains(Event.current.mousePosition)
                                            ? EnhancedEditorGUIUtility.LinkLabelActiveColor
                                            : EnhancedEditorGUIUtility.LinkLabelNormalColor;
            
            EditorGUIUtility.AddCursorRect(_position, MouseCursor.Link);
            UnderlinedLabel(_position, _label, _color, _style);

            if (EnhancedEditorGUIUtility.MainMouseUp(_position))
            {
                Application.OpenURL(_url);
            }

            // For the label color to be correctly displayed, constantly repaint the GUI (but only on repaint event).
            if (Event.current.type == EventType.Repaint)
            {
                GUI.changed = true;
            }
        }
        #endregion

        #region Underlined Label
        /// <inheritdoc cref="UnderlinedLabel(Rect, GUIContent, Color, GUIStyle)"/>
        public static void UnderlinedLabel(Rect _position, string _label)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            UnderlinedLabel(_position, _labelGUI);
        }

        /// <inheritdoc cref="UnderlinedLabel(Rect, GUIContent, Color, GUIStyle)"/>
        public static void UnderlinedLabel(Rect _position, string _label, Color _color)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            UnderlinedLabel(_position, _labelGUI, _color);
        }

        /// <inheritdoc cref="UnderlinedLabel(Rect, GUIContent, Color, GUIStyle)"/>
        public static void UnderlinedLabel(Rect _position, string _label, GUIStyle _style)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            UnderlinedLabel(_position, _labelGUI, _style);
        }

        /// <inheritdoc cref="UnderlinedLabel(Rect, GUIContent, Color, GUIStyle)"/>
        public static void UnderlinedLabel(Rect _position, string _label, Color _color, GUIStyle _style)
        {
            GUIContent _labelGUI = EnhancedEditorGUIUtility.GetLabelGUI(_label);
            UnderlinedLabel(_position, _labelGUI, _color, _style);
        }

        /// <inheritdoc cref="UnderlinedLabel(Rect, GUIContent, Color, GUIStyle)"/>
        public static void UnderlinedLabel(Rect _position, GUIContent _label)
        {
            GUIStyle _style = EditorStyles.label;
            UnderlinedLabel(_position, _label, _style);
        }

        /// <inheritdoc cref="UnderlinedLabel(Rect, GUIContent, Color, GUIStyle)"/>
        public static void UnderlinedLabel(Rect _position, GUIContent _label, Color _color)
        {
            GUIStyle _style = EditorStyles.label;
            UnderlinedLabel(_position, _label, _color, _style);
        }

        /// <inheritdoc cref="UnderlinedLabel(Rect, GUIContent, Color, GUIStyle)"/>
        public static void UnderlinedLabel(Rect _position, GUIContent _label, GUIStyle _style)
        {
            Color _color = Color.white;
            UnderlinedLabel(_position, _label, _color, _style);
        }

        /// <summary>
        /// Draws an underlined label.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_label">Label to display.</param>
        /// <param name="_color">Color of the label.</param>
        /// <param name="_style"><inheritdoc cref="DocumentationMethodExtra(Rect, ref bool, out float, GUIStyle)" path="/param[@name='_style']"/></param>
        public static void UnderlinedLabel(Rect _position, GUIContent _label, Color _color, GUIStyle _style)
        {
            using (var _scope = EnhancedGUI.GUIColor.Scope(_color))
            {
                EditorGUI.LabelField(_position, _label, _style);
            }

            _position = new Rect()
            {
                x = EditorGUI.IndentedRect(_position).x,
                y = _position.y + _position.height,
                height = 1f,
                width = _style.CalcSize(_label).x
            }; ;

            _color *= _style.normal.textColor;
            EditorGUI.DrawRect(_position, _color);
        }
        #endregion

        #region Texture
        /// <summary>
        /// Draws a texture, with its height automatically adjusted to the width of the position.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_texture">Texture to display.</param>
        /// <returns>Total height used to draw the texture.</returns>
        public static float Texture(Rect _position, Texture2D _texture)
        {
            // No texture, no draw.
            if (_texture == null)
                return ManageDynamicControlHeight(GUIContent.none, 0f);

            try
            {
                // Remove scroll size on width to avoid jitter.
                float _scrollSize = EnhancedEditorGUIUtility.ScrollSize + 5f;

                _position = EditorGUI.IndentedRect(_position);
                float _width = _position.width;

                _position.width = Mathf.Min(_position.width - _scrollSize, _texture.width);
                _position.x += (_width - _position.width) * .5f;

                float _ratio = _texture.height / (float)_texture.width;
                float _height = _position.height
                              = _position.width * _ratio;

                GUI.Label(_position, _texture);
                return ManageDynamicControlHeight(GUIContent.none, _height);
            }
            catch (MissingReferenceException)
            {
                return 0f;
            }
        }
        #endregion

        #region Toolbar Search Field
        private const float ToolbarSearchFieldCancelWidth = 14f;

        // -----------------------

        /// <inheritdoc cref="ToolbarSearchField(string, Rect, string)"/>
        public static string ToolbarSearchField(Rect _position, string _searchFilter)
        {
            string _controlName = EnhancedEditorGUIUtility.GetControlID(147, FocusType.Keyboard).ToString();
            return ToolbarSearchField(_controlName, _position, _searchFilter);
        }

        /// <summary>
        /// Makes a toolbar search field.
        /// </summary>
        /// <param name="_controlName">Optional name to use for this GUI control.</param>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_searchFilter">The search filter the field shows.</param>
        /// <returns>The search filter that has been set by the user.</returns>
        public static string ToolbarSearchField(string _controlName, Rect _position, string _searchFilter)
        {
            Event _event;
            Rect _buttonRect = new Rect(_position);
            _buttonRect.xMin = _buttonRect.xMax - ToolbarSearchFieldCancelWidth;

            // Clears filter on cancel button click.
            if (!string.IsNullOrEmpty(_searchFilter))
            {
                EditorGUIUtility.AddCursorRect(_buttonRect, MouseCursor.Arrow);
                if (_buttonRect.Event(out _event) == EventType.MouseUp)
                {
                    _searchFilter = string.Empty;

                    GUI.FocusControl(string.Empty);
                    GUI.changed = true;

                    _event.Use();
                }
            }

            // Set the search field control name and get if it was focused before drawing the field.
            GUI.SetNextControlName(_controlName);
            bool _isFocused = GUI.GetNameOfFocusedControl() == _controlName;

            // Search field.
            EditorGUIUtility.AddCursorRect(_position, MouseCursor.Text);
            _searchFilter = EditorGUI.TextField(_position, _searchFilter, EditorStyles.toolbarSearchField);

            // If the field is focused and the user clicks elsewhere, unfocus it.
            // If the event is already used and without any focus, unfocus this field before Unity refocus it by default.
            _event = Event.current;

            if (_isFocused && !_position.Contains(_event.mousePosition) &&
                ((_event.type == EventType.MouseDown) || ((_event.type == EventType.Used) && (_event.clickCount != 0) && string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))))
            {
                GUI.FocusControl(string.Empty);
                GUI.changed = true;
            }

            // Only display the cancel button when the search filter is non empty.
            if (!string.IsNullOrEmpty(_searchFilter))
            {
                GUI.Button(_buttonRect, GUIContent.none, EnhancedEditorStyles.ToolbarSearchFieldCancel);
            }

            return _searchFilter;
        }
        #endregion

        #region Toolbar Sort Options
        private static readonly GUIContent sortAscendingGUI = new GUIContent("↑", "Sort in ascending order.");
        private static readonly GUIContent sortDescendingGUI = new GUIContent("↓", "Sort in descending order.");

        // -----------------------

        /// <summary>
        /// Makes a toolbar sort options selection field.
        /// </summary>
        /// <param name="_position"><inheritdoc cref="DocumentationMethod(Rect, GUIContent)" path="/param[@name='_position']"/></param>
        /// <param name="_selectedOption">The index of the sorting option the field shows.</param>
        /// <param name="_doSortAscending">Is the sorting mode in ascending or in descending order?</param>
        /// <param name="_sortingOptions">An array of text, image and tooltips for the sorting options.</param>
        public static void ToolbarSortOptions(Rect _position, ref int _selectedOption, ref bool _doSortAscending, GUIContent[] _sortingOptions)
        {
            // Sorting option.
            _position.xMax -= EnhancedEditorGUIUtility.IconWidth;
            _selectedOption = EditorGUI.Popup(_position, _selectedOption, _sortingOptions, EditorStyles.toolbarDropDown);

            // Ascending / descending button.
            _position.x += _position.width;
            _position.width = EnhancedEditorGUIUtility.IconWidth;

            if (GUI.Button(_position, _doSortAscending ? sortDescendingGUI : sortAscendingGUI, EditorStyles.toolbarButton))
            {
                _doSortAscending = !_doSortAscending;
            }
        }
        #endregion

        // --- Utility --- \\

        #region Internal Utility
        private static readonly Dictionary<int, float> dynamicGUIControlHeight = new Dictionary<int, float>();
        internal static readonly GUIContent blankLabelGUI = new GUIContent(" ");

        // -----------------------

        internal static Rect InvisiblePrefixLabel(Rect _position, GUIContent _label)
        {
            GUIContent _prefixLabel = string.IsNullOrEmpty(_label.text)
                                    ? GUIContent.none
                                    : blankLabelGUI;

            _position = EditorGUI.PrefixLabel(_position, _prefixLabel);
            return _position;
        }

        internal static float ManageDynamicControlHeight(GUIContent _label, float _height)
        {
            // Get control id and register it.
            int _id = EnhancedEditorGUIUtility.GetControlID(_label, FocusType.Keyboard);
            if (!dynamicGUIControlHeight.ContainsKey(_id))
            {
                dynamicGUIControlHeight.Add(_id, _height);
            }

            // Only save its height on non layout event.
            EventType _eventType = Event.current.type;

            if ((_eventType != EventType.Layout) && (_eventType != EventType.Used) && (dynamicGUIControlHeight[_id] != _height))
            {
                dynamicGUIControlHeight[_id] = _height;

                // When the height has changed, set the GUI state as dirty to force repaint it.
                GUI.changed = true;
            }

            // Get saved control height.
            return dynamicGUIControlHeight[_id];
        }

        internal static bool IconButton(Rect _position, GUIContent _icon, float _margins = 0f)
        {
            GUIStyle _style = EnhancedEditorStyles.Button;
            return IconButton(_position, _icon, _style, _margins);
        }

        internal static bool IconButton(Rect _position, GUIContent _icon, GUIStyle _style, float _margins = 0f)
        {
            // Draw the icon outside of the button to avoid dealing with its margins.
            bool _click = GUI.Button(_position, GUIContent.none, _style);
            GUIStyle _labelStyle = GUI.skin.label;

            using (var _scope = EnhancedGUI.GUIStyleAlignment.Scope(_labelStyle, TextAnchor.MiddleCenter))
            {
                _position.width -= _margins;
                _position.height -= _margins;

                _position.x += _margins / 2f;
                _position.y += _margins / 2f;

                GUI.Label(_position, _icon, _labelStyle);
            }

            return _click;
        }

        private static Rect DrawFoldout(Rect _position, ref bool _foldout)
        {
            Rect _fieldPosition = new Rect(_position);
            {
                float _foldoutWidth = EnhancedEditorGUIUtility.FoldoutWidth;
                _fieldPosition.width -= _foldoutWidth;

                _position.x += _position.width;
                _position.width = _foldoutWidth;

                _foldout = EditorGUI.Foldout(_position, _foldout, GUIContent.none);
            }

            return _fieldPosition;
        }

        private static Rect GetGUIPosition(Rect _totalPosition, Rect _temp)
        {
            // Update a rect Y position if its width is too large for the screen.
            if ((_temp.xMax > (_totalPosition.xMax - 5f)) && (_temp.x > _totalPosition.x))
            {
                _temp.x = _totalPosition.x;
                _temp.y += _temp.height + EditorGUIUtility.standardVerticalSpacing;
            }

            return _temp;
        }

        internal static EditorGUI.IndentLevelScope ZeroIndentScope()
        {
            int _indentLevel = EditorGUI.indentLevel;
            var _scope = new EditorGUI.IndentLevelScope(-_indentLevel);

            return _scope;
        }
        #endregion

        #region Documentation
        /// <summary>
        /// This method is for documentation only, used by inheriting its parameters documentation to centralize it in one place.
        /// </summary>
        /// <param name="_position">Rectangle on the screen to draw within.</param>
        /// <param name="_label">Label displayed in front of the field.</param>
        internal static void DocumentationMethod(Rect _position, GUIContent _label) { }

        /// <param name="_object">The object the field shows.</param>
        /// <param name="_objectType">The type of the objects that can be assigned.</param>
        /// <param name="_allowSceneObjects">Allow or not to assign scene objects.</param>
        /// <returns>The object that has been set by the user.</returns>
        internal static Object DocumentationMethodObject(Object _object, Type _objectType, bool _allowSceneObjects) => null;

        /// <param name="_position">Rectangle on the screen to draw within (for the field only, the height will be automatically adjusted if needed).</param>
        /// <param name="_foldout">The shown foldout state.</param>
        /// <param name="_extraHeight">The extra height used to draw additional GUI controls. Use this to increment your GUI position.</param>
        /// <param name="_style">Optional <see cref="GUIStyle"/>.</param>
        internal static void DocumentationMethodExtra(Rect _position, ref bool _foldout, out float _extraHeight, GUIStyle _style) => _extraHeight = 0f;

        /// <param name="_position">Rectangle on the screen to draw within (the height will be automatically adjusted according to the content of the field).</param>
        /// <param name="_totalHeight">The total height used to draw this field. Use this to increment your GUI position.</param>
        internal static void DocumentationMethodTotal(Rect _position, out float _totalHeight) => _totalHeight = 0f;
        #endregion

        // --- Utility Windows --- \\

        #region Save / Rename Color Palette Window
        /// <summary>
        /// Utility window used to save or rename a <see cref="ColorPalette"/>.
        /// </summary>
        public class SaveRenameColorPaletteWindow : EditorWindow
        {
            /// <summary>
            /// Creates and shows a new <see cref="SaveRenameColorPaletteWindow"/> instance,
            /// used to save or rename a color palette in the project.
            /// </summary>
            /// <param name="_palette"><see cref="ColorPalette"/> to save or rename.</param>
            /// <param name="_doRename">If true, the palette will be renamed instead of saved in the database.</param>
            /// <returns><see cref="SaveRenameColorPaletteWindow"/> instance on screen.</returns>
            public static SaveRenameColorPaletteWindow GetWindow(ColorPalette _palette, bool _doRename = false)
            {
                string _title = _doRename
                              ? "Rename Color Palette"
                              : "Save Color Palette";

                SaveRenameColorPaletteWindow _window = GetWindow<SaveRenameColorPaletteWindow>(true, _title, true);

                _window.colorPalette = _palette;
                _window.paletteName = _palette.Name;
                _window.doRename = _doRename;

                _window.minSize = _window.maxSize
                                = new Vector2(310f, 70f);

                _window.ShowUtility();
                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            private const string UndoRecordTitle = "New Color Palette Name Changes";

            private const string EmptyNameMessage = "A Palette name cannot be null or empty!";
            private const string ExistingPaletteMessage = "A similar Palette with this name already exist.";
            private const string NameTooltip = "You can use underscores ( \" _ \" ) to create categories in the selection menus.";

            private readonly GUIContent paletteNameGUI = new GUIContent("Palette Name", "Name of this color palette.");
            private readonly GUIContent savePaletteGUI = new GUIContent("OK", "Save this color palette.");

            [SerializeField] private ColorPalette colorPalette = null;
            [SerializeField] private string paletteName = string.Empty;
            [SerializeField] private bool doRename = false;

            // -----------------------

            private void OnGUI()
            {
                Undo.RecordObject(this, UndoRecordTitle);

                // Palette name.
                Rect _position = new Rect(5f, 5f, 85f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(_position, paletteNameGUI);

                _position.x += _position.width + 5f;
                _position.width = position.width - _position.x - 5f;

                paletteName = EditorGUI.TextField(_position, paletteName);

                string _value = paletteName.Trim();
                GUILayout.Space(3f);

                // Invalid name mangement.
                if (string.IsNullOrEmpty(_value))
                {
                    DrawHelpBox(EmptyNameMessage, UnityEditor.MessageType.Error);
                }
                else if (Array.Exists(ColorPaletteDatabase.Palettes, (p) => p.Name == _value))
                {
                    DrawHelpBox(ExistingPaletteMessage, UnityEditor.MessageType.Info);
                }
                else
                {
                    // Save palette button.
                    DrawTooltipHelpBox();

                    _position = new Rect()
                    {
                        x = position.width - 55f,
                        y = _position.y + _position.height + 10f,
                        width = 50f,
                        height = 25f
                    };

                    if (GUI.Button(_position, savePaletteGUI))
                    {
                        if (doRename)
                        {
                            colorPalette.Name = paletteName;
                        }
                        else
                        {
                            ColorPaletteDatabase.SavePalette(colorPalette, paletteName);
                        }

                        Close();
                    }
                }

                // ----- Local Methods ----- \\

                void DrawHelpBox(string _message, UnityEditor.MessageType _messageType)
                {
                    Rect _temp = new Rect()
                    {
                        x = 5f,
                        y = _position.y + _position.height + 5f,
                        width = position.width - 10f
                    };

                    _temp.height = EnhancedEditorGUIUtility.GetHelpBoxHeight(_message, _messageType, _temp.width);
                    EditorGUI.HelpBox(_temp, _message, _messageType);
                }

                void DrawTooltipHelpBox()
                {
                    Rect _temp = new Rect()
                    {
                        x = 5f,
                        y = _position.y + _position.height + 5f,
                        width = position.width - 65f
                    };

                    _temp.height = EnhancedEditorGUIUtility.GetHelpBoxHeight(NameTooltip, UnityEditor.MessageType.Info, _temp.width);
                    EditorGUI.HelpBox(_temp, NameTooltip, UnityEditor.MessageType.Info);
                }
            }
        }
        #endregion
    }
}
