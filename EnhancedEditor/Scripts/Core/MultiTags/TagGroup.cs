// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;

namespace EnhancedEditor
{
    /// <summary>
    /// <see cref="Tag"/>[] wrapper, used to reference multiple tags in a single group.
    /// <para/>
    /// Contains multiple utility methods to dynamically and safely manipulate its content.
    /// </summary>
    [Serializable]
    public class TagGroup
    {
        #region Global Members
        /// <summary>
        /// All <see cref="Tag"/> defined in this group.
        /// </summary>
        public Tag[] Tags = new Tag[] { };

        /// <summary>
        /// Total amount of <see cref="Tag"/> defined in this group.
        /// </summary>
        public int Length => Tags.Length;

        // -----------------------

        /// <inheritdoc cref="TagGroup"/>
        public TagGroup() { }

        /// <param name="_tags">All <see cref="Tag"/> to encapsulate in this group.</param>
        /// <inheritdoc cref="TagGroup()"/>
        public TagGroup(Tag[] _tags)
        {
            Tags = _tags;
        }
        #endregion

        #region Operator
        public Tag this[int _index]
        {
            get => Tags[_index];
            set => Tags[_index] = value;
        }
        #endregion

        #region Management
        /// <summary>
        /// Adds a new <see cref="Tag"/> into this group.
        /// </summary>
        /// <param name="_tag">Tag to add.</param>
        public void AddTag(Tag _tag)
        {
            if (!Contains(_tag))
            {
                ArrayUtility.Add(ref Tags, _tag);
            }
        }

        /// <summary>
        /// Removes a <see cref="Tag"/> from this group.
        /// </summary>
        /// <param name="_tag">Tag to remove.</param>
        public void RemoveTag(Tag _tag)
        {
            if (Contains(_tag, out int _index))
            {
                ArrayUtility.RemoveAt(ref Tags, _index);
            }
        }
        #endregion

        #region Utility
        [NonSerialized] private readonly List<TagData> data = new List<TagData>();

        // -----------------------

        /// <inheritdoc cref="Contains(Tag, out int)"/>
        public bool Contains(Tag _tag)
        {
            return ArrayUtility.Contains(Tags, _tag);
        }

        /// <summary>
        /// Does this group contain a specific tag?
        /// </summary>
        /// <param name="_tag"><see cref="Tag"/> to check presence in group.</param>
        /// <param name="_index">Index of the matching tag in group.</param>
        /// <returns>True if the group contains the specified tag, false otherwise.</returns>
        public bool Contains(Tag _tag, out int _index)
        {
            for (int _i = 0; _i < Tags.Length; _i++)
            {
                if (Tags[_i] == _tag)
                {
                    _index = _i;
                    return true;
                }
            }

            _index = -1;
            return false;
        }

        /// <summary>
        /// Get all <see cref="TagData"/> referenced in this group (automatically cached and updated).
        /// </summary>
        /// <returns>All <see cref="TagData"/> referenced in this group (with null entries for each tag that could not be found).
        /// <br/> Using a <see cref="List{T}"/> allows to return a direct reference of the cached value instead of allocating a new array.
        /// Please copy its content to a new collection if you intend to modify it.</returns>
        public List<TagData> GetData()
        {
            data.Resize(Tags.Length);

            // Update data with all tag cached values.
            for (int _i = 0; _i < Length; _i++)
            {
                TagData _data = Tags[_i].GetData();
                data[_i] = _data;
            }
            
            return data;
        }

        /// <summary>
        /// Sorts all tags in this group by their name.
        /// </summary>
        public void SortTagsByName()
        {
            Array.Sort(Tags, (a, b) =>
            {
                return a.GetData().CompareNameTo(b.GetData());
            });
        }
        #endregion
    }
}
