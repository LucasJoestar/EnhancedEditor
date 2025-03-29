// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="ScriptableObject"/> wrapper class for multiple <see cref="TagData"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "TAG_TagHolder", menuName = InternalUtility.MenuPath + "Tag Holder", order = InternalUtility.MenuOrder)]
    public sealed class TagHolder : ScriptableObject {
        #region Global Members
        private const int DefaultId = 157496;
        
        [SerializeField, Enhanced, ReadOnly] private int id = DefaultId;

        [Section("Tag Holder")]

        [SerializeField] public string Name         = "Tag Holder";
        [SerializeField] public Color DefaultColor  = TagData.DefaultColor.Get();

        [SerializeField] public TagData[] Tags      = new TagData[0];

        // -----------------------

        /// <summary>
        /// Total amount of <see cref="TagData"/> in this object.
        /// </summary>
        public int Count {
            get { return Tags.Length; }
        }
        #endregion

        #region Scriptable Object
        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        private void OnValidate() {
            if (Application.isPlaying || (this == TagDatabase.Database.GetHolderAt(0)))
                return;

            // Clear content if duplicate.
            foreach (TagHolder _holder in TagDatabase.Database.holders) {
                if ((_holder != this) && (_holder.id == id)) {

                    id   = EnhancedUtility.GenerateGUID();
                    Tags = new TagData[0];
                    Name = "Tag Holder";

                    this.LogErrorMessage("Duplicate - reset values");
                    break;
                }
            }
        }
        #endif
        #endregion

        #region Operator
        public TagData this[int _index] {
            get { return Tags[_index]; }
            set { Tags[_index] = value; }
        }
        #endregion

        #region Management
        // -------------------------------------------
        // Create
        // -------------------------------------------

        public void CreateTag(string _name, Color _color) {
            TagDatabase.Database.CreateTag(_name, _color, this);
        }

        internal void AddTag(TagData _tag) {
            RecordChanges();
            ArrayUtility.Add(ref Tags, _tag);

            #if UNITY_EDITOR
            // Only sort tags in editor, for users comfort; avoid loop and string costs at runtime.
            SortTagsByName();
            #endif

            SaveChanges();
        }

        // -------------------------------------------
        // Delete
        // -------------------------------------------

        /// <param name="_id">ID of the <see cref="TagData"/> to delete.</param>
        /// <inheritdoc cref="DeleteTag(TagData)"/>
        public bool DeleteTag(long _id) {
            for (int i = Tags.Length; i-- > 0;) {
                if (Tags[i].ID == _id) {

                    RecordChanges();
                    ArrayUtility.RemoveAt(ref Tags, i);
                    SaveChanges();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Permanentely deletes a specific <see cref="TagData"/> from the project.
        /// </summary>
        /// <param name="_tag"><see cref="TagData"/> to delete.</param>
        public bool DeleteTag(TagData _tag) {
            RecordChanges();

            if (ArrayUtility.Remove(ref Tags, _tag)) {
                SaveChanges();
                return true;
            }

            return false;
        }
        #endregion

        // ===== Utility ===== \\

        #region Utility
        /// <param name="_id">ID of the <see cref="TagData"/> to find.</param>
        /// <inheritdoc cref="GetTag(string,out TagData)"/>
        public bool GetTag(long _id, out TagData _data) {
            for (int i = Tags.Length; i-- > 0;) {
                TagData _temp = Tags[i];

                if (_temp.ID == _id) {
                    _data = _temp;
                    return true;
                }
            }

            _data = null;
            return false;
        }

        /// <summary>
        /// Finds a specific <see cref="TagData"/> from this holder.
        /// </summary>
        /// <param name="_name">Name of the <see cref="TagData"/> to find.</param>
        /// <param name="_data">The retrieved <see cref="TagData"/> (null if none).</param>
        /// <returns>True if the corresponding tag could be found, false otherwise.</returns>
        public bool GetTag(string _name, out TagData _data) {
            for (int i = Tags.Length; i-- > 0;) {
                TagData _temp = Tags[i];

                if (_temp.Name.Equals(_name, StringComparison.Ordinal)) {
                    _data = _temp;
                    return true;
                }
            }

            _data = null;
            return false;
        }

        /// <summary>
        /// Sorts all tags in the database by their name.
        /// </summary>
        public void SortTagsByName() {
            Array.Sort(Tags, (a, b) => {
                return a.CompareNameTo(b);
            });
        }
        #endregion

        #region Editor Utility
        /// <summary>
        /// Record any changes to be made in this holder.
        /// </summary>
        internal void RecordChanges() {
            #if UNITY_EDITOR
            try
            {
                Undo.RegisterCompleteObjectUndo(this, "Tag Holder changes");
            }
            catch (ArgumentNullException) { }
            #endif
        }

        /// <summary>
        /// Save any changes made in this holder.
        /// </summary>
        internal void SaveChanges() {
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
        #endregion
    }
}
