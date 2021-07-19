// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Multi-Tags system related runtime utility class.
    /// <para/>
    /// Keep in mind that you have to set its <see cref="Settings"/> value
    /// on game start to be properly able to use it.
    /// </summary>
    public static class MultiTags
    {
        #region Global Members
        /// <summary>
        /// You have to set this at runtime to be properly able to use the Multi-Tags system.
        /// <para/>
        /// There are a variety of ways to assign its value:
        /// by <see cref="ScriptableObject"/> reference, using <see cref="Resources.Load(string)"/>,
        /// <see cref="AssetBundle"/>, or any other way you'd like.
        /// <para/>
        /// Prefer creating a copy of the original object if you intend to modify tags
        /// during runtime, so they won't be saved when exiting play mode.
        /// </summary>
        public static TagSettings Settings = null;
        #endregion

        #region Tag Management
        /// <inheritdoc cref="TagSettings.CreateTag(string, Color)"/>
        public static long CreateTag(string _name, Color _color)
        {
            return Settings.CreateTag(_name, _color);
        }

        /// <inheritdoc cref="TagSettings.DeleteTag(long)"/>
        public static void DeleteTag(long _id)
        {
            Settings.DeleteTag(_id);
        }
        #endregion

        #region Tag Mediator
        /// <inheritdoc cref="TagSettings.GetAllTags()"/>
        public static TagGroup GetAllTags()
        {
            return Settings.GetAllTags();
        }

        /// <inheritdoc cref="TagSettings.GetTag(string, out Tag)"/>
        public static bool GetTag(string _name, out Tag _tag)
        {
            return Settings.GetTag(_name, out _tag);
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
        #endregion
    }
}
