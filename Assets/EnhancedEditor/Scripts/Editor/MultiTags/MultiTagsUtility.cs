// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Multi-Tags system related editor utility class.
    /// <para/>
    /// Contains a bunch of useful methods and shortcuts
    /// to use and modify project tags.
    /// </summary>
	public static class MultiTagsUtility
    {
        #region Global Members
        /// <summary>
        /// Auto-managed <see cref="ScriptableObject"/> resource for the project Multi-Tags system.
        /// <para/>
        /// Use <see cref="Settings"/> to get its value.
        /// </summary>
        public static readonly AutoManagedResource<TagSettings> Resource = new AutoManagedResource<TagSettings>(false);

        /// <summary>
        /// Object storing all tags of the project.
        /// </summary>
        public static TagSettings Settings => Resource.GetResource();

        // -----------------------

        /// <summary>
        /// Called when a new tag is created.
        /// </summary>
        public static Action<long> OnCreateTag = null;

        /// <summary>
        /// Called when a tag is delete from the project.
        /// </summary>
        public static Action<long> OnDeleteTag = null;
        #endregion

        #region Tag Management
        /// <inheritdoc cref="TagSettings.CreateTag(string, Color)"/>
        public static long CreateTag(string _name, Color _color)
        {
            TagSettings _settings = Settings;

            Undo.RecordObject(_settings, "create new tag");
            long _id = _settings.CreateTag(_name, _color);

            EditorUtility.SetDirty(_settings);
            OnCreateTag?.Invoke(_id);

            return _id;
        }

        /// <inheritdoc cref="TagSettings.DeleteTag(long)"/>
        public static void DeleteTag(long _id)
        {
            TagSettings _settings = Settings;

            Undo.RecordObject(_settings, "delete tag");
            _settings.DeleteTag(_id);

            EditorUtility.SetDirty(_settings);
            OnDeleteTag?.Invoke(_id);
        }
        #endregion

        #region Tag Mediator
        /// <inheritdoc cref="TagSettings.GetAllTagObjects"/>
        public static TagObject[] GetAllTags()
        {
            return Settings.GetAllTagObjects();
        }

        /// <inheritdoc cref="TagSettings.GetTags(TagGroup)"/>
        public static TagObject[] GetTags(TagGroup _tags)
        {
            return Settings.GetTags(_tags);
        }

        /// <inheritdoc cref="TagSettings.GetTags(Tag[])"/>
        public static TagObject[] GetTags(Tag[] _tags)
        {
            return Settings.GetTags(_tags);
        }

        /// <inheritdoc cref="TagSettings.GetTag(long, out TagObject)"/>
        public static bool GetTag(long _id, out TagObject _tagObject)
        {
            return Settings.GetTag(_id, out _tagObject);
        }
        #endregion

        #region Tag Modifications
        /// <inheritdoc cref="TagSettings.SetTagName(long, string)"/>
        /// <param name="_tag">Tag to modify name.</param>
        public static bool SetTagName(Tag _tag, string _newName)
        {
            return SetTagName(_tag.ID, _newName);
        }

        /// <inheritdoc cref="TagSettings.SetTagName(long, string))"/>
        public static bool SetTagName(long _id, string _newName)
        {
            TagSettings _settings = Settings;

            Undo.RecordObject(_settings, "modify tag name");
            bool _value = _settings.SetTagName(_id, _newName);

            EditorUtility.SetDirty(_settings);

            return _value;
        }

        // -----------------------

        /// <inheritdoc cref="TagSettings.SetTagColor(long, Color)"/>
        /// <param name="_tag">Tag to modify color.</param>
        public static void SetTagColor(Tag _tag, Color _newColor)
        {
            SetTagColor(_tag.ID, _newColor);
        }

        /// <inheritdoc cref="TagSettings.SetTagColor(long, Color)"/>
        public static void SetTagColor(long _id, Color _newColor)
        {
            TagSettings _settings = Settings;

            Undo.RecordObject(_settings, "modify tag color");
            _settings.SetTagColor(_id, _newColor);

            EditorUtility.SetDirty(_settings);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="TagSettings.DoesTagExist(string, out long)"/>
        public static bool DoesTagExist(string _name, out long _id)
        {
            return Settings.DoesTagExist(_name, out _id);
        }

        /// <inheritdoc cref="TagSettings.DoesTagExist(long)"/>
        public static bool DoesTagExist(long _id)
        {
            return Settings.DoesTagExist(_id);
        }

        /// <summary>
        /// Sorts the <see cref="Tag"/>s of a group by their name.
        /// </summary>
        /// <param name="_tags">Group to sort.</param>
        public static void SortTagsByName(TagGroup _tags)
        {
            Array.Sort(_tags.Tags, (a, b) =>
            {
                if (GetTag(a.ID, out TagObject _aTag) && GetTag(b.ID, out TagObject _bTag))
                    return _aTag.CompareNameTo(_bTag);

                return a.ID.CompareTo(b.ID);
            });
        }
        #endregion
    }
}
