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
    /// <see cref="ScriptableObject"/> for editor purpose, allowing to write and share
    /// nicely displayed documents across the project.
    /// </summary>
    [CreateAssetMenu(fileName = "DOC_NewDocument", menuName = "Enhanced Editor/Document", order = 185)]
	public class Document : ScriptableObject
    {
        #region Section
        [Serializable]
        public class Section
        {
            public string Name = string.Empty;

            public string Header = string.Empty;
            [EnhancedTextArea] public string Text = string.Empty;
            [EnhancedTextArea] public string ContextText = string.Empty;
            public Texture2D Image = null;

            public string LinkText = string.Empty;
            public string URL = string.Empty;

            public float Space = 0f;
        }
        #endregion

        #region Global Members
        public string Title = "Document";
        public Texture2D Icon;

        public Section[] Sections = new Section[] { };
        #endregion
    }
}
