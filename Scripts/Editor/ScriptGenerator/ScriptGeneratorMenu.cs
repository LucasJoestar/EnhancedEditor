// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
//
// Notes:
//
// ============================================================================ //

using UnityEditor;

namespace EnhancedEditor.Editor
{
    public static partial class ScriptGenerator
    {
        [MenuItem(ScriptCreatorSubMenu + "Enhanced Editor/Template Example", false, MenuItemOrder)]
        public static void CreateTemplateExample() => ScriptGeneratorWindow.GetWindow("H:/- My Stuff -/EnhancedEditorProject/Assets/EnhancedEditor/Editor/ScriptTemplates/EnhancedEditor/TemplateExample.txt");
    }
}
