// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="ScriptableObject"/> database containing all <see cref="TagData"/> in the project.
    /// </summary>
    [NonEditable("Please use the Multi-Tags window to edit the project tags.")]
    public sealed class TagDatabase : ScriptableSettings {
        #region Global Members
        private static TagDatabase database = null;

        #if UNITY_EDITOR
        /// <summary>
        /// Editor internal getter for the <see cref="TagDatabase"/> instance.
        /// <para/>
        /// As it needs to be set manually at runtime, it uses an internal getter when in editor mode
        /// to be safely able to load it from the database, even if the user deletes it.
        /// </summary>
        internal static Func<TagDatabase> EditorTagDatabaseGetter = null;
        #endif

        /// <summary>
        /// You have to set this reference at runtime to be properly able to use the Multi-Tags system.
        /// <br/>
        /// There are a variety of ways to assign its value:
        /// <list type="bullet">
        /// <item>by <see cref="ScriptableObject"/> reference</item>
        /// <item>using <see cref="Resources.Load(string)"/></item>
        /// <item><see cref="AssetBundle"/></item>
        /// <item>... or any other way you'd like.</item>
        /// </list><para/>
        /// Prefer creating a copy of the original object if you intend to modify its tag content
        /// during runtime, so it won't be saved when exiting play mode.
        /// </summary>
        public static TagDatabase Database {
            get {
                #if UNITY_EDITOR
                if (!Application.isPlaying && (EditorTagDatabaseGetter != null)) {
                    return EditorTagDatabaseGetter();
                }

                if (database == null) {
                    Debug.LogError($"Unassigned {typeof(TagDatabase).Name} reference!\nYou must manually set this database " +
                                   $"reference on game start to be able to properly use the Multi-Tags system.");
                }
                #endif

                return database;
            }
            set {
                database = value;
            }
        }

        // -------------------------------------------
        // Database Content
        // -------------------------------------------

        [SerializeField, SerializeReference] internal TagHolder defaultHolder = null;
        [SerializeField] private TagData[] tags = new TagData[0];

        [SerializeField] internal TagHolder[] holders = new TagHolder[0];
        [SerializeField] internal long counter        = 0;

        // -----------------------

        /// <summary>
        /// Total amount of <see cref="TagHolder"/> in the project.
        /// </summary>
        public int HolderCount {
            get { return holders.Length + 1; }
        }

        /// <summary>
        /// Total count of tags in the project, including both root and <see cref="TagHolder"/> tags.
        /// </summary>
        public int TotalTagCount {
            get {
                int _count = 0;
                for (int i = HolderCount; i-- > 0;) {
                    _count += GetHolderAt(i).Count;
                }

                return _count;
            }
        }
        #endregion

        #region Initialization
        private void OnEnable() {
            // Default holder.
            if (defaultHolder == null) {

                defaultHolder = CreateInstance<TagHolder>();
                defaultHolder.Name = "<Default>";
            }

            #if UNITY_EDITOR
            // On object creation, get all existing Unity tags.
            if ((counter == 0) && (defaultHolder.Tags.Length == 0)) {
                string[] _tags = InternalEditorUtility.tags;

                foreach (string _tag in _tags) {
                    CreateTag(_tag);
                }
            }
            #endif

            // Holder init.
            if (tags.Length != 0) {
                defaultHolder.Tags = tags;
                SaveChanges();
            }
        }

        internal protected override void Init() {
            Database = this;
        }
        #endregion

        // ===== Database ===== \\

        #region Tag & Data Manipulations
        /// <summary>
        /// Get the <see cref="TagData"/> referenced by a specific <see cref="Tag"/>.
        /// </summary>
        /// <param name="_tag"><see cref="Tag"/> to get referencing <see cref="TagData"/>.</param>
        /// <param name="_data"><see cref="TagData"/> referenced by this <see cref="Tag"/> (<see cref="TagData.UnknownTag"/> if no matching tag could be found).</param>
        /// <returns>True if this <see cref="Tag"/> is referencing a valid <see cref="TagData"/> which was found, false otherwise.</returns>
        public bool GetTag(Tag _tag, out TagData _data) {
            if (_tag.GetData(out _data))
                return true;

            _data = TagData.UnknownTag;
            return false;
        }

        /// <summary>
        /// Get the <see cref="TagData"/> associated with a specific id.
        /// </summary>
        /// <param name="_id">ID to get associated <see cref="TagData"/>.</param>
        /// <param name="_data"><see cref="TagData"/> associated with this id (<see cref="TagData.UnknownTag"/> if no matching tag could be found).</param>
        /// <returns>True if a valid <see cref="TagData"/> with this id exist and was found, false otherwise.</returns>
        public bool GetTag(long _id, out TagData _data) {
            for (int i = HolderCount; i-- > 0;) {
                if (GetHolderAt(i).GetTag(_id, out _data))
                    return true;
            }

            _data = TagData.UnknownTag;
            return false;
        }

        /// <summary>
        /// Get the <see cref="TagData"/> associated with a specific name.
        /// </summary>
        /// <param name="_name">Name of the <see cref="TagData"/> to find.</param>
        /// <param name="_data"><see cref="TagData"/> associated with this name (<see cref="TagData.UnknownTag"/> if no matching tag could be found).</param>
        /// <returns>True if a valid <see cref="TagData"/> with this name exist and was found, false otherwise.</returns>
        public bool GetTag(string _name, out TagData _data) {
            for (int i = HolderCount; i-- > 0;) {
                if (GetHolderAt(i).GetTag(_name, out _data))
                    return true;
            }

            _data = TagData.UnknownTag;
            return false;
        }

        /// <summary>
        /// Get a new <see cref="Tag"/> referencing the <see cref="TagData"/> associated with a specific name.
        /// </summary>
        /// <param name="_tag"><see cref="Tag"/> referencing the matching <see cref="TagData"/> with this name.</param>
        /// <inheritdoc cref="GetTag(string, out TagData)"/>
        public bool GetTag(string _name, out Tag _tag) {
            if (GetTag(_name, out TagData _data)) {
                _tag = new Tag(_data.ID);
                return true;
            }

            _tag = default;
            return false;
        }

        /// <returns><see cref="TagData"/> referenced by this <see cref="Tag"/> (null if no matching tag could be found).</returns>
        /// <inheritdoc cref="GetTag(Tag, out TagData)"/>
        public TagData GetTag(Tag _tag) {
            return _tag.GetData();
        }

        /// <returns><see cref="TagData"/> associated with this id (null if no matching tag could be found).</returns>
        /// <inheritdoc cref="GetTag(long, out TagData)"/>
        public TagData GetTag(long _id) {
            if (GetTag(_id, out TagData _data))
                return _data;

            return null;
        }

        /// <returns><see cref="TagData"/> associated with this name (null if no matching tag could be found).</returns>
        /// <inheritdoc cref="GetTag(string, out TagData)"/>
        public TagData GetTag(string _name) {
            if (GetTag(_name, out TagData _data))
                return _data;

            return null;
        }

        // -----------------------

        /// <summary>
        /// Get all <see cref="TagData"/> referenced in a specific <see cref="TagGroup"/> (automatically cached and updated).
        /// </summary>
        /// <param name="_group">Group to get all referencing <see cref="TagData"/>.</param>
        /// <inheritdoc cref="TagGroup.GetData()"/>
        public List<TagData> GetTags(TagGroup _group) {
            return _group.GetData();
        }

        /// <summary>
        /// Get all <see cref="TagData"/> referenced in a specific <see cref="Tag"/>[].
        /// </summary>
        /// <param name="_tags"><see cref="Tag"/>[] to get referencing <see cref="TagData"/>.</param>
        /// <returns>All <see cref="TagData"/> referenced by the tags of the array (with null entries for each tag that could not be found).</returns>
        public TagData[] GetTags(Tag[] _tags) {
            TagData[] _data = Array.ConvertAll(_tags, (t) => {
                return t.GetData();
            });

            return _data;
        }

        /// <summary>
        /// Get all <see cref="TagData"/> associated with some specific ids.
        /// </summary>
        /// <param name="_tagIDs">IDs to get all associated <see cref="TagData"/>.</param>
        /// <returns>All <see cref="TagData"/>[] associated with these ids (with null entries for each tag that could not be found).</returns>
        public TagData[] GetTags(long[] _tagIDs) {
            TagData[] _data = Array.ConvertAll(_tagIDs, (i) => {
                return GetTag(i);
            });

            return _data;
        }

        /// <summary>
        /// Get all <see cref="TagData"/> referenced in a specific <see cref="Tag"/>[], and store them into the buffer.
        /// </summary>
        /// <param name="_tags"><see cref="Tag"/>[] to get referencing <see cref="TagData"/>.</param>
        /// <param name="_data">Buffer to store the data in (with null entries for each tag that could not be found).</param>
        public void GetTagsNonAlloc(Tag[] _tags, TagData[] _data) {
            int _length = Mathf.Min(_tags.Length, _data.Length);

            for (int _i = 0; _i < _length; _i++) {
                _data[_i] = _tags[_i].GetData();
            }
        }

        /// <summary>
        /// Get all <see cref="TagData"/> associated with some specific ids, and store them into the buffer.
        /// </summary>
        /// <param name="_tagIDs">IDs to get all associated <see cref="TagData"/>.</param>
        /// <param name="_data">Buffer to store the data in (with null entries for each tag that could not be found).</param>
        public void GetTagsNonAlloc(long[] _tagIDs, TagData[] _data) {
            int _length = Mathf.Min(_tagIDs.Length, _data.Length);

            for (int _i = 0; _i < _length; _i++) {
                _data[_i] = GetTag(_tagIDs[_i]);
            }
        }
        #endregion

        #region Tag Modifications
        /// <param name="_id">ID of the tag to rename.</param>
        /// <returns>True if the name of the associated tag has been successfully changed,
        /// false if no matching tag could be found or if another one with the same name already exist.</returns>
        /// <inheritdoc cref="SetTagName(TagData, string)"/>
        public bool SetTagName(long _id, string _name) {
            if (GetTag(_id, out TagData _tag)) {
                return SetTagName(_tag, _name);
            }

            return false;
        }

        /// <summary>
        /// Renames an existing tag.
        /// <br/> Note that by default, a tag name has no influence at runtime. Only its id matters.
        /// </summary>
        /// <param name="_tag"><see cref="TagData"/> to rename.</param>
        /// <param name="_name">New tag name.</param>
        /// <returns>True if the name of the associated tag has been successfully changed,
        /// false if another tag with the same name already exist.</returns>
        public bool SetTagName(TagData _tag, string _name) {
            if (DoesTagExist(_name))
                return false;

            RecordChanges();
            _tag.name = _name;
            SaveChanges();

            return true;
        }

        // -----------------------

        /// <summary>
        /// Modifies the color of an existing tag.
        /// <br/> Note that by default, a tag color has no influence at runtime. Only its id matters.
        /// </summary>
        /// <param name="_id">ID of the tag to change color.</param>
        /// <param name="_color">New tag color.</param>
        /// <returns>True if the color of the associated tag has been successfully changed,
        /// false if no matching tag could be found.</returns>
        public bool SetTagColor(long _id, Color _color) {
            if (!GetTag(_id, out TagData _tag))
                return false;

            RecordChanges();
            _tag.Color = _color;
            SaveChanges();

            return true;
        }
        #endregion

        #region Create & Delete
        // -------------------------------------------
        // Create
        // -------------------------------------------

        /// <inheritdoc cref="CreateTag(string, Color, TagHolder)"/>
        public Tag CreateTag(string _name) {
            Color _color = TagData.DefaultColor.Get();
            return CreateTag(_name, _color);
        }

        /// <inheritdoc cref="CreateTag(string, Color, TagHolder)"/>
        public Tag CreateTag(string _name, Color _color) {
            return CreateTag(_name, _color, null);
        }

        /// <summary>
        /// Creates a new tag with a new <see cref="TagData"/> in the project if none with the same name already exist.
        /// </summary>
        /// <param name="_name">Name of the tag.</param>
        /// <param name="_color">Color of the tag (mostly used in editor).</param>
        /// <param name="_holder">Optional holder used to store this tag.</param>
        /// <returns><see cref="Tag"/> referencing the newly created tag, or the one with the same name if found.</returns>
        public Tag CreateTag(string _name, Color _color, TagHolder _holder) {
            // Does not create the tag if one with the same name already exist.
            if (GetTag(_name, out Tag _tag))
                return _tag;

            RecordChanges();
            
            // Increase counter first, so all tags id will be above 0 (used to determine non-null default tags).
            counter++;

            TagData _data = new TagData(counter, _name, _color);

            AddTag(_data, _holder);
            SaveChanges();
            
            return new Tag(_data.ID);
        }

        internal void AddTag(TagData _tag, TagHolder _holder) {

            if (_holder == null) {
                _holder = defaultHolder;
            }

            RecordChanges();
            _holder.AddTag(_tag);
            SaveChanges();
        }

        // -------------------------------------------
        // Delete
        // -------------------------------------------

        /// <param name="_id">ID of the <see cref="TagData"/> to delete.</param>
        /// <inheritdoc cref="DeleteTag(TagData)"/>
        public void DeleteTag(long _id) {
            for (int i = HolderCount; i-- > 0;) {
                if (GetHolderAt(i).DeleteTag(_id)) {

                    SaveChanges();
                    return;
                }
            }
        }

        /// <summary>
        /// Permanentely deletes a specific <see cref="TagData"/> from the project.
        /// </summary>
        /// <param name="_tag"><see cref="TagData"/> to delete.</param>
        public void DeleteTag(TagData _tag) {
            for (int i = HolderCount; i-- > 0;) {
                if (GetHolderAt(i).DeleteTag(_tag)) {

                    SaveChanges();
                    return;
                }

            }
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private void UpdateDefaultTags() {
        }
        #endregion

        // ===== Utility ===== \\

        #region Utility
        /// <summary>
        /// Does a tag with a specific name exist in the project?
        /// </summary>
        /// <param name="_name">Tag name to check.</param>
        /// <returns>True if a tag with this name exist and was found in the project, false otherwise.</returns>
        public bool DoesTagExist(string _name) {
            return GetTag(_name, out TagData _);
        }

        /// <summary>
        /// Does a tag with a specific id exist in the project?
        /// </summary>
        /// <param name="_id">Tag id to check.</param>
        /// <returns>True if a tag with this id exist and was found in the project, false otherwise.</returns>
        public bool DoesTagExist(long _id) {
            return GetTag(_id, out TagData _);
        }

        // -------------------------------------------
        // Getter
        // -------------------------------------------

        /// <summary>
        /// Get this database <see cref="TagHolder"/> at the given index.
        /// </summary>
        /// <param name="_index">Index of the <see cref="TagHolder"/> to get.</param>
        /// <returns>The <see cref="TagHolder"/> at the given index.</returns>
        public TagHolder GetHolderAt(int _index) {
            if (_index == 0)
                return defaultHolder;

            return holders[_index - 1];
        }

        // -------------------------------------------
        // Other
        // -------------------------------------------

        /// <summary>
        /// Sorts all tags in the database by their name.
        /// </summary>
        public void SortTagsByName() {
            for (int i = HolderCount; i-- > 0;) {
                GetHolderAt(i).SortTagsByName();
            }
        }
        #endregion

        #region Editor Utility
        /// <summary>
        /// Delegate used to open the multi-tags window.
        /// </summary>
        internal static Action OnOpenMultiTagsWindow = null;

        // -----------------------

        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        private static void OpenMultiTagsWindow() {
            OnOpenMultiTagsWindow?.Invoke();
        }

        /// <summary>
        /// Record any changes to be made in the database.
        /// </summary>
        private void RecordChanges() {
            #if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(this, "Multi-Tags Database changes");

            for (int i = 0; i < HolderCount; i++) {
                GetHolderAt(i).RecordChanges();
            }
            #endif
        }

        /// <summary>
        /// Save any changes made in the database.
        /// </summary>
        private void SaveChanges() {
            #if UNITY_EDITOR
            tags = defaultHolder.Tags;
            EditorUtility.SetDirty(this);

            for (int i = 0; i < HolderCount; i++) {
                GetHolderAt(i).SaveChanges();
            }
            #endif
        }
        #endregion
    }
}
