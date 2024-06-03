// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Tag identifier, referencing an existing <see cref="TagData"/> with the help of its <see cref="ID"/>.
    /// <para/>
    /// It is notably used in the <see cref="ExtendedBehaviour"/> component to assign multiple tags to a single <see cref="GameObject"/>.
    /// </summary>
    [Serializable]
    public struct Tag {
        #region Global Members
        [SerializeField] private long id;
        [NonSerialized]  private TagData data;

        /// <summary>
        /// ID of this tag, defining to which <see cref="TagData"/> it refers to.
        /// <para/>
        /// You can use the <see cref="MultiTags"/> utility class to get informations about existing tags and their id.
        /// </summary>
        public long ID {
            readonly get { return id; }
            set {
                id   = value;
                data = null;
            }
        }

        /// <summary>
        /// Name of the <see cref="TagData"/> referenced by this object.
        /// </summary>
        public string Name {
            get {
                string _name = GetData(out TagData _tag)
                             ? _tag.name
                             : string.Empty;

                return _name;
            }
        }

        /// <summary>
        /// Color of the <see cref="TagData"/> referenced by this object.
        /// </summary>
        public Color Color {
            get {
                Color _color = GetData(out TagData _tag)
                             ? _tag.Color
                             : TagData.DefaultColor.Get();

                return _color;
            }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Creates a new tag identifier. Its id defines to which <see cref="TagData"/> from the <see cref="TagDatabase"/> it refers to.
        /// <para/>
        /// You can have access to many informations about existing tags and their id from the <see cref="MultiTags"/> utility class.
        /// </summary>
        /// <param name="_id">Identifier defining to which <see cref="TagData"/> this object should refer to.</param>
        public Tag(long _id) {
            id   = _id;
            data = null;
        }
        #endregion

        #region Operator
        public static implicit operator Tag(long _id) {
            return new Tag(_id);
        }

        public static implicit operator Tag(int _id) {
            return new Tag(_id);
        }

        public static bool operator ==(Tag a, Tag b) {
            return a.Equals(b);
        }

        public static bool operator !=(Tag a, Tag b) {
            return !(a == b);
        }

        public override readonly bool Equals(object obj) {
            if (obj is not Tag _tag)
                return false;

            return _tag.Equals(this);
        }

        public override readonly int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the actual <see cref="TagData"/> referenced by this <see cref="ID"/>.
        /// </summary>
        /// <param name="_data"><see cref="TagData"/> referenced by this object id (null if no matching tag could be found).</param>
        /// <returns>True if this object is referencing a valid <see cref="TagData"/> which has been found, false otherwise.</returns>
        public bool GetData(out TagData _data) {
            if ((data != null) && (data.ID != 0)) {
                _data = data;
            } else if (MultiTags.GetTag(id, out _data)) {
                data = _data;
            } else {
                _data = null;
                return false;
            }

            return true;
        }

        /// <returns><see cref="TagData"/> referenced by this object id (null if no matching tag could be found).</returns>
        /// <inheritdoc cref="GetData(out TagData)"/>
        public TagData GetData() {
            if (((data == null) || (data.ID == 0)) && MultiTags.GetTag(id, out TagData _data)) {
                data = _data;
            }

            return data;
        }

        /// <summary>
        /// Is this <see cref="ID"/> referencing a valid <see cref="TagData"/>?
        /// </summary>
        /// <returns>True if this tag id is referencing a valid <see cref="TagData"/>, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsValid() {
            return MultiTags.DoesTagExist(id);
        }

        /// <summary>
        /// Get if this <see cref="Tag"/> is the same than another <see cref="Tag"/>.
        /// </summary>
        /// <param name="_tag">The <see cref="Tag"/> to check.</param>
        /// <returns>True if this <see cref="Tag"/> matches the another given tag, false otherwise.</returns>
        public readonly bool Equals(Tag _tag) {
            return id == _tag.id;
        }
        #endregion
    }
}
