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
    /// <see cref="ScriptableObject"/> database containing all <see cref="ColorPalette"/> in the project.
    /// </summary>
	public class ColorPaletteDatabase : ScriptableObject
    {
        #region Global Members
        internal const string UndoRecordTitle = "Color Palette Changes";

        /// <summary>
        /// All <see cref="ColorPalette"/> defined in the project.
        /// </summary>
        public ColorPalette[] Palettes = new ColorPalette[] { };

        /// <summary>
        /// Total amount of color palettes in the project.
        /// </summary>
        public int Count => Palettes.Length;
        #endregion

        #region Management
        /// <summary>
        /// Saves a <see cref="ColorPalette"/> in the project and make it persistent.
        /// </summary>
        /// <param name="_palette"><see cref="ColorPalette"/> to save.</param>
        /// <param name="_name">Name of this color palette.</param>
        /// <returns>Saved palette (a new palette is created to replace the original).</returns>
        public ColorPalette SavePalette(ColorPalette _palette, string _name)
        {
            Undo.RecordObject(this, UndoRecordTitle);
            ColorPalette _newPalette = new ColorPalette(_palette)
            {
                Name = _name,
                guid = GUID.Generate().ToString(),
                isPersistent = true
            };           

            ArrayUtility.Add(ref Palettes, _newPalette);
            Array.Sort(Palettes, (a, b) => a.Name.CompareTo(b.Name));

            // Update required controls palette.
            for (int _i = 0; _i < controlsPaletteGUID.Count; _i++)
            {
                if (controlsPaletteGUID[_i] == _palette.guid)
                    controlsPaletteGUID[_i] = _newPalette.guid;
            }

            SaveChanges();

            return _newPalette;
        }

        /// <summary>
        /// Deletes a <see cref="ColorPalette"/> from the project.
        /// </summary>
        /// <param name="_palette"><see cref="ColorPalette"/> to delete.</param>
        public void DeletePalette(ColorPalette _palette)
        {
            Undo.RecordObject(this, UndoRecordTitle);
            ArrayUtility.Remove(ref Palettes, _palette);

            // Update required controls palette.
            for (int _i = 0; _i < controlsPaletteGUID.Count; _i++)
            {
                if (controlsPaletteGUID[_i] == _palette.guid)
                    controlsPaletteGUID[_i] = CreatePalette().guid;
            }

            SaveChanges();
        }

        // -----------------------

        /// <summary>
        /// Creates and registers a new <see cref="ColorPalette"/> with a generated guid.
        /// </summary>
        /// <returns>Created <see cref="ColorPalette"/>.</returns>
        internal ColorPalette CreatePalette()
        {
            ColorPalette _palette = new ColorPalette()
            {
                guid = GUID.Generate().ToString()
            };

            unsavedPalettes.Add(_palette);
            return _palette;
        }

        /// <summary>
        /// Saves any changes made in the database.
        /// </summary>
        internal void SaveChanges()
        {
            EditorUtility.SetDirty(this);
        }
        #endregion

        #region GUI Controls
        [SerializeField, HideInInspector] private List<ColorPalette> unsavedPalettes = new List<ColorPalette>();

        [SerializeField, HideInInspector] private List<int> controlsID = new List<int>();
        [SerializeField, HideInInspector] private List<string> controlsPaletteGUID = new List<string>();

        // -----------------------

        /// <summary>
        /// Get the actual <see cref="ColorPalette"/> associated with a specific control id.
        /// <br/> Creates a new palette for the control if none is already attached.
        /// </summary>
        /// <param name="_controlID">Control id to get associated palette.</param>
        /// <param name="_color">Current color value of this control.
        /// <br/> Used to determine its associated palette when none is already attached.</param>
        /// <returns><see cref="ColorPalette"/> associated with this control.</returns>
        public ColorPalette GetControlPalette(int _controlID, Color _color)
        {
            ColorPalette _palette;
            int _index = controlsID.IndexOf(_controlID);

            if (_index > -1)
            {
                string _guid = controlsPaletteGUID[_index];
                _palette = unsavedPalettes.Find(p => p.guid == _guid);

                if (_palette == null)
                {
                    _palette = Array.Find(Palettes, p => p.guid == _guid);
                }
            }
            else
            {
                // Set this control palette to the first one that could be found containing its current color.
                string _hex = ColorUtility.ToHtmlStringRGBA(_color);
                _palette = Array.Find(Palettes, (p) =>
                {
                    return Array.Exists(p.Colors, c => ColorUtility.ToHtmlStringRGBA(c).Equals(_hex));
                });

                // Creates a new palette for this control if no matching one could be found.
                if (_palette == null)
                    _palette = CreatePalette();

                controlsID.Add(_controlID);
                controlsPaletteGUID.Add(_palette.guid);
            }
            
            return _palette;
        }

        /// <summary>
        /// Set the actual <see cref="ColorPalette"/> associated with a specific control id.
        /// </summary>
        /// <param name="_controlID">Control id to set associated palette.</param>
        /// <param name="_palette">New <see cref="ColorPalette"/> associated with this control.</param>
        public void SetControlPalette(int _controlID, ColorPalette _palette)
        {
            Undo.RecordObject(this, UndoRecordTitle);
            int _index = controlsID.IndexOf(_controlID);

            if (_index > -1)
            {
                Undo.RecordObject(this, UndoRecordTitle);
                controlsPaletteGUID[_index] = _palette.guid;
            }
            else
            {
                controlsID.Add(_controlID);
                controlsPaletteGUID.Add(_palette.guid);
            }
        }

        // -----------------------

        private void ResetControlsData()
        {
            // Reset all color palette controls data.
            // The only reason a control associated palette is serialized is to allow undo operations on unsaved palettes.
            unsavedPalettes = new List<ColorPalette>();

            controlsID = new List<int>();
            controlsPaletteGUID = new List<string>();
        }
        #endregion

        #region Initialization
        private void OnEnable()
        {
            ResetControlsData();
        }
        #endregion
    }
}
