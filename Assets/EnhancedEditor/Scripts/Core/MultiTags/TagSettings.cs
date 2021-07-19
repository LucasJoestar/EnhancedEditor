// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// <see cref="ScriptableObject"/> referencing all tags within the project.
    /// </summary>
    [NonEditable("If you want to edit the project tags, please use the MultiTagsWindow instead.")]
    public class TagSettings : ScriptableObject
    {
        #region Global Members
        [SerializeField] private TagObject[] tags = new TagObject[] { };
        [SerializeField] private long counter = 0;
        #endregion

        #region Initialization
        private void OnEnable()
        {
            #if UNITY_EDITOR
            // On object creation, get all Unity tags if no one has been created.
            if ((counter == 0) && (tags.Length == 0))
            {
                string[] _tags = UnityEditorInternal.InternalEditorUtility.tags;
                for (int _i = 0; _i < _tags.Length; _i++)
                {
                    string _tag = _tags[_i];
                    CreateTag(_tag, Color.white);
                }
            }
            #endif
        }
        #endregion

        #region Tag Management
        /// <summary>
        /// Creates a new tag if none with the same name already exist.
        /// </summary>
        /// <param name="_name">Tag name.</param>
        /// <param name="_color">Tag color (mostly used in editor).</param>
        /// <returns>Id of the created tag, or that of the one with the same name if found.</returns>
        public long CreateTag(string _name, Color _color)
        {
            // Does not create the tag if one with the same name already exist.
            if (DoesTagExist(_name, out long _tagID))
                return _tagID;

            TagObject _newTag = new TagObject(counter, _name, _color);
            ArrayExtensions.Add(ref tags, _newTag);
            counter++;

            #if UNITY_EDITOR
            // Only sort in editor, for user comfort; avoid loop and string costs at runtime.
            SortTagsByName();
            #endif
          
            return _newTag.ID;
        }

        /// <summary>
        /// Deletes the tag with the specified id from the project.
        /// </summary>
        /// <param name="_id">Id of the tag to remove.</param>
        public void DeleteTag(long _id)
        {
            for (int _i = 0; _i < tags.Length; _i++)
            {
                TagObject _tag = tags[_i];
                if (_tag.ID == _id)
                {
                    ArrayExtensions.RemoveAt(ref tags, _i);
                    break;
                }
            }
        }
        #endregion

        #region Tag Object Mediator
        /// <summary>
        /// Get raw project tags as <see cref="TagObject"/>s.
        /// </summary>
        /// <returns><see cref="TagObject"/> array of all tags in the project.</returns>
        public TagObject[] GetAllTagObjects()
        {
            TagObject[] _copy = new TagObject[tags.Length];
            tags.CopyTo(_copy, 0);

            return _copy;
        }

        /// <inheritdoc cref="GetTags(Tag[])"/>
        /// <param name="_group">Group to get <see cref="Tag"/>s related <see cref="TagObject"/>s.</param>
        public TagObject[] GetTags(TagGroup _group)
        {
            return GetTags(_group.Tags);
        }

        /// <summary>
        /// Get all <see cref="TagObject"/>s referenced by specific <see cref="Tag"/>s.
        /// </summary>
        /// <param name="_tags"><see cref="Tag"/>s to get associated <see cref="TagObject"/>s.</param>
        /// <returns>Array of all referenced <see cref="TagObject"/>s.</returns>
        public TagObject[] GetTags(Tag[] _tags)
        {
            TagObject[] _tagObjects = Array.ConvertAll(_tags, (t) =>
            {
                GetTag(t.ID, out TagObject _tag);
                return _tag;
            });

            return _tagObjects;
        }

        /// <summary>
        /// Get the <see cref="TagObject"/> referenced by a specific id.
        /// </summary>
        /// <param name="_id">Id to get referenced <see cref="TagObject"/>.</param>
        /// <param name="_tag">Found <see cref="TagObject"/> associated with this id.</param>
        /// <returns>True if a <see cref="TagObject"/> associated with the specified id was found,
        /// false otherwise.</returns>
        public bool GetTag(long _id, out TagObject _tag)
        {
            for (int _i = 0; _i < tags.Length; _i++)
            {
                _tag = tags[_i];
                if (_tag.ID == _id)
                    return true;
            }

            _tag = null;
            return false;
        }
        #endregion

        #region Tag Mediator
        /// <summary>
        /// Get project tags as a <see cref="TagGroup"/>.
        /// </summary>
        /// <returns><see cref="TagGroup"/> containing all project tags.</returns>
        public TagGroup GetAllTags()
        {
            Tag[] _tags = Array.ConvertAll(tags, (t) => new Tag(t.ID));
            return new TagGroup(_tags);
        }

        /// <summary>
        /// Get the <see cref="Tag"/> associated with a specific name.
        /// </summary>
        /// <param name="_name">Name of the tag to find.</param>
        /// <param name="_tag"><see cref="Tag"/> associated with the specific name.</param>
        /// <returns>True if a tag with the specified name exist and was found, false otherwise.</returns>
        public bool GetTag(string _name, out Tag _tag)
        {
            if (DoesTagExist(_name, out long _id))
            {
                _tag = new Tag(_id);
                return true;
            }

            _tag = default;
            return false;
        }
        #endregion

        #region Tag Modifications
        /// <summary>
        /// Modifies the name of an existing tag.
        /// <para/>
        /// Note that by default, tag name has no influence during runtime. Only its id matters.
        /// </summary>
        /// <param name="_id">Id of the tag to modify.</param>
        /// <param name="_newName">New tag name.</param>
        /// <returns>True if tag name has been successfully changed, false if no tag with
        /// matching id was found or if another tag with desired name already exist.</returns>
        public bool SetTagName(long _id, string _newName)
        {
            if (GetTag(_id, out TagObject _tag) && !DoesTagExist(_newName, out _))
            {
                _tag.name = _newName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Modifies the color of an existing tag.
        /// <para/>
        /// Note that by default, tag color has no influence during runtime. Only its id matters.
        /// </summary>
        /// <param name="_id">Id of the tag to modify.</param>
        /// <param name="_newColor">New tag color.</param>
        public void SetTagColor(long _id, Color _newColor)
        {
            if (GetTag(_id, out TagObject _tag))
                _tag.Color = _newColor;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Does a tag with a specific name exist?
        /// </summary>
        /// <param name="_name">Name of the tag to check.</param>
        /// <param name="_id">Id of matching tag found.</param>
        /// <returns>True if a tag with specified name exist in project, false otherwise.</returns>
        public bool DoesTagExist(string _name, out long _id)
        {
            foreach (var tag in tags)
            {
                if (tag.name == _name)
                {
                    _id = tag.ID;
                    return true;
                }
            }

            _id = -1;
            return false;
        }

        /// <summary>
        /// Does a tag with a specific id exist?
        /// </summary>
        /// <param name="_id">Id of the tag to check.</param>
        /// <returns>True if a tag with specified id exist in project, false otherwise.</returns>
        public bool DoesTagExist(long _id)
        {
            foreach (var tag in tags)
            {
                if (tag.ID == _id)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Sorts project tags by their name (mostly used in editor).
        /// </summary>
        public void SortTagsByName()
        {
            Array.Sort(tags, (a, b) =>
            {
                return a.CompareNameTo(b);
            });
        }
        #endregion
    }

    /// <summary>
    /// Internal Multi-Tags system class used to define a tag in the project.
    /// Always prefer using <see cref="Tag"/> and <see cref="TagGroup"/> during runtime.
    /// </summary>
    [Serializable]
    public class TagObject
    {
        /// <summary>
        /// Default "unknown" tag, used when no matching tag is found and requiring a non null value (mostly used in editor).
        /// </summary>
        public static readonly TagObject UnknownTag = new TagObject(-1, "-----", Color.white);

        #region Content
        [SerializeField] private long id = 0;
        [SerializeField] internal string name = "NewTag";

        /// <summary>
        /// The id of the tag. This is the only identifier of the object,
        /// and cannot be changed.
        /// </summary>
        public long ID => id;

        /// <summary>
        /// Name associated with this tag.
        /// <para/>
        /// Always prefer using utility methods like from <see cref="MultiTags"/>
        /// or <see cref="TagSettings"/> for modifications or tag comparisons when available.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Color of this tag (mostly used in editor).
        /// </summary>
        public Color Color = Color.white;

        // -----------------------

        /// <summary>
        /// Creates a new <see cref="TagObject"/>.
        /// <para/>
        /// Internally restrict uses to avoid poor id setup.
        /// Please use <see cref="TagSettings.CreateTag(string, Color)"/> to create a new tag instead.
        /// </summary>
        /// <param name="_id">Id of the tag to create (related to <see cref="TagSettings.counter"/>).</param>
        /// <param name="_name">Tag name.</param>
        /// <param name="_color">Tag color (mostly used in editor).</param>
        internal TagObject(long _id, string _name, Color _color)
        {
            id = _id;
            name = _name;

            Color = _color;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Compares this tag with another one according to their name.
        /// </summary>
        /// <param name="_tag">Tag to compare to this one.</param>
        /// <returns>Comparison value between the two names.</returns>
        public int CompareNameTo(TagObject _tag)
        {
            return name.CompareTo(_tag.name);
        }
        #endregion
    }
}
