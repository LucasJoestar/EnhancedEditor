// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("EnhancedEditor.Editor")]
namespace EnhancedEditor
{
    /// <summary>
    /// Multi-Tags system utility class.
    /// <para/>
    /// Keep in mind that you have to set its <see cref="Database"/> reference value
    /// on game start to be properly able to use it at runtime.
    /// </summary>
    public static class MultiTags
    {
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
        public static TagDatabase Database
        {
            get
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying && (EditorTagDatabaseGetter != null))
                {
                    return EditorTagDatabaseGetter();
                }

                if (database == null)
                {
                    Debug.LogError($"Unassigned {typeof(TagDatabase).Name} reference!\nYou must manually set this database " +
                                   $"reference on game start to be able to properly use the Multi-Tags system.");

                    database = ScriptableObject.CreateInstance<TagDatabase>();
                }
                #endif

                return database;
            }
            set
            {
                database = value;
            }
        }
        #endregion

        #region Management
        /// <inheritdoc cref="CreateTag(string, Color)"/>
        public static Tag CreateTag(string _name)
        {
            Color _color = TagData.DefaultColor.Get();
            return CreateTag(_name, _color);
        }

        /// <summary>
        /// Creates a new tag with a new <see cref="TagData"/> in the project if none with the same name already exist.
        /// </summary>
        /// <param name="_name">Name of the tag.</param>
        /// <param name="_color">Color of the tag (mostly used in editor).</param>
        /// <returns><see cref="Tag"/> referencing the newly created tag, or the one with the same name if found.</returns>
        public static Tag CreateTag(string _name, Color _color)
        {
            // Does not create the tag if one with the same name already exist.
            if (GetTag(_name, out Tag _tag))
                return _tag;

            RecordChanges();
            _tag = Database.CreateTag(_name, _color);

            SaveChanges();
            return _tag;
        }

        // -----------------------

        /// <param name="_id">ID of the <see cref="TagData"/> to delete.</param>
        /// <inheritdoc cref="DeleteTag(TagData)"/>
        public static void DeleteTag(long _id)
        {
            for (int _i = 0; _i < Database.tags.Length; _i++)
            {
                TagData _tag = Database.tags[_i];
                if (_tag.ID == _id)
                {
                    RecordChanges();
                    ArrayUtility.RemoveAt(ref Database.tags, _i);

                    SaveChanges();
                    break;
                }
            }
        }

        /// <summary>
        /// Permanentely deletes a specific <see cref="TagData"/> from the project.
        /// </summary>
        /// <param name="_tag"><see cref="TagData"/> to delete.</param>
        public static void DeleteTag(TagData _tag)
        {
            RecordChanges();
            ArrayUtility.Remove(ref Database.tags, _tag);

            SaveChanges();
        }
        #endregion

        #region Tag & Data Manipulations
        /// <summary>
        /// Get the <see cref="TagData"/> referenced by a specific <see cref="Tag"/>.
        /// </summary>
        /// <param name="_tag"><see cref="Tag"/> to get referencing <see cref="TagData"/>.</param>
        /// <param name="_data"><see cref="TagData"/> referenced by this <see cref="Tag"/> (<see cref="TagData.UnknownTag"/> if no matching tag could be found).</param>
        /// <returns>True if this <see cref="Tag"/> is referencing a valid <see cref="TagData"/> which was found, false otherwise.</returns>
        public static bool GetTag(Tag _tag, out TagData _data)
        {
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
        public static bool GetTag(long _id, out TagData _data)
        {
            foreach (TagData _tag in Database.tags)
            {
                if (_tag.ID == _id)
                {
                    _data = _tag;
                    return true;
                }
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
        public static bool GetTag(string _name, out TagData _data)
        {
            foreach (TagData _tag in Database.tags)
            {
                if (_tag.name == _name)
                {
                    _data = _tag;
                    return true;
                }
            }

            _data = TagData.UnknownTag;
            return false;
        }

        /// <summary>
        /// Get a new <see cref="Tag"/> referencing the <see cref="TagData"/> associated with a specific name.
        /// </summary>
        /// <param name="_tag"><see cref="Tag"/> referencing the matching <see cref="TagData"/> with this name.</param>
        /// <inheritdoc cref="GetTag(string, out TagData)"/>
        public static bool GetTag(string _name, out Tag _tag)
        {
            if (GetTag(_name, out TagData _data))
            {
                _tag = new Tag(_data.ID);
                return true;
            }

            _tag = default;
            return false;
        }

        /// <returns><see cref="TagData"/> referenced by this <see cref="Tag"/> (null if no matching tag could be found).</returns>
        /// <inheritdoc cref="GetTag(Tag, out TagData)"/>
        public static TagData GetTag(Tag _tag)
        {
            TagData _data = _tag.GetData();
            return _data;
        }

        /// <returns><see cref="TagData"/> associated with this id (null if no matching tag could be found).</returns>
        /// <inheritdoc cref="GetTag(long, out TagData)"/>
        public static TagData GetTag(long _id)
        {
            if (GetTag(_id, out TagData _data))
                return _data;

            return null;
        }

        /// <returns><see cref="TagData"/> associated with this name (null if no matching tag could be found).</returns>
        /// <inheritdoc cref="GetTag(string, out TagData)"/>
        public static TagData GetTag(string _name)
        {
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
        public static List<TagData> GetTags(TagGroup _group)
        {
            List<TagData> _data = _group.GetData();
            return _data;
        }

        /// <summary>
        /// Get all <see cref="TagData"/> referenced in a specific <see cref="Tag"/>[].
        /// </summary>
        /// <param name="_tags"><see cref="Tag"/>[] to get referencing <see cref="TagData"/>.</param>
        /// <returns>All <see cref="TagData"/> referenced by the tags of the array (with null entries for each tag that could not be found).</returns>
        public static TagData[] GetTags(Tag[] _tags)
        {
            TagData[] _data = Array.ConvertAll(_tags, (t) =>
            {
                TagData _tagData = t.GetData();
                return _tagData;
            });

            return _data;
        }

        /// <summary>
        /// Get all <see cref="TagData"/> associated with some specific ids.
        /// </summary>
        /// <param name="_tagIDs">IDs to get all associated <see cref="TagData"/>.</param>
        /// <returns>All <see cref="TagData"/>[] associated with these ids (with null entries for each tag that could not be found).</returns>
        public static TagData[] GetTags(long[] _tagIDs)
        {
            TagData[] _data = Array.ConvertAll(_tagIDs, (i) =>
            {
                TagData _tagData = GetTag(i);
                return _tagData;
            });

            return _data;
        }

        /// <summary>
        /// Get all <see cref="TagData"/> referenced in a specific <see cref="Tag"/>[], and store them into the buffer.
        /// </summary>
        /// <param name="_tags"><see cref="Tag"/>[] to get referencing <see cref="TagData"/>.</param>
        /// <param name="_data">Buffer to store the data in (with null entries for each tag that could not be found).</param>
        public static void GetTagsNonAlloc(Tag[] _tags, TagData[] _data)
        {
            int _length = Mathf.Min(_tags.Length, _data.Length);
            for (int _i = 0; _i < _length; _i++)
            {
                TagData _tagData = _tags[_i].GetData();
                _data[_i] = _tagData;
            }
        }

        /// <summary>
        /// Get all <see cref="TagData"/> associated with some specific ids, and store them into the buffer.
        /// </summary>
        /// <param name="_tagIDs">IDs to get all associated <see cref="TagData"/>.</param>
        /// <param name="_data">Buffer to store the data in (with null entries for each tag that could not be found).</param>
        public static void GetTagsNonAlloc(long[] _tagIDs, TagData[] _data)
        {
            int _length = Mathf.Min(_tagIDs.Length, _data.Length);
            for (int _i = 0; _i < _length; _i++)
            {
                TagData _tagData = GetTag(_tagIDs[_i]);
                _data[_i] = _tagData;
            }
        }
        #endregion

        #region Tag Modifications
        /// <param name="_id">ID of the tag to rename.</param>
        /// <returns>True if the name of the associated tag has been successfully changed,
        /// false if no matching tag could be found or if another one with the same name already exist.</returns>
        /// <inheritdoc cref="SetTagName(TagData, string)"/>
        public static bool SetTagName(long _id, string _name)
        {
            if (GetTag(_id, out TagData _tag))
            {
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
        public static bool SetTagName(TagData _tag, string _name)
        {
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
        public static bool SetTagColor(long _id, Color _color)
        {
            if (!GetTag(_id, out TagData _tag))
                return false;

            RecordChanges();
            _tag.Color = _color;

            SaveChanges();
            return true;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Does a tag with a specific name exist in the project?
        /// </summary>
        /// <param name="_name">Tag name to check.</param>
        /// <returns>True if a tag with this name exist and was found in the project, false otherwise.</returns>
        public static bool DoesTagExist(string _name)
        {
            foreach (TagData _tag in Database.tags)
            {
                if (_tag.name == _name)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Does a tag with a specific id exist in the project?
        /// </summary>
        /// <param name="_id">Tag id to check.</param>
        /// <returns>True if a tag with this id exist and was found in the project, false otherwise.</returns>
        public static bool DoesTagExist(long _id)
        {
            foreach (TagData _tag in Database.tags)
            {
                if (_tag.ID == _id)
                    return true;
            }

            return false;
        }

        // -----------------------

        /// <summary>
        /// Record any changes to be made in the database.
        /// </summary>
        private static void RecordChanges()
        {
            #if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Database, "Multi-Tags Database changes");
            #endif
        }

        /// <summary>
        /// Save any changes made in the database.
        /// </summary>
        private static void SaveChanges()
        {
            #if UNITY_EDITOR
            EditorUtility.SetDirty(Database);
            #endif
        }
        #endregion
    }
}
