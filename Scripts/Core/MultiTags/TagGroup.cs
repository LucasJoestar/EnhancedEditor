// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EnhancedEditor {
    /// <summary>
    /// <see cref="Tag"/>[] wrapper, used to reference multiple tags in a single group.
    /// <para/>
    /// Contains multiple utility methods to dynamically and safely manipulate its content.
    /// </summary>
    [Serializable]
    public sealed class TagGroup {
        #region Global Members
        /// <summary>
        /// Static empty instance shared across the project.
        /// </summary>
        public static readonly TagGroup Empty = new TagGroup();

        /// <summary>
        /// All <see cref="Tag"/> defined in this group.
        /// </summary>
        public Tag[] Tags = new Tag[] { };

        /// <summary>
        /// Total amount of <see cref="Tag"/> defined in this group.
        /// </summary>
        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Tags.Length; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="TagGroup"/>
        public TagGroup() { }

        /// <param name="_tags">All <see cref="Tag"/> to encapsulate in this group.</param>
        /// <inheritdoc cref="TagGroup()"/>
        public TagGroup(Tag[] _tags) {
            Tags = _tags;
        }
        #endregion

        #region Operator
        public Tag this[int _index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Tags[_index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Tags[_index] = value;
        }
        #endregion

        #region Management
        /// <summary>
        /// Adds a new <see cref="Tag"/> into this group.
        /// </summary>
        /// <param name="_tag">Tag to add.</param>
        public void AddTag(Tag _tag) {
            if (!Contains(_tag)) {
                ArrayUtility.Add(ref Tags, _tag);
            }
        }

        /// <summary>
        /// Removes a <see cref="Tag"/> from this group.
        /// </summary>
        /// <param name="_tag">Tag to remove.</param>
        public void RemoveTag(Tag _tag) {
            if (Contains(_tag, out int _index)) {
                ArrayUtility.RemoveAt(ref Tags, _index);
            }
        }

        /// <summary>
        /// Set the tags of this group.
        /// </summary>
        /// <param name="_group">Group to copy the tags from.</param>
        public void SetTags(TagGroup _group) {
            Tags = _group.Tags;
        }
        #endregion

        #region Utility
        [NonSerialized] private readonly List<TagData> data = new List<TagData>();

        // -----------------------

        /// <inheritdoc cref="Contains(Tag, out int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Tag _tag) {
            return Contains(_tag, out _);
        }

        /// <summary>
        /// Does this group contain a specific tag?
        /// </summary>
        /// <param name="_tag"><see cref="Tag"/> to check presence in group.</param>
        /// <param name="_index">Index of the matching tag in group.</param>
        /// <returns>True if the group contains the specified tag, false otherwise.</returns>
        public bool Contains(Tag _tag, out int _index) {
            int _length = Tags.Length;

            for (int _i = 0; _i < _length; _i++) {
                if (Tags[_i] == _tag) {
                    _index = _i;
                    return true;
                }
            }

            _index = -1;
            return false;
        }

        /// <summary>
        /// Get if this group contains all other tags.
        /// </summary>
        /// <param name="_tags">All tags to check.</param>
        /// <returns>True if this group contains all given tags, false otherwise.</returns>
        public bool ContainsAll(Tag[] _tags) {
            int _length = _tags.Length;

            for (int _i = 0; _i < _length; _i++) {
                if (!Contains(_tags[_i])) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Does this group contain any tag in another group?
        /// </summary>
        /// <param name="_group">Group to check content.</param>
        /// <returns>True if this group contain any of the other group tag, false otherwise.</returns>
        public bool ContainsAny(TagGroup _group, bool validIfEmpty = false) {
            if (validIfEmpty && ((Count == 0) || _group.Count == 0))
                return true;

            foreach (Tag _tag in _group.Tags) {
                if (Contains(_tag)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get if this group contains all tags in another group.
        /// </summary>
        /// <param name="_group">Group to check the tags.</param>
        /// <returns>True if this group contains all tags of the other given group, false otherwise.</returns>
        public bool ContainsAll(TagGroup _group) {
            foreach (Tag _tag in _group.Tags) {
                if (!Contains(_tag)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get all <see cref="TagData"/> referenced in this group (automatically cached and updated).
        /// </summary>
        /// <returns>All <see cref="TagData"/> referenced in this group (with null entries for each tag that could not be found).
        /// <br/> Using a <see cref="List{T}"/> allows to return a direct reference of the cached value instead of allocating a new array.
        /// Please copy its content to a new collection if you intend to modify it.</returns>
        public List<TagData> GetData() {
            data.Resize(Tags.Length);

            // Update data with all tag cached values.
            for (int i = Tags.Length; i-- > 0;) {
                data[i] = Tags[i].GetData();
            }

            return data;
        }

        /// <summary>
        /// Sorts all tags in this group by their name.
        /// </summary>
        public void SortTagsByName() {
            Array.Sort(Tags, (a, b) => {
                return a.GetData().CompareNameTo(b.GetData());
            });
        }
        #endregion
    }
}
